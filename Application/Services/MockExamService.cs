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

    /// <summary>
    /// Returns the effective sections for an attempt, respecting SelectedSectionIdsJson.
    /// If null/empty, returns all sections ordered by SortOrder.
    /// </summary>
    private static List<MockExamSection> GetEffectiveSections(MockExamAttempt attempt)
    {
        var all = attempt.MockExam.Sections.OrderBy(s => s.SortOrder).ToList();
        if (string.IsNullOrEmpty(attempt.SelectedSectionIdsJson)) return all;

        var selectedIds = JsonSerializer.Deserialize<List<int>>(attempt.SelectedSectionIdsJson);
        if (selectedIds == null || selectedIds.Count == 0) return all;

        return all.Where(s => selectedIds.Contains(s.Id)).OrderBy(s => s.SortOrder).ToList();
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

        // Run-based access: paid runs left per template, and whether the
        // one-time free run is still available.
        var runsByMock = await _context.UserMockRuns
            .Where(r => r.UserId == userId)
            .ToDictionaryAsync(r => r.MockExamId, r => r.RunsRemaining);

        var user = await _context.Users.FindAsync(userId);
        var freeAvailable = user != null && !user.FreeMockUsed;

        var result = new List<MockExamListDto>();
        foreach (var exam in exams)
        {
            var questionCount = exam.Sections.Sum(s => s.QuestionCount);

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
                examAttempts.Count,
                runsByMock.TryGetValue(exam.Id, out var rr) ? rr : 0,
                freeAvailable,
                exam.TitleKz,
                exam.TitleEn,
                exam.DescriptionKz,
                exam.DescriptionEn
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

        var sectionDtos = exam.Sections.OrderBy(s => s.SortOrder).Select(section =>
            new MockExamSectionDto(
                section.Id,
                section.Name,
                section.TimeLimitMinutes,
                section.QuestionCount,
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
            sectionDtos,
            exam.TitleKz,
            exam.TitleEn,
            exam.DescriptionKz,
            exam.DescriptionEn
        );
    }

    public async Task<MockExamAttemptDto> StartMockExamAsync(int userId, int mockExamId, List<int>? selectedSectionIds = null)
    {
        var exam = await _context.MockExams
            .Include(m => m.Sections)
            .FirstOrDefaultAsync(m => m.Id == mockExamId && m.IsActive)
            ?? throw new ArgumentException("Mock exam not found or inactive");

        // ── Run-based access gate ──────────────────────────────
        // A start consumes one paid run for this template; if none, it consumes
        // the user's one-time free run (any subject). Otherwise it's locked.
        var runs = await _context.UserMockRuns
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MockExamId == mockExamId);
        if (runs != null && runs.RunsRemaining > 0)
        {
            runs.RunsRemaining--;
        }
        else
        {
            var accessUser = await _context.Users.FindAsync(userId);
            if (accessUser != null && !accessUser.FreeMockUsed)
            {
                accessUser.FreeMockUsed = true; // consume the one free run
            }
            else
            {
                throw new InvalidOperationException("MOCK_LOCKED");
            }
        }

        // Abandon any in-progress attempts for this mock exam
        var inProgress = await _context.MockExamAttempts
            .Where(a => a.UserId == userId && a.MockExamId == mockExamId && a.Status == "in_progress")
            .ToListAsync();
        foreach (var old in inProgress)
        {
            old.Status = "abandoned";
            old.CompletedAt = DateTime.UtcNow;
        }

        // Filter sections if selectedSectionIds provided (used for configurable mock exam sections)
        var allSections = exam.Sections.OrderBy(s => s.SortOrder).ToList();
        var sections = selectedSectionIds != null && selectedSectionIds.Count > 0
            ? allSections.Where(s => selectedSectionIds.Contains(s.Id)).OrderBy(s => s.SortOrder).ToList()
            : allSections;

        if (sections.Count == 0)
            throw new ArgumentException("No valid sections selected");

        var attempt = new MockExamAttempt
        {
            UserId = userId,
            MockExamId = mockExamId,
            StartedAt = DateTime.UtcNow,
            Status = "in_progress",
            CurrentSectionIndex = 0,
            SelectedSectionIdsJson = selectedSectionIds != null && selectedSectionIds.Count > 0
                ? JsonSerializer.Serialize(selectedSectionIds)
                : null
        };

        _context.MockExamAttempts.Add(attempt);

        // Anti-repeat: question ids this user already saw in prior sessions of this
        // template. We prefer unseen questions and only fall back to seen ones if
        // the pool is too small (per the agreed algorithm).
        var seenSet = (await _context.MockExamAnswers
            .Where(a => a.Attempt.UserId == userId && a.Attempt.MockExamId == mockExamId)
            .Select(a => a.QuestionId)
            .Distinct()
            .ToListAsync()).ToHashSet();

        // Pre-populate answers for selected sections only
        var rng = Random.Shared;
        for (int si = 0; si < sections.Count; si++)
        {
            var section = sections[si];
            if (!section.ExamSectionId.HasValue) continue;

            var questions = await _context.Questions
                .Where(q => q.Topic.SectionId == section.ExamSectionId.Value)
                .ToListAsync();
            if (questions.Count == 0) continue;

            Shuffle(questions, rng);

            // Anti-repeat: prefer questions the user hasn't seen in prior sessions.
            List<Question> AntiRepeat(IEnumerable<Question> qs) =>
                qs.Where(q => !seenSet.Contains(q.Id))
                  .Concat(qs.Where(q => seenSet.Contains(q.Id)))
                  .ToList();

            // Limit to regulation question count if set
            var count = section.QuestionCount > 0 && section.QuestionCount < questions.Count
                ? section.QuestionCount
                : questions.Count;

            // Guarantee at least one question per non-empty topic of the section,
            // then fill the rest (anti-repeat aware) up to the count.
            var byTopic = questions.GroupBy(q => q.TopicId).Select(g => AntiRepeat(g)).ToList();
            Shuffle(byTopic, rng);

            var selected = new List<Question>();
            var usedIds = new HashSet<int>();
            foreach (var topicQuestions in byTopic)
            {
                if (selected.Count >= count) break;
                selected.Add(topicQuestions[0]);
                usedIds.Add(topicQuestions[0].Id);
            }
            if (selected.Count < count)
            {
                foreach (var q in AntiRepeat(questions.Where(q => !usedIds.Contains(q.Id))))
                {
                    if (selected.Count >= count) break;
                    selected.Add(q);
                    usedIds.Add(q.Id);
                }
            }
            Shuffle(selected, rng);

            for (int qi = 0; qi < selected.Count; qi++)
            {
                _context.MockExamAnswers.Add(new MockExamAnswer
                {
                    Attempt = attempt,
                    QuestionId = selected[qi].Id,
                    SectionIndex = si,
                    SortOrder = qi,
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
            attempt.StartedAt,
            sections.Sum(s => s.TimeLimitMinutes),
            sections.Select(s => s.Name)
        );
    }

    private static void Shuffle<T>(IList<T> list, Random rng)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public async Task<MockExamSectionStateDto?> GetCurrentSectionAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId);

        if (attempt == null || attempt.Status != "in_progress") return null;

        // Server-side timer enforcement: auto-complete if total exam time expired
        var effectiveSections = GetEffectiveSections(attempt);
        var totalTimeLimit = effectiveSections.Sum(s => s.TimeLimitMinutes);
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

        return await GetSectionStateAsync(attempt, sectionIndex);
    }

    private async Task<MockExamSectionStateDto?> GetSectionStateAsync(MockExamAttempt attempt, int sectionIndex)
    {
        var sections = GetEffectiveSections(attempt);
        if (sectionIndex < 0 || sectionIndex >= sections.Count) return null;

        var section = sections[sectionIndex];

        // Get the answers for this section (with question data)
        var answers = await _context.MockExamAnswers
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Question).ThenInclude(q => q.Topic)
            .Include(a => a.Question).ThenInclude(q => q.ReadingPassage)
            .Include(a => a.SelectedOptions)
            .Where(a => a.AttemptId == attempt.Id && a.SectionIndex == sectionIndex)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        var questionDtos = answers.Select(a =>
        {
            // Deterministic shuffle of answer options per attempt+question
            var optionsList = a.Question.AnswerOptions.ToList();
            var seed = unchecked(attempt.Id * 31 + a.QuestionId);
            var optRng = new Random(seed);
            for (int i = optionsList.Count - 1; i > 0; i--)
            {
                int j = optRng.Next(i + 1);
                (optionsList[i], optionsList[j]) = (optionsList[j], optionsList[i]);
            }

            return new MockExamQuestionDto(
                a.QuestionId,
                a.Question.Text,
                a.Question.Difficulty.ToString(),
                a.Question.Topic.Name,
                optionsList.Select(o => new MockExamOptionDto(o.Id, o.Text)),
                a.SelectedOptionId,
                a.Question.ReadingPassageId,
                a.Question.ReadingPassage?.Title,
                a.Question.ReadingPassage?.Content,
                a.Question.ImageUrl,
                a.Question.IsMultipleChoice,
                a.SelectedOptions.Select(s => s.AnswerOptionId).ToList()
            );
        });

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
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.Status == "in_progress");
        if (attempt == null) return false;

        // Server-side timer enforcement: reject answers after total exam time expires
        var effectiveSections = GetEffectiveSections(attempt);
        var totalTimeLimit = effectiveSections.Sum(s => s.TimeLimitMinutes);
        if (totalTimeLimit > 0 && DateTime.UtcNow > attempt.StartedAt.AddMinutes(totalTimeLimit))
        {
            // Auto-complete the exam since time expired
            await CompleteExamInternalAsync(attempt);
            await _context.SaveChangesAsync();
            return false;
        }

        var answer = await _context.MockExamAnswers
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.SelectedOptions)
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == dto.QuestionId);
        if (answer == null) return false;

        // Resolve the selected option id(s): prefer the multi-select list, fall back
        // to the single legacy id. Keep only ids that belong to this question.
        var validIds = answer.Question.AnswerOptions.Select(o => o.Id).ToHashSet();
        var ids = ((dto.SelectedOptionIds != null && dto.SelectedOptionIds.Count > 0)
                ? dto.SelectedOptionIds
                : (dto.SelectedOptionId > 0 ? new List<int> { dto.SelectedOptionId } : new List<int>()))
            .Where(validIds.Contains).Distinct().ToList();

        // Replace the stored selection set.
        if (answer.SelectedOptions.Count > 0)
            _context.MockExamAnswerOptions.RemoveRange(answer.SelectedOptions.ToList());
        answer.SelectedOptions.Clear();
        foreach (var oid in ids)
            answer.SelectedOptions.Add(new MockExamAnswerOption { MockExamAnswerId = answer.Id, AnswerOptionId = oid });

        answer.SelectedOptionId = ids.Count > 0 ? ids[0] : (int?)null;
        answer.TimeSpentSeconds = dto.TimeSpentSeconds;

        var correctIds = answer.Question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet();
        answer.IsCorrect = answer.Question.IsMultipleChoice
            ? ids.Count > 0 && correctIds.SetEquals(ids)
            : ids.Count == 1 && correctIds.Contains(ids[0]);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<MockExamAttemptDto?> CompleteSectionAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.Status == "in_progress");
        if (attempt == null) return null;

        var sections = GetEffectiveSections(attempt);

        // Server-side timer enforcement: auto-complete if total exam time expired
        var totalTimeLimit = sections.Sum(s => s.TimeLimitMinutes);
        if (totalTimeLimit > 0 && DateTime.UtcNow > attempt.StartedAt.AddMinutes(totalTimeLimit))
        {
            await CompleteExamInternalAsync(attempt);
            await _context.SaveChangesAsync();
            return new MockExamAttemptDto(
                attempt.Id, attempt.MockExamId, attempt.MockExam.Title,
                attempt.Status, attempt.CurrentSectionIndex,
                sections.Count, attempt.StartedAt,
                totalTimeLimit,
                sections.Select(s => s.Name)
            );
        }

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
            attempt.StartedAt,
            sections.Sum(s => s.TimeLimitMinutes),
            sections.Select(s => s.Name)
        );
    }

    public async Task<MockExamAttemptDto?> CompleteExamAsync(int userId, int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.Sections)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId && a.Status == "in_progress");
        if (attempt == null) return null;

        var sections = GetEffectiveSections(attempt);
        await CompleteExamInternalAsync(attempt);

        // Free-mock consumption is handled at start time (piecewise access gate).
        await _context.SaveChangesAsync();

        return new MockExamAttemptDto(
            attempt.Id,
            attempt.MockExamId,
            attempt.MockExam.Title,
            attempt.Status,
            attempt.CurrentSectionIndex,
            sections.Count,
            attempt.StartedAt,
            sections.Sum(s => s.TimeLimitMinutes),
            sections.Select(s => s.Name)
        );
    }

    private async Task CompleteExamInternalAsync(MockExamAttempt attempt)
    {
        attempt.Status = "completed";
        attempt.CompletedAt = DateTime.UtcNow;

        var allAnswers = await _context.MockExamAnswers
            .Where(a => a.AttemptId == attempt.Id)
            .ToListAsync();

        // Load sections if not already loaded
        if (attempt.MockExam?.Sections == null || !attempt.MockExam.Sections.Any())
        {
            await _context.Entry(attempt).Reference(a => a.MockExam).Query()
                .Include(m => m.Sections).LoadAsync();
        }
        var sections = GetEffectiveSections(attempt);

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
        return await BuildResultsAsync(attempt);
    }

    /// <summary>Admin-only: results for any completed attempt (no ownership check).</summary>
    public async Task<MockExamResultDto?> GetResultsForAdminAsync(int attemptId)
    {
        var attempt = await _context.MockExamAttempts
            .Include(a => a.MockExam).ThenInclude(m => m.ExamType)
            .Include(a => a.MockExam).ThenInclude(m => m.Sections.OrderBy(s => s.SortOrder))
            .FirstOrDefaultAsync(a => a.Id == attemptId);

        if (attempt == null || attempt.Status != "completed") return null;
        return await BuildResultsAsync(attempt);
    }

    private async Task<MockExamResultDto?> BuildResultsAsync(MockExamAttempt attempt)
    {
        var attemptId = attempt.Id;
        var allAnswers = await _context.MockExamAnswers
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Question).ThenInclude(q => q.Topic).ThenInclude(t => t.Section)
            .Include(a => a.SelectedOptions)
            .Where(a => a.AttemptId == attemptId)
            .OrderBy(a => a.SectionIndex).ThenBy(a => a.SortOrder)
            .ToListAsync();

        var sections = GetEffectiveSections(attempt);

        // Correctness is re-derived from the CURRENT question (correct option + the
        // user's stored selection), so edits to a question/options are reflected in
        // old reviews instead of showing stale results.
        static (bool answered, bool correct) Evaluate(MockExamAnswer a)
        {
            var correctIds = a.Question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Id).OrderBy(x => x).ToList();
            var selectedIds = a.SelectedOptions.Select(s => s.AnswerOptionId).ToList();
            if (selectedIds.Count == 0 && a.SelectedOptionId.HasValue) selectedIds.Add(a.SelectedOptionId.Value);
            var answered = selectedIds.Count > 0;
            if (!answered || correctIds.Count == 0) return (answered, false);
            var selSorted = selectedIds.Distinct().OrderBy(x => x).ToList();
            var correct = a.Question.IsMultipleChoice
                ? selSorted.SequenceEqual(correctIds)
                : selSorted.Count == 1 && correctIds.Contains(selSorted[0]);
            return (answered, correct);
        }

        // Section results
        var sectionResults = new List<MockExamSectionResultDto>();
        for (int si = 0; si < sections.Count; si++)
        {
            var sectionAnswers = allAnswers.Where(a => a.SectionIndex == si).ToList();
            var correct = sectionAnswers.Count(a => Evaluate(a).correct);
            var unanswered = sectionAnswers.Count(a => !Evaluate(a).answered);
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

            var correctIds = a.Question.AnswerOptions.Where(o => o.IsCorrect).Select(o => o.Id).ToList();
            var selectedIds = a.SelectedOptions.Select(s => s.AnswerOptionId).ToList();
            if (selectedIds.Count == 0 && a.SelectedOptionId.HasValue) selectedIds.Add(a.SelectedOptionId.Value);
            var (answered, isCorrect) = Evaluate(a);

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
                isCorrect,
                !answered,
                a.Question.Explanation,
                a.Question.IsMultipleChoice,
                selectedIds,
                correctIds,
                a.Question.ImageUrl
            );
        }).ToList();

        var totalCorrect = allAnswers.Count(a => Evaluate(a).correct);
        var dynamicScore = allAnswers.Count > 0 ? (int)Math.Round(100.0 * totalCorrect / allAnswers.Count) : 0;

        return new MockExamResultDto(
            attempt.Id,
            attempt.MockExamId,
            attempt.MockExam.Title,
            attempt.MockExam.ExamTypeCode,
            dynamicScore,
            totalCorrect,
            allAnswers.Count,
            allAnswers.Count > 0 ? Math.Round(100.0 * totalCorrect / allAnswers.Count, 1) : 0,
            sections.Sum(s => s.TimeLimitMinutes),
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
        var effectiveSections = GetEffectiveSections(attempt);
        var totalTimeLimit = effectiveSections.Sum(s => s.TimeLimitMinutes);
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
            effectiveSections.Count,
            attempt.StartedAt,
            totalTimeLimit,
            effectiveSections.Select(s => s.Name)
        );
    }
}
