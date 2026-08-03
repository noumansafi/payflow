using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace PayFlow.Tests.Integration.Infrastructure;

public sealed class PayFlowApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Development enables EF migrations on startup and the demo credit endpoint.
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        // Keep Jwt settings from appsettings.json so TokenService and JwtBearer
        // always share the same signing key (in-memory JWT overrides can race
        // configuration binding in WebApplicationFactory).
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = connectionString,
                    // Avoid File sink path issues under the test host.
                    ["Serilog:WriteTo:1:Name"] = "Console"
                });
        });
    }
}
