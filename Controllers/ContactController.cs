using Microsoft.AspNetCore.Mvc;
using UniStart.Application.Interfaces;
using Asp.Versioning;

namespace UniStart.Controllers;

[ApiController]
[Route("api/contact")]
[ApiVersion("1.0")]
public class ContactController : ControllerBase
{
    private readonly IEmailService _email;
    private const string AdminEmail = "unistart.kz@gmail.com";

    public ContactController(IEmailService email) => _email = email;

    [HttpPost]
    public async Task<IActionResult> SendContactForm([FromBody] ContactFormDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Message))
            return BadRequest(new { error = "All fields are required" });

        if (dto.Name.Length > 200 || dto.Email.Length > 200 || dto.Message.Length > 5000)
            return BadRequest(new { error = "Field too long" });

        await _email.SendContactFormAsync(AdminEmail, dto.Name, dto.Email, dto.Message);
        return Ok(new { sent = true });
    }
}

public record ContactFormDto(string Name, string Email, string Message);
