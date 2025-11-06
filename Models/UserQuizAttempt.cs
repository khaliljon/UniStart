using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    public class UserQuizAttempt
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID пользователя")]
        [Required(ErrorMessage = "ID пользователя обязателен")]
        public string UserId { get; set; } = string.Empty; // Пока строка, позже будет связь с Identity
        
        [Display(Name = "Набранные баллы")]
        [Range(0, int.MaxValue, ErrorMessage = "Баллы должны быть положительными")]
        public int Score { get; set; }
        
        [Display(Name = "Максимальные баллы")]
        [Range(1, int.MaxValue, ErrorMessage = "Максимальные баллы должны быть больше 0")]
        public int MaxScore { get; set; }
        
        [Display(Name = "Процент правильных ответов")]
        [Range(0, 100, ErrorMessage = "Процент должен быть от 0 до 100")]
        public double Percentage { get; set; }
        
        [Display(Name = "Затраченное время (секунды)")]
        [Range(0, int.MaxValue, ErrorMessage = "Время должно быть положительным")]
        public int TimeSpentSeconds { get; set; }
        
        [Display(Name = "Время начала")]
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Время завершения")]
        public DateTime? CompletedAt { get; set; }
        
        // Внешний ключ
        [Display(Name = "ID теста")]
        public int QuizId { get; set; }
        
        [Display(Name = "Тест")]
        public Quiz Quiz { get; set; } = null!;
        
        // Навигационное свойство к пользователю
        public ApplicationUser User { get; set; } = null!;
        
        // JSON с ответами пользователя
        [Display(Name = "Ответы пользователя (JSON)")]
        public string UserAnswersJson { get; set; } = "{}"; // {"questionId": [answerId1, answerId2]}
    }
}
