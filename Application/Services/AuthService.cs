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
        if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
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
}
