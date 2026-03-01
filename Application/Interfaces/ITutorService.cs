using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ITutorService
{
    Task<TutorListResultDto> GetTutorsAsync(string? search, string? exam, string? sort, bool? available, int page, int pageSize);
    Task<TutorProfileDetailDto?> GetTutorProfileAsync(int userId);
    Task<TutorProfileDetailDto> UpdateMyProfileAsync(int userId, UpdateTutorProfileDto dto);
    Task<TutorProfileDetailDto> SetScheduleAsync(int userId, SetScheduleDto dto);
    Task<ReviewDto> LeaveReviewAsync(int studentId, int tutorUserId, CreateReviewDto dto);
    Task<List<TutorCardDto>> GetMyStudentsAsync(int tutorUserId);
    Task EnsureTutorProfileAsync(int userId);
}
