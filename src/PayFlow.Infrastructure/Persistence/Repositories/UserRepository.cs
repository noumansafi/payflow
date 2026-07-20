using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(PayFlowDbContext dbContext) : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<User?> GetByEmailVerificationTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(
            x => x.EmailVerificationTokenHash == tokenHash,
            cancellationToken);

    public Task<User?> GetByPasswordResetTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(
            x => x.PasswordResetTokenHash == tokenHash,
            cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
