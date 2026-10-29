namespace Gibeira.ArchitectureTests;

public sealed class NamingRuleTests
{
    [Fact]
    public void MisnamedModuleProjectIsReported()
    {
        var modules = Directory.CreateTempSubdirectory("gibeira-modules-");
        try
        {
            CreateProject(modules.FullName, "Ledger", "Gibeira.Ledger.Domain");
            CreateProject(modules.FullName, "Ledger", "Gibeira.Ledger.Domian");
            CreateProject(modules.FullName, "Ledger", "Gibeira.Budgets.Application");

            var violations = NamingRules.MisnamedModuleProjects(modules.FullName, "Gibeira");

            Assert.Equal(2, violations.Count);
            Assert.Contains(violations, v => v.Contains("Gibeira.Ledger.Domian.csproj", StringComparison.Ordinal));
            Assert.Contains(violations, v => v.Contains("Gibeira.Budgets.Application.csproj", StringComparison.Ordinal));
            Assert.DoesNotContain(violations, v => v.Contains("Gibeira.Ledger.Domain.csproj", StringComparison.Ordinal));
        }
        finally
        {
            modules.Delete(recursive: true);
        }
    }

    [Fact]
    public void NamingRuleHoldsOnSolution() =>
        Assert.Empty(NamingRules.MisnamedModuleProjects(Path.Combine(RepositoryRoot.Find(), "src", "Modules"), "Gibeira"));

    private static void CreateProject(string modulesDirectory, string module, string project)
    {
        var directory = Directory.CreateDirectory(Path.Combine(modulesDirectory, module, project));
        File.WriteAllText(Path.Combine(directory.FullName, project + ".csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");
    }
}