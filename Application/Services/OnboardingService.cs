using Microsoft.EntityFrameworkCore;
using UniStart.Application.DTOs;
using UniStart.Application.Interfaces;
using UniStart.Domain.Entities;
using UniStart.Domain.Interfaces;
using UniStart.Infrastructure.Data;

namespace UniStart.Application.Services;

public class OnboardingService : IOnboardingService
{
    private readonly UniStartDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStudyPlanService _studyPlanService;

    public OnboardingService(
        UniStartDbContext context,
        IUnitOfWork unitOfWork,
        IStudyPlanService studyPlanService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _studyPlanService = studyPlanService;
    }

    public async Task<OnboardingStatusDto> GetStatusAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Check if user has an active goal
        var activeGoal = await _context.Set<StudyGoal>()
            .Include(g => g.ExamType)
            .FirstOrDefaultAsync(g => g.UserId == userId && g.IsActive);

        return new OnboardingStatusDto(
            user.HasCompletedOnboarding,
            activeGoal?.ExamTypeCode,
            activeGoal?.ExamType?.Name,
            activeGoal?.TargetDate,
            activeGoal?.TargetScore
        );
    }

    public async Task<IEnumerable<ExamTypeInfoDto>> GetExamTypesInfoAsync(string lang = "ru")
    {
        var examTypes = await _context.ExamTypes
            .Where(e => e.Code == "CSCA")
            .Include(e => e.Sections)
            .ToListAsync();

        var descriptions = new Dictionary<string, Dictionary<string, string>>
        {
            ["CSCA"] = new()
            {
                ["ru"] = "China Scholastic Competency Assessment — вступительный экзамен в университеты Китая. Оценивает академическую подготовку и уровень китайского языка.",
                ["kz"] = "China Scholastic Competency Assessment — Қытай университеттеріне қабылдау емтиханы. Академиялық дайындық пен қытай тілін бағалайды.",
                ["en"] = "China Scholastic Competency Assessment — an entrance exam for Chinese universities. Assesses academic knowledge and Chinese language proficiency.",
            }
        };

        return examTypes.Select(et =>
        {
            var sections = et.Sections.ToList();
            var minTotal = sections.Sum(s => s.MinScore);
            var maxTotal = sections.Sum(s => s.MaxScore);

            var desc = descriptions.TryGetValue(et.Code, out var langMap)
                ? langMap.GetValueOrDefault(lang, langMap.GetValueOrDefault("ru", ""))
                : "";

            return new ExamTypeInfoDto(
                et.Code,
                et.Name,
                minTotal,
                maxTotal,
                desc,
                sections.Select(s => new ExamSectionInfoDto(s.Id, s.Name, s.MinScore, s.MaxScore))
            );
        });
    }

    public async Task<OnboardingStatusDto> CompleteOnboardingAsync(int userId, CompleteOnboardingDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        if (user.HasCompletedOnboarding)
            throw new InvalidOperationException("Onboarding already completed");

        // Validate exam type against the exams that actually exist in the DB
        // (admin-managed), not a hardcoded list.
        var examType = await _context.ExamTypes
            .Include(e => e.Sections)
            .FirstOrDefaultAsync(e => e.Code == dto.ExamTypeCode);
        if (examType == null)
            throw new InvalidOperationException($"Invalid exam type: {dto.ExamTypeCode}");

        // Validate target date is in the future
        if (dto.TargetDate.Date <= DateTime.UtcNow.Date)
            throw new InvalidOperationException("Target date must be in the future");

        // Validate target score is within range
        var maxScore = examType.Sections.Sum(s => s.MaxScore);
        var minScore = examType.Sections.Sum(s => s.MinScore);
        // Only enforce a range when the exam has scored sections. Exams whose section /
        // score structure isn't defined yet (e.g. CSCA) accept any positive target.
        if (maxScore > 0 && (dto.TargetScore < minScore || dto.TargetScore > maxScore))
            throw new InvalidOperationException($"Target score must be between {minScore} and {maxScore} for {examType.Name}");

        // Deactivate any existing goals
        var existingGoals = await _context.Set<StudyGoal>()
            .Where(g => g.UserId == userId && g.IsActive)
            .ToListAsync();
        foreach (var g in existingGoals)
            g.IsActive = false;

        // Create study goal
        var goal = await _studyPlanService.CreateGoalAsync(userId, new CreateStudyGoalDto(
            dto.ExamTypeCode,
            dto.TargetDate,
            dto.TargetScore
        ));

        // Generate study plan
        try
        {
            await _studyPlanService.GeneratePlanAsync(userId, goal.Id);
        }
        catch
        {
            // Plan generation may fail if no topics, that's OK for onboarding
        }

        // Mark onboarding as complete
        user.HasCompletedOnboarding = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        return new OnboardingStatusDto(
            true,
            dto.ExamTypeCode,
            examType.Name,
            dto.TargetDate,
            dto.TargetScore
        );
    }
}
