using FluentValidation;
using UniStart.DTOs;

namespace UniStart.Validators;

public class CreateFlashcardSetDtoValidator : AbstractValidator<CreateFlashcardSetDto>
{
    public CreateFlashcardSetDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название набора обязательно")
            .Length(3, 200).WithMessage("Название должно содержать от 3 до 200 символов");
            
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");
            
        RuleFor(x => x.Subject)
            .MaximumLength(100).WithMessage("Название предмета не должно превышать 100 символов");
    }
}

public class CreateFlashcardDtoValidator : AbstractValidator<CreateFlashcardDto>
{
    public CreateFlashcardDtoValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Вопрос обязателен")
            .MaximumLength(500).WithMessage("Вопрос не должен превышать 500 символов");
            
        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("Ответ обязателен")
            .MaximumLength(500).WithMessage("Ответ не должен превышать 500 символов");
            
        RuleFor(x => x.OptionsJson)
            .MaximumLength(2000).WithMessage("Опции не должны превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.OptionsJson));
            
        RuleFor(x => x.MatchingPairsJson)
            .MaximumLength(2000).WithMessage("Пары сопоставления не должны превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.MatchingPairsJson));
            
        RuleFor(x => x.SequenceJson)
            .MaximumLength(2000).WithMessage("Последовательность не должна превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.SequenceJson));
            
        RuleFor(x => x.Explanation)
            .MaximumLength(1000).WithMessage("Объяснение не должно превышать 1000 символов");
            
        RuleFor(x => x.FlashcardSetId)
            .GreaterThan(0).WithMessage("ID набора должен быть больше 0");
    }
}

public class UpdateFlashcardSetDtoValidator : AbstractValidator<UpdateFlashcardSetDto>
{
    public UpdateFlashcardSetDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название набора обязательно")
            .Length(3, 200).WithMessage("Название должно содержать от 3 до 200 символов");
            
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");
            
        RuleFor(x => x.Subject)
            .MaximumLength(100).WithMessage("Название предмета не должно превышать 100 символов");
    }
}

public class ReviewFlashcardDtoValidator : AbstractValidator<ReviewFlashcardDto>
{
    public ReviewFlashcardDtoValidator()
    {
        RuleFor(x => x.FlashcardId)
            .GreaterThan(0).WithMessage("ID карточки должен быть больше 0");
            
        RuleFor(x => x.Quality)
            .GreaterThanOrEqualTo(0).WithMessage("Оценка не может быть отрицательной")
            .LessThanOrEqualTo(5).WithMessage("Оценка должна быть от 0 до 5");
    }
}
