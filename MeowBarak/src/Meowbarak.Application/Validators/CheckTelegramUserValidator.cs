using FluentValidation;
using Meowbarak.Application.DTOs.Telegram;

namespace Meowbarak.Application.Validators;

public class CheckTelegramUserValidator : AbstractValidator<CheckTelegramUserDto>
{
    public CheckTelegramUserValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("آیدی تلگرام باید عدد مثبت باشد.");
    }
}
