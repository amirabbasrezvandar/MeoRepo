using FluentValidation;
using Meowbarak.Application.DTOs.Auth;

namespace Meowbarak.Application.Validators;

public class VerifyOtpValidator : AbstractValidator<VerifyOtpDto>
{
    public VerifyOtpValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد تأیید الزامی است")
            .Length(6).WithMessage("کد باید ۶ رقم باشد")
            .Matches(@"^\d{6}$").WithMessage("کد باید فقط شامل اعداد باشد");
    }
}
