using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IExamService
{
    Task<IEnumerable<ExamTypeDto>> GetAllExamsAsync();
    Task<ExamWithSectionsDto?> GetExamWithSectionsAsync(string examCode);
    Task<IEnumerable<ExamSectionDto>> GetExamSectionsAsync(string examCode);
}
