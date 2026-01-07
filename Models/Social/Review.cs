using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Social
{
    /// <summary>
    /// Рейтинг и отзыв на квиз
    /// </summary>
    public class QuizReview
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID квиза")]
        [Required]
        public int QuizId { get; set; }
        
        [Display(Name = "ID пользователя")]
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Рейтинг")]
        [Range(1, 5, ErrorMessage = "Рейтинг должен быть от 1 до 5")]
        public int Rating { get; set; }
        
        [Display(Name = "Комментарий")]
        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
        public string? Comment { get; set; }
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public Quiz Quiz { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }

    /// <summary>
    /// Рейтинг и отзыв на набор карточек
    /// </summary>
    public class FlashcardSetReview
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "ID набора карточек")]
        [Required]
        public int FlashcardSetId { get; set; }
        
        [Display(Name = "ID пользователя")]
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Display(Name = "Рейтинг")]
        [Range(1, 5, ErrorMessage = "Рейтинг должен быть от 1 до 5")]
        public int Rating { get; set; }
        
        [Display(Name = "Комментарий")]
        [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
        public string? Comment { get; set; }
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public FlashcardSet FlashcardSet { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}
