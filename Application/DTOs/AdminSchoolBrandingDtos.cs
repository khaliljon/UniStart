namespace UniStart.Application.DTOs;

// Admin-side school branding / white-label editor DTOs

public record AdminSchoolBrandingDto(
    int Id,
    string Name,
    string Slug,
    string? Subdomain,
    string? NavbarTitle,
    string Description,
    string? DescriptionEn,
    string? DescriptionKz,
    string? LogoUrl,
    string? WebsiteUrl,
    string? InstagramUrl,
    string? TelegramUrl,
    string Specializations,
    string? PrimaryColor,
    string? PrimaryHoverColor,
    string? AccentColor,
    bool IsActive,
    bool IsApproved,
    int? OwnerUserId
);

public record AdminCreateSchoolDto(
    string Name,
    string? Subdomain = null,
    string? Description = null,
    string? Specializations = null,
    string? NavbarTitle = null,
    string? LogoUrl = null,
    string? PrimaryColor = null,
    string? PrimaryHoverColor = null,
    string? AccentColor = null
);

public record AdminUpdateSchoolBrandingDto(
    string? Name = null,
    string? Subdomain = null,
    string? NavbarTitle = null,
    string? Description = null,
    string? DescriptionEn = null,
    string? DescriptionKz = null,
    string? LogoUrl = null,
    string? WebsiteUrl = null,
    string? InstagramUrl = null,
    string? TelegramUrl = null,
    string? Specializations = null,
    string? PrimaryColor = null,
    string? PrimaryHoverColor = null,
    string? AccentColor = null
);
