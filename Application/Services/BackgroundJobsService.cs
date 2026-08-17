using Microsoft.EntityFrameworkCore;
using UniStart.Application.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class BackgroundJobsService : IBackgroundJobsService
{
    private readonly UniStartDbContext _context;
    private readonly ILogger<BackgroundJobsService> _logger;

    public BackgroundJobsService(
        UniStartDbContext context,
        ILogger<BackgroundJobsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task PurgeSoftDeletedRecordsAsync()
    {
        _logger.LogInformation("Hangfire: Purging soft-deleted records older than 30 days...");

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var totalPurged = 0;

        var deletedQuestions = await _context.Questions
            .IgnoreQueryFilters()
            .Where(q => q.IsDeleted && q.DeletedAt != null && q.DeletedAt < cutoff)
            .ToListAsync();

        if (deletedQuestions.Any())
        {
            _context.Questions.RemoveRange(deletedQuestions);
            totalPurged += deletedQuestions.Count;
            _logger.LogInformation("Purging {Count} soft-deleted questions.", deletedQuestions.Count);
        }

        var deletedUsers = await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.IsDeleted && u.DeletedAt != null && u.DeletedAt < cutoff)
            .ToListAsync();

        if (deletedUsers.Any())
        {
            _context.Users.RemoveRange(deletedUsers);
            totalPurged += deletedUsers.Count;
            _logger.LogInformation("Purging {Count} soft-deleted users.", deletedUsers.Count);
        }

        if (totalPurged > 0)
        {
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Soft-delete purge complete. {Count} records permanently removed.", totalPurged);
    }

    public async Task CalibrateIrtParametersAsync()
    {
        _logger.LogInformation("Hangfire: Starting IRT auto-calibration...");

        const int MinAnswers = 30;
        const double MinDelta = 0.15;

        var stats = await _context.UserAnswers
            .Where(ua => ua.AnswerOption != null)
            .GroupBy(ua => ua.QuestionId)
            .Select(g => new
            {
                QuestionId = g.Key,
                Total = g.Count(),
                Correct = g.Count(ua => ua.AnswerOption!.IsCorrect)
            })
            .Where(s => s.Total >= MinAnswers)
            .ToListAsync();

        if (stats.Count == 0)
        {
            _logger.LogInformation("IRT calibration: no questions with {Min}+ answers yet.", MinAnswers);
            return;
        }

        var questionIds = stats.Select(s => s.QuestionId).ToList();
        var questions = await _context.Questions
            .Where(q => questionIds.Contains(q.Id) && !q.IsDeleted)
            .ToListAsync();

        var updated = 0;
        foreach (var q in questions)
        {
            var s = stats.First(x => x.QuestionId == q.Id);
            if (s.Total == 0) continue;

            double p = (double)s.Correct / s.Total;
            p = Math.Clamp(p, 0.01, 0.99);

            double bNew = Math.Log((1.0 - p) / p);

            if (Math.Abs(bNew - q.DifficultyParam) > MinDelta)
            {
                q.DifficultyParam = Math.Round(Math.Clamp(bNew, -4.0, 4.0), 3);
                q.UpdatedAt = DateTime.UtcNow;
                updated++;
            }
        }

        if (updated > 0)
            await _context.SaveChangesAsync();

        _logger.LogInformation(
            "IRT calibration complete. {Updated} of {Checked} questions updated.",
            updated, questions.Count);
    }
}
