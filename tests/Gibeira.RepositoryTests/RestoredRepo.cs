namespace Gibeira.RepositoryTests;

// A TempRepo restored once (hooks installed) with one initial commit, shared by a test class.
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

        var initial = await Repo.GitAsync("commit", "--allow-empty", "--no-verify", "-m", "chore: initial commit");
        if (initial.ExitCode != 0)
        {
            throw new InvalidOperationException($"initial commit failed: {initial.Output}");
        }
    }

    public ValueTask DisposeAsync() => Repo.DisposeAsync();
}