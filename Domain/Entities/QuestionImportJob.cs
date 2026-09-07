namespace UniStart.Domain.Entities;

public class QuestionImportJob
{
    public int Id { get; set; }
    public int AdminUserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string ExamTypeCode { get; set; } = string.Empty;
    public int? SectionId { get; set; }

    public int? TopicId { get; set; }

    public string Language { get; set; } = "en";

    public ImportContentType ContentType { get; set; } = ImportContentType.Questions;

    public string? ResultSummary { get; set; }

    public ImportJobStatus Status { get; set; } = ImportJobStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int TotalExtracted { get; set; }
    public int TotalApproved { get; set; }
    public int TotalRejected { get; set; }
    public string? ErrorMessage { get; set; }

    public string? Instructions { get; set; }

    public virtual User AdminUser { get; set; } = null!;
    public virtual ICollection<ImportedQuestionDraft> Drafts { get; set; } = new List<ImportedQuestionDraft>();
    public virtual ICollection<ImportJobFile> Files { get; set; } = new List<ImportJobFile>();
}

public class ImportJobFile
{
    public int Id { get; set; }
    public int ImportJobId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public FileRole Role { get; set; } = FileRole.Questions;
    public int OrderIndex { get; set; }

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

public enum ImportContentType
{
    Questions = 0,
    Theory = 1
}
