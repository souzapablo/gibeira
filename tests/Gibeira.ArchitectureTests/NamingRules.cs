using ArchUnitNET.Domain;

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

    // ARCH-10: a layer assembly whose module name is not one segment, such as `<prefix>.A.B.Domain`,
    // belongs to no module, so every other rule would skip it.
    public static IReadOnlyList<string> MultiSegmentModuleNames(Architecture architecture, string prefix) =>
        [.. architecture.Assemblies
            .Select(assembly => assembly.Name)
            .Where(name => name.StartsWith(prefix + ".", StringComparison.Ordinal)
                && Layers.Any(layer => name.EndsWith("." + layer, StringComparison.Ordinal))
                && ModuleAssembly.Parse(name, prefix) is null)
            .Distinct()
            .Select(name => $"{name} is not named {prefix}.<Module>.<Layer> with a one-segment module name")];
}