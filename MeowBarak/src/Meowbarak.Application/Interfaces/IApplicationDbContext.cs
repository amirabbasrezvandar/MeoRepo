using Meowbarak.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meowbarak.Application.Interfaces;

/// <summary>
/// Abstraction روی DbContext تا Application به Infrastructure وابسته نشود
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<VerificationCode> VerificationCodes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
