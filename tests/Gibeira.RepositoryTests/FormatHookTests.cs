namespace Gibeira.RepositoryTests;

public sealed class FormatHookTests(RestoredRepo fixture) : IClassFixture<RestoredRepo>
{
    private const string KernelDirectory = "src/Shared/Gibeira.SharedKernel";

    private TempRepo Repo => fixture.Repo;

    [Fact]
    public async Task UnformattedStagedCsFileIsRejected()
    {
        try
        {
            await StageAsync($"{KernelDirectory}/Messy.cs", Unformatted("Messy"));

            var (result, committed) = await CommitAsync("chore: add messy file");

            Assert.False(committed, result.Output);
            Assert.NotEqual(0, result.ExitCode);
            Assert.True(result.Output.Contains("Messy.cs", StringComparison.Ordinal), result.Output);
        }
        finally
        {
            await CleanAsync();
        }
    }

    [Fact]
    public async Task CommitWithoutCsFilesSkipsFormat()
    {
        var solution = Repo.PathOf("Gibeira.slnx");
        var original = File.ReadAllText(solution);
        try
        {
            // dotnet format cannot open a broken solution, so the commit only passes if it never runs.
            File.WriteAllText(solution, "not a solution");
            await StageAsync("notes.md", "# Notes\n");

            var (result, committed) = await CommitAsync("docs: add notes");

            Assert.True(committed, result.Output);
        }
        finally
        {
            File.WriteAllText(solution, original);
            await CleanAsync();
        }
    }

    [Fact]
    public async Task UnstagedFilesAreNotChecked()
    {
        try
        {
            File.WriteAllText(Repo.PathOf($"{KernelDirectory}/Unstaged.cs"), Unformatted("Unstaged"));
            await StageAsync($"{KernelDirectory}/Staged.cs", Formatted("Staged"));

            var (result, committed) = await CommitAsync("chore: add staged file");

            Assert.True(committed, result.Output);
        }
        finally
        {
            await CleanAsync();
        }
    }

    private static string Formatted(string type) => $"namespace Gibeira.SharedKernel;\r\n\r\npublic sealed class {type};";

    private static string Unformatted(string type) => $"namespace Gibeira.SharedKernel;\r\n\r\npublic   sealed class {type}   {{   }}";

    private async Task StageAsync(string relativePath, string content)
    {
        File.WriteAllText(Repo.PathOf(relativePath), content);
        var add = await Repo.GitAsync("add", relativePath);
        Assert.True(add.ExitCode == 0, add.Output);
    }

    private async Task<(ProcessResult Result, bool Committed)> CommitAsync(string message)
    {
        var before = await Repo.GitAsync("rev-parse", "HEAD");
        var result = await Repo.GitAsync("commit", "-m", message);
        var after = await Repo.GitAsync("rev-parse", "HEAD");
        return (result, before.Output != after.Output);
    }

    // Drops whatever a test staged or left in the working tree, keeping the shared repo clean for the next test.
    private async Task CleanAsync()
    {
        await Repo.GitAsync("reset", "--quiet");
        await Repo.GitAsync("clean", "--force", "--quiet", "--", KernelDirectory, "notes.md");
    }
}