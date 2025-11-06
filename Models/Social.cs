using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    /// <summary>
    /// Подписка одного пользователя на другого
    /// </summary>
    public class UserFollow
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID подписчика")]
        [Required]
        public string FollowerId { get; set; } = string.Empty; // Кто подписывается
        
        [Display(Name = "ID пользователя")]
        [Required]
        public string FollowingId { get; set; } = string.Empty; // На кого подписываются
        
        [Display(Name = "Дата подписки")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ApplicationUser Follower { get; set; } = null!;
        public ApplicationUser Following { get; set; } = null!;
    }

    /// <summary>
    /// Лента активности пользователя
    /// </summary>
    public class ActivityFeed
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID пользователя")]
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Тип активности")]
        [Required]
        public string ActivityType { get; set; } = string.Empty; // QuizCompleted, FlashcardSetCreated, AchievementUnlocked, etc.
        
        [Display(Name = "Описание")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Дополнительные данные (JSON)")]
        public string? Metadata { get; set; } // JSON с доп. данными
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ApplicationUser User { get; set; } = null!;
    }
}
