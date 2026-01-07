using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Core
{
    /// <summary>
    /// Отслеживание ежедневной активности пользователя
    /// </summary>
    public class UserStreak
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID пользователя")]
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Текущая серия дней")]
        public int CurrentStreak { get; set; } = 0;
        
        [Display(Name = "Лучшая серия дней")]
        public int LongestStreak { get; set; } = 0;
        
        [Display(Name = "Последняя активность")]
        public DateTime LastActivityDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Общее количество дней активности")]
        public int TotalActiveDays { get; set; } = 0;
        
        // Навигационные свойства
        public ApplicationUser User { get; set; } = null!;
    }
}
