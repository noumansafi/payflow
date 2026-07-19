using FluentAssertions;

namespace PayFlow.Tests.Integration.Smoke;

public sealed class SolutionSmokeTests
{
    [Fact]
    public void Integration_test_project_loads()
    {
        true.Should().BeTrue();
    }
}
