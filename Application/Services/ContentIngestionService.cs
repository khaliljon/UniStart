using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Idempotent ingestion of normalized study-pack content (Variant B: Topic = concept).
/// Safe to re-run on the same file (e.g. Drive re-sync): Skill/Topic are matched by
/// name, lessons/formulas by title, and questions by a normalized content hash.
/// </summary>
public class ContentIngestionService : IContentIngestionService
{
    private readonly UniStartDbContext _db;
    private readonly ILogger<ContentIngestionService> _logger;

    public ContentIngestionService(UniStartDbContext db, ILogger<ContentIngestionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IngestResultDto> IngestAsync(IngestContentDto payload)
    {
        var warnings = new List<string>();
        int topicsCreated = 0, topicsUpdated = 0;
        int lessonsCreated = 0, lessonsUpdated = 0;
        int formulasCreated = 0;
        int questionsCreated = 0, questionsSkipped = 0;

        if (string.IsNullOrWhiteSpace(payload.SkillName))
            throw new ArgumentException("SkillName is required.");

        // ─── ExamSection (optional link; Topic.SectionId is nullable) ───────────
        int? sectionId = null;
        if (!string.IsNullOrWhiteSpace(payload.ExamSectionName))
        {
            var section = await _db.ExamSections.FirstOrDefaultAsync(s =>
                s.ExamTypeCode == payload.ExamTypeCode &&
                s.Name.ToLower() == payload.ExamSectionName.ToLower());
            if (section == null)
                warnings.Add($"ExamSection '{payload.ExamSectionName}' for exam '{payload.ExamTypeCode}' not found; topics created without a section link.");
            else
                sectionId = section.Id;
        }

        // ─── Skill (get-or-create by name) ──────────────────────────────────────
        var skillName = payload.SkillName.Trim();
        var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Name.ToLower() == skillName.ToLower());
        bool skillCreated = false;
        if (skill == null)
        {
            skill = new Skill { Name = skillName, Code = await GenerateUniqueSkillCodeAsync(skillName) };
            _db.Skills.Add(skill);
            await _db.SaveChangesAsync();
            skillCreated = true;
        }

        // ─── Topics (each = one concept) ────────────────────────────────────────
        foreach (var topicDto in payload.Topics ?? new())
        {
            if (string.IsNullOrWhiteSpace(topicDto.Name))
            {
                warnings.Add("Skipped a topic with an empty name.");
                continue;
            }
            var topicName = topicDto.Name.Trim();

            var topic = await _db.Topics
                .FirstOrDefaultAsync(t => t.SkillId == skill.Id && t.Name.ToLower() == topicName.ToLower());
            if (topic == null)
            {
                topic = new Topic
                {
                    SkillId = skill.Id,
                    SectionId = sectionId,
                    Name = topicName,
                    SortOrder = topicDto.SortOrder,
                };
                _db.Topics.Add(topic);
                await _db.SaveChangesAsync();
                topicsCreated++;
            }
            else
            {
                // keep section/order fresh on re-sync
                if (sectionId != null) topic.SectionId = sectionId;
                topic.SortOrder = topicDto.SortOrder;
                topic.UpdatedAt = DateTime.UtcNow;
                topicsUpdated++;
            }

            // ─── Lesson (PART-1 theory) — one per concept, upsert by title ──────
            if (!string.IsNullOrWhiteSpace(topicDto.LessonContent))
            {
                var lesson = await _db.TopicLessons
                    .FirstOrDefaultAsync(l => l.TopicId == topic.Id && l.Title.ToLower() == topicName.ToLower());
                if (lesson == null)
                {
                    _db.TopicLessons.Add(new TopicLesson
                    {
                        TopicId = topic.Id,
                        Title = topicName,
                        Content = topicDto.LessonContent.Trim(),
                        SortOrder = topicDto.SortOrder,
                    });
                    lessonsCreated++;
                }
                else
                {
                    lesson.Content = topicDto.LessonContent.Trim();
                    lesson.SortOrder = topicDto.SortOrder;
                    lessonsUpdated++;
                }
            }

            // ─── Formulas — get-or-create by title within the topic ─────────────
            foreach (var f in topicDto.Formulas ?? new())
            {
                if (string.IsNullOrWhiteSpace(f.Title) || string.IsNullOrWhiteSpace(f.Formula)) continue;
                var exists = await _db.FormulaCards
                    .AnyAsync(fc => fc.TopicId == topic.Id && fc.Title.ToLower() == f.Title.Trim().ToLower());
                if (exists) continue;
                _db.FormulaCards.Add(new FormulaCard
                {
                    TopicId = topic.Id,
                    Title = f.Title.Trim(),
                    Formula = f.Formula.Trim(),
                    Description = string.IsNullOrWhiteSpace(f.Description) ? null : f.Description.Trim(),
                    SortOrder = f.SortOrder,
                });
                formulasCreated++;
            }

            // ─── Questions — dedup by normalized content hash within the topic ──
            var existingHashes = await BuildExistingQuestionHashesAsync(topic.Id);
            foreach (var q in topicDto.Questions ?? new())
            {
                if (string.IsNullOrWhiteSpace(q.Text)) continue;
                var opts = (q.Options ?? new()).Where(o => !string.IsNullOrWhiteSpace(o.Text)).ToList();
                if (opts.Count < 2)
                {
                    warnings.Add($"Question skipped (needs ≥2 options): \"{Truncate(q.Text)}\"");
                    continue;
                }
                if (!opts.Any(o => o.IsCorrect))
                {
                    warnings.Add($"Question skipped (no correct option marked): \"{Truncate(q.Text)}\"");
                    continue;
                }

                var hash = ComputeQuestionHash(q.Text, opts);
                if (!existingHashes.Add(hash))
                {
                    questionsSkipped++;
                    continue;
                }

                var difficulty = Enum.TryParse<QuestionDifficulty>(q.Difficulty, true, out var d)
                    ? d : QuestionDifficulty.Medium;

                var question = new Question
                {
                    TopicId = topic.Id,
                    Text = q.Text.Trim(),
                    Difficulty = difficulty,
                    Explanation = string.IsNullOrWhiteSpace(q.Explanation) ? null : q.Explanation.Trim(),
                    Hint = string.IsNullOrWhiteSpace(q.Hint) ? null : q.Hint.Trim(),
                    SortOrder = q.SortOrder,
                };
                _db.Questions.Add(question);
                await _db.SaveChangesAsync();

                foreach (var o in opts)
                {
                    _db.AnswerOptions.Add(new AnswerOption
                    {
                        QuestionId = question.Id,
                        Text = o.Text.Trim(),
                        IsCorrect = o.IsCorrect,
                    });
                }
                questionsCreated++;
            }

            await _db.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Ingested skill '{Skill}': +{TC} topics, +{QC} questions, {Skip} dup-skipped",
            skill.Name, topicsCreated, questionsCreated, questionsSkipped);

        return new IngestResultDto(
            skill.Id, skill.Name, skillCreated,
            topicsCreated, topicsUpdated,
            lessonsCreated, lessonsUpdated,
            formulasCreated,
            questionsCreated, questionsSkipped,
            warnings);
    }

    /// <summary>Builds the set of content hashes for questions already in a topic.</summary>
    private async Task<HashSet<string>> BuildExistingQuestionHashesAsync(int topicId)
    {
        var existing = await _db.Questions
            .Where(q => q.TopicId == topicId)
            .Select(q => new
            {
                q.Text,
                Options = q.AnswerOptions.Select(o => new IngestOptionDto(o.Text, o.IsCorrect)).ToList()
            })
            .ToListAsync();

        var set = new HashSet<string>();
        foreach (var e in existing)
            set.Add(ComputeQuestionHash(e.Text, e.Options));
        return set;
    }

    /// <summary>Stable hash over normalized question text + option set (order-independent).</summary>
    private static string ComputeQuestionHash(string text, List<IngestOptionDto> options)
    {
        var normalizedText = Normalize(text);
        var normalizedOptions = options
            .Select(o => $"{Normalize(o.Text)}|{(o.IsCorrect ? 1 : 0)}")
            .OrderBy(s => s, StringComparer.Ordinal);
        var payload = normalizedText + "\n" + string.Join("\n", normalizedOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }

    private static string Normalize(string s)
        => string.Join(' ', (s ?? string.Empty).ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string Truncate(string s, int max = 60)
        => s.Length <= max ? s : s[..max] + "…";

    private async Task<string> GenerateUniqueSkillCodeAsync(string name)
    {
        var slug = new string(name.ToUpperInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == ' ')
            .ToArray())
            .Trim()
            .Replace(' ', '_');
        if (slug.Length > 24) slug = slug[..24];
        if (string.IsNullOrWhiteSpace(slug)) slug = "SKILL";
        var code = $"SK_{slug}";

        var baseCode = code;
        var i = 1;
        while (await _db.Skills.AnyAsync(s => s.Code == code))
            code = $"{baseCode}_{i++}";
        return code;
    }
}
