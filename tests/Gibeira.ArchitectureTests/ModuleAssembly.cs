using System.Text.RegularExpressions;

namespace Gibeira.ArchitectureTests;

// An assembly named `<prefix>.<Module>.<Layer>`; any other assembly is not part of a module.
public sealed partial record ModuleAssembly(string Module, string Layer)
{
    public const string Domain = "Domain";
    public const string Application = "Application";
    public const string Infrastructure = "Infrastructure";

    public static ModuleAssembly? Parse(string assemblyName, string prefix)
    {
        if (!assemblyName.StartsWith(prefix + ".", StringComparison.Ordinal))
        {
            return null;
        }

        var match = ModuleName().Match(assemblyName[(prefix.Length + 1)..]);
        return match.Success ? new ModuleAssembly(match.Groups["module"].Value, match.Groups["layer"].Value) : null;
    }

    [GeneratedRegex(@"^(?<module>[A-Za-z0-9]+)\.(?<layer>Domain|Application|Infrastructure)$")]
    private static partial Regex ModuleName();
}