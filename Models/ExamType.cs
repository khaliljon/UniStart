using System.ComponentModel.DataAnnotations;

namespace UniStart.Models
{
    /// <summary>
    /// Тип экзамена (ЕНТ, ЕГЭ, SAT, IELTS и т.д.)
    /// </summary>
    public class ExamType
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Название типа экзамена обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty; // ЕНТ, ЕГЭ, IELTS
        
        [Display(Name = "Название на английском")]
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Display(Name = "Код")]
        [Required(ErrorMessage = "Код обязателен")]
        [StringLength(20, ErrorMessage = "Код не должен превышать 20 символов")]
        public string Code { get; set; } = string.Empty; // ENT, EGE, IELTS, SAT
        
        [Display(Name = "Описание")]
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Display(Name = "Страна (по умолчанию)")]
        public int? DefaultCountryId { get; set; }
        public Country? DefaultCountry { get; set; }
        
        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public List<Exam> Exams { get; set; } = new();
    }
}
