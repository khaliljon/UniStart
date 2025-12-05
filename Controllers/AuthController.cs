using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniStart.DTOs;
using UniStart.Models;
using UniStart.Services;

namespace UniStart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            // Проверка существования пользователя
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Пользователь с таким email уже существует" });

            // Создание пользователя
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Ошибка при регистрации",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            // Автоматически присваиваем роль Student новому пользователю
            await _userManager.AddToRoleAsync(user, UserRoles.Student);

            // Генерация JWT токена
            var token = await _tokenService.GenerateTokenAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }

        /// <summary>
        /// Вход пользователя
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Неверный email или пароль" });

            // Проверяем, заблокирован ли пользователь
            if (await _userManager.IsLockedOutAsync(user))
                return Unauthorized(new { message = "Ваш аккаунт заблокирован. Обратитесь к администратору." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Неверный email или пароль" });

            // Обновляем время последнего входа
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Генерация JWT токена
            var token = await _tokenService.GenerateTokenAsync(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }

        /// <summary>
        /// Получить профиль текущего пользователя
        /// </summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                TotalCardsStudied = user.TotalCardsStudied,
                TotalQuizzesTaken = user.TotalQuizzesTaken,
                CreatedAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Обновить профиль пользователя
        /// </summary>
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Ошибка при обновлении профиля" });

            return NoContent();
        }
    }
}
