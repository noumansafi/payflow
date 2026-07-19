using FluentAssertions;

namespace PayFlow.Tests.Unit.Architecture;

/// <summary>
/// Lightweight guardrails documenting Clean Architecture intent.
/// Deeper NetArchTest rules can replace these as the solution grows.
/// </summary>
public sealed class DependencyRulesTests
{
    [Fact]
    public void Domain_assembly_should_not_reference_infrastructure_or_application()
    {
        var domain = typeof(PayFlow.Domain.AssemblyMarker).Assembly;
        var referenced = domain.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty);

        referenced.Should().NotContain("PayFlow.Application");
        referenced.Should().NotContain("PayFlow.Infrastructure");
        referenced.Should().NotContain("PayFlow.Api");
        referenced.Should().NotContain("Microsoft.EntityFrameworkCore");
    }
}
