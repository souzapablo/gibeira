using System.Xml.Linq;

namespace Gibeira.RepositoryTests;

public sealed class ModuleTemplateTests(TemplateRepo fixture) : IClassFixture<TemplateRepo>
{
    private TempRepo Repo => fixture.Repo;

    [Fact]
    public async Task TemplateCreatesLayerProjects()
    {
        var result = await fixture.NewModuleAsync("Probe");

        Assert.True(result.ExitCode == 0, result.Output);
        Assert.All(TemplateRepo.LayerProjects("Probe"), project => Assert.True(File.Exists(Repo.PathOf(project)), $"{project} is missing"));

        var created = Directory.GetFiles(Repo.PathOf("src/Modules/Probe"), "*", SearchOption.AllDirectories);
        Assert.Equal(3, created.Length);
        Assert.All(created, file => Assert.DoesNotContain("ModuleName", File.ReadAllText(file), StringComparison.Ordinal));

        // Nothing lands outside src/Modules/Probe.
        var status = await Repo.GitAsync("status", "--porcelain", "--untracked-files=all");
        Assert.All(status.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), line =>
            Assert.StartsWith("?? src/Modules/Probe/", line, StringComparison.Ordinal));
    }

    [Fact]
    public async Task TemplateWiresOnlyAllowedReferences()
    {
        var result = await fixture.NewModuleAsync("Wired");
        Assert.True(result.ExitCode == 0, result.Output);

        var domain = ModuleReferences("Wired", "Domain");
        var application = ModuleReferences("Wired", "Application");
        var infrastructure = ModuleReferences("Wired", "Infrastructure");

        Assert.Empty(domain);
        Assert.Equal(["Gibeira.Wired.Domain"], application);
        Assert.Equal(["Gibeira.Wired.Application", "Gibeira.Wired.Domain"], infrastructure);
    }

    [Fact]
    public async Task TemplateModuleBuildsAndPassesRules()
    {
        var created = await fixture.NewModuleAsync("Built");
        Assert.True(created.ExitCode == 0, created.Output);

        var add = await Repo.RunAsync("dotnet", ["sln", "Gibeira.slnx", "add", "--solution-folder", "src/Modules/Built", .. TemplateRepo.LayerProjects("Built")]);
        Assert.True(add.ExitCode == 0, add.Output);

        var build = await Repo.RunAsync("dotnet", ["build", "Gibeira.slnx", "-warnaserror"]);
        Assert.True(build.ExitCode == 0, build.Output);

        var rules = await Repo.RunAsync("dotnet", ["test", "--project", "tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj"]);
        Assert.True(rules.ExitCode == 0, rules.Output);

        // The rules ran over the new module, not around it.
        Assert.True(File.Exists(Repo.PathOf("tests/Gibeira.ArchitectureTests/bin/Debug/net10.0/Gibeira.Built.Domain.dll")));
    }

    // The module's own projects that `<module>.<layer>` references, sorted.
    private string[] ModuleReferences(string module, string layer) =>
        [.. XDocument.Load(Repo.PathOf($"src/Modules/{module}/Gibeira.{module}.{layer}/Gibeira.{module}.{layer}.csproj"))
            .Descendants("ProjectReference")
            .Select(reference => Path.GetFileNameWithoutExtension(((string)reference.Attribute("Include")!).Replace('\\', '/')))
            .Where(name => name.StartsWith($"Gibeira.{module}.", StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)];
}