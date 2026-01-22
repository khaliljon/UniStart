using System.ComponentModel.DataAnnotations;

namespace UniStart.Models.Reference
{
    /// <summary>
    /// Город
    /// </summary>
    public class City
    {
        [Display(Name = "Идентификатор")]
        public int Id { get; set; }
        
        [Display(Name = "Название")]
        [Required(ErrorMessage = "Название города обязательно")]
        [StringLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Название на английском")]
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Display(Name = "Страна")]
        [Required(ErrorMessage = "Страна обязательна")]
        public int CountryId { get; set; }
        
        [Display(Name = "Регион/Область")]
        [StringLength(100)]
        public string? Region { get; set; }
        
        [Display(Name = "Население")]
        public int? Population { get; set; }
        
        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public Country Country { get; set; } = null!;
        public List<University> Universities { get; set; } = new();
    }
}
