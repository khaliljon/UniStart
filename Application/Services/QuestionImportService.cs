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
    private readonly IStrictDocxParserService _strictParser;
    private readonly ILlmExtractionService _llm;
    private readonly IImageUploadService _imageUpload;
    private readonly ILogger<QuestionImportService> _logger;

    public QuestionImportService(
        UniStartDbContext db,
        IFileParserService parser,
        IQuestionExtractorService extractor,
        IStrictDocxParserService strictParser,
        ILlmExtractionService llm,
        IImageUploadService imageUpload,
        ILogger<QuestionImportService> logger)
    {
        _db = db;
        _parser = parser;
        _extractor = extractor;
        _strictParser = strictParser;
        _llm = llm;
        _imageUpload = imageUpload;
        _logger = logger;
    }

    private async Task<string?> TryUploadImageAsync(ExtractedQuestion q)
    {
        if (q.ImageData is not { Length: > 0 }) return null;
        try
        {
            return await _imageUpload.UploadBytesAsync(q.ImageData, q.ImageContentType ?? "image/png");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Skipped embedded question image ({ContentType})", q.ImageContentType);
            return null;
        }
    }

    public async Task<QuestionImportJobDto> CreateImportJobAsync(
        int adminUserId, string fileName, string fileType, string examTypeCode, int? sectionId, int? topicId, string? instructions, ImportContentType contentType = ImportContentType.Questions, string language = "en")
    {
        var job = new QuestionImportJob
        {
            AdminUserId = adminUserId,
            FileName = fileName,
            FileType = fileType.ToUpper(),
            ExamTypeCode = examTypeCode,
            SectionId = sectionId,
            TopicId = topicId,
            Instructions = instructions,
            ContentType = contentType,
            Language = language,
            Status = ImportJobStatus.Pending
        };
        _db.QuestionImportJobs.Add(job);
        await _db.SaveChangesAsync();
        return MapJob(job);
    }

    public async Task ProcessImportJobAsync(int jobId, Stream fileStream, bool strictTemplate = false)
    {
        var job = await _db.QuestionImportJobs.FindAsync(jobId);
        if (job == null) return;

        try
        {
            job.Status = ImportJobStatus.Processing;
            await _db.SaveChangesAsync();

            if (job.ContentType == ImportContentType.Theory)
            {
                await ProcessTheoryAsync(job, fileStream);
                return;
            }

            List<ExtractedQuestion> extracted;

            if (strictTemplate && job.FileType.ToUpper() == "DOCX")
            {
                extracted = _strictParser.Parse(fileStream);
            }
            else
            switch (job.FileType.ToUpper())
            {
                case "PDF":
                    var pdfText = _parser.ParsePdf(fileStream);
                    extracted = _extractor.ExtractFromText(pdfText);
                    extracted = await MaybeUseLlmAsync(extracted, pdfText, job.Instructions);
                    break;
                case "DOCX":
                    var docxText = _parser.ParseDocx(fileStream);
                    extracted = _extractor.ExtractFromText(docxText);
                    extracted = await MaybeUseLlmAsync(extracted, docxText, job.Instructions);
                    break;
                case "MD":
                case "MARKDOWN":
                case "TXT":
                    var mdText = ReadAllText(fileStream);
                    extracted = _extractor.ExtractFromText(mdText);
                    extracted = await MaybeUseLlmAsync(extracted, mdText, job.Instructions);
                    break;
                case "XLSX":
                case "CSV":
                    var rows = _parser.ParseExcel(fileStream);
                    extracted = _extractor.ExtractFromRows(rows);
                    break;
                default:
                    throw new ArgumentException($"Unsupported file type: {job.FileType}");
            }

            int? defaultTopicId = null;
            if (job.TopicId.HasValue)
            {
                var topicExists = await _db.Topics.AnyAsync(t => t.Id == job.TopicId.Value);
                if (topicExists) defaultTopicId = job.TopicId.Value;
            }
            if (defaultTopicId == null && job.SectionId.HasValue)
            {
                var topic = await _db.Topics
                    .Where(t => t.SectionId == job.SectionId.Value)
                    .OrderBy(t => t.SortOrder)
                    .FirstOrDefaultAsync();
                defaultTopicId = topic?.Id;
            }

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

                var imageUrl = await TryUploadImageAsync(q);
                var draft = new ImportedQuestionDraft
                {
                    ImportJobId = jobId,
                    QuestionText = q.QuestionText,
                    OptionsJson = JsonSerializer.Serialize(q.Options),
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    ImageUrl = imageUrl,
                    TopicId = defaultTopicId,
                    Language = job.Language,
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
        if (dto.ImageUrl != null) draft.ImageUrl = dto.ImageUrl == string.Empty ? null : dto.ImageUrl;

        await _db.SaveChangesAsync();
        return MapDraft(draft);
    }

    public async Task<bool> ApproveDraftAsync(int draftId, int reviewerUserId)
    {
        var draft = await _db.ImportedQuestionDrafts.FindAsync(draftId);
        if (draft == null || draft.Status != DraftStatus.Pending) return false;

        var options = JsonSerializer.Deserialize<List<DraftOptionDto>>(draft.OptionsJson) ?? new();

        var question = new Question
        {
            TopicId = draft.TopicId ?? 0,
            Language = draft.Language,
            Text = draft.QuestionText,
            Difficulty = draft.Difficulty,
            Explanation = draft.Explanation,
            Hint = draft.Hint,
            ImageUrl = draft.ImageUrl,
            DifficultyParam = draft.IrtB,
            DiscriminationParam = draft.IrtA,
            GuessParam = draft.IrtC,
            IsMultipleChoice = options.Count(o => o.IsCorrect) > 1,
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
                Language = draft.Language,
                Text = draft.QuestionText,
                Difficulty = draft.Difficulty,
                Explanation = draft.Explanation,
                Hint = draft.Hint,
                ImageUrl = draft.ImageUrl,
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


    private async Task ProcessTheoryAsync(QuestionImportJob job, Stream fileStream)
    {
        var text = job.FileType.ToUpper() switch
        {
            "PDF" => _parser.ParsePdf(fileStream),
            "DOCX" => _parser.ParseDocx(fileStream),
            "MD" or "MARKDOWN" or "TXT" => ReadAllText(fileStream),
            _ => throw new ArgumentException($"Theory import does not support file type: {job.FileType}. Use PDF, DOCX, MD or TXT.")
        };

        int? topicId = null;
        string? topicName = null;
        if (job.TopicId.HasValue)
        {
            var t = await _db.Topics.FirstOrDefaultAsync(x => x.Id == job.TopicId.Value);
            if (t != null) { topicId = t.Id; topicName = t.Name; }
        }
        if (topicId == null && job.SectionId.HasValue)
        {
            var t = await _db.Topics
                .Where(x => x.SectionId == job.SectionId.Value)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefaultAsync();
            if (t != null) { topicId = t.Id; topicName = t.Name; }
        }
        if (topicId == null)
        {
            job.Status = ImportJobStatus.Failed;
            job.ErrorMessage = "Theory import requires a topic. Choose a topic (or a section that has at least one topic).";
            job.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        var theory = await _llm.ExtractTheoryAsync(text, job.Instructions);

        var lessonSort = await _db.TopicLessons.CountAsync(l => l.TopicId == topicId.Value);
        foreach (var l in theory.Lessons)
            _db.TopicLessons.Add(new TopicLesson { TopicId = topicId.Value, Title = l.Title, Content = l.Content, SortOrder = lessonSort++ });

        var formulaSort = await _db.FormulaCards.CountAsync(f => f.TopicId == topicId.Value);
        foreach (var f in theory.Formulas)
            _db.FormulaCards.Add(new FormulaCard { TopicId = topicId.Value, Title = f.Title, Formula = f.Formula, Description = f.Description, SortOrder = formulaSort++ });

        var cardCount = 0;
        if (theory.Flashcards.Count > 0)
        {
            var deck = await _db.FlashcardDecks.FirstOrDefaultAsync(d => d.TopicId == topicId.Value && d.IsSystem);
            if (deck == null)
            {
                deck = new FlashcardDeck
                {
                    Title = topicName ?? "Flashcards",
                    TopicId = topicId.Value,
                    ExamTypeCode = job.ExamTypeCode,
                    IsSystem = true
                };
                _db.FlashcardDecks.Add(deck);
                await _db.SaveChangesAsync();
            }
            var cardSort = await _db.Flashcards.CountAsync(c => c.DeckId == deck.Id);
            foreach (var c in theory.Flashcards)
            {
                _db.Flashcards.Add(new Flashcard { DeckId = deck.Id, Front = c.Front, Back = c.Back, SortOrder = cardSort++ });
                cardCount++;
            }
        }

        var strategySort = await _db.StrategyGuides.CountAsync(s => s.ExamTypeCode == job.ExamTypeCode);
        foreach (var s in theory.Strategies)
        {
            var wordCount = string.IsNullOrWhiteSpace(s.Content) ? 0 : s.Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            _db.StrategyGuides.Add(new StrategyGuide
            {
                ExamTypeCode = job.ExamTypeCode,
                Title = s.Title,
                Summary = s.Summary,
                Content = s.Content,
                Category = s.Category,
                EstimatedReadMinutes = Math.Max(1, wordCount / 200),
                SortOrder = strategySort++
            });
        }

        var total = theory.Lessons.Count + theory.Formulas.Count + cardCount + theory.Strategies.Count;
        job.TotalExtracted = total;
        job.TotalApproved = total;
        job.ResultSummary = $"Уроки: {theory.Lessons.Count} · Формулы: {theory.Formulas.Count} · Карточки: {cardCount} · Стратегии: {theory.Strategies.Count}";
        job.Status = total > 0 ? ImportJobStatus.Completed : ImportJobStatus.PartiallyCompleted;
        job.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Theory import job {JobId} completed: {Summary}", job.Id, job.ResultSummary);
    }

    private static string ReadAllText(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        return reader.ReadToEnd();
    }


    private QuestionImportJobDto MapJob(QuestionImportJob j) => new(        j.Id, j.FileName, j.FileType, j.ExamTypeCode, j.SectionId, j.TopicId,
        j.Status.ToString(), j.CreatedAt, j.CompletedAt,
        j.TotalExtracted, j.TotalApproved, j.TotalRejected, j.ErrorMessage,
        j.Instructions,
        j.Files?.Select(f => new ImportJobFileDto(f.Id, f.FileName, f.FileType, f.Role.ToString())).ToList(),
        j.ContentType.ToString(),
        j.ResultSummary
    );

    private static ImportedQuestionDraftDto MapDraft(ImportedQuestionDraft d)
    {
        var options = JsonSerializer.Deserialize<List<DraftOptionDto>>(d.OptionsJson) ?? new();
        return new ImportedQuestionDraftDto(
            d.Id, d.ImportJobId, d.QuestionText, options,
            d.Explanation, d.Hint, d.TopicId, d.Topic?.Name,
            d.Difficulty.ToString(), d.IrtA, d.IrtB, d.IrtC,
            d.Status.ToString(), d.Source.ToString(),
            d.CreatedAt, d.ReviewedAt, d.ImageUrl
        );
    }

    public async Task<QuestionImportJobDto> CreateMultiFileImportJobAsync(
        int adminUserId, string examTypeCode, int? sectionId, int? topicId, string? instructions, string language = "en")
    {
        var job = new QuestionImportJob
        {
            AdminUserId = adminUserId,
            FileName = "(мульти-файл)",
            FileType = "MULTI",
            ExamTypeCode = examTypeCode,
            SectionId = sectionId,
            TopicId = topicId,
            Instructions = instructions,
            Language = language,
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

            var allQuestions = new List<ExtractedQuestion>();
            var answerKeys = new Dictionary<int, string>();
            var topicSections = new List<(string Title, int StartQ, int EndQ)>();
            var fileNames = new List<string>();

            int fileIndex = 0;
            foreach (var entry in files)
            {
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
                            allQuestions.AddRange(await MaybeUseLlmAsync(regexQuestions, text, job.Instructions));
                        }
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
                        if (rows != null)
                            allQuestions.AddRange(_extractor.ExtractFromRows(rows));
                        else
                        {
                            var mixedRegex = _extractor.ExtractFromText(text);
                            allQuestions.AddRange(await MaybeUseLlmAsync(mixedRegex, text, job.Instructions));
                            var mixedKeys = _extractor.ExtractAnswerKeys(text);
                            foreach (var kv in mixedKeys)
                                answerKeys.TryAdd(kv.Key, kv.Value);
                        }
                        if (!string.IsNullOrEmpty(text))
                            topicSections.AddRange(_extractor.DetectTopicSections(text));
                        break;
                }
            }

            await _db.SaveChangesAsync();

            if (answerKeys.Count > 0)
            {
                _logger.LogInformation("Cross-matching {AnswerCount} answer keys with {QuestionCount} questions",
                    answerKeys.Count, allQuestions.Count);

                for (int i = 0; i < allQuestions.Count; i++)
                {
                    var questionNum = i + 1;
                    if (!answerKeys.TryGetValue(questionNum, out var correctLetter)) continue;

                    var q = allQuestions[i];
                    if (q.Options.Any(o => o.IsCorrect)) continue;

                    var letterIndex = "ABCD".IndexOf(correctLetter);
                    if (letterIndex < 0 || letterIndex >= q.Options.Count) continue;

                    var updatedOptions = q.Options.Select((opt, idx) =>
                        new DraftOptionDto(opt.Text, idx == letterIndex)).ToList();

                    allQuestions[i] = q with { Options = updatedOptions };
                }
            }

            var topicMap = new Dictionary<int, int?>();
            if (topicSections.Count > 0 && job.SectionId.HasValue)
            {
                var existingTopics = await _db.Topics
                    .Where(t => t.SectionId == job.SectionId.Value)
                    .ToListAsync();

                foreach (var section in topicSections)
                {
                    var existingTopic = existingTopics.FirstOrDefault(t =>
                        t.Name.Contains(section.Title, StringComparison.OrdinalIgnoreCase) ||
                        section.Title.Contains(t.Name, StringComparison.OrdinalIgnoreCase));

                    int? topicId = existingTopic?.Id;

                    if (topicId == null && section.StartQ > 0)
                    {
                        var newTopic = new Topic
                        {
                            SectionId = job.SectionId.Value,
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

            int? defaultTopicId = null;
            if (job.TopicId.HasValue)
            {
                var topicExists = await _db.Topics.AnyAsync(t => t.Id == job.TopicId.Value);
                if (topicExists) defaultTopicId = job.TopicId.Value;
            }
            if (defaultTopicId == null && job.SectionId.HasValue)
            {
                var topic = await _db.Topics
                    .Where(t => t.SectionId == job.SectionId.Value)
                    .OrderBy(t => t.SortOrder)
                    .FirstOrDefaultAsync();
                defaultTopicId = topic?.Id;
            }

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

                int? topicId = job.TopicId.HasValue && defaultTopicId == job.TopicId.Value
                    ? defaultTopicId
                    : topicMap.GetValueOrDefault(questionNum) ?? defaultTopicId;

                var imageUrl = await TryUploadImageAsync(q);
                var draft = new ImportedQuestionDraft
                {
                    ImportJobId = jobId,
                    QuestionText = q.QuestionText,
                    OptionsJson = JsonSerializer.Serialize(q.Options),
                    Explanation = q.Explanation,
                    Hint = q.Hint,
                    ImageUrl = imageUrl,
                    TopicId = topicId,
                    Language = job.Language,
                    Difficulty = difficulty,
                    IrtA = 1.0,
                    IrtB = irtB,
                    IrtC = 0.25,
                    Status = DraftStatus.Pending,
                    Source = DraftSource.Exact
                };
                _db.ImportedQuestionDrafts.Add(draft);
            }

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


    private async Task<List<ExtractedQuestion>> MaybeUseLlmAsync(List<ExtractedQuestion> regexResults, string text, string? instructions)
    {
        bool hasInstructions = !string.IsNullOrWhiteSpace(instructions);
        bool useLlm = _llm.IsConfigured && (hasInstructions || ShouldTryLlmExtraction(regexResults, text));
        if (!useLlm) return regexResults;

        _logger.LogInformation("Running LLM extraction (instructions={HasInstr}, regex={RegexCount} questions)...",
            hasInstructions, regexResults.Count);
        var llmExtracted = await _llm.ExtractQuestionsAsync(text, instructions);
        if (llmExtracted.Count == 0) return regexResults;

        if (QualityScore(llmExtracted) > QualityScore(regexResults))
        {
            _logger.LogInformation("Using LLM results: {LlmCount} questions (regex had {RegexCount})",
                llmExtracted.Count, regexResults.Count);
            return llmExtracted;
        }
        _logger.LogInformation("Keeping regex results: {RegexCount} questions (LLM had {LlmCount})",
            regexResults.Count, llmExtracted.Count);
        return regexResults;
    }

    private bool ShouldTryLlmExtraction(List<ExtractedQuestion> regexResults, string text)
    {
        if (!_llm.IsConfigured) return false;
        if (string.IsNullOrWhiteSpace(text) || text.Length < 100) return false;

        var cjkChars = text.Count(c => c >= '\u4e00' && c <= '\u9fff');
        var cjkRatio = (double)cjkChars / text.Length;
        var isCjkText = cjkRatio > 0.05;

        var withOptions = regexResults.Count(q => q.Options.Count >= 2);
        var withFullOptions = regexResults.Count(q => q.Options.Count >= 4);
        var withCorrectAnswer = regexResults.Count(q => q.Options.Any(o => o.IsCorrect));

        if (isCjkText)
        {
            _logger.LogInformation("LLM trigger: CJK text detected ({CjkRatio:P1}), {WithOptions} MCQ / {Total} total",
                cjkRatio, withOptions, regexResults.Count);
            return true;
        }

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

        if (regexResults.Count > 5 && withFullOptions < 3 && withCorrectAnswer < 2)
        {
            _logger.LogInformation("LLM trigger: {FullOpts} with 4 options, {Correct} with correct answer — quality too low",
                withFullOptions, withCorrectAnswer);
            return true;
        }

        return false;
    }

    private static double QualityScore(List<ExtractedQuestion> questions)
    {
        if (questions.Count == 0) return 0;

        double score = 0;
        foreach (var q in questions)
        {
            score += 1;

            if (q.Options.Count >= 2) score += 3;
            if (q.Options.Count >= 4) score += 1;

            if (q.Options.Any(o => o.IsCorrect)) score += 2;

            if (!string.IsNullOrWhiteSpace(q.Explanation)) score += 0.5;

            if (q.QuestionText.Length < 10) score -= 2;

            if (q.QuestionText.Length > 500) score -= 1;
        }

        return score;
    }
}
