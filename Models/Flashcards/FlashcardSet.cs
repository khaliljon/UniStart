using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Flashcards
{
    public class FlashcardSet
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название набора")]
        [Required(ErrorMessage = "Название набора обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Описание")]
        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Публичный доступ")]
        public bool IsPublic { get; set; } = false; // true = доступен всем студентам, false = только автор
        
        [Display(Name = "Опубликован")]
        public bool IsPublished { get; set; } = false; // true = виден студентам, false = черновик
        
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
        
        [Display(Name = "Карточки")]
        public List<Flashcard> Flashcards { get; set; } = new();
        
        [Display(Name = "Теги")]
        public List<Tag> Tags { get; set; } = new();
        
        [Display(Name = "Предметы")]
        public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }
}
