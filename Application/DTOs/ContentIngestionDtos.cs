namespace UniStart.Application.DTOs;

// ─── Content Ingestion DTOs (Variant B: Topic = concept) ────────────────────
// This is the normalized schema that an LLM/parser produces for ONE study-pack
// file. The shape maps 1:1 onto the existing entity tree:
//
//   ExamSection (by name, e.g. "Math")
//     └─ Skill (by name = file title, e.g. "Units, Measures & Compound Units")
//         └─ Topic[] (each = ONE concept, NOT "PART 1/2")
//              ├─ Lesson    → TopicLesson  (PART 1 theory for that concept, markdown)
//              ├─ Formulas  → FormulaCard[] (KaTeX)
//              └─ Questions → Question[]   (practice + NUET-level, already
//                                           classified onto this concept)
//
// Ingestion is idempotent: Skill/Topic are get-or-create by name; Questions are
// deduplicated by a content hash so re-importing the same file does not create
// duplicates.

/// <summary>Root payload for ingesting one study-pack file.</summary>
public record IngestContentDto(
    string ExamTypeCode,          // e.g. "NUET"
    string ExamSectionName,       // e.g. "Math"
    string SkillName,             // file title, e.g. "Units, Measures & Compound Units"
    List<IngestTopicDto> Topics
);

/// <summary>One concept. Becomes a Topic with its lesson, formulas and questions.</summary>
public record IngestTopicDto(
    string Name,                  // concept name, e.g. "Compound Unit: Speed"
    int SortOrder,
    string? LessonContent = null, // PART-1 theory as markdown (tables allowed)
    List<IngestFormulaDto>? Formulas = null,
    List<IngestQuestionDto>? Questions = null
);

public record IngestFormulaDto(
    string Title,
    string Formula,               // KaTeX
    string? Description = null,
    int SortOrder = 0
);

public record IngestQuestionDto(
    string Text,
    List<IngestOptionDto> Options,
    string? Explanation = null,   // the "Working" text
    string? Hint = null,
    string Difficulty = "Medium", // Easy | Medium | Hard
    int SortOrder = 0
);

public record IngestOptionDto(
    string Text,
    bool IsCorrect
);

// ─── Result / reconciliation report ─────────────────────────────────────────

/// <summary>Summary returned after ingesting a payload (what was created vs reused).</summary>
public record IngestResultDto(
    int SkillId,
    string SkillName,
    bool SkillCreated,
    int TopicsCreated,
    int TopicsUpdated,
    int LessonsCreated,
    int LessonsUpdated,
    int FormulasCreated,
    int QuestionsCreated,
    int QuestionsSkippedDuplicate,
    List<string> Warnings
);

// ─── Upload form for the /parse and /parse-and-ingest endpoints ─────────────

/// <summary>Multipart form: provide either raw <see cref="Text"/> or an uploaded <see cref="File"/>.</summary>
public class ParseStudyPackForm
{
    public string ExamTypeCode { get; set; } = string.Empty;
    public string ExamSectionName { get; set; } = string.Empty;
    public string? Text { get; set; }
    public Microsoft.AspNetCore.Http.IFormFile? File { get; set; }
}

// ─── Google Drive sync ──────────────────────────────────────────────────────

/// <summary>Request to start a Drive sync (enqueues a Hangfire job).</summary>
public record StartDriveSyncDto(
    string RootFolderId,
    string ExamTypeCode,
    string ExamSectionName
);

/// <summary>One row of the Drive sync reconciliation report.</summary>
public record DriveSyncItemDto(
    int Id,
    string DriveFileId,
    string Name,
    string MimeType,
    string? FolderPath,
    string Status,
    string? MappedSkillName,
    int? MappedSkillId,
    string? ErrorMessage,
    DateTime? DriveModifiedAt,
    DateTime? LastSyncedAt
);


