using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface IOnboardingService
{
    Task<OnboardingStatusDto> GetStatusAsync(int userId);
    Task<IEnumerable<ExamTypeInfoDto>> GetExamTypesInfoAsync(string lang = "ru");
    Task<OnboardingStatusDto> CompleteOnboardingAsync(int userId, CompleteOnboardingDto dto);
}
