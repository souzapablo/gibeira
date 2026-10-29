namespace Gibeira.ArchitectureTests;

public static class NamingRules
{
    private static readonly string[] Layers = ["Domain", "Application", "Infrastructure"];

    // ARCH-07: every project under `<modulesDirectory>/<Module>/` is `<prefix>.<Module>.<Layer>`.
    public static IReadOnlyList<string> MisnamedModuleProjects(string modulesDirectory, string prefix)
    {
        if (!Directory.Exists(modulesDirectory))
        {
            return [];
        }

        return [.. Directory.GetDirectories(modulesDirectory)
            .SelectMany(moduleDirectory =>
            {
                var module = Path.GetFileName(moduleDirectory);
                var allowed = Layers.Select(layer => $"{prefix}.{module}.{layer}").ToArray();

                return Directory.GetFiles(moduleDirectory, "*.csproj", SearchOption.AllDirectories)
                    .Where(project => !allowed.Contains(Path.GetFileNameWithoutExtension(project)))
                    .Select(project =>
                        $"{Path.GetRelativePath(modulesDirectory, project)} is not named {string.Join(", ", allowed)}");
            })];
    }
}