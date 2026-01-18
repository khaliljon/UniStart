using System.ComponentModel.DataAnnotations;

using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;
namespace UniStart.Models.Exams;

/// <summary>
/// Попытка прохождения экзамена студентом
/// </summary>
public class UserExamAttempt
{
    public int Id { get; set; }
    
    public int Score { get; set; } // Набранные баллы
    public int MaxScore { get; set; } // Максимальные баллы
    
    [Range(0, 100)]
    public double Percentage { get; set; } // Процент правильных ответов
    
    public bool Passed { get; set; } // Сдан ли экзамен (Score >= PassingScore)
    public int TimeSpentSeconds { get; set; }
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
    public int ExamId { get; set; }
    public Exam Exam { get; set; } = null!;
    public ICollection<UserExamAnswer> UserAnswers { get; set; } = new List<UserExamAnswer>();
}
