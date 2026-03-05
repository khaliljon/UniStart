using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(int userId);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto dto);
}
