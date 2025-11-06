using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UniStart.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Имя")]
        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        public string FirstName { get; set; } = string.Empty;
        
        [Display(Name = "Фамилия")]
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Дата регистрации")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Последний вход")]
        public DateTime? LastLoginAt { get; set; }
        
        // Статистика
        [Display(Name = "Всего изучено карточек")]
        [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть положительным")]
        public int TotalCardsStudied { get; set; } = 0;
        
        [Display(Name = "Всего пройдено тестов")]
        [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть положительным")]
        public int TotalQuizzesTaken { get; set; } = 0;
    }
}
