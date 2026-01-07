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
    public class Quiz
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название теста")]
        [Required(ErrorMessage = "Название теста обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Описание")]
        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Ограничение времени (минуты)")]
        [Range(0, 300, ErrorMessage = "Время должно быть от 0 до 300 минут")]
        public int TimeLimit { get; set; } // В минутах (0 = без ограничения)
        
        [Display(Name = "Предмет")]
        [Required(ErrorMessage = "Предмет обязателен")]
        [StringLength(100, ErrorMessage = "Название предмета не должно превышать 100 символов")]
        public string Subject { get; set; } = string.Empty; // Математика, Физика, и т.д.
        
        [Display(Name = "Уровень сложности")]
        [Required(ErrorMessage = "Уровень сложности обязателен")]
        public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        
        [Display(Name = "Опубликован")]
        public bool IsPublished { get; set; } = false;
        
        [Display(Name = "Публичный доступ")]
        public bool IsPublic { get; set; } = false; // true = доступен всем студентам, false = только автор
        
        [Display(Name = "Режим обучения")]
        public bool IsLearningMode { get; set; } = true; // true = показывать объяснения сразу, false = только в конце
        
        // Тип квиза в контексте обучения
        [Display(Name = "Тип квиза")]
        public string QuizType { get; set; } = "Standalone"; // Standalone, Practice, CaseStudy, ModuleFinal, CourseFinal (пробный ЕНТ)
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Дата обновления")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign Keys
        [Display(Name = "Идентификатор пользователя")]
        [Required(ErrorMessage = "Пользователь обязателен")]
        public string UserId { get; set; } = string.Empty;
        
        // Навигационные свойства
        [Display(Name = "Пользователь")]
        public ApplicationUser? User { get; set; }
        
        [Display(Name = "Вопросы")]
        public List<QuizQuestion> Questions { get; set; } = new();
        
        [Display(Name = "Попытки прохождения")]
        public List<UserQuizAttempt> Attempts { get; set; } = new();
        
        [Display(Name = "Теги")]
        public List<Tag> Tags { get; set; } = new();
    }
}
