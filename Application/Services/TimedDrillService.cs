using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class TimedDrillService : ITimedDrillService
{
    private readonly UniStartDbContext _context;

    public TimedDrillService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<DrillResultDto> StartDrillAsync(int userId, StartDrillRequest request)
    {
        if (!Enum.TryParse<DrillType>(request.DrillType, true, out var drillType))
            throw new ArgumentException($"Invalid drill type: {request.DrillType}");

        var result = new TimedDrillResult
        {
            UserId = userId,
            DrillType = drillType,
            ExamTypeCode = request.ExamTypeCodes?.FirstOrDefault(),
            TopicId = request.TopicId,
            StartedAt = DateTime.UtcNow
        };

        _context.TimedDrillResults.Add(result);
        await _context.SaveChangesAsync();

        return MapToDto(result);
    }

    public async Task<DrillQuestionDto?> GetNextDrillQuestionAsync(int drillResultId)
    {
        var drill = await _context.TimedDrillResults.FindAsync(drillResultId);
        if (drill == null || drill.CompletedAt != null)
            return null;

        // Get IDs of questions already answered in this drill
        var answeredQuestionIds = await _context.UserAnswers
            .Where(a => a.UserId == drill.UserId
                && a.AnsweredAt >= drill.StartedAt
                && (drill.CompletedAt == null || a.AnsweredAt <= drill.CompletedAt))
            .Select(a => a.QuestionId)
            .Distinct()
            .ToListAsync();

        // Build query for available questions
        var query = _context.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.Topic)
            .Where(q => !q.IsDeleted && !answeredQuestionIds.Contains(q.Id));

        if (drill.TopicId != null)
        {
            query = query.Where(q => q.TopicId == drill.TopicId);
        }
        else if (drill.ExamTypeCode != null)
        {
            query = query.Where(q => q.Topic.Section!.ExamTypeCode == drill.ExamTypeCode);
        }

        // Speed drills: limit to 10 questions
        if (drill.DrillType == DrillType.Speed && drill.QuestionsAnswered >= 10)
            return null;

        // Pick a random question
        var count = await query.CountAsync();
        if (count == 0) return null;

        var skip = Random.Shared.Next(count);
        var question = await query.Skip(skip).FirstAsync();

        return new DrillQuestionDto(
            question.Id,
            question.Text,
            question.Topic?.Name,
            question.Difficulty.ToString(),
            question.AnswerOptions.Select(o => new DrillAnswerOptionDto(o.Id, o.Text))
        );
    }

    public async Task<DrillAnswerResultDto> SubmitDrillAnswerAsync(int userId, SubmitDrillAnswerRequest request)
    {
        var drill = await _context.TimedDrillResults.FindAsync(request.DrillResultId)
            ?? throw new ArgumentException("Drill not found");

        if (drill.UserId != userId)
            throw new UnauthorizedAccessException();

        if (drill.CompletedAt != null)
            throw new InvalidOperationException("Drill already completed");

        var question = await _context.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId)
            ?? throw new ArgumentException("Question not found");

        var selectedOption = question.AnswerOptions.FirstOrDefault(o => o.Id == request.AnswerOptionId);
        var isCorrect = selectedOption?.IsCorrect ?? false;
        var correctOption = question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);

        // Save the answer
        var userAnswer = new UserAnswer
        {
            UserId = userId,
            QuestionId = request.QuestionId,
            AnswerOptionId = request.AnswerOptionId,
            TimeSpentSeconds = request.TimeSpentSeconds,
            AnsweredAt = DateTime.UtcNow
        };
        _context.UserAnswers.Add(userAnswer);

        // Update drill stats
        drill.QuestionsAnswered++;
        if (isCorrect) drill.CorrectAnswers++;
        drill.TotalTimeSeconds += request.TimeSpentSeconds;
        drill.AverageTimeSeconds = (double)drill.TotalTimeSeconds / drill.QuestionsAnswered;

        // Track streak
        if (isCorrect)
        {
            // Count current streak
            var currentStreak = await GetCurrentStreakAsync(drill) + 1;
            if (currentStreak > drill.BestStreak)
                drill.BestStreak = currentStreak;
        }

        // Check if drill should end
        var drillEnded = false;
        switch (drill.DrillType)
        {
            case DrillType.Speed when drill.QuestionsAnswered >= 10:
                drillEnded = true;
                break;
            case DrillType.Streak when !isCorrect:
                drillEnded = true;
                break;
        }

        if (drillEnded)
        {
            drill.CompletedAt = DateTime.UtcNow;
        }

        drill.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new DrillAnswerResultDto(
            isCorrect,
            correctOption?.Id,
            question.Explanation,
            isCorrect ? await GetCurrentStreakAsync(drill) : 0,
            drill.CorrectAnswers,
            drill.QuestionsAnswered,
            drillEnded
        );
    }

    public async Task<DrillResultDto> CompleteDrillAsync(int userId, int drillResultId)
    {
        var drill = await _context.TimedDrillResults.FindAsync(drillResultId)
            ?? throw new ArgumentException("Drill not found");

        if (drill.UserId != userId)
            throw new UnauthorizedAccessException();

        if (drill.CompletedAt == null)
        {
            drill.CompletedAt = DateTime.UtcNow;
            drill.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapToDto(drill);
    }

    public async Task<IEnumerable<PersonalBestDto>> GetPersonalBestsAsync(int userId)
    {
        var drillTypes = Enum.GetValues<DrillType>();
        var results = new List<PersonalBestDto>();

        foreach (var type in drillTypes)
        {
            var drills = await _context.TimedDrillResults
                .Where(d => d.UserId == userId && d.DrillType == type && d.CompletedAt != null)
                .ToListAsync();

            if (drills.Count == 0)
            {
                results.Add(new PersonalBestDto(type.ToString(), null, null, null, null));
                continue;
            }

            var bestScore = drills.Max(d => d.CorrectAnswers);
            var bestStreak = drills.Max(d => d.BestStreak);
            var bestAvgTime = drills.Min(d => d.AverageTimeSeconds);
            var bestDrill = drills.OrderByDescending(d => d.CorrectAnswers).First();

            results.Add(new PersonalBestDto(
                type.ToString(), bestScore, bestStreak, 
                Math.Round(bestAvgTime, 1), bestDrill.CompletedAt
            ));
        }

        return results;
    }

    public async Task<IEnumerable<DrillResultDto>> GetDrillHistoryAsync(int userId, int limit = 20)
    {
        var drills = await _context.TimedDrillResults
            .Where(d => d.UserId == userId && d.CompletedAt != null)
            .OrderByDescending(d => d.CompletedAt)
            .Take(limit)
            .ToListAsync();

        return drills.Select(MapToDto);
    }

    private async Task<int> GetCurrentStreakAsync(TimedDrillResult drill)
    {
        var answers = await _context.UserAnswers
            .Include(a => a.AnswerOption)
            .Where(a => a.UserId == drill.UserId
                && a.AnsweredAt >= drill.StartedAt
                && (drill.CompletedAt == null || a.AnsweredAt <= drill.CompletedAt))
            .OrderByDescending(a => a.AnsweredAt)
            .Select(a => a.AnswerOption.IsCorrect)
            .ToListAsync();

        int streak = 0;
        foreach (var correct in answers)
        {
            if (correct) streak++;
            else break;
        }
        return streak;
    }

    private static DrillResultDto MapToDto(TimedDrillResult d)
    {
        var accuracy = d.QuestionsAnswered > 0
            ? Math.Round(100.0 * d.CorrectAnswers / d.QuestionsAnswered, 1)
            : 0;

        return new DrillResultDto(
            d.Id, d.DrillType.ToString(),
            d.QuestionsAnswered, d.CorrectAnswers,
            d.TotalTimeSeconds, Math.Round(d.AverageTimeSeconds, 1),
            d.BestStreak, accuracy,
            d.CompletedAt ?? d.StartedAt
        );
    }
}
