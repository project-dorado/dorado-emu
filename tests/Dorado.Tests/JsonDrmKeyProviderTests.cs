using System.Text;
using Dorado.Containers.Drm;
using Xunit;

namespace Dorado.Tests;

public sealed class JsonDrmKeyProviderTests
{
    [Fact]
    public void LooksUpKeyByGuid()
    {
        const string json = """
            {
              "keys": [
                { "guid": "5DC1E614-ED1B-4D10-8259-BC7E3E926A86", "key": "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f" }
              ]
            }
            """;

        var provider = JsonDrmKeyProvider.Parse(json);
        byte[]? key = provider.TryGetContentKey(Encoding.ASCII.GetBytes("5dc1e614ed1b4d108259bc7e3e926a86"), []);

        Assert.NotNull(key);
        Assert.Equal(32, key!.Length);
        Assert.Equal(0x1F, key[^1]);
    }

    [Fact]
    public void FallsBackWhenGuidUnknown()
    {
        const string json = """{ "keys": [ { "key": "000102030405060708090a0b0c0d0e0f" } ] }""";

        var provider = JsonDrmKeyProvider.Parse(json);
        byte[]? key = provider.TryGetContentKey(Encoding.ASCII.GetBytes("deadbeef"), []);

        Assert.NotNull(key);
        Assert.Equal(16, key!.Length);
    }

    [Fact]
    public void ReturnsNullWhenNoKey()
    {
        var provider = JsonDrmKeyProvider.Parse("""{ "keys": [] }""");
        Assert.Null(provider.TryGetContentKey(Encoding.ASCII.GetBytes("00"), []));
    }

    [Fact]
    public void RejectsInvalidKeyLength()
    {
        Assert.Throws<InvalidDataException>(
            () => JsonDrmKeyProvider.Parse("""{ "keys": [ { "key": "0011" } ] }"""));
    }
}
