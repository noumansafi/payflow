using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PayFlow.Tests.Integration.Infrastructure;

namespace PayFlow.Tests.Integration.Flows;

[Collection(IntegrationCollection.Name)]
public sealed class AuditAuthorizationTests(IntegrationFixture fixture)
{
    [Fact]
    public async Task AuditLogs_WhenUser_ReturnsForbidden()
    {
        var client = fixture.CreateClient();
        var user = await client.RegisterAndLoginAsync();
        client.UseBearer(user.AccessToken);

        var response = await client.Http.GetAsync("/api/v1/admin/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuditLogs_WhenAdmin_ReturnsOk()
    {
        var client = fixture.CreateClient();
        var user = await client.RegisterAndLoginAsync();
        await client.PromoteToAdminAsync(user.UserId);

        // Role is in the JWT — re-login after promote so the token carries Admin.
        client.ClearBearer();
        var login = await client.Http.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = user.Email, password = user.Password });
        login.EnsureSuccessStatusCode();

        var body = await login.Content.ReadFromJsonAsync<AuthResponseDto>();
        body.Should().NotBeNull();
        client.UseBearer(body!.Tokens.AccessToken);

        var response = await client.Http.GetAsync("/api/v1/admin/audit-logs?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private sealed record AuthResponseDto(AuthUserDto User, AuthTokensDto Tokens);

    private sealed record AuthUserDto(
        Guid Id,
        string Email,
        string FirstName,
        string LastName,
        string Role,
        bool IsEmailVerified);

    private sealed record AuthTokensDto(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiresAtUtc);
}
