using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Infrastructure.Data;
using UniStart.Application.Helpers;

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

        if (dto.Headline != null) tp.Headline = InputSanitizer.Sanitize(dto.Headline)!;
        if (dto.Bio != null) tp.Bio = InputSanitizer.Sanitize(dto.Bio)!;
        if (dto.Experience != null) tp.Experience = InputSanitizer.Sanitize(dto.Experience)!;
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

    // ─── Schools ────────────────────────────────────────
    public async Task<List<TutorSchoolCardDto>> GetSchoolsAsync()
    {
        return await _db.TutorSchools
            .Where(s => s.IsActive)
            .Select(s => new TutorSchoolCardDto(
                s.Id, s.Name, s.Slug, s.Description, s.LogoUrl,
                s.InstagramUrl, s.TelegramUrl, s.WebsiteUrl,
                s.Specializations.Length > 0
                    ? s.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>(),
                s.IsPartner,
                s.Tutors.Count(t => t.User.Role == UserRole.Tutor && !t.User.IsDeleted)
            ))
            .ToListAsync();
    }

    public async Task<TutorSchoolDetailDto?> GetSchoolAsync(string slug)
    {
        var school = await _db.TutorSchools
            .Include(s => s.Tutors).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);

        if (school == null) return null;

        var tutorCards = school.Tutors
            .Where(t => t.User.Role == UserRole.Tutor && !t.User.IsDeleted && !t.User.IsBlocked)
            .Select(t => new TutorCardDto(
                t.UserId, t.User.Name, t.Headline, t.Bio,
                t.Specializations.Length > 0
                    ? t.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    : Array.Empty<string>(),
                t.AverageRating, t.TotalReviews, t.TotalStudents,
                t.IsAvailable, t.IsVerified, t.HourlyRate, t.AvatarUrl
            ))
            .ToList();

        return new TutorSchoolDetailDto(
            school.Id, school.Name, school.Slug, school.Description, school.LogoUrl,
            school.InstagramUrl, school.TelegramUrl, school.WebsiteUrl,
            school.Specializations.Length > 0
                ? school.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>(),
            school.IsPartner, tutorCards
        );
    }

    // ═══ Tutor-Student Binding ══════════════════════════════

    public async Task<InviteCodeDto> GenerateInviteCodeAsync(int tutorUserId)
    {
        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == tutorUserId)
            ?? throw new KeyNotFoundException("Tutor profile not found");

        profile.InviteCode = GenerateCode();
        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new InviteCodeDto(profile.InviteCode);
    }

    public async Task<InviteCodeDto?> GetInviteCodeAsync(int tutorUserId)
    {
        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == tutorUserId);
        if (profile?.InviteCode == null) return null;
        return new InviteCodeDto(profile.InviteCode);
    }

    public async Task<LinkResultDto> LinkStudentByCodeAsync(int studentUserId, string inviteCode)
    {
        var student = await _db.Users.FindAsync(studentUserId);
        if (student == null) return new LinkResultDto(false, "User not found");
        if (student.Role != UserRole.Student) return new LinkResultDto(false, "Only students can link to a tutor");

        // Check if already linked
        var existing = await _db.TutorStudents
            .FirstOrDefaultAsync(ts => ts.StudentUserId == studentUserId && ts.Status == TutorStudentStatus.Active);
        if (existing != null)
            return new LinkResultDto(false, "You are already linked to a tutor. Unlink first.");

        // Find tutor by code
        var profile = await _db.TutorProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.InviteCode == inviteCode && p.User.Role == UserRole.Tutor && !p.User.IsDeleted);
        if (profile == null) return new LinkResultDto(false, "Invalid invite code");

        // Create binding
        _db.TutorStudents.Add(new TutorStudent
        {
            TutorUserId = profile.UserId,
            StudentUserId = studentUserId,
            InviteCode = inviteCode,
            Status = TutorStudentStatus.Active,
            LinkedAt = DateTime.UtcNow,
        });

        // Denormalize on User for fast lookup
        student.LinkedTutorId = profile.UserId;
        student.UpdatedAt = DateTime.UtcNow;

        // Update tutor's student count
        profile.TotalStudents = await _db.TutorStudents.CountAsync(ts => ts.TutorUserId == profile.UserId && ts.Status == TutorStudentStatus.Active) + 1;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return new LinkResultDto(true, $"Successfully linked to tutor {profile.User.Name}");
    }

    public async Task<LinkResultDto> UnlinkStudentAsync(int tutorUserId, int studentUserId)
    {
        var binding = await _db.TutorStudents
            .FirstOrDefaultAsync(ts => ts.TutorUserId == tutorUserId && ts.StudentUserId == studentUserId && ts.Status == TutorStudentStatus.Active);
        if (binding == null) return new LinkResultDto(false, "Binding not found");

        binding.Status = TutorStudentStatus.Revoked;
        binding.RevokedAt = DateTime.UtcNow;

        var student = await _db.Users.FindAsync(studentUserId);
        if (student != null) { student.LinkedTutorId = null; student.UpdatedAt = DateTime.UtcNow; }

        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == tutorUserId);
        if (profile != null)
        {
            profile.TotalStudents = await _db.TutorStudents.CountAsync(ts => ts.TutorUserId == tutorUserId && ts.Status == TutorStudentStatus.Active) - 1;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return new LinkResultDto(true, "Student unlinked");
    }

    public async Task<LinkResultDto> UnlinkFromTutorAsync(int studentUserId)
    {
        var binding = await _db.TutorStudents
            .FirstOrDefaultAsync(ts => ts.StudentUserId == studentUserId && ts.Status == TutorStudentStatus.Active);
        if (binding == null) return new LinkResultDto(false, "No active tutor binding");

        return await UnlinkStudentAsync(binding.TutorUserId, studentUserId);
    }

    public async Task<List<TutorStudentDto>> GetLinkedStudentsAsync(int tutorUserId)
    {
        return await _db.TutorStudents
            .Include(ts => ts.StudentUser)
            .Where(ts => ts.TutorUserId == tutorUserId && ts.Status == TutorStudentStatus.Active)
            .OrderByDescending(ts => ts.LinkedAt)
            .Select(ts => new TutorStudentDto(
                ts.Id, ts.StudentUserId, ts.StudentUser.Name, ts.StudentUser.Email,
                ts.Status.ToString(), ts.LinkedAt, ts.RevokedAt
            ))
            .ToListAsync();
    }

    public async Task<LinkedTutorDto?> GetLinkedTutorAsync(int studentUserId)
    {
        var binding = await _db.TutorStudents
            .Include(ts => ts.TutorUser).ThenInclude(u => u.TutorProfile)
            .FirstOrDefaultAsync(ts => ts.StudentUserId == studentUserId && ts.Status == TutorStudentStatus.Active);

        if (binding?.TutorUser.TutorProfile == null) return null;

        var p = binding.TutorUser.TutorProfile;
        return new LinkedTutorDto(
            binding.TutorUserId, binding.TutorUser.Name, p.Headline,
            p.Specializations.Length > 0
                ? p.Specializations.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>(),
            p.AverageRating, p.AvatarUrl, binding.LinkedAt
        );
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var random = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[8];
        random.GetBytes(bytes);
        var code = new char[8];
        for (int i = 0; i < 8; i++)
            code[i] = chars[bytes[i] % chars.Length];
        return new string(code);
    }
}
