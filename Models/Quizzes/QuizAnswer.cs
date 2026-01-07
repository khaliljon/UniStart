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
    public class QuizAnswer
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Текст ответа")]
        [Required(ErrorMessage = "Текст ответа обязателен")]
        [StringLength(500, ErrorMessage = "Текст ответа не должен превышать 500 символов")]
        public string Text { get; set; } = string.Empty;
        
        [Display(Name = "Правильный ответ")]
        public bool IsCorrect { get; set; } = false;
        
        [Display(Name = "Порядковый номер")]
        public int OrderIndex { get; set; }
        
        // Внешний ключ
        [Display(Name = "ID вопроса")]
        public int QuestionId { get; set; }
        
        [Display(Name = "Вопрос")]
        public QuizQuestion Question { get; set; } = null!;
    }
}
