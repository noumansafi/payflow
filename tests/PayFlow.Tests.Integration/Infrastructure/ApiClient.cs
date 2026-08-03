using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PayFlow.Domain.Enums;
using PayFlow.Infrastructure.Persistence;

namespace PayFlow.Tests.Integration.Infrastructure;

public sealed record AuthSession(
    Guid UserId,
    string Email,
    string Password,
    string AccessToken,
    string RefreshToken);

public sealed class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly PayFlowApiFactory _factory;

    public ApiClient(PayFlowApiFactory factory)
    {
        _factory = factory;
        // HTTPS base avoids UseHttpsRedirection turning POSTs into 307s in tests.
        _http = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });
    }

    public HttpClient Http => _http;

    public async Task<AuthSession> RegisterAndLoginAsync(
        string? email = null,
        string firstName = "Test",
        string lastName = "User")
    {
        email ??= $"user_{Guid.NewGuid():N}@payflow.test";
        const string password = "Password1!";

        var registerResponse = await _http.PostAsJsonAsync(
            "/api/v1/auth/register",
            new
            {
                email,
                password,
                firstName,
                lastName
            });

        registerResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var register = await registerResponse.Content.ReadFromJsonAsync<RegisterResponseDto>(JsonOptions);
        register.Should().NotBeNull();

        var loginResponse = await _http.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, password });

        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>(JsonOptions);
        login.Should().NotBeNull();

        return new AuthSession(
            register!.User.Id,
            email,
            password,
            login!.Tokens.AccessToken,
            login.Tokens.RefreshToken);
    }

    public void UseBearer(string accessToken) =>
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

    public void ClearBearer() => _http.DefaultRequestHeaders.Authorization = null;

    public async Task CreditAsync(decimal amount)
    {
        var response = await _http.PostAsJsonAsync("/api/v1/wallets/me/credit", new { amount });
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    public async Task PromoteToAdminAsync(Guid userId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PayFlowDbContext>();
        var user = await db.Users.SingleAsync(x => x.Id == userId);
        user.Role = UserRole.Admin;
        await db.SaveChangesAsync();
    }

    private sealed record RegisterResponseDto(AuthUserDto User, string EmailVerificationToken);

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
