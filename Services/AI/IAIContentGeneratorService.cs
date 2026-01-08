namespace UniStart.Services.AI;

/// <summary>
/// Интерфейс для AI генерации контента (вопросы, объяснения, подсказки)
/// </summary>
public interface IAIContentGeneratorService
{
    /// <summary>
    /// Генерировать вопросы по теме с использованием AI
    /// </summary>
    Task<List<GeneratedQuestion>> GenerateQuestions(string subject, string difficulty, int count = 5);
    
    /// <summary>
    /// Генерировать объяснение для ответа
    /// </summary>
    Task<string> GenerateExplanation(string question, string correctAnswer);
    
    /// <summary>
    /// Генерировать персонализированную подсказку для студента
    /// </summary>
    Task<string> GenerateHint(string question, string userAnswer);
    
    /// <summary>
    /// Генерировать резюме учебного материала
    /// </summary>
    Task<string> GenerateSummary(string topic, string content);
    
    /// <summary>
    /// Проверить доступность AI сервиса
    /// </summary>
    bool IsAvailable();
}

/// <summary>
/// Сгенерированный AI вопрос
/// </summary>
public class GeneratedQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
