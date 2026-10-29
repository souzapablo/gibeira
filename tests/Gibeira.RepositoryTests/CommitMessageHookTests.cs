namespace Gibeira.RepositoryTests;

public sealed class CommitMessageHookTests(RestoredRepo fixture) : IClassFixture<RestoredRepo>
{
    private TempRepo Repo => fixture.Repo;

    [Theory]
    [InlineData("added stuff")]
    [InlineData("feature: add accounts")]
    [InlineData("feat add accounts")]
    [InlineData("feat: ")]
    public async Task NonConventionalHeaderIsRejected(string message)
    {
        var (result, committed) = await CommitAsync(message);

        Assert.False(committed, $"'{message}' was committed");
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("type(scope)!: subject", result.Output, StringComparison.Ordinal);
        Assert.Contains("feat fix docs style refactor perf test build ci chore revert", result.Output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task HeaderLongerThan100CharactersIsRejected()
    {
        var tooLong = "feat: " + new string('a', 95);
        var longest = "feat: " + new string('a', 94);
        Assert.Equal(101, tooLong.Length);
        Assert.Equal(100, longest.Length);

        var (rejected, rejectedCommitted) = await CommitAsync(tooLong);
        var (accepted, acceptedCommitted) = await CommitAsync(longest);

        Assert.False(rejectedCommitted, rejected.Output);
        Assert.NotEqual(0, rejected.ExitCode);
        Assert.True(acceptedCommitted, accepted.Output);
    }

    [Theory]
    [InlineData("chore: test")]
    [InlineData("docs: describe the ledger")]
    [InlineData("style: align the home page")]
    [InlineData("refactor: split the loader")]
    [InlineData("perf: cache the architecture")]
    [InlineData("test: cover the hook")]
    [InlineData("build: pin the SDK")]
    [InlineData("ci: add a workflow")]
    [InlineData("revert: drop the probe")]
    [InlineData("feat(ledger)!: add account import")]
    [InlineData("fix(web): handle an empty page\n\nThe home page rendered nothing when the module list was empty.\n\nRefs: #12\nCo-Authored-By: Someone <someone@example.com>")]
    public async Task ConventionalHeaderIsAccepted(string message)
    {
        var (result, committed) = await CommitAsync(message);

        Assert.True(committed, result.Output);
        Assert.Equal(0, result.ExitCode);
        var subject = await Repo.GitAsync("log", "-1", "--format=%s");
        Assert.Equal(message.Split('\n')[0], subject.Output.Trim());
    }

    [Fact]
    public async Task GitGeneratedMessagesAreAccepted()
    {
        Assert.Equal(0, (await Repo.GitAsync("checkout", "-b", "side")).ExitCode);
        File.WriteAllText(Repo.PathOf("side.txt"), "side change\n");
        Assert.Equal(0, (await Repo.GitAsync("add", "side.txt")).ExitCode);
        Assert.True((await CommitAsync("feat: add side change")).Committed);
        Assert.Equal(0, (await Repo.GitAsync("checkout", "main")).ExitCode);

        var merge = await Repo.GitAsync("merge", "--no-ff", "--no-edit", "side");
        var mergeSubject = await Repo.GitAsync("log", "-1", "--format=%s");

        Assert.True(merge.ExitCode == 0, merge.Output);
        Assert.StartsWith("Merge branch 'side'", mergeSubject.Output.Trim(), StringComparison.Ordinal);

        var revert = await Repo.GitAsync("revert", "--no-edit", "-m", "1", "HEAD");
        var revertSubject = await Repo.GitAsync("log", "-1", "--format=%s");

        Assert.True(revert.ExitCode == 0, revert.Output);
        Assert.StartsWith("Revert \"Merge branch 'side'", revertSubject.Output.Trim(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HookWithoutDotnetNamesIt()
    {
        var withoutDotnet = new Dictionary<string, string?> { ["PATH"] = PathWithoutDotnet() };
        File.WriteAllText(Repo.PathOf(".git/probe-message"), "chore: probe\n");

        var commit = await Repo.RunAsync("git", ["commit", "--allow-empty", "-m", "chore: probe"], withoutDotnet);
        var commitMsg = await Repo.RunAsync("git", ["hook", "run", "commit-msg", "--", ".git/probe-message"], withoutDotnet);

        Assert.NotEqual(0, commit.ExitCode);
        Assert.Contains("dotnet SDK not found", commit.Output, StringComparison.Ordinal);
        Assert.NotEqual(0, commitMsg.ExitCode);
        Assert.Contains("dotnet SDK not found", commitMsg.Output, StringComparison.Ordinal);
    }

    private async Task<(ProcessResult Result, bool Committed)> CommitAsync(string message)
    {
        var before = await Repo.GitAsync("rev-parse", "HEAD");
        File.WriteAllText(Repo.PathOf(".git/test-message"), message + "\n");

        var result = await Repo.GitAsync("commit", "--allow-empty", "-F", ".git/test-message");

        var after = await Repo.GitAsync("rev-parse", "HEAD");
        return (result, before.Output != after.Output);
    }

    private static string PathWithoutDotnet() =>
        string.Join(Path.PathSeparator, (Environment.GetEnvironmentVariable("PATH") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(directory => !File.Exists(Path.Combine(directory, "dotnet.exe")) && !File.Exists(Path.Combine(directory, "dotnet"))));
}