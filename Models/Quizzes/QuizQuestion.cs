using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Quizzes
{
    public class QuizQuestion
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Текст вопроса")]
        [Required(ErrorMessage = "Текст вопроса обязателен")]
        [StringLength(1000, ErrorMessage = "Текст вопроса не должен превышать 1000 символов")]
        public string Text { get; set; } = string.Empty;
        
        [Display(Name = "Баллы")]
        [Range(1, 100, ErrorMessage = "Баллы должны быть от 1 до 100")]
        public int Points { get; set; } = 1;
        
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; }
        
        [Display(Name = "URL изображения")]
        [StringLength(500, ErrorMessage = "URL не должен превышать 500 символов")]
        public string? ImageUrl { get; set; }
        
        [Display(Name = "Объяснение")]
        [StringLength(2000, ErrorMessage = "Объяснение не должно превышать 2000 символов")]
        public string? Explanation { get; set; } // Объяснение правильного ответа
        
        // Внешний ключ
        [Display(Name = "ID теста")]
        public int QuizId { get; set; }
        
        [Display(Name = "Тест")]
        public Quiz Quiz { get; set; } = null!;
        
        // Навигационные свойства
        [Display(Name = "Варианты ответов")]
        public List<QuizAnswer> Answers { get; set; } = new();
    }
}
