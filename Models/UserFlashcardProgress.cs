using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Прогресс пользователя по конкретной карточке (индивидуальный для каждого пользователя)
/// </summary>
public class UserFlashcardProgress
{
    [Display(Name = "Идентификатор")]
    public int Id { get; set; }
    
    // Связи
    [Display(Name = "ID пользователя")]
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Display(Name = "Пользователь")]
    public ApplicationUser User { get; set; } = null!;
    
    [Display(Name = "ID карточки")]
    public int FlashcardId { get; set; }
    
    [Display(Name = "Карточка")]
    public Flashcard Flashcard { get; set; } = null!;
    
    // Spaced Repetition Algorithm (SM-2) параметры (индивидуальные для каждого пользователя)
    [Display(Name = "Коэффициент легкости")]
    [Range(1.3, 5.0, ErrorMessage = "Коэффициент легкости должен быть от 1.3 до 5.0")]
    public double EaseFactor { get; set; } = 2.5; // Начальный коэффициент легкости
    
    [Display(Name = "Интервал повторения (дни)")]
    [Range(0, int.MaxValue, ErrorMessage = "Интервал должен быть положительным")]
    public int Interval { get; set; } = 0; // Интервал в днях до следующего повтора
    
    [Display(Name = "Количество повторений")]
    [Range(0, int.MaxValue, ErrorMessage = "Количество повторений должно быть положительным")]
    public int Repetitions { get; set; } = 0; // Количество успешных повторений
    
    [Display(Name = "Дата следующего повторения")]
    public DateTime? NextReviewDate { get; set; } // Дата следующего повторения
    
    [Display(Name = "Последняя проверка")]
    public DateTime? LastReviewedAt { get; set; } // Последняя проверка
    
    // Дополнительная статистика
    [Display(Name = "Всего повторений")]
    public int TotalReviews { get; set; } = 0; // Всего повторений карточки
    
    [Display(Name = "Правильных повторений")]
    public int CorrectReviews { get; set; } = 0; // Правильных повторений (quality >= 3)
    
    [Display(Name = "Первое изучение")]
    public DateTime? FirstReviewedAt { get; set; } // Первое изучение карточки
    
    [Display(Name = "Изучена полностью")]
    public bool IsMastered { get; set; } = false; // Изучена ли полностью (Repetitions >= 3 и EaseFactor >= 2.0)
    
    [Display(Name = "Дата создания записи")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Дата обновления")]
    public DateTime? UpdatedAt { get; set; }
}

