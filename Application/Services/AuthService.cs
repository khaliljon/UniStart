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

    public AuthService(UniStartDbContext context, IJwtService jwtService, IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        ValidatePasswordComplexity(dto.Password);

        ValidateName(dto.FirstName, "First name");
        ValidateName(dto.LastName, "Last name");

        await ValidateEmailDomainAsync(dto.Email);

        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
            {
                var restoreCode = GenerateVerificationCode();
                existingUser.IsDeleted = false;
                existingUser.DeletedAt = null;
                existingUser.DeletedBy = null;
                existingUser.FirstName = InputSanitizer.Sanitize(dto.FirstName)!;
                existingUser.LastName = InputSanitizer.Sanitize(dto.LastName)!;
                existingUser.Name = $"{existingUser.FirstName} {existingUser.LastName}".Trim();
                existingUser.PasswordHash = BC.HashPassword(dto.Password);
                existingUser.HasCompletedOnboarding = false;
                existingUser.CreatedAt = DateTime.UtcNow;
                existingUser.EmailVerified = false;
                existingUser.EmailVerificationCode = restoreCode;
                existingUser.EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
                await _unitOfWork.SaveChangesAsync();

                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendVerificationCodeAsync(existingUser.Email, existingUser.Name, restoreCode); }
                    catch {}
                });

                var restoredToken = _jwtService.GenerateToken(existingUser);
                var restoredExpiresAt = DateTime.UtcNow.AddHours(24);
                var restoredSub = await GetSchoolSubdomainAsync(existingUser);
                return new AuthResponseDto(
                    existingUser.Id,
                    existingUser.Email,
                    existingUser.FirstName,
                    existingUser.LastName,
                    existingUser.Name,
                    existingUser.Role.ToString(),
                    existingUser.HasCompletedOnboarding,
                    existingUser.SubscriptionTier.ToString(),
                    existingUser.SubscriptionExpiresAt,
                    existingUser.EmailVerified,
                    restoredToken,
                    restoredExpiresAt,
                    restoredSub,
                    existingUser.PhoneNumber
                );
            }

            if (!existingUser.EmailVerified)
            {
                var reCode = GenerateVerificationCode();
                existingUser.FirstName = InputSanitizer.Sanitize(dto.FirstName)!;
                existingUser.LastName = InputSanitizer.Sanitize(dto.LastName)!;
                existingUser.Name = $"{existingUser.FirstName} {existingUser.LastName}".Trim();
                existingUser.PasswordHash = BC.HashPassword(dto.Password);
                existingUser.EmailVerificationCode = reCode;
                existingUser.EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
                await _unitOfWork.SaveChangesAsync();

                _ = Task.Run(async () =>
                {
                    try { await _emailService.SendVerificationCodeAsync(existingUser.Email, existingUser.Name, reCode); }
                    catch {}
                });

                var reToken = _jwtService.GenerateToken(existingUser);
                var reExpiresAt = DateTime.UtcNow.AddHours(24);
                var reSub = await GetSchoolSubdomainAsync(existingUser);
                return new AuthResponseDto(
                    existingUser.Id,
                    existingUser.Email,
                    existingUser.FirstName,
                    existingUser.LastName,
                    existingUser.Name,
                    existingUser.Role.ToString(),
                    existingUser.HasCompletedOnboarding,
                    existingUser.SubscriptionTier.ToString(),
                    existingUser.SubscriptionExpiresAt,
                    existingUser.EmailVerified,
                    reToken,
                    reExpiresAt,
                    reSub,
                    existingUser.PhoneNumber
                );
            }

            throw new InvalidOperationException("User with this email already exists");
        }

        var code = GenerateVerificationCode();

        var firstName = InputSanitizer.Sanitize(dto.FirstName)!;
        var lastName = InputSanitizer.Sanitize(dto.LastName)!;

        var user = new User
        {
            Email = dto.Email,
            FirstName = firstName,
            LastName = lastName,
            Name = $"{firstName} {lastName}".Trim(),
            PasswordHash = BC.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber?.Trim(),
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow,
            EmailVerified = false,
            EmailVerificationCode = code,
            EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(dto.ReferralCode))
        {
            var refCode = await _context.ReferralCodes
                .FirstOrDefaultAsync(r => r.Code == dto.ReferralCode.Trim().ToUpperInvariant() && r.IsActive);
            if (refCode != null && refCode.OwnerUserId != user.Id)
            {
                user.ReferredByCodeId = refCode.Id;
                _context.ReferralUsages.Add(new ReferralUsage
                {
                    ReferralCodeId = refCode.Id,
                    ReferredUserId = user.Id,
                    RegisteredAt = DateTime.UtcNow
                });
                await _unitOfWork.SaveChangesAsync();
            }
        }

        await _unitOfWork.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try { await _emailService.SendVerificationCodeAsync(user.Email, user.Name, code); }
            catch {}
        });

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var subdomain = await GetSchoolSubdomainAsync(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
            user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt,
            user.EmailVerified,
            token,
            expiresAt,
            subdomain,
            user.PhoneNumber
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

        if (user != null && user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            var minutesLeft = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            throw new UnauthorizedAccessException($"Account is locked. Try again in {minutesLeft} minute(s)");
        }

        if (user == null || !BC.Verify(dto.Password, user.PasswordHash))
        {
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

        if (user.FailedLoginAttempts > 0)
        {
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            await _context.SaveChangesAsync();
        }

        await ReconcileOnboardingStateAsync(user);

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var loginSub = await GetSchoolSubdomainAsync(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
            user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt,
            user.EmailVerified,
            token,
            expiresAt,
            loginSub,
            user.PhoneNumber
        );
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found");

        await ReconcileOnboardingStateAsync(user);

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var refreshSub = await GetSchoolSubdomainAsync(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
            user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt,
            user.EmailVerified,
            token,
            expiresAt,
            refreshSub,
            user.PhoneNumber
        );
    }

    private async Task ReconcileOnboardingStateAsync(User user)
    {
        if (!user.HasCompletedOnboarding) return;

        var activeGoal = await _context.Set<StudyGoal>()
            .FirstOrDefaultAsync(g => g.UserId == user.Id && g.IsActive);
        var hasValidExam = activeGoal != null
            && await _context.ExamTypes.AnyAsync(e => e.Code == activeGoal.ExamTypeCode);

        if (!hasValidExam)
        {
            user.HasCompletedOnboarding = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
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

        _ = Task.Run(async () =>
        {
            try { await _emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
            catch {}
        });

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var verifySub = await GetSchoolSubdomainAsync(user);
        return new AuthResponseDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt,
            verifySub,
            user.PhoneNumber
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
        var fullName = payload.Name ?? email.Split('@')[0];
        var googleId = payload.Subject;
        var nameParts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var gFirstName = InputSanitizer.Sanitize(nameParts[0]) ?? "";
        var gLastName = nameParts.Length > 1 ? (InputSanitizer.Sanitize(nameParts[1]) ?? "") : "";

        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new User
            {
                Email = email,
                FirstName = gFirstName,
                LastName = gLastName,
                Name = $"{gFirstName} {gLastName}".Trim(),
                PasswordHash = BC.HashPassword(Guid.NewGuid().ToString()),
                Role = UserRole.Student,
                CreatedAt = DateTime.UtcNow,
                EmailVerified = true,
                GoogleId = googleId
            };
            _context.Users.Add(user);
            await _unitOfWork.SaveChangesAsync();

            _ = Task.Run(async () =>
            {
                try { await _emailService.SendWelcomeEmailAsync(user.Email, user.Name); }
                catch {}
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
        var googleSub = await GetSchoolSubdomainAsync(user);
        return new AuthResponseDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt,
            googleSub,
            user.PhoneNumber
        );
    }

    public async Task<AuthResponseDto> UpdatePhoneNumberAsync(int userId, UpdatePhoneDto dto)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var phone = dto.PhoneNumber.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+\d{7,15}$"))
            throw new InvalidOperationException("Enter a valid phone number in international format (+...).");

        user.PhoneNumber = phone;
        await _unitOfWork.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        var phoneSub = await GetSchoolSubdomainAsync(user);
        return new AuthResponseDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt,
            phoneSub,
            user.PhoneNumber
        );
    }

    private static string GenerateVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    private Task<string?> GetSchoolSubdomainAsync(User user)
    {
        return Task.FromResult<string?>(null);
    }

    private static void ValidatePasswordComplexity(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 10)
            throw new InvalidOperationException("Password must be at least 10 characters long");
        if (!password.Any(char.IsUpper))
            throw new InvalidOperationException("Password must contain at least one uppercase letter");
        if (!password.Any(char.IsLower))
            throw new InvalidOperationException("Password must contain at least one lowercase letter");
        if (!password.Any(char.IsDigit))
            throw new InvalidOperationException("Password must contain at least one digit");
        if (!password.Any(c => !char.IsLetterOrDigit(c)))
            throw new InvalidOperationException("Password must contain at least one special character");
    }

    private static void ValidateName(string name, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new InvalidOperationException($"{fieldName} must be at least 2 characters");
        if (name.Any(char.IsDigit))
            throw new InvalidOperationException($"{fieldName} must not contain digits");
    }

    protected virtual async Task ValidateEmailDomainAsync(string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[1]))
            throw new InvalidOperationException("Invalid email format");

        var domain = parts[1].Trim().ToLowerInvariant();

        try
        {
            var addresses = await System.Net.Dns.GetHostAddressesAsync(domain);
            if (addresses.Length == 0)
                throw new InvalidOperationException("Email domain does not exist. Please use a valid email address.");
        }
        catch (System.Net.Sockets.SocketException)
        {
            throw new InvalidOperationException("Email domain does not exist. Please use a valid email address.");
        }
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

        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = InputSanitizer.Sanitize(dto.FirstName)!;
        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = InputSanitizer.Sanitize(dto.LastName)!;
        if (!string.IsNullOrWhiteSpace(dto.FirstName) || !string.IsNullOrWhiteSpace(dto.LastName))
            user.Name = $"{user.FirstName} {user.LastName}".Trim();
        
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
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

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return;

        var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        user.PasswordResetCode = code;
        user.PasswordResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendPasswordResetCodeAsync(user.Email, user.Name, code);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        ValidatePasswordComplexity(dto.NewPassword);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new KeyNotFoundException("User not found");

        if (user.PasswordResetCode != dto.Code)
            throw new InvalidOperationException("Invalid reset code");

        if (user.PasswordResetCodeExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Reset code has expired");

        user.PasswordHash = BC.HashPassword(dto.NewPassword);
        user.PasswordResetCode = null;
        user.PasswordResetCodeExpiresAt = null;
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }
}
