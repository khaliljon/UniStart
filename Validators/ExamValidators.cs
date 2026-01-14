using FluentValidation;
using UniStart.DTOs;

namespace UniStart.Validators;

public class CreateExamDtoValidator : AbstractValidator<CreateExamDto>
{
    public CreateExamDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название экзамена обязательно")
            .Length(3, 200).WithMessage("Название должно содержать от 3 до 200 символов");
            
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Описание не должно превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.Description));
            
        RuleFor(x => x.TimeLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Время не может быть отрицательным")
            .LessThanOrEqualTo(480).WithMessage("Время не должно превышать 480 минут (8 часов)");
            
        RuleFor(x => x.PassingScore)
            .GreaterThanOrEqualTo(0).WithMessage("Проходной балл не может быть отрицательным")
            .LessThanOrEqualTo(100).WithMessage("Проходной балл не должен превышать 100%");
            
        RuleFor(x => x.SubjectIds)
            .Must(x => x == null || x.Count <= 10)
            .WithMessage("Нельзя выбрать больше 10 предметов");
    }
}

public class UpdateExamDtoValidator : AbstractValidator<UpdateExamDto>
{
    public UpdateExamDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название экзамена обязательно")
            .Length(3, 200).WithMessage("Название должно содержать от 3 до 200 символов");
            
        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов")
            .When(x => !string.IsNullOrEmpty(x.Description));
            
        RuleFor(x => x.SubjectIds)
            .NotEmpty().WithMessage("Необходимо выбрать хотя бы один предмет")
            .Must(x => x.Count <= 10).WithMessage("Нельзя выбрать больше 10 предметов");
            
        RuleFor(x => x.TimeLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Время не может быть отрицательным")
            .LessThanOrEqualTo(480).WithMessage("Время не должно превышать 480 минут (8 часов)");
            
        RuleFor(x => x.PassingScore)
            .GreaterThanOrEqualTo(0).WithMessage("Проходной балл не может быть отрицательным")
            .LessThanOrEqualTo(100).WithMessage("Проходной балл не должен превышать 100%");
            
        RuleFor(x => x.MaxAttempts)
            .GreaterThan(0).WithMessage("Максимальное количество попыток должно быть больше 0")
            .LessThanOrEqualTo(10).WithMessage("Максимальное количество попыток не должно превышать 10");
    }
}
