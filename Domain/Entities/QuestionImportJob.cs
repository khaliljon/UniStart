namespace UniStart.Domain.Entities;

public class QuestionImportJob
{
    public int Id { get; set; }
    public int AdminUserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // "PDF", "DOCX", "XLSX", "CSV", "MULTI"
    public string ExamTypeCode { get; set; } = string.Empty;
    public int? SectionId { get; set; }

    /// <summary>Optional explicit target topic. When set, all extracted questions default to this topic.</summary>
    public int? TopicId { get; set; }

    /// <summary>Whether this job imports practice/mock questions or theory content (lessons, formulas, flashcards, strategies).</summary>
    public ImportContentType ContentType { get; set; } = ImportContentType.Questions;

    /// <summary>Human-readable summary of what was created (used for theory imports that apply directly).</summary>
    public string? ResultSummary { get; set; }

    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int TotalExtracted { get; set; }
    public int TotalApproved { get; set; }
    public int TotalRejected { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>Admin's context/instructions for the parser (e.g. "File A has questions, File B has answers")</summary>
    public string? Instructions { get; set; }

    // Navigation properties
    public virtual User AdminUser { get; set; } = null!;
    public virtual ICollection<ImportedQuestionDraft> Drafts { get; set; } = new List<ImportedQuestionDraft>();
    public virtual ICollection<ImportJobFile> Files { get; set; } = new List<ImportJobFile>();
}

/// <summary>Individual file within a multi-file import job</summary>
public class ImportJobFile
{
    public int Id { get; set; }
    public int ImportJobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // "PDF", "DOCX", "XLSX", "CSV"
    public FileRole Role { get; set; } = FileRole.Questions;
    public int OrderIndex { get; set; }

    // Navigation
    public virtual QuestionImportJob ImportJob { get; set; } = null!;
}

public enum FileRole
{
    Questions = 0,
    Answers = 1,
    Mixed = 2
}

public enum ImportJobStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    PartiallyCompleted = 4
}

/// <summary>What kind of content an import job ingests.</summary>
public enum ImportContentType
{
    /// <summary>Practice / mock-exam questions (default flow with draft review).</summary>
    Questions = 0,
    /// <summary>Theory: lessons, formulas, flashcards, strategies (applied directly).</summary>
    Theory = 1
}
