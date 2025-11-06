using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs
{
    // DTOs для FlashcardSet
    public class FlashcardSetDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TotalCards { get; set; }
        public int CardsToReview { get; set; }
    }

    public class CreateFlashcardSetDto
    {
        [Required(ErrorMessage = "Название набора обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateFlashcardSetDto
    {
        [Required(ErrorMessage = "Название набора обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
    }
}
