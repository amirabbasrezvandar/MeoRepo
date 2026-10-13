using System.Text;
using FluentValidation;
using Meowbarak.API.Middleware;
using Meowbarak.Application.Validators;
using Meowbarak.Infrastructure;
using Meowbarak.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger با پشتیبانی JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Meowbarak API",
        Version = "v1",
        Description = "API پروژه Meowbarak - احراز هویت با ایمیل و OTP + JWT"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Infrastructure (DbContext, Email, JWT, AuthService)
builder.Services.AddInfrastructure(builder.Configuration);

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RequestOtpValidator>();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS برای Next.js (فعلاً باز برای توسعه)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJs", policy =>
    {
        policy.AllowAnyOrigin()   // در Production محدود به دامنه فرانت‌اند کن
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// -------------------- Middleware Pipeline --------------------

// Global Exception Handler
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meowbarak API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowNextJs");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// -------------------- Auto Migrate DB (فقط Development) --------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();
        app.Logger.LogInformation("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database. Make sure SQL Server is running and connection string is correct.");
    }
}

app.Run();
