using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PayFlow.Tests.Integration.Infrastructure;

namespace PayFlow.Tests.Integration.Flows;

[Collection(IntegrationCollection.Name)]
public sealed class TransferFlowTests(IntegrationFixture fixture)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Register_Credit_Transfer_Shows_History_And_Notification()
    {
        var senderClient = fixture.CreateClient();
        var receiverClient = fixture.CreateClient();

        var sender = await senderClient.RegisterAndLoginAsync(firstName: "Sender", lastName: "One");
        var receiver = await receiverClient.RegisterAndLoginAsync(firstName: "Receiver", lastName: "Two");

        senderClient.UseBearer(sender.AccessToken);
        await senderClient.CreditAsync(100m);

        var transferResponse = await senderClient.Http.PostAsJsonAsync(
            "/api/v1/transfers",
            new
            {
                receiverUserId = receiver.UserId,
                amount = 25.50m,
                note = "lunch"
            });

        transferResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var transfer = await transferResponse.Content.ReadFromJsonAsync<TransferResultDto>(JsonOptions);
        transfer.Should().NotBeNull();
        transfer!.Amount.Should().Be(25.50m);
        transfer.ReferenceNumber.Should().NotBeNullOrWhiteSpace();

        var senderTx = await senderClient.Http.GetFromJsonAsync<PagedResultDto<TransactionDto>>(
            "/api/v1/transactions?page=1&pageSize=20",
            JsonOptions);
        senderTx.Should().NotBeNull();
        senderTx!.Items.Should().Contain(x =>
            x.ReferenceNumber == transfer.ReferenceNumber &&
            x.Direction == "Sent" &&
            x.CounterpartyName == "Receiver Two");

        receiverClient.UseBearer(receiver.AccessToken);

        var receiverBalance = await receiverClient.Http.GetFromJsonAsync<WalletBalanceDto>(
            "/api/v1/wallets/me/balance",
            JsonOptions);
        receiverBalance.Should().NotBeNull();
        receiverBalance!.Balance.Should().Be(25.50m);

        var notifications = await receiverClient.Http.GetFromJsonAsync<PagedResultDto<NotificationDto>>(
            "/api/v1/notifications?page=1&pageSize=20",
            JsonOptions);
        notifications.Should().NotBeNull();
        notifications!.Items.Should().Contain(x =>
            x.Type == "TransferReceived" &&
            x.IsRead == false);
    }

    private sealed record TransferResultDto(
        Guid TransactionId,
        string ReferenceNumber,
        decimal Amount,
        decimal Fee,
        string Status,
        DateTime CompletedAtUtc);

    private sealed record PagedResultDto<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private sealed record TransactionDto(
        Guid Id,
        string ReferenceNumber,
        string Direction,
        Guid CounterpartyWalletId,
        string? CounterpartyName,
        decimal Amount,
        decimal Fee,
        string Status,
        string TransactionType,
        string? Note,
        DateTime CreatedAtUtc,
        DateTime? CompletedAtUtc);

    private sealed record WalletBalanceDto(
        Guid WalletId,
        decimal Balance,
        string Currency,
        string Status);

    private sealed record NotificationDto(
        Guid Id,
        string Title,
        string Body,
        string Type,
        bool IsRead,
        Guid? RelatedEntityId,
        DateTime CreatedAtUtc);
}
