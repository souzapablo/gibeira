using static Gibeira.ArchitectureTests.SolutionArchitecture;

namespace Gibeira.ArchitectureTests;

public sealed class BoundaryRuleTests
{
    [Fact]
    public void CrossModuleDependencyIsReported()
    {
        var violations = BoundaryRules.CrossModuleDependency(Load(FixturePrefix), FixturePrefix);

        var violation = Assert.Single(violations);
        Assert.StartsWith("Module Ledger depends on module Budgets: ", violation, StringComparison.Ordinal);
        Assert.Contains("Fixture.Ledger.Domain.Account", violation, StringComparison.Ordinal);
        Assert.Contains("Fixture.Budgets.Domain.Budget", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void ModuleIsolationRuleHoldsOnSolution() =>
        Assert.Empty(BoundaryRules.CrossModuleDependency(Load(SolutionPrefix), SolutionPrefix));

    [Fact]
    public void SharedKernelDependingOnModulesOrHostsIsReported()
    {
        var violations = BoundaryRules.SharedKernelDependingOnModulesOrHosts(Load(FixturePrefix), FixturePrefix);

        Assert.Equal(3, violations.Count);
        Assert.Contains(violations, v => v.StartsWith("Fixture.SharedKernel.Money", StringComparison.Ordinal) && v.Contains("Fixture.Budgets.Domain.Budget", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.StartsWith("Fixture.SharedKernel.Money", StringComparison.Ordinal) && v.Contains("Fixture.Web.HomePage", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.StartsWith("Fixture.SharedKernel.Money", StringComparison.Ordinal) && v.Contains("Fixture.AppHost.Orchestrator", StringComparison.Ordinal));
    }

    [Fact]
    public void SharedKernelRuleHoldsOnSolution() =>
        Assert.Empty(BoundaryRules.SharedKernelDependingOnModulesOrHosts(Load(SolutionPrefix), SolutionPrefix));

    [Fact]
    public void DependencyOnWebOutsideAppHostIsReported()
    {
        var violations = BoundaryRules.DependencyOnWebOutsideAppHost(Load(FixturePrefix), FixturePrefix);

        var violation = Assert.Single(violations);
        Assert.StartsWith("Fixture.SharedKernel.Money", violation, StringComparison.Ordinal);
        Assert.Contains("Fixture.Web.HomePage", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void WebRuleHoldsOnSolution() =>
        Assert.Empty(BoundaryRules.DependencyOnWebOutsideAppHost(Load(SolutionPrefix), SolutionPrefix));
}