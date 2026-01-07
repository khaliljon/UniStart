using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Flashcards;

/// <summary>
/// Доступ пользователя к набору карточек и его прогресс по набору
/// </summary>
public class UserFlashcardSetAccess
{
    [Display(Name = "Идентификатор")]
    public int Id { get; set; }
    
    // Связи
    [Display(Name = "ID пользователя")]
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Display(Name = "Пользователь")]
    public ApplicationUser User { get; set; } = null!;
    
    [Display(Name = "ID набора карточек")]
    public int FlashcardSetId { get; set; }
    
    [Display(Name = "Набор карточек")]
    public FlashcardSet FlashcardSet { get; set; } = null!;
    
    // Метаданные доступа
    [Display(Name = "Первый доступ")]
    public DateTime FirstAccessedAt { get; set; } = DateTime.UtcNow; // Первое открытие набора
    
    [Display(Name = "Последний доступ")]
    public DateTime? LastAccessedAt { get; set; } // Последний доступ к набору
    
    [Display(Name = "Количество открытий")]
    public int AccessCount { get; set; } = 1; // Количество раз, когда открывали набор
    
    // Прогресс по набору
    [Display(Name = "Завершен")]
    public bool IsCompleted { get; set; } = false; // Полностью изучен ли набор (все карточки освоены)
    
    [Display(Name = "Дата завершения")]
    public DateTime? CompletedAt { get; set; } // Когда набор был завершен
    
    [Display(Name = "Изучено карточек")]
    public int CardsStudiedCount { get; set; } = 0; // Сколько карточек изучено (IsMastered = true)
    
    [Display(Name = "Всего карточек")]
    public int TotalCardsCount { get; set; } // Всего карточек в наборе (на момент последнего обновления)
    
    [Display(Name = "Дата создания записи")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Дата обновления")]
    public DateTime? UpdatedAt { get; set; }
}

