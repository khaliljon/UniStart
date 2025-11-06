using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    public class Tag
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название тега")]
        [Required(ErrorMessage = "Название тега обязательно")]
        [StringLength(50, ErrorMessage = "Название тега не должно превышать 50 символов")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Цвет")]
        [StringLength(7, ErrorMessage = "Цвет должен быть в формате #RRGGBB")]
        public string Color { get; set; } = "#3B82F6"; // Default blue
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public List<FlashcardSet> FlashcardSets { get; set; } = new();
        public List<Quiz> Quizzes { get; set; } = new();
    }
}
