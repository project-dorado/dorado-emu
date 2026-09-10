namespace Dorado.Tests;

/// <summary>Locates locally-fetched corpus fixtures (see tools/corpus-fetch.sh).</summary>
internal static class Fixtures
{
    public static string RepoRoot { get; } = FindRepoRoot();

    public static string Corpus { get; } = Path.Combine(RepoRoot, "corpus");

    public static string? CorpusFile(string name)
    {
        string path = Path.Combine(Corpus, name);
        return File.Exists(path) ? path : null;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Dorado.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
