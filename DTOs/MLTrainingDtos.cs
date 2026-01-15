using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs;

/// <summary>
/// DTO для ручного ввода тренировочных данных
/// </summary>
public class ManualTrainingDataDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int FlashcardId { get; set; }
    
    [Range(1.3, 2.5)]
    public double EaseFactor { get; set; } = 2.5;
    
    [Range(0, 365)]
    public int Interval { get; set; }
    
    [Range(0, 100)]
    public int Repetitions { get; set; }
    
    [Range(0, 365)]
    public double DaysSinceLastReview { get; set; }
    
    [Range(0, 100)]
    public double UserRetentionRate { get; set; } = 70.0;
    
    [Range(0.1, 5.0)]
    public double UserForgettingSpeed { get; set; } = 1.0;
    
    [Range(0, 100)]
    public double CorrectAfterBreak { get; set; }
    
    public bool IsMastered { get; set; }
    
    /// <summary>
    /// Целевое значение - оптимальное время для повторения в часах
    /// </summary>
    [Range(1, 8760)]
    public double OptimalReviewHours { get; set; }
}

public class TrainingDataImportResult
{
    public bool Success { get; set; }
    public int RecordsAdded { get; set; }
    public int TotalRecords { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class TrainingStatsDto
{
    public int TotalRecords { get; set; }
    public int RecordsLast24Hours { get; set; }
    public int RecordsLast7Days { get; set; }
    public int RecordsLast30Days { get; set; }
    public bool CanTrain => TotalRecords >= 100;
    public bool IsModelTrained { get; set; }
    public DateTime? LastTrainingDate { get; set; }
    public int UniqueUsers { get; set; }
    public int UniqueFlashcards { get; set; }
    public double AverageEaseFactor { get; set; }
    public double AverageInterval { get; set; }
    public double AverageRetentionRate { get; set; }
}
