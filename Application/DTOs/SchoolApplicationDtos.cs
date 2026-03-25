using System.ComponentModel.DataAnnotations;

namespace UniStart.Application.DTOs;

public record CreateSchoolApplicationDto(
    [Required, MaxLength(200)] string ContactName,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [MaxLength(30)] string? Phone,
    [Required, MaxLength(300)] string SchoolName,
    [MaxLength(1000)] string? Message
);

public record SchoolApplicationDto(
    int Id,
    string ContactName,
    string Email,
    string? Phone,
    string SchoolName,
    string? Message,
    string Status,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    int? ReviewedByUserId
);

public record UpdateSchoolApplicationStatusDto(
    [Required] string Status // "Approved" or "Rejected"
);
