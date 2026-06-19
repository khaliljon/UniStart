using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin/question-import")]
[Authorize(Roles = "Admin")]
[ApiVersion("1.0")]
public class QuestionImportController : ControllerBase
{
    private readonly IQuestionImportService _importService;
    private readonly ILogger<QuestionImportController> _logger;

    public QuestionImportController(IQuestionImportService importService, ILogger<QuestionImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>Upload a file (PDF/DOCX/XLSX/CSV/MD) and start question or theory extraction</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)] // 50 MB
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string examTypeCode,
        [FromForm] int? sectionId = null,
        [FromForm] int? topicId = null,
        [FromForm] string? instructions = null,
        [FromForm] string? contentType = null)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded" });

        var ext = Path.GetExtension(file.FileName).ToLower();
        var fileType = ext switch
        {
            ".pdf" => "PDF",
            ".docx" => "DOCX",
            ".xlsx" => "XLSX",
            ".csv" => "CSV",
            ".md" => "MD",
            ".markdown" => "MD",
            ".txt" => "TXT",
            _ => (string?)null
        };

        if (fileType == null)
            return BadRequest(new { error = $"Unsupported file format: {ext}. Supported: PDF, DOCX, XLSX, CSV, MD, TXT" });

        var importContentType = string.Equals(contentType, "theory", StringComparison.OrdinalIgnoreCase)
            ? ImportContentType.Theory
            : ImportContentType.Questions;

        if (importContentType == ImportContentType.Theory && fileType is "XLSX" or "CSV")
            return BadRequest(new { error = "Theory import supports text files only: PDF, DOCX, MD, TXT." });

        var adminId = GetAdminId();
        var job = await _importService.CreateImportJobAsync(adminId, file.FileName, fileType, examTypeCode, sectionId, topicId, instructions, importContentType);

        // Process synchronously for now (can be moved to Hangfire for large files)
        using var stream = file.OpenReadStream();
        await _importService.ProcessImportJobAsync(job.Id, stream);

        // Reload to get updated status
        var updated = await _importService.GetJobAsync(job.Id);
        return Ok(updated);
    }

    /// <summary>Upload multiple files (questions + answers) with context instructions for cross-matching</summary>
    [HttpPost("upload-batch")]
    [RequestSizeLimit(100_000_000)] // 100 MB for multi-file
    public async Task<IActionResult> UploadBatch(
        List<IFormFile> files,
        [FromForm] string examTypeCode,
        [FromForm] string fileRoles,  // comma-separated: "Questions,Answers,Mixed"
        [FromForm] int? sectionId = null,
        [FromForm] int? topicId = null,
        [FromForm] string? instructions = null)
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No files uploaded" });

        var roles = fileRoles?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (roles.Length != files.Count)
            return BadRequest(new { error = $"fileRoles count ({roles.Length}) must match files count ({files.Count})" });

        var supportedExts = new HashSet<string> { ".pdf", ".docx", ".xlsx", ".csv" };
        foreach (var f in files)
        {
            var ext = Path.GetExtension(f.FileName).ToLower();
            if (!supportedExts.Contains(ext))
                return BadRequest(new { error = $"Unsupported file format: {ext} ({f.FileName}). Supported: PDF, DOCX, XLSX, CSV" });
        }

        var adminId = GetAdminId();
        var job = await _importService.CreateMultiFileImportJobAsync(adminId, examTypeCode, sectionId, topicId, instructions);

        // Build file entries
        var entries = new List<ImportFileEntry>();
        var streams = new List<Stream>();
        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];
                var ext = Path.GetExtension(f.FileName).ToLower().TrimStart('.');
                var fileType = ext.ToUpper();
                var stream = f.OpenReadStream();
                streams.Add(stream);
                entries.Add(new ImportFileEntry(stream, f.FileName, fileType, roles[i]));
            }

            await _importService.ProcessMultiFileImportAsync(job.Id, entries);
        }
        finally
        {
            foreach (var s in streams) s.Dispose();
        }

        var updated = await _importService.GetJobAsync(job.Id);
        return Ok(updated);
    }

    /// <summary>List all import jobs</summary>
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _importService.GetJobsAsync();
        return Ok(jobs);
    }

    /// <summary>Get a specific import job</summary>
    [HttpGet("jobs/{jobId:int}")]
    public async Task<IActionResult> GetJob(int jobId)
    {
        var job = await _importService.GetJobAsync(jobId);
        if (job == null) return NotFound();
        return Ok(job);
    }

    /// <summary>Get draft questions from an import job</summary>
    [HttpGet("jobs/{jobId:int}/drafts")]
    public async Task<IActionResult> GetDrafts(int jobId, [FromQuery] string? status = null)
    {
        var drafts = await _importService.GetDraftsAsync(jobId, status);
        return Ok(drafts);
    }

    /// <summary>Get a specific draft</summary>
    [HttpGet("drafts/{draftId:int}")]
    public async Task<IActionResult> GetDraft(int draftId)
    {
        var draft = await _importService.GetDraftAsync(draftId);
        if (draft == null) return NotFound();
        return Ok(draft);
    }

    /// <summary>Update a draft question (edit text, options, topic, etc.)</summary>
    [HttpPut("drafts/{draftId:int}")]
    public async Task<IActionResult> UpdateDraft(int draftId, [FromBody] UpdateDraftDto dto)
    {
        var result = await _importService.UpdateDraftAsync(draftId, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>Approve a draft → creates actual Question + AnswerOptions in DB</summary>
    [HttpPut("drafts/{draftId:int}/approve")]
    public async Task<IActionResult> ApproveDraft(int draftId)
    {
        var adminId = GetAdminId();
        var ok = await _importService.ApproveDraftAsync(draftId, adminId);
        if (!ok) return BadRequest(new { error = "Cannot approve: draft not found, already reviewed, or no topic assigned" });
        return Ok(new { message = "Draft approved and question created" });
    }

    /// <summary>Reject a draft</summary>
    [HttpPut("drafts/{draftId:int}/reject")]
    public async Task<IActionResult> RejectDraft(int draftId)
    {
        var adminId = GetAdminId();
        var ok = await _importService.RejectDraftAsync(draftId, adminId);
        if (!ok) return BadRequest(new { error = "Cannot reject: draft not found or already reviewed" });
        return Ok(new { message = "Draft rejected" });
    }

    /// <summary>Approve all pending drafts in a job (that have a topic assigned)</summary>
    [HttpPost("jobs/{jobId:int}/approve-all")]
    public async Task<IActionResult> ApproveAll(int jobId)
    {
        var adminId = GetAdminId();
        var approved = await _importService.ApproveAllPendingAsync(jobId, adminId);
        return Ok(new { approved, message = $"{approved} questions approved and created" });
    }

    /// <summary>Delete a single import job and all its drafts</summary>
    [HttpDelete("jobs/{jobId:int}")]
    public async Task<IActionResult> DeleteJob(int jobId)
    {
        var ok = await _importService.DeleteJobAsync(jobId);
        if (!ok) return NotFound();
        return Ok(new { message = "Job deleted" });
    }

    /// <summary>Delete ALL import jobs (clear history)</summary>
    [HttpDelete("jobs")]
    public async Task<IActionResult> DeleteAllJobs()
    {
        var deleted = await _importService.DeleteAllJobsAsync();
        return Ok(new { deleted, message = $"{deleted} jobs deleted" });
    }

    private int GetAdminId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst("sub")?.Value;
        return int.Parse(idClaim ?? "0");
    }
}
