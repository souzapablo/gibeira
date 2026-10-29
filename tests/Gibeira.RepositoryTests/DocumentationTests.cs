using System.Text.RegularExpressions;

namespace Gibeira.RepositoryTests;

public sealed partial class DocumentationTests
{
    private static readonly string Root = RepositoryRoot.Find();

    [Fact]
    public void ReadmeDocumentsBuildTestAndRun()
    {
        var readme = Read("README.md");

        Assert.Contains("## Prerequisites", readme, StringComparison.Ordinal);
        Assert.Contains(".NET 10 SDK", readme, StringComparison.Ordinal);
        Assert.Contains("dotnet build Gibeira.slnx", readme, StringComparison.Ordinal);
        Assert.Contains("dotnet test Gibeira.slnx", readme, StringComparison.Ordinal);
        Assert.Contains("dotnet run --project src/Host/Gibeira.AppHost", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void ContributingDocumentsConventions()
    {
        var contributing = Read("CONTRIBUTING.md");

        // Commit message format (CONV-02..05).
        Assert.Contains("type(scope)!: subject", contributing, StringComparison.Ordinal);
        Assert.Contains("feat fix docs style refactor perf test build ci chore revert", contributing, StringComparison.Ordinal);
        Assert.Contains("at most 100 characters", contributing, StringComparison.Ordinal);
        // Hook setup (CONV-01).
        Assert.Contains("dotnet restore Gibeira.slnx", contributing, StringComparison.Ordinal);
        Assert.Contains("core.hooksPath", contributing, StringComparison.Ordinal);
        // Module template (TMPL-01).
        Assert.Contains("dotnet new install ./templates/module", contributing, StringComparison.Ordinal);
        Assert.Contains("dotnet new gibeira-module -n", contributing, StringComparison.Ordinal);
    }

    [Fact]
    public void ArchitectureDocListsEveryRule()
    {
        var architecture = Read("docs/architecture.md");
        var specRules = Enumerable.Range(1, 10).Select(n => $"ARCH-{n:00}");
        var testedRules = Directory.GetFiles(Path.Combine(Root, "tests", "Gibeira.ArchitectureTests"), "*.cs")
            .SelectMany(file => RuleId().Matches(File.ReadAllText(file)).Select(match => match.Value));

        Assert.All(specRules.Union(testedRules), rule =>
            Assert.Matches($@"\| {rule} \| \S", architecture));
    }

    [Fact]
    public void LicenseIsMit()
    {
        var license = Read("LICENSE");

        Assert.StartsWith("MIT License", license, StringComparison.Ordinal);
        Assert.Contains("Copyright (c) 2026 Pablo Souza", license, StringComparison.Ordinal);
        Assert.Contains("Permission is hereby granted, free of charge, to any person obtaining a copy", license, StringComparison.Ordinal);
        Assert.Contains("The above copyright notice and this permission notice shall be included in all", license, StringComparison.Ordinal);
        Assert.Contains("THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND", license, StringComparison.Ordinal);
    }

    private static string Read(string relativePath)
    {
        var path = Path.Combine(Root, relativePath);
        Assert.True(File.Exists(path), $"{relativePath} is missing");
        return File.ReadAllText(path).ReplaceLineEndings("\n");
    }

    [GeneratedRegex(@"ARCH-\d\d")]
    private static partial Regex RuleId();
}