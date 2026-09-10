using System.IO.Compression;
using System.Text;
using Dorado.Containers.Internal;
using Xunit;

namespace Dorado.Tests;

public sealed class PuffTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(64)]
    [InlineData(4096)]
    [InlineData(100_000)]
    public void RoundTripsRawDeflate(int size)
    {
        var data = new byte[size];
        new Random(size + 1).NextBytes(data);

        byte[] compressed = RawDeflate(data);
        byte[] result = Puff.Inflate(compressed, compressed.Length, Array.Empty<byte>(), data.Length);

        Assert.Equal(data, result);
    }

    [Fact]
    public void RoundTripsHighlyCompressibleData()
    {
        byte[] data = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("dorado-zune-hd-", 8000)));
        byte[] compressed = RawDeflate(data);

        Assert.True(compressed.Length < data.Length);
        Assert.Equal(data, Puff.Inflate(compressed, compressed.Length, Array.Empty<byte>(), data.Length));
    }

    [Fact]
    public void DecodesMultipleFlushedBlocks()
    {
        // Writing + flushing twice to one DeflateStream produces one stream with
        // several blocks; Puff must decode them in sequence. Preset-dictionary
        // behaviour (MSZIP across blocks) is covered by CcgameReaderTests against
        // the real multi-folder corpus.
        byte[] first = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("abcdefghij", 400)));
        byte[] second = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("klmnopqrst", 400)));

        using var ms = new MemoryStream();
        using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(first);
            deflate.Flush();
            deflate.Write(second);
        }

        byte[] compressed = ms.ToArray();
        byte[] expected = [.. first, .. second];

        Assert.Equal(expected, Puff.Inflate(compressed, compressed.Length, Array.Empty<byte>(), expected.Length));
    }

    [Fact]
    public void ThrowsOnTruncatedInput()
    {
        byte[] compressed = RawDeflate(Encoding.ASCII.GetBytes("some data to compress"));
        var truncated = compressed[..(compressed.Length / 2)];
        Assert.Throws<PuffException>(
            () => Puff.Inflate(truncated, truncated.Length, Array.Empty<byte>(), 1024));
    }

    private static byte[] RawDeflate(byte[] data)
    {
        using var ms = new MemoryStream();
        using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal, leaveOpen: true))
        {
            deflate.Write(data, 0, data.Length);
        }

        return ms.ToArray();
    }
}
