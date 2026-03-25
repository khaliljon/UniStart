using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Controllers;

[ApiController]
[Route("api/applications")]
[ApiVersion("1.0")]
public class ApplicationsController : ControllerBase
{
    private readonly UniStartDbContext _db;

    public ApplicationsController(UniStartDbContext db)
    {
        _db = db;
    }

    [HttpPost("school")]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitSchoolApplication([FromBody] CreateSchoolApplicationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var application = new SchoolApplication
        {
            ContactName = dto.ContactName.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            Phone = dto.Phone?.Trim(),
            SchoolName = dto.SchoolName.Trim(),
            Message = dto.Message?.Trim(),
            Status = SchoolApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.SchoolApplications.Add(application);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Application submitted successfully" });
    }
}
