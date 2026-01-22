using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Reference
{
    /// <summary>
    /// Университет
    /// </summary>
    public class University
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Название университета обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Название на английском")]
        [StringLength(200)]
        public string? NameEn { get; set; }
        
        [Display(Name = "Город")]
        public int? CityId { get; set; }
        public City? City { get; set; }
        
        [Display(Name = "Описание")]
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [Display(Name = "Веб-сайт")]
        [StringLength(200)]
        [Url(ErrorMessage = "Неверный формат URL")]
        public string? Website { get; set; }
        
        [Display(Name = "Тип университета")]
        public UniversityType Type { get; set; } = UniversityType.Public;
        
        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Страна")]
        [Required(ErrorMessage = "Страна обязательна")]
        public int CountryId { get; set; }
        public Country Country { get; set; } = null!;
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // ИИ Рекомендации - Дополнительные данные
        [Display(Name = "Стоимость обучения (год)")]
        [Range(0, double.MaxValue)]
        public decimal? TuitionFee { get; set; }
        
        [Display(Name = "Минимальный балл для поступления")]
        [Range(0, 100)]
        public int? MinScore { get; set; }
        
        [Display(Name = "Программы обучения (JSON)")]
        public string? ProgramsJson { get; set; } // ["IT", "Engineering", "Medicine"]
        
        [Display(Name = "Сильные предметы (JSON)")]
        public string? StrongSubjectsJson { get; set; } // ["Math", "Physics"]
        
        // Навигационные свойства
        public List<Exam> Exams { get; set; } = new();
        public List<ExamType> ExamTypes { get; set; } = new();
        public List<UniversityRecommendation> Recommendations { get; set; } = new();
    }
    
    public enum UniversityType
    {
        [Display(Name = "Государственный")]
        Public = 0,
        
        [Display(Name = "Частный")]
        Private = 1,
        
        [Display(Name = "Международный")]
        International = 2
    }
}
