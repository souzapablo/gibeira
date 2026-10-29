using System.Text.Json;
using System.Xml.Linq;

namespace Gibeira.RepositoryTests;

public sealed class StructureTests
{
    private static readonly string Root = RepositoryRoot.Find();
    private static readonly string[] BuildSettings = ["TargetFramework", "TargetFrameworks", "Nullable", "TreatWarningsAsErrors"];

    [Fact]
    public async Task SolutionContainsAgreedLayout()
    {
        string[] projects =
        [
            "src/Host/Gibeira.Web/Gibeira.Web.csproj",
            "src/Host/Gibeira.AppHost/Gibeira.AppHost.csproj",
            "src/Shared/Gibeira.SharedKernel/Gibeira.SharedKernel.csproj",
            "src/Shared/Gibeira.ServiceDefaults/Gibeira.ServiceDefaults.csproj",
            "tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj",
        ];
        string[] directories = ["src/Modules", "templates", "docs"];

        var files = await RepositoryRoot.ListFilesAsync(Root, TestContext.Current.CancellationToken);
        var solutionProjects = SolutionProjects();

        Assert.All(projects, project =>
        {
            Assert.Contains(project, files);
            Assert.Contains(project, solutionProjects);
        });
        Assert.All(directories, directory =>
            Assert.Contains(files, file => file.StartsWith(directory + "/", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task EveryProjectTargetsNet10WithNullableAndWarningsAsErrors()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var projects = SolutionProjects();
        Assert.NotEmpty(projects);

        var evaluations = await Task.WhenAll(projects.Select(project => EvaluateAsync(project, cancellationToken)));

        Assert.All(evaluations, evaluation =>
        {
            var (project, properties) = evaluation;
            Assert.True(properties["TargetFramework"] == "net10.0", $"{project}: TargetFramework is '{properties["TargetFramework"]}'");
            Assert.True(properties["TargetFrameworks"] is "" or "net10.0", $"{project}: TargetFrameworks is '{properties["TargetFrameworks"]}'");
            Assert.True(properties["Nullable"] == "enable", $"{project}: Nullable is '{properties["Nullable"]}'");
            Assert.True(properties["TreatWarningsAsErrors"] == "true", $"{project}: TreatWarningsAsErrors is '{properties["TreatWarningsAsErrors"]}'");
        });

        // The values come from Directory.Build.props; no project restates or overrides them.
        Assert.All(projects, project =>
        {
            var overridden = XDocument.Load(Path.Combine(Root, project)).Descendants()
                .Select(element => element.Name.LocalName)
                .Intersect(BuildSettings)
                .ToArray();
            Assert.True(overridden.Length == 0, $"{project} sets {string.Join(", ", overridden)}");
        });
    }

    [Fact]
    public async Task NoProjectDeclaresPackageVersions()
    {
        var files = await RepositoryRoot.ListFilesAsync(Root, TestContext.Current.CancellationToken);
        var projects = files.Where(file => file.EndsWith(".csproj", StringComparison.Ordinal)).ToArray();

        Assert.Contains("Directory.Packages.props", files);
        Assert.NotEmpty(projects);
        Assert.All(projects, project =>
        {
            var versioned = XDocument.Load(Path.Combine(Root, project)).Descendants()
                .Where(element => element.Name.LocalName == "PackageReference")
                .Where(reference =>
                    reference.Attributes().Any(a => a.Name.LocalName is "Version" or "VersionOverride")
                    || reference.Elements().Any(e => e.Name.LocalName is "Version" or "VersionOverride"))
                .Select(reference => (string?)reference.Attribute("Include"))
                .ToArray();
            Assert.True(versioned.Length == 0, $"{project} declares a version for {string.Join(", ", versioned)}");
        });
    }

    [Fact]
    public async Task RootHasConventionFiles()
    {
        var files = await RepositoryRoot.ListFilesAsync(Root, TestContext.Current.CancellationToken);

        Assert.All([".editorconfig", ".gitattributes", ".gitignore"], file =>
        {
            Assert.Contains(file, files);
            Assert.NotEqual(0, new FileInfo(Path.Combine(Root, file)).Length);
        });
    }

    private static string[] SolutionProjects() =>
        [.. XDocument.Load(Path.Combine(Root, "Gibeira.slnx")).Descendants("Project")
            .Select(project => (string)project.Attribute("Path")!)];

    private static async Task<(string Project, Dictionary<string, string> Properties)> EvaluateAsync(string project, CancellationToken cancellationToken)
    {
        var result = await ProcessRunner.RunAsync(
            "dotnet",
            ["msbuild", project, .. BuildSettings.Select(name => $"-getProperty:{name}")],
            Root,
            cancellationToken: cancellationToken);
        Assert.True(result.ExitCode == 0, $"{project}: {result.Output}");

        using var json = JsonDocument.Parse(result.Output);
        var properties = json.RootElement.GetProperty("Properties").EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.GetString() ?? "");
        return (project, properties);
    }
}