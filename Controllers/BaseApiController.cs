using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace UniStart.Controllers;

/// <summary>
/// Базовый контроллер с общей функциональностью
/// </summary>
[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Получить ID текущего пользователя
    /// </summary>
    protected string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("Пользователь не аутентифицирован");
        return userId;
    }

    /// <summary>
    /// Получить ID текущего пользователя или null
    /// </summary>
    protected string? GetUserIdOrNull()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    /// <summary>
    /// Проверить, аутентифицирован ли пользователь
    /// </summary>
    protected bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    /// <summary>
    /// Проверить, есть ли у пользователя определенная роль
    /// </summary>
    protected bool IsInRole(string role) => User.IsInRole(role);

    /// <summary>
    /// Создать ответ с пагинацией
    /// </summary>
    protected object CreatePaginatedResponse<T>(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize)
    {
        return new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            hasNextPage = page * pageSize < totalCount,
            hasPreviousPage = page > 1
        };
    }

    /// <summary>
    /// Обработать результат операции
    /// </summary>
    protected ActionResult HandleResult<T>(T? result, string notFoundMessage = "Ресурс не найден")
    {
        if (result == null)
            return NotFound(new { message = notFoundMessage });
        
        return Ok(result);
    }

    /// <summary>
    /// Обработать булевый результат
    /// </summary>
    protected ActionResult HandleBoolResult(
        bool success,
        string successMessage = "Операция выполнена успешно",
        string errorMessage = "Операция не выполнена")
    {
        if (success)
            return Ok(new { message = successMessage });
        
        return BadRequest(new { message = errorMessage });
    }
}
