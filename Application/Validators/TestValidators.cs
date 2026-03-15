using FluentValidation;
using UniStart.Application.DTOs;

namespace UniStart.Application.Validators;

// ═══════════════════════════════════════════════════════════
//  START TEST SESSION
// ═══════════════════════════════════════════════════════════

public class StartTestSessionDtoValidator : AbstractValidator<StartTestSessionDto>
{
    public StartTestSessionDtoValidator()
    {
        RuleFor(x => x.ExamTypeCodes)
            .NotNull().WithMessage("ExamTypeCodes are required.")
            .Must(codes => codes is not null && codes.Length > 0)
            .WithMessage("At least one ExamTypeCode is required.")
            .Must(codes => codes is not null && codes.Length <= 5)
            .WithMessage("No more than 5 ExamTypeCodes allowed.")
            .Must(codes => codes is not null && codes.All(c => !string.IsNullOrWhiteSpace(c)))
            .WithMessage("ExamTypeCodes must not contain empty values.");

        RuleFor(x => x.SectionId)
            .GreaterThan(0).WithMessage("SectionId must be a positive integer.")
            .When(x => x.SectionId.HasValue);

        RuleFor(x => x.TopicId)
            .GreaterThan(0).WithMessage("TopicId must be a positive integer.")
            .When(x => x.TopicId.HasValue);
    }
}

// ═══════════════════════════════════════════════════════════
//  CREATE STUDY GOAL
// ═══════════════════════════════════════════════════════════

public class CreateStudyGoalDtoValidator : AbstractValidator<CreateStudyGoalDto>
{
    public CreateStudyGoalDtoValidator()
    {
        RuleFor(x => x.ExamTypeCode)
            .NotEmpty().WithMessage("ExamTypeCode is required.")
            .MaximumLength(20).WithMessage("ExamTypeCode must not exceed 20 characters.");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("TargetDate must be in the future.");

        RuleFor(x => x.TargetScore)
            .GreaterThan(0).WithMessage("TargetScore must be positive.")
            .LessThanOrEqualTo(2400).WithMessage("TargetScore must not exceed 2400.");
    }
}

// ═══════════════════════════════════════════════════════════
//  UPDATE STUDY GOAL
// ═══════════════════════════════════════════════════════════

public class UpdateStudyGoalDtoValidator : AbstractValidator<UpdateStudyGoalDto>
{
    public UpdateStudyGoalDtoValidator()
    {
        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("TargetDate must be in the future.")
            .When(x => x.TargetDate.HasValue);

        RuleFor(x => x.TargetScore)
            .GreaterThan(0).WithMessage("TargetScore must be positive.")
            .LessThanOrEqualTo(2400).WithMessage("TargetScore must not exceed 2400.")
            .When(x => x.TargetScore.HasValue);
    }
}

// ═══════════════════════════════════════════════════════════
//  COMPLETE STUDY PLAN ENTRY
// ═══════════════════════════════════════════════════════════

public class CompleteEntryDtoValidator : AbstractValidator<CompleteEntryDto>
{
    public CompleteEntryDtoValidator()
    {
        RuleFor(x => x.QuestionsAnswered)
            .GreaterThanOrEqualTo(0).WithMessage("QuestionsAnswered cannot be negative.");

        RuleFor(x => x.CorrectAnswers)
            .GreaterThanOrEqualTo(0).WithMessage("CorrectAnswers cannot be negative.")
            .LessThanOrEqualTo(x => x.QuestionsAnswered)
            .WithMessage("CorrectAnswers cannot exceed QuestionsAnswered.");
    }
}

// ═══════════════════════════════════════════════════════════
//  MOCK EXAM — SUBMIT ANSWER
// ═══════════════════════════════════════════════════════════

public class MockExamSubmitAnswerDtoValidator : AbstractValidator<MockExamSubmitAnswerDto>
{
    public MockExamSubmitAnswerDtoValidator()
    {
        RuleFor(x => x.QuestionId)
            .GreaterThan(0).WithMessage("QuestionId must be a positive integer.");

        RuleFor(x => x.SelectedOptionId)
            .GreaterThan(0).WithMessage("SelectedOptionId must be a positive integer.");

        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("TimeSpentSeconds cannot be negative.")
            .LessThanOrEqualTo(7200).WithMessage("TimeSpentSeconds cannot exceed 7200 (2 hours).")
            .When(x => x.TimeSpentSeconds.HasValue);
    }
}

// ═══════════════════════════════════════════════════════════
//  COMPLETE ONBOARDING
// ═══════════════════════════════════════════════════════════

public class CompleteOnboardingDtoValidator : AbstractValidator<CompleteOnboardingDto>
{
    public CompleteOnboardingDtoValidator()
    {
        RuleFor(x => x.ExamTypeCode)
            .NotEmpty().WithMessage("ExamTypeCode is required.")
            .MaximumLength(20).WithMessage("ExamTypeCode must not exceed 20 characters.");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("TargetDate must be in the future.");

        RuleFor(x => x.TargetScore)
            .GreaterThan(0).WithMessage("TargetScore must be positive.")
            .LessThanOrEqualTo(2400).WithMessage("TargetScore must not exceed 2400.");
    }
}

// ═══════════════════════════════════════════════════════════
//  DIAGNOSTIC — ANSWER
// ═══════════════════════════════════════════════════════════

public class DiagnosticAnswerDtoValidator : AbstractValidator<DiagnosticAnswerDto>
{
    public DiagnosticAnswerDtoValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("SessionId must be a positive integer.");

        RuleFor(x => x.QuestionId)
            .GreaterThan(0).WithMessage("QuestionId must be a positive integer.");

        RuleFor(x => x.AnswerOptionId)
            .GreaterThan(0).WithMessage("AnswerOptionId must be a positive integer.");

        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("TimeSpentSeconds cannot be negative.")
            .LessThanOrEqualTo(7200).WithMessage("TimeSpentSeconds cannot exceed 7200 (2 hours).")
            .When(x => x.TimeSpentSeconds.HasValue);
    }
}

// ═══════════════════════════════════════════════════════════
//  START DIAGNOSTIC
// ═══════════════════════════════════════════════════════════

public class StartDiagnosticDtoValidator : AbstractValidator<StartDiagnosticDto>
{
    public StartDiagnosticDtoValidator()
    {
        RuleFor(x => x.ExamTypeCode)
            .NotEmpty().WithMessage("ExamTypeCode is required.")
            .MaximumLength(20).WithMessage("ExamTypeCode must not exceed 20 characters.");
    }
}

// ═══════════════════════════════════════════════════════════
//  SUBSCRIPTION — UPGRADE
// ═══════════════════════════════════════════════════════════

public class UpgradeRequestDtoValidator : AbstractValidator<UpgradeRequestDto>
{
    private static readonly string[] ValidPlans = { "Pro", "ProYearly" };

    public UpgradeRequestDtoValidator()
    {
        RuleFor(x => x.Plan)
            .NotEmpty().WithMessage("Plan is required.")
            .Must(p => ValidPlans.Contains(p))
            .WithMessage("Plan must be one of: Pro, ProAnnual.");
    }
}
