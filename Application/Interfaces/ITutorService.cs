using UniStart.Application.DTOs;

namespace UniStart.Application.Interfaces;

public interface ITutorService
{
    Task<TutorListResultDto> GetTutorsAsync(string? search, string? exam, string? sort, bool? available, int page, int pageSize);
    Task<TutorProfileDetailDto?> GetTutorProfileAsync(int userId);
    Task<TutorProfileDetailDto> UpdateMyProfileAsync(int userId, UpdateTutorProfileDto dto);
    Task<TutorProfileDetailDto> SetScheduleAsync(int userId, SetScheduleDto dto);
    Task<ReviewDto> LeaveReviewAsync(int studentId, int tutorUserId, CreateReviewDto dto);
    Task<List<StudentDto>> GetMyStudentsAsync(int tutorUserId);
    Task<List<PendingRequestDto>> GetPendingRequestsAsync(int tutorUserId);
    Task<AcceptDeclineResultDto> AcceptStudentAsync(int tutorUserId, int conversationId);
    Task<AcceptDeclineResultDto> DeclineStudentAsync(int tutorUserId, int conversationId, string? reason);
    Task<int> GetStudentIdByConversationAsync(int conversationId);
    Task EnsureTutorProfileAsync(int userId);
    Task<List<TutorSchoolCardDto>> GetSchoolsAsync();
    Task<TutorSchoolDetailDto?> GetSchoolAsync(string slug);

    // Tutor-Student binding
    Task<InviteCodeDto> GenerateInviteCodeAsync(int tutorUserId);
    Task<InviteCodeDto?> GetInviteCodeAsync(int tutorUserId);
    Task<LinkResultDto> LinkStudentByCodeAsync(int studentUserId, string inviteCode);
    Task<LinkResultDto> UnlinkStudentAsync(int tutorUserId, int studentUserId);
    Task<LinkResultDto> UnlinkFromTutorAsync(int studentUserId);
    Task<List<TutorStudentDto>> GetLinkedStudentsAsync(int tutorUserId);
    Task<LinkedTutorDto?> GetLinkedTutorAsync(int studentUserId);

    // Tutor question management (Этап 2)
    Task<TutorQuestionDetailDto> CreateQuestionAsync(int tutorUserId, CreateQuestionDto dto);
    Task<TutorQuestionDetailDto?> UpdateQuestionAsync(int tutorUserId, int questionId, UpdateQuestionDto dto);
    Task<bool> DeleteQuestionAsync(int tutorUserId, int questionId);
    Task<TutorQuestionsPageDto> GetMyQuestionsAsync(int tutorUserId, string? search, string? examType, int page, int pageSize);
    Task<TutorQuestionDetailDto?> GetQuestionByIdAsync(int tutorUserId, int questionId);
    Task<List<AdminTopicSummaryDto>> GetTopicsAsync();

    // Assignments (Этап 3)
    Task<AssignmentDetailDto> CreateAssignmentAsync(int tutorUserId, CreateAssignmentDto dto);
    Task<AssignmentDetailDto?> UpdateAssignmentAsync(int tutorUserId, int assignmentId, UpdateAssignmentDto dto);
    Task<bool> DeleteAssignmentAsync(int tutorUserId, int assignmentId);
    Task<List<AssignmentListItemDto>> GetAssignmentsAsync(int tutorUserId);
    Task<AssignmentDetailDto?> GetAssignmentAsync(int tutorUserId, int assignmentId);

    // Student-facing assignments
    Task<List<StudentAssignmentListItemDto>> GetStudentAssignmentsAsync(int studentUserId);
    Task<StudentAssignmentDetailDto?> GetStudentAssignmentAsync(int studentUserId, int assignmentId);
    Task<SubmitAssignmentAnswerResultDto> SubmitAssignmentAnswerAsync(int studentUserId, int assignmentId, SubmitAssignmentAnswerDto dto);
}
