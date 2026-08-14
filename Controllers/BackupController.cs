using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniStart.Application.Interfaces;

namespace UniStart.Controllers;

[ApiController]
[Route("api/admin/backups")]
[Authorize(Roles = "Admin")]
public class BackupController : ControllerBase
{
    private readonly IBackupService _backups;
    private readonly ILogger<BackupController> _logger;

    public BackupController(IBackupService backups, ILogger<BackupController> logger)
    {
        _backups = backups;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult List() => Ok(_backups.ListBackups());

    [HttpPost]
    public async Task<IActionResult> Create()
    {
        try
        {
            var result = await _backups.CreateBackupAsync("manual");
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Manual backup failed");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{fileName}/download")]
    public IActionResult Download(string fileName)
    {
        var path = _backups.ResolveBackupPath(fileName);
        if (path == null) return NotFound(new { error = "Backup not found" });
        var stream = System.IO.File.OpenRead(path);
        return File(stream, "application/gzip", fileName);
    }

    [HttpDelete("{fileName}")]
    public IActionResult Delete(string fileName)
    {
        var removed = _backups.DeleteBackup(fileName);
        return removed ? NoContent() : NotFound(new { error = "Backup not found" });
    }
}
