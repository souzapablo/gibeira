using static Gibeira.ArchitectureTests.SolutionArchitecture;

namespace Gibeira.ArchitectureTests;

public sealed class LayerRuleTests
{
    private const string Account = "Fixture.Ledger.Domain.Account";
    private const string LedgerService = "Fixture.Ledger.Application.LedgerService";

    [Fact]
    public void DomainDependingOnOuterLayersIsReported()
    {
        var violations = LayerRules.DomainDependingOnOuterLayers(Load(FixturePrefix), FixturePrefix);

        Assert.Contains(violations, v => v.StartsWith(Account, StringComparison.Ordinal) && v.Contains("Fixture.Ledger.Application.LedgerService", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.StartsWith(Account, StringComparison.Ordinal) && v.Contains("Fixture.Ledger.Infrastructure.LedgerRepository", StringComparison.Ordinal));
        Assert.All(violations, v => Assert.StartsWith("Fixture.Ledger.Domain.", v, StringComparison.Ordinal));
    }

    [Fact]
    public void DomainLayerRuleHoldsOnSolution() =>
        Assert.Empty(LayerRules.DomainDependingOnOuterLayers(Load(SolutionPrefix), SolutionPrefix));

    [Fact]
    public void ApplicationDependingOnInfrastructureIsReported()
    {
        var violations = LayerRules.ApplicationDependingOnInfrastructure(Load(FixturePrefix), FixturePrefix);

        var violation = Assert.Single(violations);
        Assert.StartsWith("Fixture.Ledger.Application.LedgerService", violation, StringComparison.Ordinal);
        Assert.Contains("Fixture.Ledger.Infrastructure.LedgerRepository", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplicationLayerRuleHoldsOnSolution() =>
        Assert.Empty(LayerRules.ApplicationDependingOnInfrastructure(Load(SolutionPrefix), SolutionPrefix));

    [Fact]
    public void InnerLayersUsingFrameworksAreReported()
    {
        var violations = LayerRules.InnerLayersUsingFrameworks(Load(FixturePrefix), FixturePrefix);

        Assert.Contains(violations, v => v.StartsWith(Account, StringComparison.Ordinal) && v.Contains("Microsoft.AspNetCore.Http.HttpContext", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.StartsWith(Account, StringComparison.Ordinal) && v.Contains("Microsoft.EntityFrameworkCore.DbContext", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.StartsWith(LedgerService, StringComparison.Ordinal) && v.Contains("Microsoft.EntityFrameworkCore.DbContext", StringComparison.Ordinal));
    }

    [Fact]
    public void FrameworkRuleHoldsOnSolution() =>
        Assert.Empty(LayerRules.InnerLayersUsingFrameworks(Load(SolutionPrefix), SolutionPrefix));
}