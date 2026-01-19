using FluentValidation;
using UniStart.DTOs;

namespace UniStart.Validators;

public class CreateQuizDtoValidator : AbstractValidator<CreateQuizDto>
{
    public CreateQuizDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Название теста обязательно")
            .Length(3, 200).WithMessage("Название должно содержать от 3 до 200 символов");
            
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание обязательно")
            .MaximumLength(1000).WithMessage("Описание не должно превышать 1000 символов");
            
        RuleFor(x => x.TimeLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Время не может быть отрицательным")
            .LessThanOrEqualTo(300).WithMessage("Время не должно превышать 300 минут");
            
        RuleFor(x => x.Difficulty)
            .NotEmpty().WithMessage("Уровень сложности обязателен")
            .Must(x => x == "Easy" || x == "Medium" || x == "Hard")
            .WithMessage("Сложность должна быть: Easy, Medium или Hard");
    }
}

public class CreateQuizQuestionDtoValidator : AbstractValidator<CreateQuizQuestionDto>
{
    public CreateQuizQuestionDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст вопроса обязателен")
            .Length(5, 1000).WithMessage("Текст вопроса должен содержать от 5 до 1000 символов");
            
        RuleFor(x => x.Points)
            .GreaterThanOrEqualTo(1).WithMessage("Баллы должны быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Баллы не должны превышать 100");
            
        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("URL не должен превышать 500 символов")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
            
        RuleFor(x => x.Explanation)
            .MaximumLength(2000).WithMessage("Объяснение не должно превышать 2000 символов")
            .When(x => !string.IsNullOrEmpty(x.Explanation));
            
        RuleFor(x => x.QuizId)
            .GreaterThan(0).WithMessage("ID квиза должен быть больше 0");
    }
}

public class CreateQuizAnswerDtoValidator : AbstractValidator<CreateQuizAnswerDto>
{
    public CreateQuizAnswerDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Текст ответа обязателен")
            .MaximumLength(500).WithMessage("Текст ответа не должен превышать 500 символов");
            
        RuleFor(x => x.QuestionId)
            .GreaterThan(0).WithMessage("ID вопроса должен быть больше 0");
    }
}

public class SubmitQuizDtoValidator : AbstractValidator<SubmitQuizDto>
{
    public SubmitQuizDtoValidator()
    {
        RuleFor(x => x.QuizId)
            .GreaterThan(0).WithMessage("ID теста должен быть больше 0");
            
        RuleFor(x => x.TimeSpentSeconds)
            .GreaterThanOrEqualTo(0).WithMessage("Время должно быть положительным");
            
        RuleFor(x => x.UserAnswers)
            .NotNull().WithMessage("Ответы обязательны")
            .NotEmpty().WithMessage("Должен быть хотя бы один ответ");
    }
}
