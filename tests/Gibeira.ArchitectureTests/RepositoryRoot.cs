namespace Gibeira.ArchitectureTests;

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
}