using FluentValidation;
using Meowbarak.Application.DTOs.Auth;

namespace Meowbarak.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است");
    }
}
