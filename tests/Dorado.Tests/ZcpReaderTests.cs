using System.Buffers.Binary;
using System.Text;
using Dorado.Containers;
using Xunit;

namespace Dorado.Tests;

public sealed class ZcpReaderTests
{
    [Fact]
    public void ParsesManifestMetadata()
    {
        byte[] package = SyntheticPackages.BuildZcp(
            guid: "5dc1e614ed1b4d108259bc7e3e926a86",
            executable: "Calculator.exe",
            title: "Calculator",
            description: "Basic and Scientific calculator.");

        ZunePackage result = ZcpReader.Read(package);

        Assert.Equal(ContainerKind.Zcp, result.Kind);
        Assert.Equal("Calculator", result.Metadata.Title);
        Assert.Equal("Calculator.exe", result.Metadata.Executable);
        Assert.Equal("Calculator.exe", result.Metadata.StartupAssembly);
        Assert.Equal("Zune.v3.1", result.Metadata.Platform);
        Assert.Equal("5dc1e614ed1b4d108259bc7e3e926a86", result.Metadata.GameGuid);
        Assert.Equal("Basic and Scientific calculator.", result.Metadata.Description);
        Assert.True(result.IsEncrypted);
        Assert.Equal(0, result.EntryCount);
    }

    [Fact]
    public void DetectsNxMagic()
    {
        byte[] package = SyntheticPackages.BuildZcp("00", "App.exe", "App", "desc");
        Assert.True(ZcpReader.LooksLikeNx(package));
        Assert.False(ZcpReader.LooksLikeNx(new byte[64]));
    }

    [Fact]
    public void RejectsNonNxData()
    {
        Assert.Throws<InvalidDataException>(() => ZcpReader.Read(new byte[0x3000]));
    }
}
