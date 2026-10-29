using System.Collections.Concurrent;

using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

namespace Gibeira.ArchitectureTests;

// Loads every `<prefix>.*` assembly built into the test output, except test assemblies.
// `Gibeira` selects the real solution, `Fixture` the deliberate violations (AD-002).
public static class SolutionArchitecture
{
    public const string SolutionPrefix = "Gibeira";
    public const string FixturePrefix = "Fixture";

    private static readonly ConcurrentDictionary<string, Lazy<Architecture>> Cache = new();

    public static Architecture Load(string prefix) =>
        Cache.GetOrAdd(prefix, p => new Lazy<Architecture>(() => Build(p))).Value;

    // Reads the files with Mono.Cecil rather than loading them into the test process,
    // which Windows App Control can block for unsigned fixture assemblies.
    private static Architecture Build(string prefix)
    {
        var files = Directory.GetFiles(AppContext.BaseDirectory, $"{prefix}.*.dll")
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(file => !Path.GetFileNameWithoutExtension(file).EndsWith("Tests", StringComparison.Ordinal))
            .ToArray();

        if (files.Length == 0)
        {
            throw new InvalidOperationException($"No {prefix}.*.dll found in {AppContext.BaseDirectory}.");
        }

        var loader = new ArchLoader();
        foreach (var file in files)
        {
            loader.LoadFilteredDirectory(AppContext.BaseDirectory, file);
        }

        return loader.Build();
    }
}