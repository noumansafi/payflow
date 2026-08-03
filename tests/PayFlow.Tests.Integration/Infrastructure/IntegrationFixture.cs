using Testcontainers.MsSql;

namespace PayFlow.Tests.Integration.Infrastructure;

/// <summary>
/// One SQL Server container + one API host for the integration suite.
/// </summary>
public sealed class IntegrationFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

    public PayFlowApiFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(
                _container.GetConnectionString())
            {
                InitialCatalog = "PayFlow_Integration"
            }
            .ConnectionString;

        Factory = new PayFlowApiFactory(connectionString);

        // Force host start so Development migrations apply before tests run.
        using var client = Factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
        var health = await client.GetAsync("/api/v1/health");
        health.EnsureSuccessStatusCode();
    }

    public ApiClient CreateClient() => new(Factory);

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _container.DisposeAsync().AsTask();
    }
}
