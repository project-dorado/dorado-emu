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

    /// <summary>
    /// Locates the plaintext XNA runtime ZCP. It is Microsoft content and is
    /// never committed: tests that need it no-op when it is absent so CI stays
    /// green (AGENTS.md invariant 5).
    /// </summary>
    public static string? RuntimeZcp()
    {
        string? fromEnvironment = Environment.GetEnvironmentVariable("DORADO_RUNTIME_ZCP");
        if (!string.IsNullOrWhiteSpace(fromEnvironment) && File.Exists(fromEnvironment))
        {
            return fromEnvironment;
        }

        string[] candidates =
        [
            Path.Combine(Corpus, "runtimeZune.v3.1.zcp"),
            Path.Combine(RepoRoot, "..", "zune-hd-disassembly", "assets", "runtimeZune.v3.1.zcp"),
        ];
        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
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
