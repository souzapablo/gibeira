namespace Gibeira.RepositoryTests;

public static class RepositoryRoot
{
    // The nearest directory above the test output that holds Gibeira.slnx.
    public static string Find()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Gibeira.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException($"Gibeira.slnx not found above {AppContext.BaseDirectory}.");
    }

    // The files a clone would hold: tracked plus untracked-but-not-ignored, as `/`-separated relative paths.
    public static async Task<IReadOnlyList<string>> ListFilesAsync(string root, CancellationToken cancellationToken)
    {
        var result = await ProcessRunner.RunAsync("git", ["-c", "core.quotePath=false", "ls-files", "--cached", "--others", "--exclude-standard"], root, cancellationToken: cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"git ls-files failed: {result.Output}");
        }

        return [.. result.Output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .Where(path => File.Exists(Path.Combine(root, path)))];
    }
}