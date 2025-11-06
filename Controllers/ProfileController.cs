using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniStart.DTOs;
using UniStart.Models;

namespace UniStart.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Все аутентифицированные пользователи
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    private string GetUserId() => _userManager.GetUserId(User) 
        ?? throw new UnauthorizedAccessException("Пользователь не аутентифицирован");

    /// <summary>
    /// Получить свой профиль
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetProfile()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.TotalCardsStudied,
            user.TotalQuizzesTaken,
            user.EmailConfirmed,
            Roles = roles
        });
    }

    /// <summary>
    /// Обновить свой профиль
    /// </summary>
    [HttpPut]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        // Обновляем поля
        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = dto.FirstName;

        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;

        if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
        {
            // Проверяем уникальность UserName
            var existingUser = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUser != null && existingUser.Id != userId)
            {
                return BadRequest(new { Message = "Это имя пользователя уже занято" });
            }
            user.UserName = dto.UserName;
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось обновить профиль", Errors = result.Errors });

        _logger.LogInformation("Пользователь {Email} обновил профиль", user.Email);

        return Ok(new { Message = "Профиль успешно обновлён" });
    }

    /// <summary>
    /// Изменить пароль
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
            return BadRequest(new { 
                Message = "Не удалось изменить пароль", 
                Errors = result.Errors.Select(e => e.Description) 
            });

        _logger.LogInformation("Пользователь {Email} изменил пароль", user.Email);

        return Ok(new { Message = "Пароль успешно изменён" });
    }

    /// <summary>
    /// Удалить свой аккаунт
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        // Проверяем пароль для подтверждения
        var passwordCheck = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordCheck)
            return BadRequest(new { Message = "Неверный пароль" });

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return BadRequest(new { Message = "Не удалось удалить аккаунт", Errors = result.Errors });

        _logger.LogWarning("Пользователь {Email} удалил свой аккаунт", user.Email);

        return Ok(new { Message = "Аккаунт успешно удалён" });
    }

    /// <summary>
    /// Получить свои роли
    /// </summary>
    [HttpGet("roles")]
    public async Task<ActionResult<object>> GetMyRoles()
    {
        var userId = GetUserId();
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { Message = "Пользователь не найден" });

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            Roles = roles
        });
    }
}
