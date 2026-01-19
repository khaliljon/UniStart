using UniStart.Models.Flashcards;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Learning;

namespace UniStart.Services;

/// <summary>
/// Service for intelligent content recommendations
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Get recommended flashcard sets based on user preferences and activity
    /// </summary>
    Task<IEnumerable<FlashcardSet>> GetRecommendedFlashcardSetsAsync(string userId, int count = 10);
    
    /// <summary>
    /// Get recommended quizzes based on user preferences and activity
    /// </summary>
    Task<IEnumerable<Quiz>> GetRecommendedQuizzesAsync(string userId, int count = 10);
    
    /// <summary>
    /// Get recommended exams based on user preferences and goals
    /// </summary>
    Task<IEnumerable<Exam>> GetRecommendedExamsAsync(string userId, int count = 10);
}
