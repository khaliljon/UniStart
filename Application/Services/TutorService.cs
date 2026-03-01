using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class TutorService : ITutorService
{
    private readonly UniStartDbContext _db;

    public TutorService(UniStartDbContext db)
    {
        _db = db;
    }

    public async Task<TutorListResultDto> GetTutorsAsync(
        string? search, string? exam, string? sort, bool? available, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);

        var query = _db.TutorProfiles
            .Include(tp => tp.User)
            .Where(tp => tp.User.Role == UserRole.Tutor && !tp.User.IsDeleted && !tp.User.IsBlocked)
            .AsQueryable();

        if (available == true)
            query = query.Where(tp => tp.IsAvailable);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(tp => tp.User.Name.ToLower().Contains(s)
                                   || tp.Headline.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(exam))
            query = query.Where(tp => tp.Specializations.Contains(exam));

        // Sort
        query = sort?.ToLower() switch
        {
            "reviews" => query.OrderByDescending(tp => tp.TotalReviews),
            "students" => query.OrderByDescending(tp => tp.TotalStudents),
            "price_asc" => query.OrderBy(tp => tp.HourlyRate ?? decimal.MaxValue),
            "price_desc" => query.OrderByDescending(tp => tp.HourlyRate ?? 0),
            "newest" => query.OrderByDescending(tp => tp.CreatedAt),
            _ => query.OrderByDescending(tp => tp.AverageRating)
                      .ThenByDescending(tp => tp.TotalReviews) // default: by rating
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(tp => new TutorCardDto(
                tp.UserId,
                tp.User.Name,
                tp.Headline,
                tp.Bio,
                tp.Specializations.Length > 0
                    ? tp.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>(),
                tp.AverageRating,
                tp.TotalReviews,
                tp.TotalStudents,
                tp.IsAvailable,
                tp.IsVerified,
                tp.HourlyRate,
                tp.AvatarUrl
            ))
            .ToListAsync();

        return new TutorListResultDto(
            items,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize)
        );
    }

    public async Task<TutorProfileDetailDto?> GetTutorProfileAsync(int userId)
    {
        var tp = await _db.TutorProfiles
            .Include(x => x.User)
            .Include(x => x.Schedule)
            .Include(x => x.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (tp == null) return null;

        return MapToDetail(tp);
    }

    public async Task<TutorProfileDetailDto> UpdateMyProfileAsync(int userId, UpdateTutorProfileDto dto)
    {
        var tp = await _db.TutorProfiles
            .Include(x => x.User)
            .Include(x => x.Schedule)
            .Include(x => x.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Tutor profile not found");

        if (dto.Headline != null) tp.Headline = dto.Headline;
        if (dto.Bio != null) tp.Bio = dto.Bio;
        if (dto.Experience != null) tp.Experience = dto.Experience;
        if (dto.Specializations != null) tp.Specializations = string.Join(",", dto.Specializations);
        if (dto.HourlyRate.HasValue) tp.HourlyRate = dto.HourlyRate;
        if (dto.IsAvailable.HasValue) tp.IsAvailable = dto.IsAvailable.Value;
        if (dto.ContactPreference != null && Enum.TryParse<ContactPreference>(dto.ContactPreference, true, out var cp))
            tp.ContactPreference = cp;

        await _db.SaveChangesAsync();
        return MapToDetail(tp);
    }

    public async Task<TutorProfileDetailDto> SetScheduleAsync(int userId, SetScheduleDto dto)
    {
        var tp = await _db.TutorProfiles
            .Include(x => x.User)
            .Include(x => x.Schedule)
            .Include(x => x.Reviews.OrderByDescending(r => r.CreatedAt).Take(10))
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new KeyNotFoundException("Tutor profile not found");

        // Replace all schedule slots
        _db.TutorScheduleSlots.RemoveRange(tp.Schedule);

        foreach (var slot in dto.Slots)
        {
            tp.Schedule.Add(new TutorScheduleSlot
            {
                TutorProfileId = tp.Id,
                DayOfWeek = (DayOfWeek)slot.DayOfWeek,
                StartTime = TimeOnly.Parse(slot.StartTime),
                EndTime = TimeOnly.Parse(slot.EndTime)
            });
        }

        await _db.SaveChangesAsync();
        return MapToDetail(tp);
    }

    public async Task<ReviewDto> LeaveReviewAsync(int studentId, int tutorUserId, CreateReviewDto dto)
    {
        var tp = await _db.TutorProfiles.FirstOrDefaultAsync(x => x.UserId == tutorUserId)
            ?? throw new KeyNotFoundException("Tutor not found");

        // Check if student already reviewed this tutor
        var existing = await _db.TutorReviews
            .AnyAsync(r => r.TutorProfileId == tp.Id && r.StudentId == studentId);
        if (existing)
            throw new InvalidOperationException("Вы уже оставляли отзыв этому тьютору");

        var review = new TutorReview
        {
            TutorProfileId = tp.Id,
            StudentId = studentId,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Comment = dto.Comment?.Length > 1000 ? dto.Comment[..1000] : dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _db.TutorReviews.Add(review);

        // Recalc aggregates
        var allRatings = await _db.TutorReviews
            .Where(r => r.TutorProfileId == tp.Id)
            .Select(r => r.Rating)
            .ToListAsync();
        allRatings.Add(review.Rating);

        tp.AverageRating = (decimal)allRatings.Average();
        tp.TotalReviews = allRatings.Count;

        await _db.SaveChangesAsync();

        var student = await _db.Users.FindAsync(studentId);
        return new ReviewDto(review.Id, studentId, student?.Name ?? "—", review.Rating, review.Comment, review.CreatedAt);
    }

    public async Task<List<StudentDto>> GetMyStudentsAsync(int tutorUserId)
    {
        // Get active conversations with student data
        var conversations = await _db.Conversations
            .Include(c => c.Student)
            .Where(c => c.TutorId == tutorUserId && c.Status == ConversationStatus.Active)
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .ToListAsync();

        return conversations.Select(c => new StudentDto(
            c.Student.Id,
            c.Student.Name,
            c.Student.Email,
            c.CreatedAt,
            c.LastMessageAt,
            c.LastMessagePreview
        )).ToList();
    }

    public async Task EnsureTutorProfileAsync(int userId)
    {
        var exists = await _db.TutorProfiles.AnyAsync(tp => tp.UserId == userId);
        if (!exists)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null || user.Role != UserRole.Tutor) return;

            _db.TutorProfiles.Add(new TutorProfile
            {
                UserId = userId,
                Headline = $"Тьютор {user.Name}",
                Bio = "",
                Experience = "",
                Specializations = "",
                IsAvailable = true
            });
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<PendingRequestDto>> GetPendingRequestsAsync(int tutorUserId)
    {
        var requests = await _db.Conversations
            .Include(c => c.Student)
            .Where(c => c.TutorId == tutorUserId && c.Status == ConversationStatus.Pending)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return requests.Select(c => new PendingRequestDto(
            c.Id,
            c.Student.Id,
            c.Student.Name,
            c.Student.Email,
            c.RequestMessage,
            c.CreatedAt
        )).ToList();
    }

    public async Task<AcceptDeclineResultDto> AcceptStudentAsync(int tutorUserId, int conversationId)
    {
        var conv = await _db.Conversations
            .Include(c => c.Student)
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.TutorId == tutorUserId)
            ?? throw new KeyNotFoundException("Request not found");

        if (conv.Status != ConversationStatus.Pending)
            throw new InvalidOperationException("Эту заявку уже нельзя принять");

        conv.Status = ConversationStatus.Active;

        // System message
        var tutor = await _db.Users.FindAsync(tutorUserId);
        var sysMsg = new Message
        {
            ConversationId = conversationId,
            SenderId = tutorUserId,
            Text = $"{tutor?.Name ?? "Тьютор"} принял(а) вашу заявку. Можете начать общение!",
            SentAt = DateTime.UtcNow,
            Type = MessageType.System
        };
        _db.Messages.Add(sysMsg);

        conv.LastMessagePreview = sysMsg.Text;
        conv.LastMessageAt = sysMsg.SentAt;
        conv.UnreadCountStudent++;

        // Update tutor's student count
        var tutorProfile = await _db.TutorProfiles.FirstOrDefaultAsync(tp => tp.UserId == tutorUserId);
        if (tutorProfile != null)
        {
            var studentCount = await _db.Conversations
                .CountAsync(c => c.TutorId == tutorUserId && c.Status == ConversationStatus.Active);
            tutorProfile.TotalStudents = studentCount; // already includes current after status change
        }

        await _db.SaveChangesAsync();
        return new AcceptDeclineResultDto(conversationId, "Active", sysMsg.Text);
    }

    public async Task<AcceptDeclineResultDto> DeclineStudentAsync(int tutorUserId, int conversationId, string? reason)
    {
        var conv = await _db.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.TutorId == tutorUserId)
            ?? throw new KeyNotFoundException("Request not found");

        if (conv.Status != ConversationStatus.Pending)
            throw new InvalidOperationException("Эту заявку уже нельзя отклонить");

        conv.Status = ConversationStatus.Declined;
        conv.DeclinedAt = DateTime.UtcNow;
        conv.DeclineReason = reason?.Length > 500 ? reason[..500] : reason;

        // System message
        var tutor = await _db.Users.FindAsync(tutorUserId);
        var sysText = string.IsNullOrWhiteSpace(reason)
            ? $"{tutor?.Name ?? "Тьютор"} отклонил(а) заявку"
            : $"{tutor?.Name ?? "Тьютор"} отклонил(а) заявку: {reason}";

        var sysMsg = new Message
        {
            ConversationId = conversationId,
            SenderId = tutorUserId,
            Text = sysText,
            SentAt = DateTime.UtcNow,
            Type = MessageType.System
        };
        _db.Messages.Add(sysMsg);

        conv.LastMessagePreview = sysText;
        conv.LastMessageAt = sysMsg.SentAt;
        conv.UnreadCountStudent++;

        await _db.SaveChangesAsync();
        return new AcceptDeclineResultDto(conversationId, "Declined", sysText);
    }

    public async Task<int> GetStudentIdByConversationAsync(int conversationId)
    {
        var conv = await _db.Conversations.FindAsync(conversationId);
        return conv?.StudentId ?? 0;
    }

    // ─── Mapping ────────────────────────────────────────
    private static readonly string[] DayNames = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"];

    private static TutorProfileDetailDto MapToDetail(TutorProfile tp)
    {
        return new TutorProfileDetailDto(
            tp.UserId,
            tp.User.Name,
            tp.User.Email,
            tp.Headline,
            tp.Bio,
            tp.Experience,
            tp.Specializations.Length > 0
                ? tp.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>(),
            tp.AverageRating,
            tp.TotalReviews,
            tp.TotalStudents,
            tp.IsAvailable,
            tp.IsVerified,
            tp.HourlyRate,
            tp.AvatarUrl,
            tp.ContactPreference.ToString(),
            tp.CreatedAt,
            tp.Schedule.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime).Select(s => new ScheduleSlotDto(
                s.Id,
                (int)s.DayOfWeek,
                DayNames[(int)s.DayOfWeek == 0 ? 6 : (int)s.DayOfWeek - 1],
                s.StartTime.ToString("HH:mm"),
                s.EndTime.ToString("HH:mm")
            )).ToList(),
            tp.Reviews.Select(r => new ReviewDto(
                r.Id, r.StudentId, r.Student?.Name ?? "—", r.Rating, r.Comment, r.CreatedAt
            )).ToList()
        );
    }
}
