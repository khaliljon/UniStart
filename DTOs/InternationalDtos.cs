using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs
{
    // ============ Country DTOs ============
    public class CountryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? FlagEmoji { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UniversitiesCount { get; set; }
        public int ExamsCount { get; set; }
    }

    public class CreateCountryDto
    {
        [Required(ErrorMessage = "Название страны обязательно")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Required(ErrorMessage = "Код страны обязателен")]
        [StringLength(3, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string? FlagEmoji { get; set; }
    }

    public class UpdateCountryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Required]
        [StringLength(3, MinimumLength = 2)]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(10)]
        public string? FlagEmoji { get; set; }
        
        public bool IsActive { get; set; }
    }

    // ============ University DTOs ============
    public class UniversityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public List<int> ExamTypeIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public int ExamsCount { get; set; }
    }

    public class CreateUniversityDto
    {
        [Required(ErrorMessage = "Название университета обязательно")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? NameEn { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(200)]
        [Url]
        public string? Website { get; set; }
        
        public int Type { get; set; } // 0=Public, 1=Private, 2=International
        
        [Required(ErrorMessage = "Страна обязательна")]
        public int CountryId { get; set; }
        
        [Required(ErrorMessage = "Необходимо выбрать хотя бы один тип экзамена")]
        [MinLength(1, ErrorMessage = "Необходимо выбрать хотя бы один тип экзамена")]
        public List<int> ExamTypeIds { get; set; } = new();
    }

    public class UpdateUniversityDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? NameEn { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(200)]
        [Url]
        public string? Website { get; set; }
        
        public int Type { get; set; }
        
        [Required]
        public int CountryId { get; set; }
        
        [Required(ErrorMessage = "Необходимо выбрать хотя бы один тип экзамена")]
        [MinLength(1, ErrorMessage = "Необходимо выбрать хотя бы один тип экзамена")]
        public List<int> ExamTypeIds { get; set; } = new();
        
        public bool IsActive { get; set; }
    }

    // ============ ExamType DTOs ============
    public class ExamTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? DefaultCountryId { get; set; }
        public string? DefaultCountryName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ExamsCount { get; set; }
    }

    public class CreateExamTypeDto
    {
        [Required(ErrorMessage = "Название типа экзамена обязательно")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Required(ErrorMessage = "Код обязателен")]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? DefaultCountryId { get; set; }
    }

    public class UpdateExamTypeDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? DefaultCountryId { get; set; }
        
        public bool IsActive { get; set; }
    }

    // ============ City DTOs ============
    public class CityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        public string? Region { get; set; }
        public int? Population { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCityDto
    {
        [Required(ErrorMessage = "Название города обязательно")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Required(ErrorMessage = "Страна обязательна")]
        public int CountryId { get; set; }
        
        [StringLength(100)]
        public string? Region { get; set; }
        
        public int? Population { get; set; }
    }

    public class UpdateCityDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? NameEn { get; set; }
        
        [Required]
        public int CountryId { get; set; }
        
        [StringLength(100)]
        public string? Region { get; set; }
        
        public int? Population { get; set; }
        
        public bool IsActive { get; set; }
    }
}
