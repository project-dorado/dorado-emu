using Dorado.Containers;
using Dorado.Platform;
using Dorado.Platform.Desktop.Software;
using Dorado.Runtime;

namespace Dorado.Tests;

/// <summary>
/// Runs official Zune HD framework-path titles from the external decompiled
/// corpus. The corpus is Microsoft content and the native ZDK bridge is built
/// locally, so the tests no-op when either is absent (AGENTS.md invariant 5).
/// </summary>
public sealed class OfficialAppSmokeTests
{
    // Titles that spin their own background threads (checkers, calendar and
    // other ZuneGames/ZuneCore titles) are exercised by
    // tools/smoke_official.py in a separate process: an unhandled worker
    // exception would otherwise take the whole test host down.
    [Theory]
    [InlineData("calculator", "calculator")]
    [InlineData("alarm", "alarm_clock")]
    [InlineData("solitaire", "solitaire")]
    public void FrameworkPathTitleRunsHeadless(string slug, string corpusDirectory)
    {
        string? appDirectory = OfficialCorpus.AppDirectory(corpusDirectory);
        string? zdk = OfficialCorpus.ZdkLibrary();
        if (appDirectory is null || zdk is null)
        {
            return;
        }

        string? previousZdk = Environment.GetEnvironmentVariable("DORADO_ZDK_LIB");
        string? previousFonts = Environment.GetEnvironmentVariable("DORADO_FONT_DIR");
        string previousStorage = PlatformHost.StorageRoot;
        try
        {
            Environment.SetEnvironmentVariable("DORADO_ZDK_LIB", zdk);
            string? fonts = OfficialCorpus.FontDirectory();
            if (fonts is not null)
            {
                Environment.SetEnvironmentVariable("DORADO_FONT_DIR", fonts);
            }

            PlatformHost.StorageRoot = Path.Combine(Path.GetTempPath(), "dorado-test-saves");

            ZunePackage package = ZunePackageReader.Read(appDirectory);
            var backend = new SoftwareGraphicsBackend();
            ZuneRunResult result = ZuneAppRunner.Run(package, new ZuneRunOptions
            {
                Graphics = backend,
                FrameLimit = 3,
                GameTitle = slug,
            });

            Assert.True(result.FramesRendered >= 3, $"{slug} rendered {result.FramesRendered} frame(s).");
            Assert.False(string.IsNullOrWhiteSpace(result.EntryPoint));
        }
        finally
        {
            Environment.SetEnvironmentVariable("DORADO_ZDK_LIB", previousZdk);
            Environment.SetEnvironmentVariable("DORADO_FONT_DIR", previousFonts);
            PlatformHost.StorageRoot = previousStorage;
        }
    }

    private static class OfficialCorpus
    {
        private static readonly string? Root = FindCorpus();

        public static string? AppDirectory(string corpusDirectory)
        {
            if (Root is null)
            {
                return null;
            }

            string gametitle = Path.Combine(Root, corpusDirectory, "gametitle");
            if (!Directory.Exists(gametitle))
            {
                return null;
            }

            return Directory.EnumerateDirectories(gametitle).FirstOrDefault();
        }

        public static string? ZdkLibrary()
        {
            string? fromEnvironment = Environment.GetEnvironmentVariable("DORADO_ZDK_LIB");
            if (!string.IsNullOrWhiteSpace(fromEnvironment) && File.Exists(fromEnvironment))
            {
                return fromEnvironment;
            }

            string candidate = Path.Combine(Fixtures.RepoRoot, "native", "zdk-bridge", "libZDK.so");
            return File.Exists(candidate) ? candidate : null;
        }

        public static string? FontDirectory()
        {
            string? fromEnvironment = Environment.GetEnvironmentVariable("DORADO_FONT_DIR");
            if (!string.IsNullOrWhiteSpace(fromEnvironment) && Directory.Exists(fromEnvironment))
            {
                return fromEnvironment;
            }

            string candidate = Path.GetFullPath(Path.Combine(
                Fixtures.RepoRoot, "..", "zune-hd-disassembly", "assets", "fonts"));
            return Directory.Exists(candidate) ? candidate : null;
        }

        private static string? FindCorpus()
        {
            string? fromEnvironment = Environment.GetEnvironmentVariable("DORADO_OFFICIAL_APPS");
            if (!string.IsNullOrWhiteSpace(fromEnvironment) && Directory.Exists(fromEnvironment))
            {
                return fromEnvironment;
            }

            string candidate = Path.GetFullPath(Path.Combine(
                Fixtures.RepoRoot, "..", "Zune HD Apps (Decompiled)"));
            return Directory.Exists(candidate) ? candidate : null;
        }
    }
}
