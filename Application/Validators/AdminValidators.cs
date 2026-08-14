using FluentValidation;
using UniStart.Application.DTOs;

namespace UniStart.Application.Validators;


public class CreateQuestionDtoValidator : AbstractValidator<CreateQuestionDto>
{
    private static readonly string[] ValidDifficulties = { "Easy", "Medium", "Hard" };

    public CreateQuestionDtoValidator()
    {
        RuleFor(x => x.TopicId)
            .GreaterThan(0)
            .WithMessage("TopicId must be a positive integer.");

        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required.")
            .MinimumLength(3).WithMessage("Question text must be at least 3 characters.")
            .MaximumLength(5000).WithMessage("Question text must not exceed 5000 characters.");

        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Difficulty is required.")
            .Must(d => ValidDifficulties.Contains(d))
            .WithMessage("Difficulty must be one of: Easy, Medium, Hard.");

        RuleFor(x => x.Explanation)
            .MaximumLength(3000).WithMessage("Explanation must not exceed 3000 characters.")
            .When(x => x.Explanation is not null);

        RuleFor(x => x.DifficultyParam)
            .InclusiveBetween(-3.0, 3.0).WithMessage("DifficultyParam (b) must be between -3 and 3.")
            .When(x => x.DifficultyParam.HasValue);

        RuleFor(x => x.DiscriminationParam)
            .InclusiveBetween(0.1, 3.0).WithMessage("DiscriminationParam (a) must be between 0.1 and 3.")
            .When(x => x.DiscriminationParam.HasValue);

        RuleFor(x => x.GuessParam)
            .InclusiveBetween(0.0, 0.5).WithMessage("GuessParam (c) must be between 0 and 0.5.")
            .When(x => x.GuessParam.HasValue);

        RuleFor(x => x.AnswerOptions)
            .NotNull().WithMessage("AnswerOptions are required.")
            .Must(opts => opts is not null && opts.Count >= 2)
            .WithMessage("At least 2 answer options are required.")
            .Must(opts => opts is not null && opts.Count <= 6)
            .WithMessage("No more than 6 answer options allowed.")
            .Must(opts => opts is not null && opts.Count(o => o.IsCorrect) == 1)
            .WithMessage("Exactly one answer option must be marked as correct.");

        RuleForEach(x => x.AnswerOptions)
            .SetValidator(new CreateAnswerOptionDtoValidator());
    }
}

public class CreateAnswerOptionDtoValidator : AbstractValidator<CreateAnswerOptionDto>
{
    public CreateAnswerOptionDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Answer option text is required.")
            .MaximumLength(2000).WithMessage("Answer option text must not exceed 2000 characters.");
    }
}


public class UpdateQuestionDtoValidator : AbstractValidator<UpdateQuestionDto>
{
    private static readonly string[] ValidDifficulties = { "Easy", "Medium", "Hard" };

    public UpdateQuestionDtoValidator()
    {
        RuleFor(x => x.Text)
            .MinimumLength(3).WithMessage("Question text must be at least 3 characters.")
            .MaximumLength(5000).WithMessage("Question text must not exceed 5000 characters.")
            .When(x => x.Text is not null);

        RuleFor(x => x.Difficulty)
            .Must(d => ValidDifficulties.Contains(d!))
            .WithMessage("Difficulty must be one of: Easy, Medium, Hard.")
            .When(x => x.Difficulty is not null);

        RuleFor(x => x.Explanation)
            .MaximumLength(3000).WithMessage("Explanation must not exceed 3000 characters.")
            .When(x => x.Explanation is not null);

        RuleFor(x => x.DifficultyParam)
            .InclusiveBetween(-3.0, 3.0).WithMessage("DifficultyParam (b) must be between -3 and 3.")
            .When(x => x.DifficultyParam.HasValue);

        RuleFor(x => x.DiscriminationParam)
            .InclusiveBetween(0.1, 3.0).WithMessage("DiscriminationParam (a) must be between 0.1 and 3.")
            .When(x => x.DiscriminationParam.HasValue);

        RuleFor(x => x.GuessParam)
            .InclusiveBetween(0.0, 0.5).WithMessage("GuessParam (c) must be between 0 and 0.5.")
            .When(x => x.GuessParam.HasValue);

        RuleFor(x => x.AnswerOptions)
            .Must(opts => opts!.Count >= 2)
            .WithMessage("At least 2 answer options are required.")
            .Must(opts => opts!.Count <= 6)
            .WithMessage("No more than 6 answer options allowed.")
            .Must(opts => opts!.Count(o => o.IsCorrect) == 1)
            .WithMessage("Exactly one answer option must be marked as correct.")
            .When(x => x.AnswerOptions is not null);

        RuleForEach(x => x.AnswerOptions)
            .SetValidator(new CreateAnswerOptionDtoValidator())
            .When(x => x.AnswerOptions is not null);
    }
}


public class BulkImportDtoValidator : AbstractValidator<BulkImportDto>
{
    public BulkImportDtoValidator()
    {
        RuleFor(x => x.Questions)
            .NotNull().WithMessage("Questions list is required.")
            .Must(q => q is not null && q.Count >= 1)
            .WithMessage("At least 1 question is required for import.")
            .Must(q => q is not null && q.Count <= 500)
            .WithMessage("No more than 500 questions per import batch.");

        RuleForEach(x => x.Questions)
            .SetValidator(new CreateQuestionDtoValidator());
    }
}


public class AdminUpdateUserDtoValidator : AbstractValidator<AdminUpdateUserDto>
{
    private static readonly string[] ValidRoles = { "Student", "Tutor", "Admin", "SchoolAdmin", "SchoolTutor" };
    private static readonly string[] ValidTiers = { "Free", "Pro" };

    public AdminUpdateUserDtoValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
            .When(x => x.Email is not null);

        RuleFor(x => x.Role)
            .Must(r => ValidRoles.Contains(r!))
            .WithMessage("Role must be one of: Student, Tutor, Admin, SchoolAdmin.")
            .When(x => x.Role is not null);

        RuleFor(x => x.SubscriptionTier)
            .Must(t => ValidTiers.Contains(t!))
            .WithMessage("SubscriptionTier must be one of: Free, Pro.")
            .When(x => x.SubscriptionTier is not null);

        RuleFor(x => x.SubscriptionExpiresAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("SubscriptionExpiresAt must be in the future.")
            .When(x => x.SubscriptionExpiresAt.HasValue);
    }
}


public class CreateTopicDtoValidator : AbstractValidator<CreateTopicDto>
{
    public CreateTopicDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Topic name is required.")
            .MinimumLength(2).WithMessage("Topic name must be at least 2 characters.")
            .MaximumLength(200).WithMessage("Topic name must not exceed 200 characters.");

        RuleFor(x => x.SectionId)
            .GreaterThan(0).WithMessage("SectionId must be a positive integer.");
    }
}
