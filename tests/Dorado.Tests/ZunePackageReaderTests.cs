using System.Text;
using Dorado.Containers;
using Xunit;

namespace Dorado.Tests;

public sealed class ZunePackageReaderTests
{
    [Fact]
    public void ReadsExtractedApplicationTree()
    {
        string root = CreateTree();
        try
        {
            ZunePackage package = ZunePackageReader.Read(Path.Combine(root, "gametitle", "584E07D1"));

            Assert.Equal(ContainerKind.Directory, package.Kind);
            Assert.False(package.IsEncrypted);
            Assert.True(package.CanReadEntries);
            Assert.Equal("My App", package.Metadata.Title);
            Assert.Equal("MyApp.exe", package.Metadata.Executable);
            Assert.Equal(2, package.Files.Count);

            Assert.True(package.TryGetEntry("MyApp.exe", out byte[]? executable));
            Assert.Equal("MZ", Encoding.ASCII.GetString(executable, 0, 2));
            Assert.True(package.TryGetEntry("Content/data.xnb", out byte[]? content));
            Assert.Equal("XNB", Encoding.ASCII.GetString(content, 0, 3));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void DirectoryReadPrioritisesDefaultExecutable()
    {
        string root = Path.Combine(Path.GetTempPath(), "dorado-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllBytes(Path.Combine(root, "Other.exe"), [0x4D, 0x5A]);
            File.WriteAllBytes(Path.Combine(root, "default.exe"), [0x4D, 0x5A]);

            ZunePackage package = ZunePackageReader.Read(root);

            Assert.Equal("default.exe", package.Metadata.Executable);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string CreateTree()
    {
        string root = Path.Combine(Path.GetTempPath(), "dorado-tests", Guid.NewGuid().ToString("N"));
        string payload = Path.Combine(root, "gametitle", "584E07D1");
        Directory.CreateDirectory(Path.Combine(payload, "Content"));
        File.WriteAllBytes(Path.Combine(payload, "MyApp.exe"), [0x4D, 0x5A, 0x90]);
        File.WriteAllBytes(Path.Combine(payload, "Content", "data.xnb"), [0x58, 0x4E, 0x42]);
        File.WriteAllText(
            Path.Combine(root, "gametitle", "gameinfo.xml"),
            "<GameInfo><Title>My App</Title></GameInfo>");
        return root;
    }
}
