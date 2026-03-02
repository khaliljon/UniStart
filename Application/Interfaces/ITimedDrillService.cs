using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ITimedDrillService
{
    Task<DrillResultDto> StartDrillAsync(int userId, StartDrillRequest request);
    Task<DrillQuestionDto?> GetNextDrillQuestionAsync(int drillResultId);
    Task<DrillAnswerResultDto> SubmitDrillAnswerAsync(int userId, SubmitDrillAnswerRequest request);
    Task<DrillResultDto> CompleteDrillAsync(int userId, int drillResultId);
    Task<IEnumerable<PersonalBestDto>> GetPersonalBestsAsync(int userId);
    Task<IEnumerable<DrillResultDto>> GetDrillHistoryAsync(int userId, int limit = 20);
}
