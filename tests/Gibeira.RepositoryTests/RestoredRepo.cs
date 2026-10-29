namespace Gibeira.RepositoryTests;

// A TempRepo restored once (hooks installed) with its files committed, shared by a test class.
public sealed class RestoredRepo : IAsyncLifetime
{
    public TempRepo Repo { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Repo = await TempRepo.CreateAsync();

        var restore = await Repo.RestoreAsync();
        if (restore.ExitCode != 0)
        {
            throw new InvalidOperationException($"dotnet restore failed: {restore.Output}");
        }

        var hooksPath = await Repo.GitAsync("config", "--local", "core.hooksPath");
        if (hooksPath.Output.Trim() != ".husky")
        {
            throw new InvalidOperationException($"dotnet restore did not install the hooks: {restore.Output}");
        }

        // Commit the copied tree so it is tracked, as in a clone; tests then clean up only what they add.
        await Repo.GitAsync("add", "--all");
        var initial = await Repo.GitAsync("commit", "--no-verify", "-m", "chore: initial commit");
        if (initial.ExitCode != 0)
        {
            throw new InvalidOperationException($"initial commit failed: {initial.Output}");
        }
    }

    public ValueTask DisposeAsync() => Repo.DisposeAsync();
}