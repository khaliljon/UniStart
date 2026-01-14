using FluentValidation;
using UniStart.DTOs;

namespace UniStart.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Неверный формат email")
            .MaximumLength(256).WithMessage("Email не должен превышать 256 символов");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(6).WithMessage("Пароль должен содержать минимум 6 символов")
            .MaximumLength(100).WithMessage("Пароль не должен превышать 100 символов")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру");
            
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов");
            
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(100).WithMessage("Фамилия не должна превышать 100 символов");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Неверный формат email");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен");
    }
}

public class UpdateProfileDtoValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("Имя не должно превышать 100 символов")
            .When(x => !string.IsNullOrEmpty(x.FirstName));
            
        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Фамилия не должна превышать 100 символов")
            .When(x => !string.IsNullOrEmpty(x.LastName));
            
        RuleFor(x => x.UserName)
            .MaximumLength(100).WithMessage("Имя пользователя не должно превышать 100 символов")
            .When(x => !string.IsNullOrEmpty(x.UserName));
    }
}
