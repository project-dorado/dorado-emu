using Dorado.Containers;
using Xunit;

namespace Dorado.Tests;

/// <summary>
/// Integration tests over the local homebrew corpus (tools/corpus-fetch.sh).
/// They no-op when a fixture is absent so CI stays green without copyrighted
/// content; run the fetch script locally for full coverage.
/// </summary>
public sealed class CcgameReaderTests
{
    [Fact]
    public void ParsesXnaPong()
    {
        string? path = Fixtures.CorpusFile("XNA Pong.ccgame");
        if (path is null)
        {
            return;
        }

        ZunePackage package = ZunePackageReader.Read(path);

        Assert.Equal(ContainerKind.Ccgame, package.Kind);
        Assert.False(package.IsEncrypted);
        Assert.Equal("ZunePong.exe", package.Metadata.StartupAssembly);
        Assert.Equal("Pong", package.Metadata.Title);
        Assert.Equal("Zune.v3.1", package.Metadata.RuntimeProfile);
        Assert.Equal(7, package.EntryCount);

        Assert.True(package.TryGetEntry("ZunePong.exe", out byte[]? exe));
        Assert.Equal((byte)'M', exe![0]);
        Assert.Equal((byte)'Z', exe[1]);

        PackageFile? content = package.Files.FirstOrDefault(f => f.Path.EndsWith("PongGame.xnb", StringComparison.Ordinal));
        Assert.NotNull(content);
        Assert.True(package.TryGetEntry(content!.Path, out byte[]? xnb));
        Assert.Equal("XNB", System.Text.Encoding.ASCII.GetString(xnb!, 0, 3));
    }

    [Fact]
    public void ParsesMultiFolderPackage()
    {
        string? path = Fixtures.CorpusFile("Android in XNA v1.0.ccgame");
        if (path is null)
        {
            return;
        }

        ZunePackage package = ZunePackageReader.Read(path);

        Assert.Equal(ContainerKind.Ccgame, package.Kind);
        Assert.Equal(70, package.EntryCount);
        Assert.Contains("System.dll", package.Files.Select(f => f.Path));

        // Multi-folder MSZIP must inflate every entry to its declared size.
        foreach (PackageFile file in package.Files)
        {
            Assert.True(package.TryGetEntry(file.Path, out byte[]? payload));
            Assert.Equal(file.Size, payload!.Length);
        }
    }

    [Fact]
    public void ParsesSmallPackages()
    {
        foreach (string name in new[] { "Flashlight.ccgame", "Alarm.ccgame" })
        {
            string? path = Fixtures.CorpusFile(name);
            if (path is null)
            {
                continue;
            }

            ZunePackage package = ZunePackageReader.Read(path);
            Assert.Equal(ContainerKind.Ccgame, package.Kind);
            Assert.EndsWith(".exe", package.Metadata.StartupAssembly!, StringComparison.OrdinalIgnoreCase);
            Assert.True(package.EntryCount > 0);
        }
    }

    [Fact]
    public void RejectsUnknownContainers()
    {
        Assert.Throws<InvalidDataException>(() => ZunePackageReader.Read(new byte[64]));
    }
}
