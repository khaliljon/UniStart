using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
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
