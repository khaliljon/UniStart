using System.ComponentModel.DataAnnotations;

namespace UniStart.DTOs;

/// <summary>
/// DTO для фильтрации и поиска квизов
/// </summary>
public class QuizFilterDto
{
    public string? Search { get; set; }
    public string? Subject { get; set; }
    public string? Difficulty { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// DTO для фильтрации и поиска экзаменов
/// </summary>
public class ExamFilterDto
{
    public string? Search { get; set; }
    public string? Subject { get; set; }
    public string? Difficulty { get; set; }
    
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    
    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Результат с пагинацией
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}
