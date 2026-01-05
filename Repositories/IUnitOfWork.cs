using UniStart.Data;

namespace UniStart.Repositories;

/// <summary>
/// Unit of Work pattern for managing transactions and repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories
    IQuizRepository Quizzes { get; }
    IExamRepository Exams { get; }
    IFlashcardSetRepository FlashcardSets { get; }
    IUserQuizAttemptRepository QuizAttempts { get; }
    IUserExamAttemptRepository ExamAttempts { get; }
    IUserFlashcardProgressRepository FlashcardProgress { get; }
    IAchievementRepository Achievements { get; }
    
    // Generic repository access for entities without specific repository
    IRepository<T> Repository<T>() where T : class;
    
    // Transaction management
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
