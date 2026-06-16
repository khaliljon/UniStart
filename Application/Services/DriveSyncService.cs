using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Exceptions;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

/// <summary>
/// Hangfire-driven Google Drive → content sync. Idempotent and resumable:
/// each file is tracked in <see cref="DriveSyncItem"/> by a checksum token, so a
/// re-run only re-parses files that actually changed. Parsing/ingestion reuse the
/// same pipeline as the manual /parse + /ingest endpoints.
/// </summary>
public class DriveSyncService : IDriveSyncService
{
    private readonly UniStartDbContext _db;
    private readonly IGoogleDriveService _drive;
    private readonly IStudyPackParserService _parser;
    private readonly IContentIngestionService _ingestion;
    private readonly IFileParserService _fileParser;
    private readonly ILogger<DriveSyncService> _logger;

    private const string GoogleDocMime = "application/vnd.google-apps.document";
    private const string DocxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    /// <summary>Active folder→skill mapping rules for the section being synced (loaded per run).</summary>
    private IReadOnlyList<ContentMappingRule> _rules = Array.Empty<ContentMappingRule>();

    public DriveSyncService(
        UniStartDbContext db,
        IGoogleDriveService drive,
        IStudyPackParserService parser,
        IContentIngestionService ingestion,
        IFileParserService fileParser,
        ILogger<DriveSyncService> logger)
    {
        _db = db;
        _drive = drive;
        _parser = parser;
        _ingestion = ingestion;
        _fileParser = fileParser;
        _logger = logger;
    }

    public async Task SyncFolderAsync(string rootFolderId, string examTypeCode, string examSectionName)
    {
        if (!_drive.IsConfigured)
        {
            _logger.LogError("Drive sync aborted: Google Drive is not configured.");
            return;
        }
        if (!_parser.IsConfigured)
        {
            _logger.LogError("Drive sync aborted: LLM parser is not configured.");
            return;
        }

        _logger.LogInformation("Drive sync started for folder {Folder} ({Exam}/{Section})",
            rootFolderId, examTypeCode, examSectionName);

        var nodes = await _drive.ListFilesRecursiveAsync(rootFolderId);
        _logger.LogInformation("Drive sync: discovered {Count} file(s)", nodes.Count);

        _rules = await LoadRulesAsync(examSectionName);

        var seenIds = new HashSet<string>();

        // TSA past papers come as split files under ".../TSA/questions" and
        // ".../TSA/answers". They must be PAIRED and parsed together, so pull them
        // out of the normal per-file flow and handle them as pairs afterwards.
        var tsaQuestions = nodes.Where(IsTsaQuestions).ToList();
        var tsaAnswers = nodes.Where(IsTsaAnswers).ToList();
        var normalNodes = nodes.Where(n => !IsTsaQuestions(n) && !IsTsaAnswers(n)).ToList();

        // The fixed Critical-Thinking units (e.g. "Unit 1".."Unit 7") that TSA
        // questions are distributed across: derive them from the unit files that sit
        // directly in the synced section root.
        var unitNames = normalNodes
            .Where(n => string.IsNullOrWhiteSpace(n.FolderPath)
                        && IsIngestible(n.MimeType, n.Name)
                        && System.Text.RegularExpressions.Regex.IsMatch(
                               Path.GetFileNameWithoutExtension(n.Name), @"\bunit\b",
                               System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .Select(n => DeriveSkill(n))
            .Where(s => s != null)
            .Select(s => s!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => DeriveFileOrder(s))
            .ToList();

        foreach (var node in normalNodes)
        {
            seenIds.Add(node.Id);
            try
            {
                await SyncSingleAsync(node, examTypeCode, examSectionName);
            }
            catch (LlmPaymentRequiredException ex)
            {
                // Fatal for the whole batch – the balance is exhausted, so every
                // remaining file would fail identically. Record this file and stop.
                _logger.LogError(ex, "Drive sync aborted: LLM balance exhausted while processing {Name} ({Id})", node.Name, node.Id);
                await MarkFailedAsync(node.Id, ex.Message);
                _logger.LogError("Drive sync stopped early: top up the LLM provider balance and re-run the sync.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Drive sync: failed to process {Name} ({Id})", node.Name, node.Id);
                await MarkFailedAsync(node.Id, ex.Message);
            }
        }

        // Process TSA question/answer pairs.
        foreach (var (q, a) in PairTsaFiles(tsaQuestions, tsaAnswers))
        {
            seenIds.Add(q.Id);
            if (a != null) seenIds.Add(a.Id);
            try
            {
                await SyncTsaPairAsync(q, a, examTypeCode, examSectionName, unitNames);
            }
            catch (LlmPaymentRequiredException ex)
            {
                _logger.LogError(ex, "Drive sync aborted: LLM balance exhausted while processing TSA pair {Name}", q.Name);
                await MarkFailedAsync(q.Id, ex.Message);
                _logger.LogError("Drive sync stopped early: top up the LLM provider balance and re-run the sync.");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Drive sync: failed to process TSA pair {Name} ({Id})", q.Name, q.Id);
                await MarkFailedAsync(q.Id, ex.Message);
            }
        }

        // Mark items that vanished from Drive as orphaned (do not delete content).
        var orphans = await _db.DriveSyncItems
            .Where(x => x.Status != DriveSyncStatus.Orphaned)
            .ToListAsync();
        foreach (var o in orphans.Where(o => !seenIds.Contains(o.DriveFileId)))
        {
            o.Status = DriveSyncStatus.Orphaned;
            o.LastSyncedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        _logger.LogInformation("Drive sync finished for folder {Folder}", rootFolderId);
    }

    public async Task<DriveSyncPlanDto> PreviewFolderAsync(string rootFolderId, string examTypeCode, string examSectionName)
    {
        var warnings = new List<string>();
        if (!_drive.IsConfigured)
            warnings.Add("Google Drive is not configured.");
        if (!_parser.IsConfigured)
            warnings.Add("LLM parser is not configured (sync would not run).");

        var nodes = await _drive.ListFilesRecursiveAsync(rootFolderId);
        _rules = await LoadRulesAsync(examSectionName);

        // Existing tracked items → to flag which files actually changed since last sync.
        var tracked = await _db.DriveSyncItems.ToDictionaryAsync(x => x.DriveFileId, x => x);
        bool IsChanged(DriveNode n)
        {
            var token = ComputeChecksumToken(n);
            return !(tracked.TryGetValue(n.Id, out var it)
                     && it.Status == DriveSyncStatus.Synced && it.Checksum == token);
        }

        var tsaQuestions = nodes.Where(IsTsaQuestions).ToList();
        var tsaAnswers = nodes.Where(IsTsaAnswers).ToList();
        var normalNodes = nodes.Where(n => !IsTsaQuestions(n) && !IsTsaAnswers(n)).ToList();

        var unitNames = normalNodes
            .Where(n => string.IsNullOrWhiteSpace(n.FolderPath)
                        && IsIngestible(n.MimeType, n.Name)
                        && System.Text.RegularExpressions.Regex.IsMatch(
                               Path.GetFileNameWithoutExtension(n.Name), @"\bunit\b",
                               System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .Select(n => DeriveSkill(n))
            .Where(s => s != null).Select(s => s!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => DeriveFileOrder(s))
            .ToList();

        var normalPlan = new List<DrivePlanFileDto>();
        foreach (var n in normalNodes)
        {
            var ingestible = IsIngestible(n.MimeType, n.Name);
            var skill = ingestible ? DeriveSkill(n) : null;
            var rule = MatchRule(n);
            normalPlan.Add(new DrivePlanFileDto(
                n.Id, n.Name, n.FolderPath, skill, DeriveFileOrder(n.Name),
                ingestible, ingestible && skill != null && IsChanged(n), rule?.Pattern));
            if (ingestible && skill == null)
                warnings.Add(rule?.IsIgnore == true
                    ? $"'{n.Name}' matches ignore rule '{rule.Pattern}' and will be skipped."
                    : $"'{n.Name}' could not be mapped to a skill and would be skipped.");
        }

        var tsaPlan = new List<DrivePlanTsaPairDto>();
        foreach (var (q, a) in PairTsaFiles(tsaQuestions, tsaAnswers))
        {
            var changed = IsChanged(q) || (a != null && IsChanged(a));
            tsaPlan.Add(new DrivePlanTsaPairDto(q.Name, a?.Name, changed));
            if (a == null)
                warnings.Add($"TSA questions '{q.Name}' has no matching answer-key file.");
        }

        var ingestibleCount = normalPlan.Count(p => p.Ingestible && p.MappedSkillName != null);
        var changedCount = normalPlan.Count(p => p.Changed) + tsaPlan.Count(p => p.Changed);

        return new DriveSyncPlanDto(
            nodes.Count, ingestibleCount + tsaPlan.Count, changedCount,
            unitNames, normalPlan, tsaPlan, warnings);
    }

    private async Task SyncSingleAsync(DriveNode node, string examTypeCode, string examSectionName)
    {
        var checksum = ComputeChecksumToken(node);
        var item = await _db.DriveSyncItems.FirstOrDefaultAsync(x => x.DriveFileId == node.Id);
        if (item == null)
        {
            item = new DriveSyncItem
            {
                DriveFileId = node.Id,
                Name = node.Name,
                MimeType = node.MimeType,
                FolderPath = node.FolderPath,
                CreatedAt = DateTime.UtcNow,
            };
            _db.DriveSyncItems.Add(item);
        }

        // Refresh metadata each run.
        item.Name = node.Name;
        item.MimeType = node.MimeType;
        item.FolderPath = node.FolderPath;
        item.DriveModifiedAt = node.ModifiedAt;

        // Unsupported file type → record and skip.
        if (!IsIngestible(node.MimeType, node.Name))
        {
            item.Status = DriveSyncStatus.Unmapped;
            item.ErrorMessage = "Unsupported file type; not ingested.";
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        // Hierarchy backbone = the Drive structure (NOT the LLM's free-form title):
        //   - file inside a subfolder  → Skill = that folder (e.g. "1.Units" → "Units")
        //   - file in the synced root  → Skill = the file name (e.g. "Unit 3" for
        //     Critical-Thinking units that sit directly in the section folder)
        // Anything that yields no skill is skipped as Unmapped to avoid garbage topics.
        var skill = DeriveSkill(node);
        if (skill == null)
        {
            var ignoreRule = MatchRule(node);
            item.Status = DriveSyncStatus.Unmapped;
            item.ErrorMessage = ignoreRule?.IsIgnore == true
                ? $"Ignored by mapping rule '{ignoreRule.Pattern}'."
                : "Could not map file to a skill; skipped to avoid garbage topics.";
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        // Unchanged since last successful sync → skip (idempotent / incremental).
        if (item.Status == DriveSyncStatus.Synced && item.Checksum == checksum)
        {
            item.Status = DriveSyncStatus.SkippedUnchanged;
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        var text = await ExtractTextAsync(node);
        if (string.IsNullOrWhiteSpace(text))
        {
            item.Status = DriveSyncStatus.Failed;
            item.ErrorMessage = "Empty text extracted from file.";
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        var payload = await _parser.ParseAsync(text, examTypeCode, examSectionName);
        // Force the Skill to come from the folder/filename, ignoring whatever title
        // the LLM invented. The LLM still produces the Topics/lessons/questions.
        payload = payload with { SkillName = skill };
        // Keep cross-file ordering stable: files like M1.1, M1.2 contribute topics to
        // the SAME skill, so offset each file's topic SortOrder by the file number.
        var fileOrder = DeriveFileOrder(node.Name);
        if (fileOrder > 0 && payload.Topics.Count > 0)
            payload = payload with
            {
                Topics = payload.Topics
                    .Select(t => t with { SortOrder = fileOrder * 1000 + t.SortOrder })
                    .ToList()
            };
        var result = await _ingestion.IngestAsync(payload);

        item.Status = DriveSyncStatus.Synced;
        item.Checksum = checksum;
        item.MappedSkillId = result.SkillId;
        item.MappedSkillName = result.SkillName;
        item.LastResultJson = JsonSerializer.Serialize(result);
        item.ErrorMessage = null;
        item.LastSyncedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Drive sync: '{Name}' → skill '{Skill}' (+{Q} questions)",
            node.Name, result.SkillName, result.QuestionsCreated);
    }

    /// <summary>
    /// Parses a TSA past-paper pair (questions file + answer-key file) and ingests the
    /// classified questions into the fixed Critical-Thinking units. Tracked via the
    /// questions file's <see cref="DriveSyncItem"/>; the answer file is recorded too.
    /// </summary>
    private async Task SyncTsaPairAsync(
        DriveNode q, DriveNode? a, string examTypeCode, string examSectionName, IReadOnlyList<string> unitNames)
    {
        var qItem = await GetOrCreateItemAsync(q);
        var aItem = a != null ? await GetOrCreateItemAsync(a) : null;

        if (!IsIngestible(q.MimeType, q.Name))
        {
            qItem.Status = DriveSyncStatus.Unmapped;
            qItem.ErrorMessage = "Unsupported TSA questions file type; not ingested.";
            qItem.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }

        var checksum = ComputeChecksumToken(q) + "|" + (a != null ? ComputeChecksumToken(a) : "none");
        if (qItem.Status == DriveSyncStatus.Synced && qItem.Checksum == checksum)
        {
            qItem.Status = DriveSyncStatus.SkippedUnchanged;
            qItem.LastSyncedAt = DateTime.UtcNow;
            if (aItem != null) { aItem.Status = DriveSyncStatus.SkippedUnchanged; aItem.LastSyncedAt = DateTime.UtcNow; }
            await _db.SaveChangesAsync();
            return;
        }

        var qText = await ExtractTextAsync(q);
        if (string.IsNullOrWhiteSpace(qText))
        {
            qItem.Status = DriveSyncStatus.Failed;
            qItem.ErrorMessage = "Empty text extracted from TSA questions file.";
            qItem.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return;
        }
        var aText = a != null ? await ExtractTextAsync(a) : string.Empty;
        if (a == null)
            _logger.LogWarning("TSA: no answer-key file paired with '{Name}'; answers may be guessed.", q.Name);

        var payloads = await _parser.ParseTsaPairAsync(qText, aText, examTypeCode, examSectionName, unitNames, BuildGlossary());

        int questionsCreated = 0, topicsCreated = 0;
        var skillsTouched = new List<string>();
        foreach (var payload in payloads)
        {
            var res = await _ingestion.IngestAsync(payload);
            questionsCreated += res.QuestionsCreated;
            topicsCreated += res.TopicsCreated;
            skillsTouched.Add(res.SkillName);
            await PersistClassificationsAsync(payload, examSectionName);
        }

        var summary = $"TSA → {skillsTouched.Count} unit(s): {string.Join(", ", skillsTouched.Distinct())}";
        qItem.Status = DriveSyncStatus.Synced;
        qItem.Checksum = checksum;
        qItem.MappedSkillName = Truncate(summary, 300);
        qItem.LastResultJson = JsonSerializer.Serialize(new { questionsCreated, topicsCreated, units = skillsTouched });
        qItem.ErrorMessage = null;
        qItem.LastSyncedAt = DateTime.UtcNow;
        if (aItem != null)
        {
            aItem.Status = DriveSyncStatus.Synced;
            aItem.Checksum = checksum;
            aItem.MappedSkillName = $"(answer key for {q.Name})";
            aItem.ErrorMessage = null;
            aItem.LastSyncedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        _logger.LogInformation("Drive sync: TSA pair '{Name}' → {Summary} (+{Q} questions)",
            q.Name, summary, questionsCreated);
    }

    /// <summary>Gets the tracked item for a node (or creates it) and refreshes its metadata.</summary>
    private async Task<DriveSyncItem> GetOrCreateItemAsync(DriveNode node)
    {
        var item = await _db.DriveSyncItems.FirstOrDefaultAsync(x => x.DriveFileId == node.Id);
        if (item == null)
        {
            item = new DriveSyncItem
            {
                DriveFileId = node.Id,
                Name = node.Name,
                MimeType = node.MimeType,
                FolderPath = node.FolderPath,
                CreatedAt = DateTime.UtcNow,
            };
            _db.DriveSyncItems.Add(item);
        }
        item.Name = node.Name;
        item.MimeType = node.MimeType;
        item.FolderPath = node.FolderPath;
        item.DriveModifiedAt = node.ModifiedAt;
        return item;
    }

    /// <summary>Marks a tracked item Failed with a truncated error message.</summary>
    private async Task MarkFailedAsync(string driveFileId, string message)
    {
        var item = await _db.DriveSyncItems.FirstOrDefaultAsync(x => x.DriveFileId == driveFileId);
        if (item != null)
        {
            item.Status = DriveSyncStatus.Failed;
            item.ErrorMessage = Truncate(message, 1000);
            item.LastSyncedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    private async Task<string> ExtractTextAsync(DriveNode node)
    {
        var ext = Path.GetExtension(node.Name).ToLowerInvariant();

        if (node.MimeType == GoogleDocMime || ext is ".md" or ".markdown" or ".txt")
            return await _drive.DownloadTextAsync(node.Id, node.MimeType);

        if (ext == ".pdf" || node.MimeType == "application/pdf")
        {
            var bytes = await _drive.DownloadBytesAsync(node.Id);
            using var ms = new MemoryStream(bytes);
            return _fileParser.ParsePdf(ms);
        }

        if (ext == ".docx" || node.MimeType == DocxMime)
        {
            var bytes = await _drive.DownloadBytesAsync(node.Id);
            using var ms = new MemoryStream(bytes);
            return _fileParser.ParseDocx(ms);
        }

        // Fallback: try plain text decode.
        return await _drive.DownloadTextAsync(node.Id, node.MimeType);
    }

    /// <summary>
    /// Maps a Drive file to a Skill name. An admin <see cref="ContentMappingRule"/> wins
    /// first; otherwise the name-based heuristic applies: a file inside a subfolder uses
    /// the first path segment (e.g. "1.Units" → "Units"); a file in the synced root uses
    /// its own name (e.g. "Unit 3.pdf" → "Unit 3"). Returns <c>null</c> only when nothing
    /// usable can be derived.
    /// </summary>
    private string? DeriveSkill(DriveNode node)
    {
        var rule = MatchRule(node);
        if (rule != null)
            return rule.IsIgnore ? null : rule.SkillName;
        return DeriveSkillHeuristic(node);
    }

    /// <summary>The first active mapping rule that matches this file, or null.</summary>
    private ContentMappingRule? MatchRule(DriveNode node)
    {
        if (_rules.Count == 0) return null;

        var firstSegment = string.IsNullOrWhiteSpace(node.FolderPath)
            ? null
            : node.FolderPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                  .FirstOrDefault();
        var fileName = Path.GetFileNameWithoutExtension(node.Name) ?? string.Empty;

        foreach (var r in _rules)
        {
            var hay = r.MatchType == ContentMatchType.FileName ? fileName : firstSegment;
            if (!string.IsNullOrWhiteSpace(hay)
                && hay.Contains(r.Pattern, StringComparison.OrdinalIgnoreCase))
                return r;
        }
        return null;
    }

    /// <summary>Name-based skill heuristic used when no mapping rule matches.</summary>
    private static string? DeriveSkillHeuristic(DriveNode node)
    {
        if (!string.IsNullOrWhiteSpace(node.FolderPath))
        {
            var first = node.FolderPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(first))
                return StripNumberPrefix(first);
        }

        var baseName = Path.GetFileNameWithoutExtension(node.Name)?.Trim();
        return string.IsNullOrWhiteSpace(baseName) ? null : StripNumberPrefix(baseName);
    }

    /// <summary>Loads the active mapping rules that apply to this section (null section = all).</summary>
    private async Task<IReadOnlyList<ContentMappingRule>> LoadRulesAsync(string examSectionName)
    {
        var all = await _db.ContentMappingRules
            .Where(r => r.IsActive)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id)
            .ToListAsync();
        return all
            .Where(r => string.IsNullOrWhiteSpace(r.ExamSectionName)
                        || string.Equals(r.ExamSectionName, examSectionName, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>Unit glossary (skillName → description) built from mapping rules, for the TSA classifier.</summary>
    private IReadOnlyDictionary<string, string> BuildGlossary()
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in _rules)
        {
            if (!string.IsNullOrWhiteSpace(r.Glossary) && !dict.ContainsKey(r.SkillName))
                dict[r.SkillName] = r.Glossary!.Trim();
        }
        return dict;
    }

    /// <summary>Upserts per-question TSA classifications (hash → unit/topic) for audit and reuse.</summary>
    private async Task PersistClassificationsAsync(IngestContentDto payload, string examSectionName)
    {
        foreach (var topic in payload.Topics)
        {
            if (topic.Questions == null) continue;
            foreach (var q in topic.Questions)
            {
                if (string.IsNullOrWhiteSpace(q.Text)) continue;
                var hash = HashQuestion(q.Text);
                var existing = await _db.TsaClassifications.FirstOrDefaultAsync(x => x.QuestionHash == hash);
                if (existing == null)
                {
                    _db.TsaClassifications.Add(new TsaClassification
                    {
                        QuestionHash = hash,
                        SkillName = payload.SkillName,
                        TopicName = topic.Name,
                        ExamSectionName = examSectionName,
                        QuestionPreview = Truncate(q.Text.Trim(), 500),
                        CreatedAt = DateTime.UtcNow,
                        LastSeenAt = DateTime.UtcNow,
                    });
                }
                else
                {
                    existing.SkillName = payload.SkillName;
                    existing.TopicName = topic.Name;
                    existing.ExamSectionName = examSectionName;
                    existing.LastSeenAt = DateTime.UtcNow;
                }
            }
        }
        await _db.SaveChangesAsync();
    }

    /// <summary>SHA-256 (hex) of normalized question text (trim + lowercase + collapse whitespace).</summary>
    private static string HashQuestion(string text)
    {
        var normalized = System.Text.RegularExpressions.Regex
            .Replace(text.Trim().ToLowerInvariant(), @"\s+", " ");
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes);
    }

    /// <summary>Strips a leading numbering prefix like "1.", "2 ", "3) ", "4 - ".</summary>
    private static string StripNumberPrefix(string s)
    {
        var cleaned = System.Text.RegularExpressions.Regex
            .Replace(s, @"^\s*\d+\s*[.\-)]*\s*", string.Empty)
            .Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? s.Trim() : cleaned;
    }

    /// <summary>
    /// Pulls a file-ordering number from a filename (the last digit group), so files
    /// like "M1.1" → 1, "M1.2" → 2, "Unit 3" → 3, "TSA 2015" → 2015. Used to offset
    /// topic SortOrder so multiple files in one skill keep a stable sequence.
    /// </summary>
    private static int DeriveFileOrder(string name)
    {
        var baseName = Path.GetFileNameWithoutExtension(name) ?? string.Empty;
        var matches = System.Text.RegularExpressions.Regex.Matches(baseName, @"\d+");
        if (matches.Count == 0) return 0;
        return int.TryParse(matches[^1].Value, out var n) ? n : 0;
    }

    private static bool IsTsaQuestions(DriveNode n) => IsTsaSide(n.FolderPath, "questions");
    private static bool IsTsaAnswers(DriveNode n) => IsTsaSide(n.FolderPath, "answers");

    /// <summary>True when the file lives under a ".../TSA/&lt;side&gt;" folder (case-insensitive).</summary>
    private static bool IsTsaSide(string? folderPath, string side)
    {
        if (string.IsNullOrWhiteSpace(folderPath)) return false;
        var segs = folderPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segs.Length == 0) return false;
        var last = segs[^1];
        var isSide = last.Equals(side, StringComparison.OrdinalIgnoreCase)
                     || last.Equals(side.TrimEnd('s'), StringComparison.OrdinalIgnoreCase);
        var hasTsa = segs.Any(s => s.Contains("TSA", StringComparison.OrdinalIgnoreCase));
        return isSide && hasTsa;
    }

    /// <summary>Pairs each TSA questions file with the best-matching answer-key file by normalized name.</summary>
    private static List<(DriveNode Questions, DriveNode? Answers)> PairTsaFiles(
        List<DriveNode> questions, List<DriveNode> answers)
    {
        var remaining = new List<DriveNode>(answers);
        var pairs = new List<(DriveNode, DriveNode?)>();

        foreach (var q in questions)
        {
            var qNorm = NormalizeTsaName(q.Name);
            DriveNode? best = remaining.FirstOrDefault(a => NormalizeTsaName(a.Name) == qNorm);

            if (best == null)
            {
                var qToken = MainToken(q.Name);
                if (!string.IsNullOrEmpty(qToken))
                    best = remaining.FirstOrDefault(a => MainToken(a.Name) == qToken);
            }

            if (best == null && questions.Count == 1 && remaining.Count == 1)
                best = remaining[0];

            if (best != null) remaining.Remove(best);
            pairs.Add((q, best));
        }

        return pairs;
    }

    private static string NormalizeTsaName(string name)
    {
        var s = Path.GetFileNameWithoutExtension(name).ToLowerInvariant();
        s = System.Text.RegularExpressions.Regex.Replace(
            s, @"questions?|answers?|keys?|solutions?|mark\s*scheme|\bms\b", " ");
        s = System.Text.RegularExpressions.Regex.Replace(s, @"[^a-z0-9]", string.Empty);
        return s;
    }

    /// <summary>The most identifying token of a TSA filename (a 4-digit year, else any number).</summary>
    private static string MainToken(string name)
    {
        var baseName = Path.GetFileNameWithoutExtension(name);
        var year = System.Text.RegularExpressions.Regex.Match(baseName, @"(19|20)\d{2}");
        if (year.Success) return year.Value;
        var num = System.Text.RegularExpressions.Regex.Match(baseName, @"\d+");
        return num.Success ? num.Value : string.Empty;
    }

    private static bool IsIngestible(string mimeType, string name)
    {
        if (mimeType == GoogleDocMime || mimeType == DocxMime || mimeType == "application/pdf"
            || mimeType == "text/plain" || mimeType == "text/markdown")
            return true;
        var ext = Path.GetExtension(name).ToLowerInvariant();
        return ext is ".md" or ".markdown" or ".txt" or ".pdf" or ".docx";
    }

    /// <summary>md5 for binaries; modifiedTime token for Google Docs (which have no md5).</summary>
    private static string ComputeChecksumToken(DriveNode node)
    {
        if (!string.IsNullOrWhiteSpace(node.Md5Checksum))
            return "md5:" + node.Md5Checksum;
        return "mtime:" + (node.ModifiedAt?.ToString("o") ?? "unknown");
    }

    public async Task<List<DriveSyncItemDto>> GetItemsAsync()
    {
        return await _db.DriveSyncItems
            .OrderBy(x => x.FolderPath).ThenBy(x => x.Name)
            .Select(x => new DriveSyncItemDto(
                x.Id, x.DriveFileId, x.Name, x.MimeType, x.FolderPath,
                x.Status.ToString(), x.MappedSkillName, x.MappedSkillId,
                x.ErrorMessage, x.DriveModifiedAt, x.LastSyncedAt))
            .ToListAsync();
    }

    private static string Truncate(string s, int max)
        => string.IsNullOrEmpty(s) ? s : (s.Length <= max ? s : s[..max]);
}
