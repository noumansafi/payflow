using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PayFlow.Tests.Integration.Infrastructure;

namespace PayFlow.Tests.Integration.Flows;

[Collection(IntegrationCollection.Name)]
public sealed class TransferRuleTests(IntegrationFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Transfer_WhenInsufficientFunds_ReturnsConflict_AndBalancesUnchanged()
    {
        var senderClient = fixture.CreateClient();
        var receiverClient = fixture.CreateClient();

        var sender = await senderClient.RegisterAndLoginAsync();
        var receiver = await receiverClient.RegisterAndLoginAsync();

        senderClient.UseBearer(sender.AccessToken);
        await senderClient.CreditAsync(10m);

        var response = await senderClient.Http.PostAsJsonAsync(
            "/api/v1/transfers",
            new
            {
                receiverUserId = receiver.UserId,
                amount = 50m,
                note = (string?)null
            });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var senderBalance = await senderClient.Http.GetFromJsonAsync<WalletBalanceDto>(
            "/api/v1/wallets/me/balance",
            JsonOptions);
        senderBalance!.Balance.Should().Be(10m);

        receiverClient.UseBearer(receiver.AccessToken);
        var receiverBalance = await receiverClient.Http.GetFromJsonAsync<WalletBalanceDto>(
            "/api/v1/wallets/me/balance",
            JsonOptions);
        receiverBalance!.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task Transfer_WhenWalletFrozen_ReturnsConflict()
    {
        var senderClient = fixture.CreateClient();
        var receiverClient = fixture.CreateClient();

        var sender = await senderClient.RegisterAndLoginAsync();
        var receiver = await receiverClient.RegisterAndLoginAsync();

        senderClient.UseBearer(sender.AccessToken);
        await senderClient.CreditAsync(100m);

        var freeze = await senderClient.Http.PostAsJsonAsync(
            "/api/v1/wallets/me/status",
            new { status = "Frozen" });
        freeze.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await senderClient.Http.PostAsJsonAsync(
            "/api/v1/transfers",
            new
            {
                receiverUserId = receiver.UserId,
                amount = 10m,
                note = (string?)null
            });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private sealed record WalletBalanceDto(
        Guid WalletId,
        decimal Balance,
        string Currency,
        string Status);
}
