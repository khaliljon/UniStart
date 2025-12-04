using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs
{
    // DTOs для FlashcardSet
    public class FlashcardSetDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int CardCount { get; set; }
        public int TotalCards { get; set; }
        public int CardsToReview { get; set; }
    }

    public class CreateFlashcardSetDto
    {
        [Required(ErrorMessage = "Название набора обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Название предмета не должно превышать 100 символов")]
        public string Subject { get; set; } = string.Empty;
        
        public bool IsPublic { get; set; } = false;
    }

    public class UpdateFlashcardSetDto
    {
        [Required(ErrorMessage = "Название набора обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(100, ErrorMessage = "Название предмета не должно превышать 100 символов")]
        public string Subject { get; set; } = string.Empty;
        
        public bool IsPublic { get; set; } = false;
    }
}
