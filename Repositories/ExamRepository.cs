using Microsoft.EntityFrameworkCore;
using UniStart.Data;
using UniStart.Models.Core;
using UniStart.Models.Quizzes;
using UniStart.Models.Exams;
using UniStart.Models.Flashcards;
using UniStart.Models.Reference;
using UniStart.Models.Learning;
using UniStart.Models.Social;

namespace UniStart.Repositories;

/// <summary>
/// Repository implementation for Exam entity
/// </summary>
public class ExamRepository : Repository<Exam>, IExamRepository
{
    public ExamRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Exam>> GetPublishedExamsAsync()
    {
        return await _dbSet
            .Where(e => e.IsPublished)
            .Include(e => e.User)
            .Include(e => e.Subjects)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetExamsBySubjectAsync(string subject)
    {
        return await _dbSet
            .Where(e => e.Subject == subject || e.Subjects.Any(s => s.Name == subject))
            .Include(e => e.User)
            .Include(e => e.Subjects)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exam>> GetExamsByUserAsync(string userId)
    {
        return await _dbSet
            .Where(e => e.UserId == userId)
            .Include(e => e.Questions)
            .Include(e => e.Subjects)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Exam?> GetExamWithQuestionsAsync(int examId)
    {
        return await _dbSet
            .Include(e => e.Questions)
            .ThenInclude(q => q.Answers)
            .Include(e => e.User)
            .Include(e => e.Subjects)
            .FirstOrDefaultAsync(e => e.Id == examId);
    }

    public async Task<Exam?> GetExamWithAttemptsAsync(int examId)
    {
        return await _dbSet
            .Include(e => e.Attempts)
            .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(e => e.Id == examId);
    }
}
