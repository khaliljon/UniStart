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

        // Name validation: no digits, min 2 chars each
        ValidateName(dto.FirstName, "First name");
        ValidateName(dto.LastName, "Last name");

        // Verify email domain exists (MX / A record)
        await ValidateEmailDomainAsync(dto.Email);

        // Check if user already exists (including soft-deleted)
        var existingUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            if (existingUser.IsDeleted)
            {
                // Restore soft-deleted user with new credentials — require re-verification
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
                    catch { /* logged inside EmailService */ }
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
                    restoredSub
                );
            }

            // Existing unverified user — update credentials and resend verification code
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
                    catch { /* logged inside EmailService */ }
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
                    reSub
                );
            }

            throw new InvalidOperationException("User with this email already exists");
        }

        // Determine role (only Student or Tutor allowed from registration)
        var role = UserRole.Student;
        if (!string.IsNullOrWhiteSpace(dto.Role) &&
            string.Equals(dto.Role, "Tutor", StringComparison.OrdinalIgnoreCase))
        {
            role = UserRole.Tutor;
        }

        // Create new user with verification code
        var code = GenerateVerificationCode();

        // Auto-bind to school if registering from White Label subdomain
        int? schoolId = null;
        TutorSchool? inviteSchool = null;
        bool autoApproveSchool = false;

        if (!string.IsNullOrWhiteSpace(dto.SchoolSlug))
        {
            var school = await _context.TutorSchools
                .FirstOrDefaultAsync(s => s.IsActive && (s.Subdomain == dto.SchoolSlug || s.Slug == dto.SchoolSlug));
            if (school != null) schoolId = school.Id;
        }

        // School invite code: bind tutor to school (auto-approve if RequireApproval is false)
        if (!string.IsNullOrWhiteSpace(dto.SchoolInviteCode) && role == UserRole.Tutor)
        {
            inviteSchool = await _context.TutorSchools
                .FirstOrDefaultAsync(s => s.IsActive && s.SchoolInviteCode == dto.SchoolInviteCode.Trim().ToUpperInvariant());
            if (inviteSchool != null)
            {
                schoolId = inviteSchool.Id;
                autoApproveSchool = !inviteSchool.RequireApproval;
            }
            else
            {
                throw new InvalidOperationException("Invalid school invite code");
            }
        }

        var firstName = InputSanitizer.Sanitize(dto.FirstName)!;
        var lastName = InputSanitizer.Sanitize(dto.LastName)!;

        var user = new User
        {
            Email = dto.Email,
            FirstName = firstName,
            LastName = lastName,
            Name = $"{firstName} {lastName}".Trim(),
            PasswordHash = BC.HashPassword(dto.Password),
            Role = role,
            SchoolId = schoolId,
            CreatedAt = DateTime.UtcNow,
            EmailVerified = false,
            EmailVerificationCode = code,
            EmailVerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        _context.Users.Add(user);
        await _unitOfWork.SaveChangesAsync();

        // Auto-create TutorProfile for tutor registrations
        if (role == UserRole.Tutor)
        {
            _context.TutorProfiles.Add(new TutorProfile
            {
                UserId = user.Id,
                Headline = $"Tutor {user.Name}",
                Bio = "",
                Experience = "",
                Specializations = "",
                IsAvailable = true,
                IsVerified = false,
                SchoolId = schoolId
            });
            await _unitOfWork.SaveChangesAsync();

            // Auto-create school application if tutor selected a school during registration
            if (dto.ApplyToSchoolId.HasValue)
            {
                var targetSchoolId = dto.ApplyToSchoolId.Value;
                var targetSchoolExists = await _context.TutorSchools
                    .AnyAsync(s => s.Id == targetSchoolId && s.IsActive);
                if (targetSchoolExists)
                {
                    _context.TutorSchoolApplications.Add(new TutorSchoolApplication
                    {
                        UserId = user.Id,
                        SchoolId = targetSchoolId,
                        Message = "Application submitted during registration",
                    });
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            // School invite code registration — auto-approve or pending based on school setting
            else if (inviteSchool != null)
            {
                var appStatus = autoApproveSchool
                    ? TutorSchoolApplicationStatus.Approved
                    : TutorSchoolApplicationStatus.Pending;
                _context.TutorSchoolApplications.Add(new TutorSchoolApplication
                {
                    UserId = user.Id,
                    SchoolId = inviteSchool.Id,
                    Message = "Joined via school invite code",
                    Status = appStatus,
                    ReviewedAt = autoApproveSchool ? DateTime.UtcNow : null,
                });
                await _unitOfWork.SaveChangesAsync();
            }
            // Auto-create application for WL subdomain registration
            else if (schoolId != null)
            {
                _context.TutorSchoolApplications.Add(new TutorSchoolApplication
                {
                    UserId = user.Id,
                    SchoolId = schoolId.Value,
                    Message = "Registered from school subdomain",
                });
                await _unitOfWork.SaveChangesAsync();
            }
        }

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
            subdomain
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
            loginSub
        );
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId)
            ?? throw new UnauthorizedAccessException("User not found");

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
            refreshSub
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
        var verifySub = await GetSchoolSubdomainAsync(user);
        return new AuthResponseDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt,
            verifySub
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
        var googleSub = await GetSchoolSubdomainAsync(user);
        return new AuthResponseDto(
            user.Id, user.Email, user.FirstName, user.LastName, user.Name, user.Role.ToString(),
            user.HasCompletedOnboarding, user.SubscriptionTier.ToString(),
            user.SubscriptionExpiresAt, user.EmailVerified, token, expiresAt,
            googleSub
        );
    }

    private static string GenerateVerificationCode()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }

    private async Task<string?> GetSchoolSubdomainAsync(User user)
    {
        if (user.SchoolId == null) return null;
        return await _context.TutorSchools
            .Where(s => s.Id == user.SchoolId && s.IsActive && s.Subdomain != null)
            .Select(s => s.Subdomain)
            .FirstOrDefaultAsync();
    }

    /// <summary>S-4: Password must be 10+ chars with uppercase, lowercase, digit, and special character</summary>
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

    /// <summary>Check that the email domain has valid DNS records (MX or A) so it can receive mail</summary>
    private static async Task ValidateEmailDomainAsync(string email)
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

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        // Always return success to avoid email enumeration
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
