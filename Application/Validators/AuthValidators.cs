using FluentValidation;
using UniStart.Application.DTOs;

namespace UniStart.Application.Validators;

// ═══════════════════════════════════════════════════════════
//  REGISTER
// ═══════════════════════════════════════════════════════════

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .Matches(@"^[^\d]+$").WithMessage("First name must not contain digits.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .Matches(@"^[^\d]+$").WithMessage("Last name must not contain digits.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(10).WithMessage("Password must be at least 10 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+77\d{9}$").WithMessage("Phone must be a Kazakhstan number in the format +77XXXXXXXXX.");
    }
}

// ═══════════════════════════════════════════════════════════
//  LOGIN
// ═══════════════════════════════════════════════════════════

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

// ═══════════════════════════════════════════════════════════
//  UPDATE PHONE (profile completion)
// ═══════════════════════════════════════════════════════════

public class UpdatePhoneDtoValidator : AbstractValidator<UpdatePhoneDto>
{
    public UpdatePhoneDtoValidator()
    {
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+77\d{9}$").WithMessage("Phone must be a Kazakhstan number in the format +77XXXXXXXXX.");
    }
}

// ═══════════════════════════════════════════════════════════
//  UPDATE USER (self)
// ═══════════════════════════════════════════════════════════

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .Matches(@"^[^\d]+$").WithMessage("First name must not contain digits.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .Matches(@"^[^\d]+$").WithMessage("Last name must not contain digits.")
            .When(x => x.LastName is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.")
            .When(x => x.Email is not null);
    }
}
