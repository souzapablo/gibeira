namespace Gibeira.RepositoryTests;

// A fresh git repository holding a copy of the files a clone of this repository would hold.
public sealed class TempRepo : IAsyncDisposable
{
    private TempRepo(string root) => Root = root;

    public string Root { get; }

    public static async Task<TempRepo> CreateAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var source = RepositoryRoot.Find();
        var repo = new TempRepo(Directory.CreateTempSubdirectory("gibeira-repo-").FullName);

        foreach (var file in await RepositoryRoot.ListFilesAsync(source, cancellationToken))
        {
            var target = Path.Combine(repo.Root, file);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(Path.Combine(source, file), target);
        }

        await repo.GitAsync("init", "-b", "main");
        await repo.GitAsync("config", "user.name", "Gibeira Tests");
        await repo.GitAsync("config", "user.email", "tests@gibeira.invalid");
        await repo.GitAsync("config", "commit.gpgsign", "false");
        return repo;
    }

    public Task<ProcessResult> RunAsync(string fileName, IEnumerable<string> arguments, IReadOnlyDictionary<string, string?>? environment = null) =>
        ProcessRunner.RunAsync(fileName, arguments, Root, environment, TestContext.Current.CancellationToken);

    public Task<ProcessResult> GitAsync(params string[] arguments) => RunAsync("git", arguments);

    // HUSKY=0 in the developer's environment would skip the install under test.
    public Task<ProcessResult> RestoreAsync() =>
        RunAsync("dotnet", ["restore", "Gibeira.slnx"], new Dictionary<string, string?> { ["HUSKY"] = null });

    public string PathOf(string relativePath) => Path.Combine(Root, relativePath);

    public ValueTask DisposeAsync()
    {
        // git marks its object files read-only, which Directory.Delete refuses on Windows.
        foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(Root, recursive: true);
        return ValueTask.CompletedTask;
    }
}