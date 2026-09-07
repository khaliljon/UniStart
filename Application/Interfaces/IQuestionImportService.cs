using UniStart.Application.DTOs;
using UniStart.Domain.Entities;

namespace UniStart.Application.Interfaces;

public interface IFileParserService
{
    string ParsePdf(Stream fileStream);

    string ParseDocx(Stream fileStream);

    List<Dictionary<string, string>> ParseExcel(Stream fileStream);
}

public interface IQuestionExtractorService
{
    List<ExtractedQuestion> ExtractFromText(string text);

    List<ExtractedQuestion> ExtractFromRows(List<Dictionary<string, string>> rows);

    Dictionary<int, string> ExtractAnswerKeys(string text);

    List<(string Title, int StartQuestion, int EndQuestion)> DetectTopicSections(string text);
}

public record ImportFileEntry(Stream Stream, string FileName, string FileType, string Role);

public interface IQuestionImportService
{
    Task<QuestionImportJobDto> CreateImportJobAsync(int adminUserId, string fileName, string fileType, string examTypeCode, int? sectionId, int? topicId, string? instructions, ImportContentType contentType = ImportContentType.Questions, string language = "en");
    Task ProcessImportJobAsync(int jobId, Stream fileStream, bool strictTemplate = false);

    Task<QuestionImportJobDto> CreateMultiFileImportJobAsync(int adminUserId, string examTypeCode, int? sectionId, int? topicId, string? instructions, string language = "en");

    Task ProcessMultiFileImportAsync(int jobId, List<ImportFileEntry> files);

    Task<List<QuestionImportJobDto>> GetJobsAsync();
    Task<QuestionImportJobDto?> GetJobAsync(int jobId);
    Task<List<ImportedQuestionDraftDto>> GetDraftsAsync(int jobId, string? status = null);
    Task<ImportedQuestionDraftDto?> GetDraftAsync(int draftId);
    Task<ImportedQuestionDraftDto?> UpdateDraftAsync(int draftId, UpdateDraftDto dto);
    Task<bool> ApproveDraftAsync(int draftId, int reviewerUserId);
    Task<bool> RejectDraftAsync(int draftId, int reviewerUserId);
    Task<int> ApproveAllPendingAsync(int jobId, int reviewerUserId);
    Task<bool> DeleteJobAsync(int jobId);
    Task<int> DeleteAllJobsAsync();
}
