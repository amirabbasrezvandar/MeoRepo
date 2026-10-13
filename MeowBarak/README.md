# Meowbarak API

پروژه Web API با معماری Clean Architecture سبک برای احراز هویت مبتنی بر ایمیل + OTP + JWT.

## ساختار پروژه

```
Meowbarak/
├── src/
│   ├── Meowbarak.Domain/          # موجودیت‌ها
│   ├── Meowbarak.Application/     # سرویس‌ها، DTO، Validator، Interface
│   ├── Meowbarak.Infrastructure/  # EF Core, Email واقعی, JWT
│   └── Meowbarak.API/             # Controllers, Middleware, Program.cs
└── Meowbarak.sln
```

## پیش‌نیازها

1. .NET 8 SDK
2. SQL Server (Express / LocalDB) + SSMS
3. یک اکانت Gmail (برای ارسال واقعی کد OTP)

---

## تنظیم ارسال ایمیل واقعی (خیلی مهم)

برای اینکه کد OTP واقعاً به ایمیل کاربر برود، باید **App Password** جیمیل بسازی:

### قدم‌به‌قدم ساخت App Password در Gmail:

1. برو به: https://myaccount.google.com/security
2. **۲-Step Verification** را روشن کن (اگر روشن نیست).
3. بعد از روشن شدن، دوباره به همان صفحه برو و روی **App passwords** کلیک کن.
4. یک نام انتخاب کن (مثلاً Meowbarak) و Generate بزن.
5. یک رمز **۱۶ رقمی** بهت می‌دهد (مثل `abcd efgh ijkl mnop`).
6. این ۱۶ رقم را کپی کن (فاصله‌ها را حذف کن یا با فاصله بگذار، هر دو کار می‌کند).

### قرار دادن در پروژه:

فایل‌های زیر را باز کن و مقادیر را جایگزین کن:

- `src/Meowbarak.API/appsettings.json`
- `src/Meowbarak.API/appsettings.Development.json`

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "your.email@gmail.com",
  "SmtpPassword": "abcdefghijklmnop",
  "FromEmail": "your.email@gmail.com",
  "FromName": "Meowbarak"
}
```

**نکته:** بعد از تغییر appsettings حتماً پروژه را یکبار Stop و دوباره Run کن.

اگر این مقادیر را پر نکنی، کد فقط در لاگ کنسول چاپ می‌شود (مثل قبل).

---

## نحوه اجرا

```bash
dotnet run --project src/Meowbarak.API
```

سپس Swagger را باز کن (معمولاً `https://localhost:7161/swagger`).

### تست کامل:

1. `POST /api/auth/request-otp` با ایمیل واقعی خودت
2. ایمیل را چک کن (کد ۶ رقمی آمده)
3. `POST /api/auth/verify-otp`
4. `POST /api/auth/set-password`
5. `POST /api/auth/login`

---

## معماری و افزودن قابلیت جدید

همان ساختار Clean Architecture سبک قبلی حفظ شده است.
برای اضافه کردن Entity جدید:

1. Entity در Domain
2. DbSet + Configuration در Infrastructure
3. Migration
4. DTO + Service + Validator در Application
5. Controller در API

---

## نکات

- OTP فقط ۵ دقیقه اعتبار دارد و یکبار مصرف است.
- رمز عبور با BCrypt هش می‌شود.
- در Production بهتر است App Password را در Environment Variable بگذاری نه داخل appsettings.
