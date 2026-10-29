namespace Gibeira.RepositoryTests;

public sealed class NewModuleArchitectureTests(TemplateRepo fixture) : IClassFixture<TemplateRepo>
{
    private TempRepo Repo => fixture.Repo;

    [Fact]
    public async Task NewModuleIsCoveredByArchitectureTests()
    {
        var created = await fixture.NewModuleAsync("Leaky");
        Assert.True(created.ExitCode == 0, created.Output);

        // Infrastructure stops referencing the inner layers, so Domain can reference it without a project cycle.
        const string Module = "src/Modules/Leaky";
        File.WriteAllText(Repo.PathOf($"{Module}/Gibeira.Leaky.Infrastructure/Gibeira.Leaky.Infrastructure.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        File.WriteAllText(Repo.PathOf($"{Module}/Gibeira.Leaky.Infrastructure/LedgerStore.cs"), "namespace Gibeira.Leaky.Infrastructure;\r\n\r\npublic sealed class LedgerStore;");
        File.WriteAllText(
            Repo.PathOf($"{Module}/Gibeira.Leaky.Domain/Gibeira.Leaky.Domain.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <ItemGroup>\r\n    <ProjectReference Include=\"..\\Gibeira.Leaky.Infrastructure\\Gibeira.Leaky.Infrastructure.csproj\" />\r\n  </ItemGroup>\r\n</Project>");
        File.WriteAllText(
            Repo.PathOf($"{Module}/Gibeira.Leaky.Domain/Account.cs"),
            "using Gibeira.Leaky.Infrastructure;\r\n\r\nnamespace Gibeira.Leaky.Domain;\r\n\r\npublic sealed class Account\r\n{\r\n    public LedgerStore? Store { get; set; }\r\n}");

        // The architecture test project is not edited: its glob picks the module up.
        var rules = await Repo.RunAsync("dotnet", ["test", "--project", "tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj"]);

        Assert.NotEqual(0, rules.ExitCode);
        Assert.True(rules.Output.Contains("DomainLayerRuleHoldsOnSolution", StringComparison.Ordinal), rules.Output);
        Assert.True(rules.Output.Contains("Gibeira.Leaky.Domain.Account", StringComparison.Ordinal), rules.Output);
    }
}