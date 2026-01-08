namespace UniStart.Services.AI;

/// <summary>
/// Интерфейс для AI рекомендаций контента и персонализации обучения
/// </summary>
public interface IContentRecommendationService
{
    /// <summary>
    /// Рекомендовать квизы на основе слабых сторон пользователя
    /// </summary>
    Task<List<int>> RecommendQuizzesForWeaknesses(string userId, int count = 5);
    
    /// <summary>
    /// Рекомендовать экзамены на основе целей пользователя
    /// </summary>
    Task<List<int>> RecommendExamsForGoals(string userId, int count = 3);
    
    /// <summary>
    /// Рекомендовать наборы flashcards по темам, требующим улучшения
    /// </summary>
    Task<List<int>> RecommendFlashcardSets(string userId, int count = 5);
    
    /// <summary>
    /// Определить следующую оптимальную тему для изучения
    /// </summary>
    Task<string?> GetNextTopicToStudy(string userId);
    
    /// <summary>
    /// Получить персонализированные советы по улучшению результатов
    /// </summary>
    Task<List<string>> GetPersonalizedTips(string userId);
}
