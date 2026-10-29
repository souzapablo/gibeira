namespace Gibeira.RepositoryTests;

public sealed class HookInstallTests
{
    private static readonly string[] Hooks = ["commit-msg", "pre-commit"];

    [Fact]
    public async Task RestoreInstallsGitHooks()
    {
        await using var repo = await TempRepo.CreateAsync();
        var before = await repo.GitAsync("config", "--local", "core.hooksPath");
        Assert.NotEqual(0, before.ExitCode);

        var restore = await repo.RestoreAsync();

        Assert.True(restore.ExitCode == 0, restore.Output);
        await AssertHooksInstalledAsync(repo);
    }

    [Fact]
    public async Task RestoreTwiceLeavesHooksInstalled()
    {
        await using var repo = await TempRepo.CreateAsync();
        var first = await repo.RestoreAsync();
        Assert.True(first.ExitCode == 0, first.Output);
        var hooksAfterFirst = Hooks.Select(hook => File.ReadAllText(repo.PathOf($".husky/{hook}"))).ToArray();

        var second = await repo.RestoreAsync();

        Assert.True(second.ExitCode == 0, second.Output);
        await AssertHooksInstalledAsync(repo);

        // The restore stamp skips the install on a repeat restore, so run the install itself a second time too.
        var reinstall = await repo.HuskyInstallAsync();

        Assert.True(reinstall.ExitCode == 0, reinstall.Output);
        Assert.Equal(hooksAfterFirst, Hooks.Select(hook => File.ReadAllText(repo.PathOf($".husky/{hook}"))));
        await AssertHooksInstalledAsync(repo);
    }

    // Installed means git itself finds and runs both hooks through core.hooksPath.
    private static async Task AssertHooksInstalledAsync(TempRepo repo)
    {
        var hooksPath = await repo.GitAsync("config", "--local", "core.hooksPath");
        Assert.Equal(".husky", hooksPath.Output.Trim());

        File.WriteAllText(repo.PathOf("probe-message.txt"), "chore: probe\n");
        var commitMsg = await repo.GitAsync("hook", "run", "commit-msg", "--", "probe-message.txt");
        var preCommit = await repo.GitAsync("hook", "run", "pre-commit");

        Assert.True(commitMsg.ExitCode == 0, commitMsg.Output);
        Assert.True(preCommit.ExitCode == 0, preCommit.Output);
    }
}