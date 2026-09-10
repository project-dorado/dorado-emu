using System.Buffers.Binary;
using System.Text;
using Dorado.Containers;
using Xunit;

namespace Dorado.Tests;

public sealed class ZcpReaderTests
{
    private const int ManifestOffset = 0x2800;
    private const int RecordHeaderSize = 0x1F0;

    [Fact]
    public void ParsesManifestMetadata()
    {
        byte[] package = BuildNx(
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
        byte[] package = BuildNx("00", "App.exe", "App", "desc");
        Assert.True(ZcpReader.LooksLikeNx(package));
        Assert.False(ZcpReader.LooksLikeNx(new byte[64]));
    }

    [Fact]
    public void RejectsNonNxData()
    {
        Assert.Throws<InvalidDataException>(() => ZcpReader.Read(new byte[0x3000]));
    }

    private static byte[] BuildNx(string guid, string executable, string title, string description)
    {
        var data = new byte[ManifestOffset + RecordHeaderSize + 0x800];

        // NX header is eight little-endian u32 fields; write by byte offset.
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(0 * 4), 2);          // version
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4 * 4), 0x1F0);      // signature offset
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(6 * 4), ManifestOffset); // manifest offset
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(7 * 4), 0x1DEF);     // signature length
        data[0x32] = (byte)'N';
        data[0x33] = (byte)'X';

        int cursor = ManifestOffset + RecordHeaderSize;

        byte[] execPayload = Encoding.ASCII.GetBytes(
            guid.PadRight(32)[..32] + "\0" + executable + "\0Zune.v3.1");
        cursor = WriteRecord(data, cursor, 0x0409, "EXEC"u8, execPayload);

        byte[] titlPayload = Encoding.Unicode.GetBytes(title + "\0" + description);
        WriteRecord(data, cursor, 0, "TITL"u8, titlPayload);

        return data;
    }

    private static int WriteRecord(byte[] data, int offset, ushort lang, ReadOnlySpan<byte> tag, byte[] payload)
    {
        if (lang != 0)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), lang);
            offset += 2;
        }

        tag.CopyTo(data.AsSpan(offset));
        offset += 4;
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), (uint)payload.Length);
        offset += 4;
        payload.CopyTo(data, offset);
        return offset + payload.Length;
    }
}
