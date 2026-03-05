using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class MockExamService : IMockExamService
{
    private readonly UniStartDbContext _context;

    public MockExamService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MockExamListDto>> GetAvailableMockExamsAsync(int userId)
    {
        var exams = await _context.MockExams
            .Include(m => m.ExamType)
            .Include(m => m.Sections)
            .Where(m => m.IsActive)
            .ToListAsync();

        var attempts = await _context.MockExamAttempts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        // Batch: get question counts per section in a single query (fix N+1)
        var allSectionIds = exams
            .SelectMany(e => e.Sections)
            .Where(s => s.ExamSectionId.HasValue)
            .Select(s => s.ExamSectionId!.Value)
            .Distinct()
            .ToList();

        var questionCountsBySection = await _context.Questions
            .Where(q => q.Topic.SectionId.HasValue && allSectionIds.Contains(q.Topic.SectionId.Value))
            .GroupBy(q => q.Topic.SectionId!.Value)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SectionId, x => x.Count);

        var result = new List<MockExamListDto>();
        foreach (var exam in exams)
        {
            var sectionIds = exam.Sections
                .Where(s => s.ExamSectionId.HasValue)
                .Select(s => s.ExamSectionId!.Value)
                .ToList();
            var questionCount = sectionIds
                .Sum(sid => questionCountsBySection.GetValueOrDefault(sid, 0));

            var examAttempts = attempts.Where(a => a.MockExamId == exam.Id).ToList();
            var bestScore = examAttempts
                .Where(a => a.Status == "completed" && a.TotalScore.HasValue)
                .Select(a => (int?)Math.Round(a.TotalScore!.Value))
                .OrderByDescending(s => s)
                .FirstOrDefault();

            result.Add(new MockExamListDto(
                exam.Id,
                exam.ExamTypeCode,
                exam.ExamType.Name,
                exam.Title,
                exam.Description,
                exam.TotalTimeMinutes,
                exam.Sections.Count,
                questionCount,
                bestScore,
                examAttempts.Count
            ));
        }
        return result;
    }

    public async Task<MockExamDetailDto?> GetMockExamDetailAsync(int mockExamId)
    {
        var exam = await _context.MockExams
            .Include(m => m.ExamType)
            .Include(m => m.Sections.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(m => m.Id == mockExamId);

        if (exam == null) return null;

        // Batch: get question counts per section in a single query (fix N+1)
        var sectionIds = exam.Sections
            .Where(s => s.ExamSectionId.HasValue)
            .Select(s => s.ExamSectionId!.Value)
            .Distinct()
            .ToList();

        var questionCountsBySection = await _context.Questions
            .Where(q => q.Topic.SectionId.HasValue && sectionIds.Contains(q.Topic.SectionId.Value))
            .GroupBy(q => q.Topic.SectionId!.Value)
            .Select(g => new { SectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SectionId, x => x.Count);

        var sectionDtos = exam.Sections.OrderBy(s => s.SortOrder).Select(section =>
            new MockExamSectionDto(
                section.Id,
                section.Name,
                section.TimeLimitMinutes,
                section.ExamSectionId.HasValue
                    ? questionCountsBySection.GetValueOrDefault(section.ExamSectionId.Value, 0)
                    : 0,
                section.SortOrder,
                section.Instructions
            )).ToList();

        return new MockExamDetailDto(
            exam.Id,
            exam.ExamTypeCode,
            exam.ExamType.Name,
            exam.Title,
            exam.Description,
            exam.TotalTimeMinutes,
            sectionDtos
        );
    }

    public async Task<MockExamAttemptDto> StartMockExamAsync(int userId, int mockExamId)
    {
        var exam = await _context.MockExams
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == mockExamId && m.IsActive)
            ?? throw new ArgumentException("Mock exam not found or inactive");

        // Abandon any in-progress attempts for this mock exam
        var inProgress = await _context.MockExamAttempts
            .Where(a => a.UserId == userId && a.MockExamId == mockExamId && a.Status == "in_progress")
            .ToListAsync();
        foreach (var old in inProgress)
        {
            old.Status = "abandoned";
            old.CompletedAt = DateTime.UtcNow;
        }

        var attempt = new MockExamAttempt
        {
            UserId = userId,
            MockExamId = mockExamId,
            StartedAt = DateTime.UtcNow,
            Status = "in_progress",
            CurrentSectionIndex = 0
        };

        _context.MockExamAttempts.Add(attempt);

        // Pre-populate answers for all sections
        var sections = exam.Sections.OrderBy(s => s.SortOrder).ToList();
        for (int si = 0; si < sections.Count; si++)
        {
            var section = sections[si];
            if (!section.ExamSectionId.HasValue) continue;

            var questions = await _context.Questions
                .Where(q => q.Topic.SectionId == section.ExamSectionId.Value)
                .OrderBy(q => q.Id) // Deterministic order
                .ToListAsync();

            foreach (var q in questions)
            {
                _context.MockExamAnswers.Add(new MockExamAnswer
                {
                    Attempt = attempt,
                    QuestionId = q.Id,
                    SectionIndex = si,
                    SelectedOptionId = null,
                    IsCorrect = false
                });
            }
        }

        await _context.SaveChangesAsync();

        return new MockExamAttemptDto(
            attempt.Id,
            attempt.MockExamId,
            exam.Title,
            attempt.Status,
            attempt.CurrentSectionIndex,
            sections.Count,
            attempt.StartedAt
        );
    }

    public async Task<MockExamSectionStateDto?> GetCurrentSectionAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

        if (attempt == null || attempt.Status != "in_progress") return null;

        // Server-side timer enforcement: auto-complete if total exam time expired
        var totalTimeLimit = attempt.MockExam.TotalTimeMinutes;
        if (totalTimeLimit > 0 && DateTime.UtcNow > attempt.StartedAt.AddMinutes(totalTimeLimit))
        {
            await CompleteExamInternalAsync(attempt);
            await _context.SaveChangesAsync();
            return null; // Exam auto-completed, no more sections
        }

        return await GetSectionStateAsync(attempt, attempt.CurrentSectionIndex);
    }

    public async Task<MockExamSectionStateDto?> GetSectionAsync(int userId, int attemptId, int sectionIndex)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

        if (attempt == null) return null;

        // Only allow viewing current or past sections (not future ones unless completed)
        if (attempt.Status == "in_progress" && sectionIndex > attempt.CurrentSectionIndex) return null;

        return await GetSectionStateAsync(attempt, sectionIndex);
    }

    private async Task<MockExamSectionStateDto?> GetSectionStateAsync(MockExamAttempt attempt, int sectionIndex)
    {
        var sections = attempt.MockExam.Sections.OrderBy(s => s.SortOrder).ToList();
        if (sectionIndex < 0 || sectionIndex >= sections.Count) return null;

        var section = sections[sectionIndex];

        // Get the answers for this section (with question data)
        var answers = await _context.MockExamAnswers
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Question).ThenInclude(q => q.Topic)
            .Include(a => a.Question).ThenInclude(q => q.ReadingPassage)
            .Where(a => a.AttemptId == attempt.Id && a.SectionIndex == sectionIndex)
            .OrderBy(a => a.Question.Id)
            .ToListAsync();

        var questionDtos = answers.Select(a => new MockExamQuestionDto(
            a.QuestionId,
            a.Question.Text,
            a.Question.Difficulty.ToString(),
            a.Question.Topic.Name,
            a.Question.AnswerOptions.Select(o => new MockExamOptionDto(o.Id, o.Text)),
            a.SelectedOptionId,
            a.Question.ReadingPassageId,
            a.Question.ReadingPassage?.Title,
            a.Question.ReadingPassage?.Content
        ));

        return new MockExamSectionStateDto(
            sectionIndex,
            section.Name,
            section.TimeLimitMinutes,
            section.Instructions,
            questionDtos,
            answers.Count,
            answers.Count(a => a.SelectedOptionId.HasValue)
        );
    }

    public async Task<bool> SubmitAnswerAsync(int userId, int attemptId, MockExamSubmitAnswerDto dto)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.Status == "in_progress");
        if (attempt == null) return false;

        // Server-side timer enforcement: reject answers after total exam time expires
        var totalTimeLimit = attempt.MockExam.TotalTimeMinutes;
        if (totalTimeLimit > 0 && DateTime.UtcNow > attempt.StartedAt.AddMinutes(totalTimeLimit))
        {
            // Auto-complete the exam since time expired
            await CompleteExamInternalAsync(attempt);
            await _context.SaveChangesAsync();
            return false;
        }

        var answer = await _context.MockExamAnswers
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == dto.QuestionId);
        if (answer == null) return false;

        // Only allow answering current section
        if (answer.SectionIndex != attempt.CurrentSectionIndex) return false;

        answer.SelectedOptionId = dto.SelectedOptionId;
        answer.TimeSpentSeconds = dto.TimeSpentSeconds;
        answer.IsCorrect = answer.Question.AnswerOptions
            .Any(o => o.Id == dto.SelectedOptionId && o.IsCorrect);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MockExamAttemptDto?> CompleteSectionAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.Status == "in_progress");
        if (attempt == null) return null;

        // Server-side timer enforcement: auto-complete if total exam time expired
        var totalTimeLimit = attempt.MockExam.TotalTimeMinutes;
        if (totalTimeLimit > 0 && DateTime.UtcNow > attempt.StartedAt.AddMinutes(totalTimeLimit))
        {
            await CompleteExamInternalAsync(attempt);
            await _context.SaveChangesAsync();
            return new MockExamAttemptDto(
                attempt.Id, attempt.MockExamId, attempt.MockExam.Title,
                attempt.Status, attempt.CurrentSectionIndex,
                attempt.MockExam.Sections.Count, attempt.StartedAt
            );
        }

        var sections = attempt.MockExam.Sections.OrderBy(s => s.SortOrder).ToList();
        var nextIndex = attempt.CurrentSectionIndex + 1;

        if (nextIndex >= sections.Count)
        {
            // Last section — complete the exam
            await CompleteExamInternalAsync(attempt);
        }
        else
        {
            attempt.CurrentSectionIndex = nextIndex;
        }

        await _context.SaveChangesAsync();

        return new MockExamAttemptDto(
            attempt.Id,
            attempt.MockExamId,
            attempt.MockExam.Title,
            attempt.Status,
            attempt.CurrentSectionIndex,
            sections.Count,
            attempt.StartedAt
        );
    }

    private async Task CompleteExamInternalAsync(MockExamAttempt attempt)
    {
        attempt.Status = "completed";
        attempt.CompletedAt = DateTime.UtcNow;

        var allAnswers = await _context.MockExamAnswers
            .Where(a => a.AttemptId == attempt.Id)
            .ToListAsync();

        var sections = await _context.MockExamSections
            .Where(s => s.MockExamId == attempt.MockExamId)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        var sectionScores = new List<object>();
        var totalCorrect = 0;
        var totalCount = 0;

        for (int si = 0; si < sections.Count; si++)
        {
            var sectionAnswers = allAnswers.Where(a => a.SectionIndex == si).ToList();
            var correct = sectionAnswers.Count(a => a.IsCorrect);
            totalCorrect += correct;
            totalCount += sectionAnswers.Count;

            sectionScores.Add(new
            {
                sectionName = sections[si].Name,
                correct,
                total = sectionAnswers.Count,
                score = sectionAnswers.Count > 0 ? Math.Round(100.0 * correct / sectionAnswers.Count, 1) : 0
            });
        }

        attempt.TotalScore = totalCount > 0 ? Math.Round(100.0 * totalCorrect / totalCount, 1) : 0;
        attempt.SectionScoresJson = JsonSerializer.Serialize(sectionScores);
    }

    public async Task<MockExamResultDto?> GetResultsAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.ExamType)
            .Include(a => a.MockExam).ThenInclude(m => m.Sections.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

        if (attempt == null || attempt.Status != "completed") return null;

        var allAnswers = await _context.MockExamAnswers
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Section)
            .Where(a => a.AttemptId == attemptId)
            .OrderBy(a => a.SectionIndex).ThenBy(a => a.QuestionId)
            .ToListAsync();

        var sections = attempt.MockExam.Sections.OrderBy(s => s.SortOrder).ToList();

        // Section results
        var sectionResults = new List<MockExamSectionResultDto>();
        for (int si = 0; si < sections.Count; si++)
        {
            var sectionAnswers = allAnswers.Where(a => a.SectionIndex == si).ToList();
            var correct = sectionAnswers.Count(a => a.IsCorrect);
            var unanswered = sectionAnswers.Count(a => !a.SelectedOptionId.HasValue);
            sectionResults.Add(new MockExamSectionResultDto(
                si,
                sections[si].Name,
                sectionAnswers.Count,
                correct,
                unanswered,
                sectionAnswers.Count > 0 ? Math.Round(100.0 * correct / sectionAnswers.Count, 1) : 0,
                sections[si].TimeLimitMinutes
            ));
        }

        // Answer review
        var answerReview = allAnswers.Select(a =>
        {
            var correctOption = a.Question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
            var selectedOption = a.SelectedOptionId.HasValue
                ? a.Question.AnswerOptions.FirstOrDefault(o => o.Id == a.SelectedOptionId.Value)
                : null;
            var sectionName = a.SectionIndex < sections.Count ? sections[a.SectionIndex].Name : "Unknown";

            return new MockExamAnswerReviewDto(
                a.QuestionId,
                a.Question.Text,
                a.Question.Topic.Name,
                a.Question.Difficulty.ToString(),
                sectionName,
                a.SelectedOptionId,
                selectedOption?.Text,
                correctOption?.Id ?? 0,
                correctOption?.Text ?? "",
                a.IsCorrect,
                !a.SelectedOptionId.HasValue,
                a.Question.Explanation
            );
        }).ToList();

        var totalCorrect = allAnswers.Count(a => a.IsCorrect);

        return new MockExamResultDto(
            attempt.Id,
            attempt.MockExamId,
            attempt.MockExam.Title,
            attempt.MockExam.ExamTypeCode,
            attempt.TotalScore ?? 0,
            totalCorrect,
            allAnswers.Count,
            allAnswers.Count > 0 ? Math.Round(100.0 * totalCorrect / allAnswers.Count, 1) : 0,
            attempt.MockExam.TotalTimeMinutes,
            attempt.StartedAt,
            attempt.CompletedAt,
            sectionResults,
            answerReview
        );
    }

    public async Task<IEnumerable<MockExamHistoryDto>> GetHistoryAsync(int userId)
    {
        return await _context.MockExamAttempts
            .Include(a => a.MockExam)
            .Where(a => a.UserId == userId && a.Status != "abandoned")
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new MockExamHistoryDto(
                a.Id,
                a.MockExamId,
                a.MockExam.Title,
                a.MockExam.ExamTypeCode,
                a.Status,
                a.TotalScore,
                a.StartedAt,
                a.CompletedAt
            ))
            .ToListAsync();
    }

    public async Task<bool> AbandonAttemptAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.Status == "in_progress");
        if (attempt == null) return false;

        attempt.Status = "abandoned";
        attempt.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MockExamAttemptDto?> GetActiveAttemptAsync(int userId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .Where(a => a.UserId == userId && a.Status == "in_progress")
            .OrderByDescending(a => a.StartedAt)
            .FirstOrDefaultAsync();

        if (attempt == null) return null;

        // Auto-complete if total exam time expired
        var totalTimeLimit = attempt.MockExam.TotalTimeMinutes;
        if (totalTimeLimit > 0 && DateTime.UtcNow > attempt.StartedAt.AddMinutes(totalTimeLimit))
        {
            await CompleteExamInternalAsync(attempt);
            await _context.SaveChangesAsync();
            return null; // No longer active
        }

        return new MockExamAttemptDto(
            attempt.Id,
            attempt.MockExamId,
            attempt.MockExam.Title,
            attempt.Status,
            attempt.CurrentSectionIndex,
            attempt.MockExam.Sections.Count,
            attempt.StartedAt
        );
    }
}
