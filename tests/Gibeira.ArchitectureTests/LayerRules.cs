using ArchUnitNET.Domain;

namespace Gibeira.ArchitectureTests;

public static class LayerRules
{
    private static readonly string[] FrameworkNamespaces = ["Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore"];

    // ARCH-01: a Domain assembly does not depend on its module's Application or Infrastructure.
    public static IReadOnlyList<string> DomainDependingOnOuterLayers(Architecture architecture, string prefix) =>
        WithinModule(architecture, prefix, (origin, target) =>
            origin.Layer == ModuleAssembly.Domain
            && target.Layer is ModuleAssembly.Application or ModuleAssembly.Infrastructure);

    // ARCH-02: an Application assembly does not depend on its module's Infrastructure.
    public static IReadOnlyList<string> ApplicationDependingOnInfrastructure(Architecture architecture, string prefix) =>
        WithinModule(architecture, prefix, (origin, target) =>
            origin.Layer == ModuleAssembly.Application && target.Layer == ModuleAssembly.Infrastructure);

    // ARCH-03: Domain and Application assemblies do not depend on ASP.NET Core or EF Core.
    public static IReadOnlyList<string> InnerLayersUsingFrameworks(Architecture architecture, string prefix) =>
        [.. TypeDependencies.CrossAssembly(architecture)
            .Where(d => ModuleAssembly.Parse(d.OriginAssembly, prefix)?.Layer is ModuleAssembly.Domain or ModuleAssembly.Application
                && FrameworkNamespaces.Any(ns => IsWithin(d.Target.Namespace.FullName, ns)))
            .Select(d => d.ToString())];

    private static string[] WithinModule(Architecture architecture, string prefix, Func<ModuleAssembly, ModuleAssembly, bool> forbidden) =>
        [.. TypeDependencies.CrossAssembly(architecture)
            .Select(d => (Dependency: d, Origin: ModuleAssembly.Parse(d.OriginAssembly, prefix), Target: ModuleAssembly.Parse(d.TargetAssembly, prefix)))
            .Where(x => x.Origin is not null && x.Target is not null
                && x.Origin.Module == x.Target.Module
                && forbidden(x.Origin, x.Target))
            .Select(x => x.Dependency.ToString())];

    private static bool IsWithin(string @namespace, string root) =>
        @namespace == root || @namespace.StartsWith(root + ".", StringComparison.Ordinal);
}