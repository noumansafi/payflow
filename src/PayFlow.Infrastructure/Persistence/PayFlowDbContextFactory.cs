using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PayFlow.Infrastructure.Persistence;

/// <summary>
/// Design-time factory so EF CLI can create migrations without running the API host.
/// </summary>
public sealed class PayFlowDbContextFactory : IDesignTimeDbContextFactory<PayFlowDbContext>
{
    public PayFlowDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("PAYFLOW_CONNECTION_STRING")
            ?? "Server=localhost,1433;Database=PayFlow;User Id=sa;Password=PayFlow_Strong_Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<PayFlowDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PayFlowDbContext(optionsBuilder.Options);
    }
}
