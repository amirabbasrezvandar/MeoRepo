using FluentValidation;
using Meowbarak.Application.DTOs.Auth;

namespace Meowbarak.Application.Validators;

public class SetPasswordValidator : AbstractValidator<SetPasswordDto>
{
    public SetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("کد تأیید الزامی است")
            .Length(6).WithMessage("کد باید ۶ رقم باشد");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(8).WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد")
            .Matches(@"[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد")
            .Matches(@"[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد")
            .Matches(@"[0-9]").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("تأیید رمز عبور با رمز عبور مطابقت ندارد");
    }
}
