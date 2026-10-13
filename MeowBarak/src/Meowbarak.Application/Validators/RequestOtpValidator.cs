using FluentValidation;
using Meowbarak.Application.DTOs.Auth;

namespace Meowbarak.Application.Validators;

public class RequestOtpValidator : AbstractValidator<RequestOtpDto>
{
    public RequestOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست");
    }
}
