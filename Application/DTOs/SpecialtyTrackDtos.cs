namespace UniStart.Application.DTOs;

public record SpecialtyTrackDto(
    int Id,
    string Name,
    string? NameKz,
    string? NameEn,
    IEnumerable<string> Subjects,
    bool ConditionalChinese,
    int SortOrder,
    bool IsActive);

public record SaveSpecialtyTrackDto(
    string Name,
    string? NameKz,
    string? NameEn,
    IEnumerable<string> Subjects,
    bool ConditionalChinese,
    int SortOrder,
    bool IsActive);
