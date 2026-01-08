using UniStart.Models.Learning;

namespace UniStart.Services.AI;

/// <summary>
/// Интерфейс для ML-предсказаний оптимального времени повторения карточек
/// </summary>
public interface IMLPredictionService
{
    /// <summary>
    /// Предсказать оптимальное время следующего повторения карточки
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <param name="flashcardId">ID карточки</param>
    /// <returns>Предсказание с оптимальным временем и уверенностью модели</returns>
    Task<FlashcardReviewPrediction?> PredictNextReviewTime(string userId, int flashcardId);
    
    /// <summary>
    /// Сгенерировать персональный учебный план на основе ML предсказаний
    /// </summary>
    /// <param name="userId">ID пользователя</param>
    /// <returns>Список карточек к повторению с приоритетами</returns>
    Task<List<FlashcardReviewPrediction>> GenerateStudyPlan(string userId);
    
    /// <summary>
    /// Переобучить ML модель на свежих данных
    /// </summary>
    /// <returns>True если переобучение прошло успешно</returns>
    Task<bool> RetrainModel();
    
    /// <summary>
    /// Проверить, обучена ли модель и готова к использованию
    /// </summary>
    bool IsModelTrained();
}
