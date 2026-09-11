using Dorado.Containers;
using Dorado.Containers.Drm;
using Xunit;

namespace Dorado.Tests;

public sealed class ZcstfsReaderTests
{
    private sealed class FixedKeyProvider(byte[] key) : IDrmKeyProvider
    {
        public byte[]? TryGetContentKey(ReadOnlySpan<byte> packageGuid, ReadOnlySpan<byte> containerHeader) => key;
    }

    [Fact]
    public void ParsesPlaintextVolume()
    {
        byte[] package = SyntheticPackages.BuildZcstfs();

        ZunePackage result = ZcpReader.Read(package);

        Assert.False(result.IsEncrypted);
        Assert.True(result.CanReadEntries);
        Assert.NotNull(result.Volume);
        Assert.Equal(2, result.Files.Count);

        Assert.True(result.TryGetEntry(SyntheticPackages.Zcstfs.FirstPath, out byte[]? first));
        Assert.Equal(SyntheticPackages.Zcstfs.FirstLength, first.Length);
        Assert.Equal(0x11, first[0]);
        Assert.NotEqual(0, first[^1]);

        Assert.True(result.TryGetEntry(SyntheticPackages.Zcstfs.SecondPath, out byte[]? second));
        Assert.Equal(SyntheticPackages.Zcstfs.SecondLength, second.Length);
    }

    [Fact]
    public void DecryptsVolumeWhenKeyProvided()
    {
        byte[] key = new byte[32];
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)(0x40 + i);
        }

        byte[] plain = SyntheticPackages.BuildZcstfs();
        byte[] encrypted = SyntheticPackages.BuildZcstfs(encrypt: true, key: key);

        ZunePackage result = ZcpReader.Read(encrypted, new FixedKeyProvider(key));

        Assert.False(result.IsEncrypted);
        Assert.True(result.TryGetEntry(SyntheticPackages.Zcstfs.FirstPath, out byte[]? first));
        Assert.True(result.TryGetEntry(SyntheticPackages.Zcstfs.SecondPath, out byte[]? second));

        // The decrypted volume must match the plaintext build byte-for-byte.
        ZunePackage expected = ZcpReader.Read(plain);
        Assert.True(expected.TryGetEntry(SyntheticPackages.Zcstfs.FirstPath, out byte[]? expectedFirst));
        Assert.True(expected.TryGetEntry(SyntheticPackages.Zcstfs.SecondPath, out byte[]? expectedSecond));
        Assert.Equal(expectedFirst, first);
        Assert.Equal(expectedSecond, second);
    }

    [Fact]
    public void ReportsEncryptedWithoutKey()
    {
        byte[] key = new byte[32];
        byte[] encrypted = SyntheticPackages.BuildZcstfs(encrypt: true, key: key);

        ZunePackage result = ZcpReader.Read(encrypted);

        Assert.True(result.IsEncrypted);
        Assert.False(result.CanReadEntries);
        Assert.NotNull(result.Volume);
        Assert.True(result.Volume!.IsEncrypted);
        Assert.Empty(result.Volume.Files);
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(260, 2)]
    [InlineData(682, 2)]
    [InlineData(683, 3)]
    [InlineData(4889, 9)]
    public void CountsHashBlocks(int blocks, int expected) =>
        Assert.Equal(expected, ZcstfsReader.CountHashBlocks(blocks));

    [Fact]
    public void CorruptDescriptorFallsBackToManifestOnly()
    {
        byte[] package = SyntheticPackages.BuildZcstfs();
        // Break the volume descriptor: the container is no longer recognised as
        // a ZCSTFS volume, so the reader falls back to the NX manifest path.
        package[0xB4] = 0x10;

        ZunePackage result = ZcpReader.Read(package);

        Assert.Null(result.Volume);
        Assert.False(result.CanReadEntries);
    }

    [Fact]
    public void ExtractsRuntimeVolumeFixture()
    {
        string? fixture = Fixtures.RuntimeZcp();
        if (fixture is null)
        {
            return; // corpus not present: stay green in CI (see AGENTS.md invariant 5)
        }

        ZunePackage result = ZcpReader.Read(fixture);

        Assert.False(result.IsEncrypted);
        Assert.Equal(10, result.Files.Count);
        Assert.Contains(result.Files, f => f.Path == "Microsoft.Xna.Framework.dll");
        Assert.Contains(result.Files, f => f.Path == "mscorlib.dll");

        Assert.True(result.TryGetEntry("Microsoft.Xna.Framework.dll", out byte[]? payload));
        Assert.True(payload.Length > 0x1000);
        Assert.Equal((byte)'M', payload[0]);
        Assert.Equal((byte)'Z', payload[1]);
        int peOffset = BitConverter.ToInt32(payload, 0x3C);
        Assert.Equal("PE\0\0", System.Text.Encoding.ASCII.GetString(payload, peOffset, 4));

        // Every stored block's SHA-1 must match its hash entry.
        ZcstfsVolume volume = result.Volume!;
        int stored = volume.TotalBlockCount - volume.FreeBlockCount;
        for (int i = 0; i < stored; i++)
        {
            long offset = volume.BlockOffset(i);
            byte[] actual = System.Security.Cryptography.SHA1.HashData(
                volume.Data.AsSpan((int)offset, ZcstfsReader.BlockSize));
            Assert.Equal(volume.HashEntries[i].Sha1, actual);
        }
    }
}
