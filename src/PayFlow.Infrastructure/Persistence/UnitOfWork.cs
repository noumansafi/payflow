using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;

namespace PayFlow.Infrastructure.Persistence;

public sealed class UnitOfWork(PayFlowDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            throw new ConflictException("A record with the same unique value already exists.");
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sql &&
        sql.Number is 2601 or 2627;
}
