using System.Security.Cryptography;
using Dorado.Containers;
using Dorado.Platform;
using Dorado.Platform.Desktop.Software;
using Dorado.Runtime;
using Xunit;

namespace Dorado.Tests;

/// <summary>
/// M1 integration: run the real homebrew XNA applications through the shim and
/// the software backend. Golden hashes are renderer regressions; regenerate with
/// <c>dorado run &lt;package&gt; --frames 20 --hash</c> after intentional changes.
/// No-ops when the corpus has not been fetched.
/// </summary>
public sealed class M1RuntimeTests
{
    // dorado run "XNA Pong.ccgame" --frames 20 --hash
    private const string PongGolden = "df43578d888a430fb497f6c13ec3b81f8c661cd2516a668de019174f20571ced";

    // dorado run "Etch-A-Sketch.ccgame" --frames 20 --hash
    private const string EtchGolden = "115e12aa669ad4af2723dfed1ec256248318e214238a0105e8f6a46798f51eba";

    [Fact]
    public void PongRunsAndRendersDeterministically()
    {
        string? path = Fixtures.CorpusFile("XNA Pong.ccgame");
        if (path is null)
        {
            return;
        }

        uint[] first = RunPackage(path, 20, ScriptedInputSource.Empty);
        uint[] second = RunPackage(path, 20, ScriptedInputSource.Empty);

        Assert.NotEmpty(first);
        Assert.Equal(Hash(first), Hash(second));
        Assert.Equal(PongGolden, Hash(first));
        Assert.True(CountDistinct(first) > 100, "Pong frame looks blank.");
    }

    [Fact]
    public void EtchRendersGoldenFrame()
    {
        string? path = Fixtures.CorpusFile("Etch-A-Sketch.ccgame");
        if (path is null)
        {
            return;
        }

        uint[] frame = RunPackage(path, 20, ScriptedInputSource.Empty);

        Assert.Equal(EtchGolden, Hash(frame));
    }

    [Fact]
    public void TouchInputChangesTheFrame()
    {
        string? path = Fixtures.CorpusFile("Etch-A-Sketch.ccgame");
        if (path is null)
        {
            return;
        }

        uint[] idle = RunPackage(path, 40, ScriptedInputSource.Empty);

        var script = new ScriptedInputSource(frame => frame is >= 5 and < 35
            ? new InputSnapshot
            {
                Touches = new[] { new TouchPoint(1, 240f + frame, 136f, TouchState.Moved, 1f) },
            }
            : InputSnapshot.Empty);

        uint[] drawn = RunPackage(path, 40, script);

        Assert.NotEqual(Hash(idle), Hash(drawn));
    }

    private static uint[] RunPackage(string path, int frames, IInputSource input)
    {
        ZunePackage package = ZunePackageReader.Read(path);
        var backend = new SoftwareGraphicsBackend();

        ZuneAppRunner.Run(package, new ZuneRunOptions
        {
            Graphics = backend,
            Input = input,
            FrameLimit = frames,
        });

        return backend.SnapshotBackbuffer();
    }

    private static string Hash(uint[] pixels)
    {
        var bytes = new byte[pixels.Length * 4];
        for (int i = 0; i < pixels.Length; i++)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), pixels[i]);
        }

        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static int CountDistinct(uint[] pixels) => pixels.Distinct().Count();
}
