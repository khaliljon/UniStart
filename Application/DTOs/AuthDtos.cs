using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;

// Auth DTOs
public record RegisterDto(
    [Required][EmailAddress] string Email,
    [Required][MinLength(2)] string Name,
    [Required][MinLength(6)] string Password
);

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    int UserId,
    string Email,
    string Name,
    string Role,
    string Token,
    DateTime ExpiresAt
);

// User DTOs
public record UserDto(
    int Id,
    string Email,
    string Name,
    string Role,
    DateTime CreatedAt
);

public record UpdateUserDto(
    string? Name,
    string? Email
);
