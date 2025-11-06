using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    public class Achievement
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Название обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Описание")]
        [StringLength(500, ErrorMessage = "Описание не должно превышать 500 символов")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Иконка")]
        [StringLength(50, ErrorMessage = "Иконка не должна превышать 50 символов")]
        public string Icon { get; set; } = "🏆"; // Emoji или icon class
        
        [Display(Name = "Тип")]
        [Required(ErrorMessage = "Тип обязателен")]
        public string Type { get; set; } = string.Empty; // FlashcardsStudied, QuizzesPassed, StreakDays, etc.
        
        [Display(Name = "Целевое значение")]
        [Range(1, int.MaxValue, ErrorMessage = "Целевое значение должно быть больше 0")]
        public int TargetValue { get; set; } = 1;
        
        [Display(Name = "Уровень")]
        [Range(1, 5, ErrorMessage = "Уровень должен быть от 1 до 5")]
        public int Level { get; set; } = 1; // Bronze, Silver, Gold, Platinum, Diamond
        
        // Навигационные свойства
        public List<UserAchievement> UserAchievements { get; set; } = new();
    }

    public class UserAchievement
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID пользователя")]
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "ID достижения")]
        public int AchievementId { get; set; }
        
        [Display(Name = "Прогресс")]
        public int Progress { get; set; } = 0;
        
        [Display(Name = "Завершено")]
        public bool IsCompleted { get; set; } = false;
        
        [Display(Name = "Дата получения")]
        public DateTime? CompletedAt { get; set; }
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ApplicationUser User { get; set; } = null!;
        public Achievement Achievement { get; set; } = null!;
    }
}
