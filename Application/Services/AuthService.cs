using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;

namespace UniStart.Application.Services;

public class AuthService : IAuthService
{
    private readonly UniStartDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(UniStartDbContext context, IJwtService jwtService, IUnitOfWork unitOfWork)
    {
        _context = context;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        // Create new user
        var user = new User
        {
            Email = dto.Email,
            Name = dto.Name,
            PasswordHash = BC.HashPassword(dto.Password),
            Role = UserRole.Student,
            CreatedAt = DateTime.UtcNow
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

        // Generate token
        var token = _jwtService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.Name,
            user.Role.ToString(),
            user.HasCompletedOnboarding,
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
            token,
            expiresAt
        );
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
            user.CreatedAt
        );
    }

    public async Task<UserDto?> UpdateUserAsync(int userId, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name;
        
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
            user.CreatedAt
        );
    }
}
