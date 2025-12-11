using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Настройки системы
/// </summary>
public class SystemSettings
{
    [Key]
    public int Id { get; set; } = 1; // Всегда один запись
    
    [Display(Name = "Название сайта")]
    [Required]
    [StringLength(200)]
    public string SiteName { get; set; } = "UniStart";
    
    [Display(Name = "Описание сайта")]
    [StringLength(1000)]
    public string SiteDescription { get; set; } = "Образовательная платформа для изучения с помощью карточек и тестов";
    
    [Display(Name = "Разрешить регистрацию")]
    public bool AllowRegistration { get; set; } = true;
    
    [Display(Name = "Требовать подтверждение email")]
    public bool RequireEmailVerification { get; set; } = false;
    
    [Display(Name = "Максимальное количество попыток для тестов")]
    [Range(1, 10)]
    public int MaxQuizAttempts { get; set; } = 3;
    
    [Display(Name = "Таймаут сессии (минуты)")]
    [Range(5, 120)]
    public int SessionTimeout { get; set; } = 30;
    
    [Display(Name = "Включить уведомления")]
    public bool EnableNotifications { get; set; } = true;
    
    [Display(Name = "Дата обновления")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

