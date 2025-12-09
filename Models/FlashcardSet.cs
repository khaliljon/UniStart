using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
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
        
        [Display(Name = "Предмет")]
        [StringLength(100, ErrorMessage = "Название предмета не должно превышать 100 символов")]
        public string Subject { get; set; } = string.Empty; // Математика, Физика, и т.д.
        
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
    }
}
