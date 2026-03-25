using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(int userId);
    Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto);
    Task ResendVerificationCodeAsync(ResendCodeDto dto);
    Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task ChangeEmailAsync(int userId, ChangeEmailDto dto);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
