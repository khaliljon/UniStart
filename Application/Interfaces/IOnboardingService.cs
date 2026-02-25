using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IOnboardingService
{
    Task<OnboardingStatusDto> GetStatusAsync(int userId);
    Task<IEnumerable<ExamTypeInfoDto>> GetExamTypesInfoAsync();
    Task<OnboardingStatusDto> CompleteOnboardingAsync(int userId, CompleteOnboardingDto dto);
}
