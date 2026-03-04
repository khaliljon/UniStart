using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class QuestionImportService : IQuestionImportService
{
    private readonly UniStartDbContext _db;
    private readonly IFileParserService _parser;
    private readonly IQuestionExtractorService _extractor;
    private readonly ILlmExtractionService _llm;
    private readonly ILogger<QuestionImportService> _logger;

    public QuestionImportService(
        UniStartDbContext db,
        IFileParserService parser,
        IQuestionExtractorService extractor,
        ILlmExtractionService llm,
        ILogger<QuestionImportService> logger)
    {
        _db = db;
        _parser = parser;
        _extractor = extractor;
        _llm = llm;
        _logger = logger;
    }

    public async Task<QuestionImportJobDto> CreateImportJobAsync(
        int adminUserId, string fileName, string fileType, string examTypeCode, int? sectionId)
    {
        var job = new QuestionImportJob
        {
            AdminUserId = adminUserId,
            FileName = fileName,
            FileType = fileType.ToUpper(),
            ExamTypeCode = examTypeCode,
            SectionId = sectionId,
            Status = ImportJobStatus.Pending
        };
        _db.QuestionImportJobs.Add(job);
        await _db.SaveChangesAsync();
        return MapJob(job);
    }

    public async Task ProcessImportJobAsync(int jobId, Stream fileStream)
    {
        var job = await _db.QuestionImportJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = ImportJobStatus.Processing;
            await _db.SaveChangesAsync();

            List<ExtractedQuestion> extracted;

            switch (job.FileType.ToUpper())
            {
                case "PDF":
                    var pdfText = _parser.ParsePdf(fileStream);
                    extracted = _extractor.ExtractFromText(pdfText);
                    // LLM fallback: if regex extraction yields poor results on CJK/OCR text
                    if (ShouldTryLlmExtraction(extracted, pdfText))
                    {
                        _logger.LogInformation("Regex extraction quality is low ({Count} questions, {WithOptions} with options). Trying LLM extraction...",
                            extracted.Count, extracted.Count(q => q.Options.Count >= 2));
                        var llmExtracted = await _llm.ExtractQuestionsAsync(pdfText);
                        if (llmExtracted.Count > 0 && QualityScore(llmExtracted) > QualityScore(extracted))
                        {
                            _logger.LogInformation("LLM extraction is better: {LlmCount} vs {RegexCount} questions. Using LLM results.",
                                llmExtracted.Count, extracted.Count);
                            extracted = llmExtracted;
                        }
                    }
                    break;
                case "DOCX":
                    var docxText = _parser.ParseDocx(fileStream);
                    extracted = _extractor.ExtractFromText(docxText);
                    break;
                case "XLSX":
                case "CSV":
                    var rows = _parser.ParseExcel(fileStream);
                    extracted = _extractor.ExtractFromRows(rows);
                    break;
                default:
                    throw new ArgumentException($"Unsupported file type: {job.FileType}");
            }

            // Auto-assign topic if section is provided
            int? defaultTopicId = null;
            if (job.SectionId.HasValue)
            {
                var topic = await _db.Topics
                    .Where(t => t.SectionId == job.SectionId.Value)
                    .FirstOrDefaultAsync();
                defaultTopicId = topic?.Id;
            }

            // Save extracted questions as drafts
            foreach (var q in extracted)
            {
                var difficulty = q.Difficulty switch
                {
                    "Easy" => QuestionDifficulty.Easy,
                    "Hard" => QuestionDifficulty.Hard,
                    _ => QuestionDifficulty.Medium
                };

                var irtB = difficulty switch
                {
                    QuestionDifficulty.Easy => -1.0,
                    QuestionDifficulty.Hard => 1.0,
                    _ => 0.0
                };

                var draft = new ImportedQuestionDraft
                {
                    ImportJobId = jobId,
                    QuestionText = q.QuestionText,
                    OptionsJson = JsonSerializer.Serialize(q.Options),
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    TopicId = defaultTopicId,
                    Difficulty = difficulty,
                    IrtA = 1.0,
                    IrtB = irtB,
                    IrtC = 0.25,
                    Status = DraftStatus.Pending,
                    Source = DraftSource.Exact
                };
                _db.ImportedQuestionDrafts.Add(draft);
            }

            job.TotalExtracted = extracted.Count;
            job.Status = extracted.Count > 0 ? ImportJobStatus.Completed : ImportJobStatus.PartiallyCompleted;
            job.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Import job {JobId} completed: {Count} questions extracted from {FileName}",
                jobId, extracted.Count, job.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import job {JobId} failed", jobId);
            job.Status = ImportJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<QuestionImportJobDto>> GetJobsAsync()
    {
        var jobs = await _db.QuestionImportJobs
            .Include(j => j.Files)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();
        return jobs.Select(MapJob).ToList();
    }

    public async Task<QuestionImportJobDto?> GetJobAsync(int jobId)
    {
        var job = await _db.QuestionImportJobs
            .Include(j => j.Files)
            .FirstOrDefaultAsync(j => j.Id == jobId);
        return job != null ? MapJob(job) : null;
    }

    public async Task<List<ImportedQuestionDraftDto>> GetDraftsAsync(int jobId, string? status = null)
    {
        var query = _db.ImportedQuestionDrafts
            .Include(d => d.Topic)
            .Where(d => d.ImportJobId == jobId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<DraftStatus>(status, true, out var s))
        {
            query = query.Where(d => d.Status == s);
        }

        var drafts = await query
            .OrderBy(d => d.Id)
            .ToListAsync();
        return drafts.Select(MapDraft).ToList();
    }

    public async Task<ImportedQuestionDraftDto?> GetDraftAsync(int draftId)
    {
        var draft = await _db.ImportedQuestionDrafts
            .Include(d => d.Topic)
            .FirstOrDefaultAsync(d => d.Id == draftId);
        return draft != null ? MapDraft(draft) : null;
    }

    public async Task<ImportedQuestionDraftDto?> UpdateDraftAsync(int draftId, UpdateDraftDto dto)
    {
        var draft = await _db.ImportedQuestionDrafts
            .Include(d => d.Topic)
            .FirstOrDefaultAsync(d => d.Id == draftId);
        if (draft == null) return null;

        if (dto.QuestionText != null) draft.QuestionText = dto.QuestionText;
        if (dto.Options != null) draft.OptionsJson = JsonSerializer.Serialize(dto.Options);
        if (dto.Explanation != null) draft.Explanation = dto.Explanation;
        if (dto.Hint != null) draft.Hint = dto.Hint;
        if (dto.TopicId.HasValue) draft.TopicId = dto.TopicId;
        if (dto.Difficulty != null && Enum.TryParse<QuestionDifficulty>(dto.Difficulty, true, out var diff))
            draft.Difficulty = diff;
        if (dto.IrtA.HasValue) draft.IrtA = dto.IrtA.Value;
        if (dto.IrtB.HasValue) draft.IrtB = dto.IrtB.Value;
        if (dto.IrtC.HasValue) draft.IrtC = dto.IrtC.Value;

        await _db.SaveChangesAsync();
        return MapDraft(draft);
    }

    public async Task<bool> ApproveDraftAsync(int draftId, int reviewerUserId)
    {
        var draft = await _db.ImportedQuestionDrafts.FindAsync(draftId);
        if (draft == null || draft.Status != DraftStatus.Pending) return false;

        // Create actual Question + AnswerOptions in the database
        var options = JsonSerializer.Deserialize<List<DraftOptionDto>>(draft.OptionsJson) ?? new();

        var question = new Question
        {
            TopicId = draft.TopicId ?? 0,
            Text = draft.QuestionText,
            Difficulty = draft.Difficulty,
            Explanation = draft.Explanation,
            Hint = draft.Hint,
            DifficultyParam = draft.IrtB,
            DiscriminationParam = draft.IrtA,
            GuessParam = draft.IrtC,
        };

        if (question.TopicId == 0)
        {
            _logger.LogWarning("Draft {DraftId} has no topic assigned, cannot approve", draftId);
            return false;
        }

        _db.Questions.Add(question);
        await _db.SaveChangesAsync();

        foreach (var opt in options)
        {
            _db.AnswerOptions.Add(new AnswerOption
            {
                QuestionId = question.Id,
                Text = opt.Text,
                IsCorrect = opt.IsCorrect
            });
        }

        draft.Status = DraftStatus.Approved;
        draft.ReviewedAt = DateTime.UtcNow;
        draft.ReviewedByUserId = reviewerUserId;

        // Update job counters
        var job = await _db.QuestionImportJobs.FindAsync(draft.ImportJobId);
        if (job != null) job.TotalApproved++;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectDraftAsync(int draftId, int reviewerUserId)
    {
        var draft = await _db.ImportedQuestionDrafts.FindAsync(draftId);
        if (draft == null || draft.Status != DraftStatus.Pending) return false;

        draft.Status = DraftStatus.Rejected;
        draft.ReviewedAt = DateTime.UtcNow;
        draft.ReviewedByUserId = reviewerUserId;

        var job = await _db.QuestionImportJobs.FindAsync(draft.ImportJobId);
        if (job != null) job.TotalRejected++;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> ApproveAllPendingAsync(int jobId, int reviewerUserId)
    {
        var drafts = await _db.ImportedQuestionDrafts
            .Where(d => d.ImportJobId == jobId && d.Status == DraftStatus.Pending && d.TopicId != null)
            .ToListAsync();

        int approved = 0;
        foreach (var draft in drafts)
        {
            var options = JsonSerializer.Deserialize<List<DraftOptionDto>>(draft.OptionsJson) ?? new();
            if (options.Count < 2 || !options.Any(o => o.IsCorrect) || draft.TopicId == null)
                continue;

            var question = new Question
            {
                TopicId = draft.TopicId.Value,
                Text = draft.QuestionText,
                Difficulty = draft.Difficulty,
                Explanation = draft.Explanation,
                Hint = draft.Hint,
                DifficultyParam = draft.IrtB,
                DiscriminationParam = draft.IrtA,
                GuessParam = draft.IrtC,
            };
            _db.Questions.Add(question);
            await _db.SaveChangesAsync();

            foreach (var opt in options)
            {
                _db.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.Id,
                    Text = opt.Text,
                    IsCorrect = opt.IsCorrect
                });
            }

            draft.Status = DraftStatus.Approved;
            draft.ReviewedAt = DateTime.UtcNow;
            draft.ReviewedByUserId = reviewerUserId;
            approved++;
        }

        var job = await _db.QuestionImportJobs.FindAsync(jobId);
        if (job != null) job.TotalApproved += approved;

        await _db.SaveChangesAsync();
        return approved;
    }

    public async Task<bool> DeleteJobAsync(int jobId)
    {
        var job = await _db.QuestionImportJobs
            .Include(j => j.Drafts)
            .Include(j => j.Files)
            .FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null) return false;

        _db.ImportedQuestionDrafts.RemoveRange(job.Drafts);
        _db.ImportJobFiles.RemoveRange(job.Files);
        _db.QuestionImportJobs.Remove(job);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteAllJobsAsync()
    {
        var jobs = await _db.QuestionImportJobs
            .Include(j => j.Drafts)
            .Include(j => j.Files)
            .ToListAsync();

        foreach (var job in jobs)
        {
            _db.ImportedQuestionDrafts.RemoveRange(job.Drafts);
            _db.ImportJobFiles.RemoveRange(job.Files);
        }
        _db.QuestionImportJobs.RemoveRange(jobs);
        await _db.SaveChangesAsync();
        return jobs.Count;
    }

    // ── Mapping helpers ──────────────────────────────────────

    private QuestionImportJobDto MapJob(QuestionImportJob j) => new(
        j.Id, j.FileName, j.FileType, j.ExamTypeCode, j.SectionId,
        j.Status.ToString(), j.CreatedAt, j.CompletedAt,
        j.TotalExtracted, j.TotalApproved, j.TotalRejected, j.ErrorMessage,
        j.Instructions,
        j.Files?.Select(f => new ImportJobFileDto(f.Id, f.FileName, f.FileType, f.Role.ToString())).ToList()
    );

    private static ImportedQuestionDraftDto MapDraft(ImportedQuestionDraft d)
    {
        var options = JsonSerializer.Deserialize<List<DraftOptionDto>>(d.OptionsJson) ?? new();
        return new ImportedQuestionDraftDto(
            d.Id, d.ImportJobId, d.QuestionText, options,
            d.Explanation, d.Hint, d.TopicId, d.Topic?.Name,
            d.Difficulty.ToString(), d.IrtA, d.IrtB, d.IrtC,
            d.Status.ToString(), d.Source.ToString(),
            d.CreatedAt, d.ReviewedAt
        );
    }

    // ══════════════════════════════════════════════════════════
    //  MULTI-FILE IMPORT
    // ══════════════════════════════════════════════════════════

    public async Task<QuestionImportJobDto> CreateMultiFileImportJobAsync(
        int adminUserId, string examTypeCode, int? sectionId, string? instructions)
    {
        var job = new QuestionImportJob
        {
            AdminUserId = adminUserId,
            FileName = "(мульти-файл)",
            FileType = "MULTI",
            ExamTypeCode = examTypeCode,
            SectionId = sectionId,
            Instructions = instructions,
            Status = ImportJobStatus.Pending
        };
        _db.QuestionImportJobs.Add(job);
        await _db.SaveChangesAsync();
        return MapJob(job);
    }

    public async Task ProcessMultiFileImportAsync(int jobId, List<ImportFileEntry> files)
    {
        var job = await _db.QuestionImportJobs
            .Include(j => j.Files)
            .FirstOrDefaultAsync(j => j.Id == jobId);
        if (job == null) return;

        try
        {
            job.Status = ImportJobStatus.Processing;
            await _db.SaveChangesAsync();

            // 1. Parse each file based on its role and collect results
            var allQuestions = new List<ExtractedQuestion>();
            var answerKeys = new Dictionary<int, string>();
            var topicSections = new List<(string Title, int StartQ, int EndQ)>();
            var fileNames = new List<string>();

            int fileIndex = 0;
            foreach (var entry in files)
            {
                // Save file metadata
                var importFile = new ImportJobFile
                {
                    ImportJobId = jobId,
                    FileName = entry.FileName,
                    FileType = entry.FileType.ToUpper(),
                    Role = Enum.TryParse<FileRole>(entry.Role, true, out var role) ? role : FileRole.Mixed,
                    OrderIndex = fileIndex++
                };
                _db.ImportJobFiles.Add(importFile);
                fileNames.Add(entry.FileName);

                // Parse file content (per-file error handling)
                string text;
                List<Dictionary<string, string>>? rows = null;

                try
                {
                    switch (entry.FileType.ToUpper())
                    {
                        case "PDF":
                            text = _parser.ParsePdf(entry.Stream);
                            break;
                        case "DOCX":
                            text = _parser.ParseDocx(entry.Stream);
                            break;
                        case "XLSX":
                        case "CSV":
                            rows = _parser.ParseExcel(entry.Stream);
                            text = "";
                            break;
                        default:
                            _logger.LogWarning("Skipping unsupported file type: {FileType} for {FileName}",
                                entry.FileType, entry.FileName);
                            continue;
                    }
                }
                catch (Exception fileEx)
                {
                    _logger.LogWarning(fileEx, "Failed to parse file {FileName}, skipping", entry.FileName);
                    continue;
                }

                switch (importFile.Role)
                {
                    case FileRole.Questions:
                        if (rows != null)
                            allQuestions.AddRange(_extractor.ExtractFromRows(rows));
                        else
                        {
                            var regexQuestions = _extractor.ExtractFromText(text);
                            // LLM fallback for individual files with poor regex results
                            if (ShouldTryLlmExtraction(regexQuestions, text))
                            {
                                _logger.LogInformation("Low quality regex extraction for '{FileName}' ({Count} questions, {WithOpts} with options). Trying LLM...",
                                    entry.FileName, regexQuestions.Count, regexQuestions.Count(q => q.Options.Count >= 2));
                                var llmResult = await _llm.ExtractQuestionsAsync(text);
                                if (llmResult.Count > 0 && QualityScore(llmResult) > QualityScore(regexQuestions))
                                {
                                    _logger.LogInformation("LLM result better for '{FileName}': {LlmCount} vs {RegexCount}. Using LLM.",
                                        entry.FileName, llmResult.Count, regexQuestions.Count);
                                    allQuestions.AddRange(llmResult);
                                }
                                else
                                    allQuestions.AddRange(regexQuestions);
                            }
                            else
                                allQuestions.AddRange(regexQuestions);
                        }
                        // Also try topic detection from question files
                        if (!string.IsNullOrEmpty(text))
                            topicSections.AddRange(_extractor.DetectTopicSections(text));
                        break;

                    case FileRole.Answers:
                        if (!string.IsNullOrEmpty(text))
                        {
                            var keys = _extractor.ExtractAnswerKeys(text);
                            foreach (var kv in keys)
                                answerKeys.TryAdd(kv.Key, kv.Value);
                        }
                        // Also handle Excel answer keys
                        if (rows != null)
                        {
                            foreach (var row in rows)
                            {
                                var numStr = row.Values.FirstOrDefault(v => int.TryParse(v, out _));
                                var letterStr = row.Values.FirstOrDefault(v =>
                                    v.Length == 1 && "ABCDАБВГ".Contains(v, StringComparison.OrdinalIgnoreCase));
                                if (numStr != null && letterStr != null && int.TryParse(numStr, out var n))
                                    answerKeys.TryAdd(n, letterStr.ToUpper());
                            }
                        }
                        break;

                    case FileRole.Mixed:
                    default:
                        // Mixed: extract both questions and answers from the same file
                        if (rows != null)
                            allQuestions.AddRange(_extractor.ExtractFromRows(rows));
                        else
                        {
                            var mixedRegex = _extractor.ExtractFromText(text);
                            // LLM fallback for mixed files with poor regex
                            if (ShouldTryLlmExtraction(mixedRegex, text))
                            {
                                var llmMixed = await _llm.ExtractQuestionsAsync(text);
                                if (llmMixed.Count > 0 && QualityScore(llmMixed) > QualityScore(mixedRegex))
                                {
                                    _logger.LogInformation("LLM result better for mixed '{FileName}': {LlmCount} vs {RegexCount}",
                                        entry.FileName, llmMixed.Count, mixedRegex.Count);
                                    allQuestions.AddRange(llmMixed);
                                }
                                else
                                {
                                    allQuestions.AddRange(mixedRegex);
                                }
                            }
                            else
                            {
                                allQuestions.AddRange(mixedRegex);
                            }
                            var mixedKeys = _extractor.ExtractAnswerKeys(text);
                            foreach (var kv in mixedKeys)
                                answerKeys.TryAdd(kv.Key, kv.Value);
                        }
                        if (!string.IsNullOrEmpty(text))
                            topicSections.AddRange(_extractor.DetectTopicSections(text));
                        break;
                }
            }

            await _db.SaveChangesAsync(); // save ImportJobFiles

            // 2. Cross-match answer keys with questions (by order index = question number)
            if (answerKeys.Count > 0)
            {
                _logger.LogInformation("Cross-matching {AnswerCount} answer keys with {QuestionCount} questions",
                    answerKeys.Count, allQuestions.Count);

                for (int i = 0; i < allQuestions.Count; i++)
                {
                    var questionNum = i + 1;
                    if (!answerKeys.TryGetValue(questionNum, out var correctLetter)) continue;

                    var q = allQuestions[i];
                    // Skip if already has a correct answer
                    if (q.Options.Any(o => o.IsCorrect)) continue;

                    var letterIndex = "ABCD".IndexOf(correctLetter);
                    if (letterIndex < 0 || letterIndex >= q.Options.Count) continue;

                    // Rebuild options with the correct one marked
                    var updatedOptions = q.Options.Select((opt, idx) =>
                        new DraftOptionDto(opt.Text, idx == letterIndex)).ToList();

                    allQuestions[i] = q with { Options = updatedOptions };
                }
            }

            // 3. Auto-detect topics and create/find Topic entities
            var topicMap = new Dictionary<int, int?>(); // questionNumber → topicId
            if (topicSections.Count > 0 && job.SectionId.HasValue)
            {
                var existingTopics = await _db.Topics
                    .Where(t => t.SectionId == job.SectionId.Value)
                    .ToListAsync();

                // Find SkillId from existing topics in this section (needed for auto-creation)
                var defaultSkillId = existingTopics.FirstOrDefault()?.SkillId ?? 0;
                if (defaultSkillId == 0)
                {
                    // Fallback: find any skill
                    var skill = await _db.Skills.FirstOrDefaultAsync();
                    defaultSkillId = skill?.Id ?? 0;
                }

                foreach (var section in topicSections)
                {
                    // Try to find existing topic by name similarity
                    var existingTopic = existingTopics.FirstOrDefault(t =>
                        t.Name.Contains(section.Title, StringComparison.OrdinalIgnoreCase) ||
                        section.Title.Contains(t.Name, StringComparison.OrdinalIgnoreCase));

                    int? topicId = existingTopic?.Id;

                    if (topicId == null && section.StartQ > 0 && defaultSkillId > 0)
                    {
                        // Auto-create a new topic
                        var newTopic = new Topic
                        {
                            SectionId = job.SectionId.Value,
                            SkillId = defaultSkillId,
                            Name = section.Title,
                        };
                        _db.Topics.Add(newTopic);
                        await _db.SaveChangesAsync();
                        topicId = newTopic.Id;
                        existingTopics.Add(newTopic);
                        _logger.LogInformation("Auto-created topic '{TopicName}' for section Q{Start}-Q{End}",
                            section.Title, section.StartQ, section.EndQ);
                    }

                    if (topicId != null && section.StartQ > 0 && section.EndQ > 0)
                    {
                        for (int qn = section.StartQ; qn <= section.EndQ; qn++)
                            topicMap.TryAdd(qn, topicId);
                    }
                }
            }

            // 4. Fallback topic: first topic in the section
            int? defaultTopicId = null;
            if (job.SectionId.HasValue)
            {
                var topic = await _db.Topics
                    .Where(t => t.SectionId == job.SectionId.Value)
                    .FirstOrDefaultAsync();
                defaultTopicId = topic?.Id;
            }

            // 5. Save extracted questions as drafts
            for (int i = 0; i < allQuestions.Count; i++)
            {
                var q = allQuestions[i];
                var questionNum = i + 1;

                var difficulty = q.Difficulty switch
                {
                    "Easy" => QuestionDifficulty.Easy,
                    "Hard" => QuestionDifficulty.Hard,
                    _ => QuestionDifficulty.Medium
                };

                var irtB = difficulty switch
                {
                    QuestionDifficulty.Easy => -1.0,
                    QuestionDifficulty.Hard => 1.0,
                    _ => 0.0
                };

                // Determine topic for this question
                int? topicId = topicMap.GetValueOrDefault(questionNum) ?? defaultTopicId;

                var draft = new ImportedQuestionDraft
                {
                    ImportJobId = jobId,
                    QuestionText = q.QuestionText,
                    OptionsJson = JsonSerializer.Serialize(q.Options),
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    TopicId = topicId,
                    Difficulty = difficulty,
                    IrtA = 1.0,
                    IrtB = irtB,
                    IrtC = 0.25,
                    Status = DraftStatus.Pending,
                    Source = DraftSource.Exact
                };
                _db.ImportedQuestionDrafts.Add(draft);
            }

            // Update file name to reflect all processed files
            job.FileName = string.Join(" + ", fileNames.Take(3));
            if (fileNames.Count > 3) job.FileName += $" (+{fileNames.Count - 3})";

            job.TotalExtracted = allQuestions.Count;
            job.Status = allQuestions.Count > 0 ? ImportJobStatus.Completed : ImportJobStatus.PartiallyCompleted;
            job.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Multi-file import job {JobId} completed: {Count} questions from {FileCount} files, {AnswerKeys} answer keys matched, {Sections} topic sections detected",
                jobId, allQuestions.Count, files.Count, answerKeys.Count, topicSections.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Multi-file import job {JobId} failed", jobId);
            job.Status = ImportJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    // ═══════════════════════════════════════════════════════
    //  LLM QUALITY EVALUATION HELPERS
    // ═══════════════════════════════════════════════════════

    /// <summary>
    /// Decide whether to try LLM extraction as a fallback.
    /// Returns true when regex extraction quality appears poor (OCR-garbled text, many items without options).
    /// </summary>
    private bool ShouldTryLlmExtraction(List<ExtractedQuestion> regexResults, string text)
    {
        if (!_llm.IsConfigured) return false;
        if (string.IsNullOrWhiteSpace(text) || text.Length < 100) return false;

        // Check if text has significant CJK content (likely OCR from Chinese PDF)
        var cjkChars = text.Count(c => c >= '\u4e00' && c <= '\u9fff');
        var cjkRatio = (double)cjkChars / text.Length;
        var isCjkText = cjkRatio > 0.05; // lower threshold — even 5% CJK triggers

        // Count questions with actual MCQ options (≥2 options)
        var withOptions = regexResults.Count(q => q.Options.Count >= 2);
        var withFullOptions = regexResults.Count(q => q.Options.Count >= 4);
        var withCorrectAnswer = regexResults.Count(q => q.Options.Any(o => o.IsCorrect));

        // ALWAYS use LLM for CJK/OCR text — regex can't handle OCR artifacts reliably
        if (isCjkText)
        {
            _logger.LogInformation("LLM trigger: CJK text detected ({CjkRatio:P1}), {WithOptions} MCQ / {Total} total",
                cjkRatio, withOptions, regexResults.Count);
            return true;
        }

        // Non-CJK: use LLM when regex quality is clearly poor
        if (text.Length > 5000 && regexResults.Count < 3)
        {
            _logger.LogInformation("LLM trigger: long text ({Length} chars) with only {Count} questions", text.Length, regexResults.Count);
            return true;
        }

        if (regexResults.Count > 5 && withOptions < regexResults.Count * 0.3)
        {
            _logger.LogInformation("LLM trigger: {WithOptions}/{Total} questions have options (low ratio)", withOptions, regexResults.Count);
            return true;
        }

        // Many questions but few with 4 options and few correct answers
        if (regexResults.Count > 5 && withFullOptions < 3 && withCorrectAnswer < 2)
        {
            _logger.LogInformation("LLM trigger: {FullOpts} with 4 options, {Correct} with correct answer — quality too low",
                withFullOptions, withCorrectAnswer);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Compute a quality score for a set of extracted questions.
    /// Higher is better. Prioritizes: number of MCQ questions, correct answers marked, reasonable option counts.
    /// </summary>
    private static double QualityScore(List<ExtractedQuestion> questions)
    {
        if (questions.Count == 0) return 0;

        double score = 0;
        foreach (var q in questions)
        {
            // Base score for having a question with content
            score += 1;

            // Bonus for having MCQ options
            if (q.Options.Count >= 2) score += 3;
            if (q.Options.Count >= 4) score += 1;

            // Bonus for having a correct answer marked
            if (q.Options.Any(o => o.IsCorrect)) score += 2;

            // Bonus for having explanation
            if (!string.IsNullOrWhiteSpace(q.Explanation)) score += 0.5;

            // Penalty for very short question text (likely garbage)
            if (q.QuestionText.Length < 10) score -= 2;

            // Penalty for very long question text (likely textbook paragraph)
            if (q.QuestionText.Length > 500) score -= 1;
        }

        return score;
    }
}
