namespace Gibeira.RepositoryTests;

// A RestoredRepo with the gibeira-module template installed in a private hive,
// so the developer's own `dotnet new` template list is never touched.
public sealed class TemplateRepo : IAsyncLifetime
{
    private static readonly string[] Layers = ["Domain", "Application", "Infrastructure"];

    private readonly RestoredRepo restored = new();
    private DirectoryInfo hive = null!;

    public TempRepo Repo => restored.Repo;

    public async ValueTask InitializeAsync()
    {
        await restored.InitializeAsync();
        hive = Directory.CreateTempSubdirectory("gibeira-hive-");

        var install = await Repo.RunAsync("dotnet", ["new", "install", Repo.PathOf("templates/module"), "--debug:custom-hive", hive.FullName]);
        if (install.ExitCode != 0)
        {
            throw new InvalidOperationException($"template install failed: {install.Output}");
        }
    }

    // Runs `dotnet new gibeira-module -n <name>` from the repository root.
    public Task<ProcessResult> NewModuleAsync(string name) =>
        Repo.RunAsync("dotnet", ["new", "gibeira-module", "-n", name, "--debug:custom-hive", hive.FullName]);

    public static string[] LayerProjects(string module) =>
        [.. Layers.Select(layer => $"src/Modules/{module}/Gibeira.{module}.{layer}/Gibeira.{module}.{layer}.csproj")];

    public async ValueTask DisposeAsync()
    {
        await restored.DisposeAsync();
        hive.Delete(recursive: true);
    }
}