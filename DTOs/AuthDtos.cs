using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
        public string LastName { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Пароль обязателен")]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int TotalCardsStudied { get; set; }
        public int TotalQuizzesTaken { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
