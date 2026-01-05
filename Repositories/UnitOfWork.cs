using Microsoft.EntityFrameworkCore.Storage;
using UniStart.Data;

namespace UniStart.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    // Repository instances
    private IQuizRepository? _quizzes;
    private IExamRepository? _exams;
    private IFlashcardSetRepository? _flashcardSets;
    private IUserQuizAttemptRepository? _quizAttempts;
    private IUserExamAttemptRepository? _examAttempts;
    private IUserFlashcardProgressRepository? _flashcardProgress;
    private IAchievementRepository? _achievements;
    
    // Generic repository cache
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lazy initialization of repositories
    public IQuizRepository Quizzes =>
        _quizzes ??= new QuizRepository(_context);

    public IExamRepository Exams =>
        _exams ??= new ExamRepository(_context);

    public IFlashcardSetRepository FlashcardSets =>
        _flashcardSets ??= new FlashcardSetRepository(_context);

    public IUserQuizAttemptRepository QuizAttempts =>
        _quizAttempts ??= new UserQuizAttemptRepository(_context);

    public IUserExamAttemptRepository ExamAttempts =>
        _examAttempts ??= new UserExamAttemptRepository(_context);

    public IUserFlashcardProgressRepository FlashcardProgress =>
        _flashcardProgress ??= new UserFlashcardProgressRepository(_context);

    public IAchievementRepository Achievements =>
        _achievements ??= new AchievementRepository(_context);

    // Generic repository access
    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new Repository<T>(_context);
        }
        return (IRepository<T>)_repositories[type];
    }

    // Transaction management
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
