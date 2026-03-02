using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class MistakeService : IMistakeService
{
    private readonly UniStartDbContext _context;

    public MistakeService(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MistakeEntryDto>> GetMistakesAsync(
        int userId, string? examTypeCode = null, int? topicId = null,
        string? errorType = null, int page = 1, int pageSize = 20)
    {
        // Get wrong answers
        var query = _context.UserAnswers
            .Include(a => a.Question).ThenInclude(q => q.Topic)
            .Include(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.AnswerOption)
            .Where(a => a.UserId == userId && !a.AnswerOption.IsCorrect);

        if (topicId != null)
            query = query.Where(a => a.Question.TopicId == topicId);

        if (!string.IsNullOrEmpty(examTypeCode))
            query = query.Where(a => a.Question.Topic.Section!.ExamTypeCode == examTypeCode);

        var answers = await query
            .OrderByDescending(a => a.AnsweredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var answerIds = answers.Select(a => a.Id).ToList();
        var notes = await _context.UserMistakeNotes
            .Where(n => n.UserId == userId && answerIds.Contains(n.UserAnswerId))
            .ToDictionaryAsync(n => n.UserAnswerId);

        // Apply errorType filter after loading notes
        var entries = answers.Select(a =>
        {
            notes.TryGetValue(a.Id, out var note);
            var correctOption = a.Question.AnswerOptions.FirstOrDefault(o => o.IsCorrect);
            return new MistakeEntryDto(
                a.Id,
                a.QuestionId,
                a.Question.Text,
                a.Question.Topic?.Name ?? "",
                a.Question.Difficulty.ToString(),
                a.AnswerOption.Text,
                correctOption?.Text,
                a.Question.Explanation,
                a.AnsweredAt,
                note?.ErrorType?.ToString(),
                note?.NoteText
            );
        });

        if (!string.IsNullOrEmpty(errorType))
        {
            if (errorType == "unclassified")
                entries = entries.Where(e => e.ErrorType == null);
            else
                entries = entries.Where(e => e.ErrorType == errorType);
        }

        return entries.ToList();
    }

    public async Task SetErrorTypeAsync(int userId, SetErrorTypeRequest request)
    {
        var note = await _context.UserMistakeNotes
            .FirstOrDefaultAsync(n => n.UserId == userId && n.UserAnswerId == request.UserAnswerId);

        ErrorType? parsedType = null;
        if (!string.IsNullOrEmpty(request.ErrorType) &&
            Enum.TryParse<ErrorType>(request.ErrorType, true, out var et))
        {
            parsedType = et;
        }

        if (note == null)
        {
            note = new UserMistakeNote
            {
                UserId = userId,
                UserAnswerId = request.UserAnswerId,
                ErrorType = parsedType
            };
            _context.UserMistakeNotes.Add(note);
        }
        else
        {
            note.ErrorType = parsedType;
            note.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task SetNoteAsync(int userId, SetMistakeNoteRequest request)
    {
        var note = await _context.UserMistakeNotes
            .FirstOrDefaultAsync(n => n.UserId == userId && n.UserAnswerId == request.UserAnswerId);

        if (note == null)
        {
            note = new UserMistakeNote
            {
                UserId = userId,
                UserAnswerId = request.UserAnswerId,
                NoteText = request.NoteText
            };
            _context.UserMistakeNotes.Add(note);
        }
        else
        {
            note.NoteText = request.NoteText;
            note.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<MistakeAnalysisDto> GetAnalysisAsync(int userId, string? examTypeCode = null)
    {
        var wrongAnswers = _context.UserAnswers
            .Include(a => a.Question).ThenInclude(q => q.Topic)
            .Include(a => a.AnswerOption)
            .Where(a => a.UserId == userId && !a.AnswerOption.IsCorrect);

        if (!string.IsNullOrEmpty(examTypeCode))
            wrongAnswers = wrongAnswers.Where(a => a.Question.Topic.Section!.ExamTypeCode == examTypeCode);

        var answerList = await wrongAnswers.ToListAsync();
        var totalMistakes = answerList.Count;

        var answerIds = answerList.Select(a => a.Id).ToList();
        var notes = await _context.UserMistakeNotes
            .Where(n => n.UserId == userId && answerIds.Contains(n.UserAnswerId))
            .ToListAsync();

        // Error patterns
        var classified = notes.Where(n => n.ErrorType != null).ToList();
        var patterns = classified
            .GroupBy(n => n.ErrorType!.Value)
            .Select(g => new ErrorPatternDto(
                g.Key.ToString(),
                g.Count(),
                totalMistakes > 0 ? Math.Round(100.0 * g.Count() / totalMistakes, 1) : 0
            ))
            .OrderByDescending(p => p.Count)
            .ToList();

        // Topic breakdown
        var topicBreakdown = answerList
            .GroupBy(a => new { a.Question.TopicId, TopicName = a.Question.Topic?.Name ?? "Unknown" })
            .Select(g => new TopicMistakeDto(g.Key.TopicId, g.Key.TopicName, g.Count()))
            .OrderByDescending(t => t.MistakeCount)
            .ToList();

        return new MistakeAnalysisDto(totalMistakes, patterns, topicBreakdown);
    }

    public async Task<int> GetMistakeCountAsync(int userId)
    {
        return await _context.UserAnswers
            .Include(a => a.AnswerOption)
            .CountAsync(a => a.UserId == userId && !a.AnswerOption.IsCorrect);
    }
}
