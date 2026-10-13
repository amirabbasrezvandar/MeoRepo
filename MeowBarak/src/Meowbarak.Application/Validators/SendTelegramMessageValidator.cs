using FluentValidation;
using Meowbarak.Application.DTOs.Telegram;

namespace Meowbarak.Application.Validators;

public class SendTelegramMessageValidator : AbstractValidator<SendTelegramMessageDto>
{
    public SendTelegramMessageValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("آیدی تلگرام باید عدد مثبت باشد.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("متن پیام نمی‌تواند خالی باشد.")
            .MaximumLength(4096)
            .WithMessage("طول پیام نمی‌تواند بیشتر از ۴۰۹۶ کاراکتر باشد.");
    }
}
