using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;

// Auth DTOs
public record RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(2)] string FirstName,
    [Required][MinLength(2)] string LastName,
    [Required][MinLength(10)] string Password,
    string? PhoneNumber = null,
    string? Role = null,
    string? SchoolSlug = null,
    int? ApplyToSchoolId = null,
    string? SchoolInviteCode = null,
    string? SchoolName = null,
    string? ReferralCode = null
);

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    int UserId,
    string Email,
    string FirstName,
    string LastName,
    string Name,
    string Role,
    bool HasCompletedOnboarding,
    string SubscriptionTier,
    DateTime? SubscriptionExpiresAt,
    bool EmailVerified,
    string Token,
    DateTime ExpiresAt,
    string? SchoolSubdomain = null,
    string? PhoneNumber = null,
    bool HasFullAccess = false
);

public record VerifyEmailDto(
    [Required][EmailAddress] string Email,
    [Required][StringLength(6, MinimumLength = 6)] string Code
);

public record ResendCodeDto(
    [Required][EmailAddress] string Email
);

public record GoogleLoginDto(
    [Required] string IdToken
);

public record UpdatePhoneDto(
    [Required] string PhoneNumber
);

public record ChangePasswordDto(
    [Required] string CurrentPassword,
    [Required][MinLength(10)] string NewPassword
);

public record ChangeEmailDto(
    [Required][EmailAddress] string NewEmail,
    [Required] string Password
);

public record ForgotPasswordDto(
    [Required][EmailAddress] string Email
);

public record ResetPasswordDto(
    [Required][EmailAddress] string Email,
    [Required][StringLength(6, MinimumLength = 6)] string Code,
    [Required][MinLength(10)] string NewPassword
);

// User DTOs
public record UserDto(
    int Id,
    string Email,
    string Name,
    string Role,
    DateTime CreatedAt,
    DateTime? LastSeenAt = null
);

public record UpdateUserDto(
    string? FirstName,
    string? LastName,
    string? Email
);
