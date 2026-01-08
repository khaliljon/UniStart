using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Core
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
        
        [Display(Name = "Всего пройдено квизов")]
        [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть положительным")]
        public int TotalQuizzesTaken { get; set; } = 0;
        
        [Display(Name = "Всего пройдено экзаменов")]
        [Range(0, int.MaxValue, ErrorMessage = "Значение должно быть положительным")]
        public int TotalExamsTaken { get; set; } = 0;
        
        // ИИ Рекомендации - Предпочтения пользователя
        [Display(Name = "Предпочтительный город")]
        [StringLength(100)]
        public string? PreferredCity { get; set; }
        
        [Display(Name = "Максимальный бюджет (год)")]
        [Range(0, double.MaxValue)]
        public decimal? MaxBudget { get; set; }
        
        [Display(Name = "Карьерная цель")]
        [StringLength(200)]
        public string? CareerGoal { get; set; }
        
        // Навигационные свойства для ИИ
        public UserLearningPattern? LearningPattern { get; set; }
        public List<UniversityRecommendation> UniversityRecommendations { get; set; } = new();
    }
}
