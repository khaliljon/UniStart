using System.ComponentModel.DataAnnotations;

namespace UniStart.Models;

/// <summary>
/// Попытка прохождения теста студентом
/// </summary>
public class UserTestAttempt
{
    public int Id { get; set; }
    
    public int Score { get; set; } // Набранные баллы
    public int TotalPoints { get; set; } // Максимальные баллы
    
    [Range(0, 100)]
    public double Percentage { get; set; } // Процент правильных ответов
    
    public bool Passed { get; set; } // Сдан ли тест (Score >= PassingScore)
    public TimeSpan TimeSpent { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Дополнительные данные
    [Range(1, 10)]
    public int AttemptNumber { get; set; } // Номер попытки (1, 2, 3...)
    
    [StringLength(50)]
    public string? IpAddress { get; set; } // Для контроля
    
    [StringLength(500)]
    public string? UserAgent { get; set; } // Для контроля
    
    // Связи
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int TestId { get; set; }
    public Test Test { get; set; } = null!;
    public ICollection<UserTestAnswer> UserAnswers { get; set; } = new List<UserTestAnswer>();
}
