using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

/// <summary>Parses uploaded files and extracts text content</summary>
public interface IFileParserService
{
    /// <summary>Extract raw text from a PDF file</summary>
    string ParsePdf(Stream fileStream);

    /// <summary>Extract raw text from a DOCX file</summary>
    string ParseDocx(Stream fileStream);

    /// <summary>Extract rows from an XLSX file as list of dictionaries (column→value)</summary>
    List<Dictionary<string, string>> ParseExcel(Stream fileStream);
}

/// <summary>Extracts question/answer pairs from parsed text or structured data</summary>
public interface IQuestionExtractorService
{
    /// <summary>Extract questions from free-form text (PDF/DOCX content)</summary>
    List<ExtractedQuestion> ExtractFromText(string text);

    /// <summary>Extract questions from structured Excel rows</summary>
    List<ExtractedQuestion> ExtractFromRows(List<Dictionary<string, string>> rows);

    /// <summary>Extract answer keys from an answer-only file. Returns dict: questionNumber → answerLetter</summary>
    Dictionary<int, string> ExtractAnswerKeys(string text);

    /// <summary>Detect topic/chapter sections in text. Returns list of (title, startQ, endQ)</summary>
    List<(string Title, int StartQuestion, int EndQuestion)> DetectTopicSections(string text);
}

/// <summary>Represents a single file in a multi-file upload batch</summary>
public record ImportFileEntry(Stream Stream, string FileName, string FileType, string Role);

/// <summary>Manages the question import workflow</summary>
public interface IQuestionImportService
{
    Task<QuestionImportJobDto> CreateImportJobAsync(int adminUserId, string fileName, string fileType, string examTypeCode, int? sectionId);
    Task ProcessImportJobAsync(int jobId, Stream fileStream);

    /// <summary>Create a multi-file import job with context instructions</summary>
    Task<QuestionImportJobDto> CreateMultiFileImportJobAsync(int adminUserId, string examTypeCode, int? sectionId, string? instructions);

    /// <summary>Process all files in a multi-file import job, cross-matching answers with questions</summary>
    Task ProcessMultiFileImportAsync(int jobId, List<ImportFileEntry> files);

    Task<List<QuestionImportJobDto>> GetJobsAsync();
    Task<QuestionImportJobDto?> GetJobAsync(int jobId);
    Task<List<ImportedQuestionDraftDto>> GetDraftsAsync(int jobId, string? status = null);
    Task<ImportedQuestionDraftDto?> GetDraftAsync(int draftId);
    Task<ImportedQuestionDraftDto?> UpdateDraftAsync(int draftId, UpdateDraftDto dto);
    Task<bool> ApproveDraftAsync(int draftId, int reviewerUserId);
    Task<bool> RejectDraftAsync(int draftId, int reviewerUserId);
    Task<int> ApproveAllPendingAsync(int jobId, int reviewerUserId);
}
