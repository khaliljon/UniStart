using Microsoft.ML;

namespace UniStart.Services.AI;

/// <summary>
/// Данные для обучения ML модели предсказания времени повторения
/// </summary>
public class FlashcardReviewData
{
    /// <summary>
    /// Текущий ease factor карточки (1.3 - 5.0)
    /// </summary>
    public float EaseFactor { get; set; }
    
    /// <summary>
    /// Текущий интервал повторения (в днях)
    /// </summary>
    public float Interval { get; set; }
    
    /// <summary>
    /// Количество повторений карточки
    /// </summary>
    public float Repetitions { get; set; }
    
    /// <summary>
    /// Дней с последнего повторения
    /// </summary>
    public float DaysSinceLastReview { get; set; }
    
    /// <summary>
    /// Средний retention rate пользователя (0-100)
    /// </summary>
    public float UserRetentionRate { get; set; }
    
    /// <summary>
    /// Скорость забывания пользователя (0.1-5.0)
    /// </summary>
    public float UserForgettingSpeed { get; set; }
    
    /// <summary>
    /// Количество раз, когда пользователь ответил правильно после перерыва
    /// </summary>
    public float CorrectAfterBreak { get; set; }
    
    /// <summary>
    /// Освоена ли карточка
    /// </summary>
    public bool IsMastered { get; set; }
    
    /// <summary>
    /// LABEL: Оптимальное время до следующего повторения (в часах)
    /// </summary>
    public float OptimalReviewHours { get; set; }
}

/// <summary>
/// Результат предсказания ML модели
/// </summary>
public class FlashcardReviewPredictionResult
{
    public float Score { get; set; }
}
