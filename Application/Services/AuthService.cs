using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;
using UniStart.Application.Helpers;
using BC = BCrypt.Net.BCrypt;
using System.Security.Cryptography;

namespace UniStart.Application.Services;

public class AuthService : IAuthService
{
    private readonly UniStartDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public AuthService(UniStartDbContext context, IJwtService jwtService, IUnitOfWork unitOfWork,
        IEmailService emailService, INotificationService notificationService)
    {
        _context = context;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // S-4: Password complexity check
        ValidatePasswordComplexity(dto.Password);

        // Check if user already exists (including soft-deleted)
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
            {
                // Restore soft-deleted user with new credentials
                existingUser.IsDeleted = false;
                existingUser.DeletedAt = null;
                existingUser.DeletedBy = null;
                existingUser.Name = InputSanitizer.Sanitize(dto.Name)!;
                existingUser.PasswordHash = BC.HashPassword(dto.Password);
                existingUser.HasCompletedOnboarding = false;
                existingUser.CreatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                var restoredToken = _jwtService.GenerateToken(existingUser);
                var restoredExpiresAt = DateTime.UtcNow.AddHours(24);
                return new AuthResponseDto(
                    existingUser.Id,
                    existingUser.Email,
                    existingUser.Name,
                    existingUser.Role.ToString(),
                    existingUser.HasCompletedOnboarding,
                    existingUser.SubscriptionTier.ToString(),
                    existingUser.SubscriptionExpiresAt,
                    existingUser.EmailVerified,
                    restoredToken,
                    restoredExpiresAt
                );
            }
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user with verification code
        var code = GenerateVerificationCode();
        var user = new User
        {
            Email = dto.Email,
            Name = InputSanitizer.Sanitize(dto.Name)!,
            PasswordHash = BC.HashPassword(dto.Password),
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            EmailVerified = false,
            EmailVerificationCode = code,
            EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();

        // Initialize default skill profiles for the user
        var skills = await _context.Skills.ToListAsync();
        foreach (var skill in skills)
        {
            _context.UserSkillProfiles.Add(new UserSkillProfile
            {
                UserId = user.Id,
                SkillId = skill.Id,
                Level = 50 // Default starting level
            });
        }
        await _unitOfWork.SaveChangesAsync();

        // Create default notification preferences and send verification code
        await _notificationService.EnsurePreferencesExistAsync(user.Id);
        _ = Task.Run(async () =>
        {
            try { await _emailService.SendVerificationCodeAsync(user.Email, user.Name, code); }
            catch { /* logged inside EmailService */ }
        });

        // Generate token
        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
            user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt,
            user.EmailVerified,
            token,
            expiresAt
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // S-5: Account lockout check
        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var minutesLeft = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            throw new UnauthorizedAccessException($"Account is locked. Try again in {minutesLeft} minute(s)");
        }

        if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
        {
            // S-5: Track failed attempts
            if (user != null)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    await _context.SaveChangesAsync();
                    throw new UnauthorizedAccessException("Too many failed attempts. Account locked for 15 minutes");
                }
                await _context.SaveChangesAsync();
            }
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Reset lockout on successful login
        if (user.FailedLoginAttempts > 0)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();
        }

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
            user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt,
            user.EmailVerified,
            token,
            expiresAt
        );
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found");

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
            user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt,
            user.EmailVerified,
            token,
            expiresAt
        );
    }

    public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new KeyNotFoundException("User not found");

        if (user.EmailVerified)
            throw new InvalidOperationException("Email already verified");

        if (user.EmailVerificationCode != dto.Code
            || user.EmailVerificationCodeExpiresAt == null
            || user.EmailVerificationCodeExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired verification code");

        user.EmailVerified = true;
        user.EmailVerificationCode = null;
        user.EmailVerificationCodeExpiresAt = null;
        await _unitOfWork.SaveChangesAsync();

        // Send welcome email after verification
        _ = Task.Run(async () =>
        {
            try { await _emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
            catch { /* logged inside EmailService */ }
        });

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        return new AuthResponseDto(
            user.Id, user.Email, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt
        );
    }

    public async Task ResendVerificationCodeAsync(ResendCodeDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new KeyNotFoundException("User not found");

        if (user.EmailVerified)
            throw new InvalidOperationException("Email already verified");

        var code = GenerateVerificationCode();
        user.EmailVerificationCode = code;
        user.EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendVerificationCodeAsync(user.Email, user.Name, code);
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
        var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
        var email = payload.Email;
        var name = payload.Name ?? email.Split('@')[0];
        var googleId = payload.Subject;

        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                Name = InputSanitizer.Sanitize(name)!,
                PasswordHash = BC.HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow,
                EmailVerified = true,
                GoogleId = googleId
            };
            _context.Users.Add(user);
            await _unitOfWork.SaveChangesAsync();

            var skills = await _context.Skills.ToListAsync();
            foreach (var skill in skills)
            {
                _context.UserSkillProfiles.Add(new UserSkillProfile
                {
                    UserId = user.Id, SkillId = skill.Id, Level = 50
                });
            }
            await _unitOfWork.SaveChangesAsync();
            await _notificationService.EnsurePreferencesExistAsync(user.Id);

            _ = Task.Run(async () =>
            {
                try { await _emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
                catch { /* logged inside EmailService */ }
            });
        }
        else
        {
            if (user.IsDeleted)
            {
                user.IsDeleted = false;
                user.DeletedAt = null;
                user.DeletedBy = null;
            }
            if (string.IsNullOrEmpty(user.GoogleId))
                user.GoogleId = googleId;
            if (!user.EmailVerified)
                user.EmailVerified = true;
            await _unitOfWork.SaveChangesAsync();
        }

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        return new AuthResponseDto(
            user.Id, user.Email, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt
        );
    }

    private static string GenerateVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    /// <summary>S-4: Password must be 8+ chars with uppercase, lowercase, digit, and special character</summary>
    private static void ValidatePasswordComplexity(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            throw new InvalidOperationException("Password must be at least 8 characters long");
        if (!password.Any(char.IsUpper))
            throw new InvalidOperationException("Password must contain at least one uppercase letter");
        if (!password.Any(char.IsLower))
            throw new InvalidOperationException("Password must contain at least one lowercase letter");
        if (!password.Any(char.IsDigit))
            throw new InvalidOperationException("Password must contain at least one digit");
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            throw new InvalidOperationException("Password must contain at least one special character");
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        return new UserDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role.ToString(),
            user.CreatedAt,
            user.LastSeenAt
        );
    }

    public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = InputSanitizer.Sanitize(dto.Name)!;
        
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            // Check if email is already taken
            var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId);
            if (emailExists)
                throw new InvalidOperationException("Email is already taken");
            user.Email = dto.Email;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return new UserDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role.ToString(),
            user.CreatedAt,
            user.LastSeenAt
        );
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        ValidatePasswordComplexity(dto.NewPassword);

        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (!BC.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        user.PasswordHash = BC.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangeEmailAsync(int userId, ChangeEmailDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        if (!BC.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Password is incorrect");

        var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.NewEmail && u.Id != userId);
        if (emailExists)
            throw new InvalidOperationException("Email is already taken");

        user.Email = dto.NewEmail;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }
}
