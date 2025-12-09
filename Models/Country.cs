using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    /// <summary>
    /// Страна
    /// </summary>
    public class Country
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Название страны обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Название на английском")]
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Display(Name = "Код страны")]
        [Required(ErrorMessage = "Код страны обязателен")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "Код должен быть 2-3 символа")]
        public string Code { get; set; } = string.Empty; // KZ, RU, CN, etc.
        
        [Display(Name = "Флаг (emoji)")]
        [StringLength(10)]
        public string? FlagEmoji { get; set; } // 🇰🇿, 🇷🇺, 🇨🇳
        
        [Display(Name = "Активна")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public List<University> Universities { get; set; } = new();
        public List<Exam> Exams { get; set; } = new();
    }
}
