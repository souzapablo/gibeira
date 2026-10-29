using ArchUnitNET.Domain;

namespace Gibeira.ArchitectureTests;

public static class BoundaryRules
{
    // ARCH-04: no assembly of one module depends on an assembly of another module.
    public static IReadOnlyList<string> CrossModuleDependency(Architecture architecture, string prefix) =>
        [.. TypeDependencies.CrossAssembly(architecture)
            .Select(d => (Dependency: d, Origin: ModuleAssembly.Parse(d.OriginAssembly, prefix), Target: ModuleAssembly.Parse(d.TargetAssembly, prefix)))
            .Where(x => x.Origin is not null && x.Target is not null && x.Origin.Module != x.Target.Module)
            .Select(x => $"Module {x.Origin!.Module} depends on module {x.Target!.Module}: {x.Dependency}")];

    // ARCH-05: the shared kernel depends on no module and on neither host.
    public static IReadOnlyList<string> SharedKernelDependingOnModulesOrHosts(Architecture architecture, string prefix) =>
        [.. TypeDependencies.CrossAssembly(architecture)
            .Where(d => d.OriginAssembly == $"{prefix}.SharedKernel"
                && (ModuleAssembly.Parse(d.TargetAssembly, prefix) is not null
                    || d.TargetAssembly == $"{prefix}.Web"
                    || d.TargetAssembly == $"{prefix}.AppHost"))
            .Select(d => d.ToString())];

    // ARCH-06: only the AppHost depends on the Web host.
    public static IReadOnlyList<string> DependencyOnWebOutsideAppHost(Architecture architecture, string prefix) =>
        [.. TypeDependencies.CrossAssembly(architecture)
            .Where(d => d.TargetAssembly == $"{prefix}.Web" && d.OriginAssembly != $"{prefix}.AppHost")
            .Select(d => d.ToString())];
}