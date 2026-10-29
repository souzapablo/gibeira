using ArchUnitNET.Domain;

namespace Gibeira.ArchitectureTests;

public sealed record TypeDependency(IType Origin, IType Target)
{
    public string OriginAssembly => Origin.Assembly.Name;

    public string TargetAssembly => Target.Assembly.Name;

    public override string ToString() =>
        $"{Origin.FullName} ({OriginAssembly}) depends on {Target.FullName} ({TargetAssembly})";
}

public static class TypeDependencies
{
    // Every dependency from a loaded type to a type in another assembly.
    public static IEnumerable<TypeDependency> CrossAssembly(Architecture architecture) =>
        architecture.Types
            .Where(type => !type.IsStub)
            .SelectMany(type => type.Dependencies.Select(dependency => new TypeDependency(type, dependency.Target)))
            .Where(dependency => dependency.OriginAssembly != dependency.TargetAssembly)
            .Distinct();
}