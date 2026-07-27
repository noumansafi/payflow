using FluentAssertions;
using NSubstitute;
using PayFlow.Application.Beneficiaries.Queries.GetBeneficiaries;
using PayFlow.Application.Common.Exceptions;
using PayFlow.Application.Common.Interfaces;
using PayFlow.Domain.Entities;

namespace PayFlow.Tests.Unit.Beneficiaries;

public sealed class GetBeneficiariesQueryHandlerTests
{
    private readonly IBeneficiaryRepository _beneficiaries = Substitute.For<IBeneficiaryRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private GetBeneficiariesQueryHandler CreateSut() =>
        new(_beneficiaries, _currentUser);

    [Fact]
    public async Task Handle_WhenOwner_ReturnsPagedDtos()
    {
        var ownerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var beneficiary = new Beneficiary
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            BeneficiaryUserId = targetId,
            DisplayName = "Sara",
            CreatedAtUtc = DateTime.UtcNow,
            BeneficiaryUser = new User
            {
                Id = targetId,
                Email = "sara@example.com",
                PasswordHash = "hash",
                FirstName = "Sara",
                LastName = "Khan",
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        _currentUser.UserId.Returns(ownerId);
        _beneficiaries.ListForOwnerAsync(ownerId, 0, 20, Arg.Any<CancellationToken>())
            .Returns(new BeneficiaryListResult([beneficiary], 1));

        var result = await CreateSut().Handle(new GetBeneficiariesQuery(), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Email.Should().Be("sara@example.com");
        result.Items[0].DisplayName.Should().Be("Sara");
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ThrowsUnauthorized()
    {
        _currentUser.UserId.Returns((Guid?)null);

        var act = () => CreateSut().Handle(new GetBeneficiariesQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>();
    }
}
