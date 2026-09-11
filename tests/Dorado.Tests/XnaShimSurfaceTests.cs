using System.Text;
using Dorado.Containers;
using Dorado.Platform;
using Dorado.Platform.Desktop.Software;
using Dorado.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Dorado.Tests;

/// <summary>
/// Coverage for the official-title shim surfaces added in the corpus-gap pass:
/// value-type display modes, the texture/render-target hierarchy, mouse and
/// touch capabilities, case-insensitive content paths and instance math gaps.
/// </summary>
public sealed class XnaShimSurfaceTests
{
    [Fact]
    public void InstanceNormalizeMutatesVectorInPlace()
    {
        Vector2 v2 = new(3f, 4f);
        v2.Normalize();
        AssertNear(0.6f, v2.X);
        AssertNear(0.8f, v2.Y);

        Vector3 v3 = new(0f, 3f, 4f);
        v3.Normalize();
        AssertNear(0f, v3.X);
        AssertNear(0.6f, v3.Y);
        AssertNear(0.8f, v3.Z);

        Vector4 v4 = new(0f, 0f, 3f, 4f);
        v4.Normalize();
        AssertNear(0.6f, v4.Z);
        AssertNear(0.8f, v4.W);

        Vector3 zero = Vector3.Zero;
        zero.Normalize();
        Assert.Equal(Vector3.Zero, zero);
    }

    [Fact]
    public void MatrixAxisSettersRoundTripTheGetters()
    {
        Matrix matrix = Matrix.Identity;
        matrix.Forward = new Vector3(0f, 0f, -1f);
        matrix.Up = new Vector3(0f, 1f, 0f);
        matrix.Right = new Vector3(1f, 0f, 0f);
        matrix.Translation = new Vector3(4f, 5f, 6f);

        Assert.Equal(new Vector3(0f, 0f, -1f), matrix.Forward);
        Assert.Equal(new Vector3(0f, 1f, 0f), matrix.Up);
        Assert.Equal(new Vector3(1f, 0f, 0f), matrix.Right);
        Assert.Equal(new Vector3(4f, 5f, 6f), matrix.Translation);
        Assert.Equal(Vector3.Transform(Vector3.Zero, matrix), matrix.Translation);
    }

    [Fact]
    public void BoundingBoxContainsPointsAndIntersectsRays()
    {
        BoundingBox box = new(new Vector3(-1f, -1f, -1f), new Vector3(1f, 1f, 1f));

        Assert.Equal(ContainmentType.Contains, box.Contains(Vector3.Zero));
        Assert.Equal(ContainmentType.Disjoint, box.Contains(new Vector3(2f, 0f, 0f)));

        AssertNear(4f, box.Intersects(new Ray(new Vector3(-5f, 0f, 0f), Vector3.UnitX))!.Value);
        AssertNear(0f, box.Intersects(new Ray(Vector3.Zero, Vector3.UnitX))!.Value);
        Assert.Null(box.Intersects(new Ray(new Vector3(-5f, 5f, 0f), Vector3.UnitX)));
        Assert.Null(box.Intersects(new Ray(new Vector3(5f, 0f, 0f), Vector3.UnitX)));
    }

    [Fact]
    public void RectangleLocationSetterMovesTheRectangle()
    {
        Rectangle rectangle = new(1, 2, 3, 4);
        rectangle.Location = new Point(10, 20);

        Assert.Equal(10, rectangle.X);
        Assert.Equal(20, rectangle.Y);
        Assert.Equal(3, rectangle.Width);
        Assert.Equal(4, rectangle.Height);
    }

    [Fact]
    public void DisplayModeIsAValueTypeAndAdaptersEnumerateModes()
    {
        Assert.True(typeof(DisplayMode).IsValueType);

        DisplayMode current = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        Assert.Equal(480, current.Width);
        Assert.Equal(272, current.Height);
        Assert.Equal(SurfaceFormat.Color, current.Format);

        DisplayMode[] modes = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.ToArray();
        Assert.Single(modes);
        Assert.Single(GraphicsAdapter.DefaultAdapter.SupportedDisplayModes[SurfaceFormat.Color]);
        Assert.Empty(GraphicsAdapter.DefaultAdapter.SupportedDisplayModes[SurfaceFormat.Bgr565]);
    }

    [Fact]
    public void Texture2DConstructorAndRegionReadbackExposePixels()
    {
        using var device = new GraphicsDevice(new SoftwareGraphicsBackend());
        var texture = new Texture2D(device, 2, 2, 1, TextureUsage.None, SurfaceFormat.Color);

        Assert.Equal(SurfaceFormat.Color, texture.Format);
        Assert.Equal(TextureUsage.None, texture.TextureUsage);
        Assert.Equal(1, texture.LevelCount);

        var pixels = new[]
        {
            Color.Red, Color.Green,
            Color.Blue, Color.White,
        };
        texture.SetData(pixels, 0, pixels.Length, SetDataOptions.None);

        var sampled = new Color[1];
        texture.GetData(0, new Rectangle(1, 1, 1, 1), sampled, 0, 1);
        Assert.Equal(Color.White, sampled[0]);
    }

    [Fact]
    public void RenderTarget2DExposesItsTextureToTheDevice()
    {
        using var device = new GraphicsDevice(new SoftwareGraphicsBackend());
        var target = new RenderTarget2D(device, 16, 16, 1, SurfaceFormat.Color, RenderTargetUsage.PreserveContents);

        device.SetRenderTarget(target);
        Assert.Same(target, device.GetRenderTarget(0));
        Assert.NotNull(target.GetTexture());
        Assert.Equal(16, target.Width);
        Assert.Equal(16, target.Height);

        device.SetRenderTarget(null);
        Assert.Null(device.GetRenderTarget(0));

        var resolve = new ResolveTexture2D(device, 16, 16, 1, SurfaceFormat.Color);
        device.ResolveBackBuffer(resolve);
        Assert.False(resolve.IsContentLost);
    }

    [Fact]
    public void PackedColorValueRoundTripsThroughTheSetter()
    {
        Color color = Color.Black;
        color.PackedValue = 0x80402010;

        Assert.Equal(0x40, color.R);
        Assert.Equal(0x20, color.G);
        Assert.Equal(0x10, color.B);
        Assert.Equal(0x80, color.A);
    }

    [Fact]
    public void ContentManagerResolvesWindowsCasedAssetPaths()
    {
        string root = Path.Combine(Path.GetTempPath(), "dorado-xna-content", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Sub Dir"));
        File.WriteAllBytes(Path.Combine(root, "Sub Dir", "MiXeD.xnb"), BuildStringAsset("hello"));

        try
        {
            var content = new ContentManager(new GameServiceContainer(), root);
            Assert.Equal("hello", content.Load<string>("sub dir\\mixed"));
            Assert.Equal("hello", content.ReadAsset<string>("SUB DIR/MIXED.XNB", null));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GamePadStateReportsConstructedButtons()
    {
        var state = new GamePadState(
            Vector2.Zero,
            Vector2.Zero,
            0.5f,
            0f,
            Buttons.A,
            Buttons.DPadUp);

        Assert.True(state.IsButtonDown(Buttons.A));
        Assert.False(state.IsButtonDown(Buttons.B));
        Assert.True(state.IsButtonDown(Buttons.DPadUp));
        Assert.False(state.IsButtonDown(Buttons.DPadDown));
        Assert.True(state.IsButtonDown(Buttons.LeftTrigger));
        Assert.True(state.IsButtonUp(Buttons.B));
        AssertNear(0.5f, state.Triggers.Left);

        GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);
        Assert.True(capabilities.IsConnected);
        Assert.Equal(GamePadType.GamePad, capabilities.GamePadType);
    }

    [Fact]
    public void MouseAndTouchCapabilitiesReportHeadlessDefaults()
    {
        PlatformHost.Input = EmptyInputSource.Instance;
        MouseState mouse = Mouse.GetState();
        Assert.Equal(0, mouse.X);
        Assert.Equal(0, mouse.Y);
        Assert.Equal(ButtonState.Released, mouse.LeftButton);

        TouchPanelCapabilities touch = TouchPanel.GetCapabilities();
        Assert.True(touch.IsConnected);
        Assert.Equal(4, touch.MaximumTouchCount);
        Assert.True(touch.HasPressure);

        var location = new TouchLocation(7, TouchLocationState.Pressed, new Vector2(12f, 34f), 0.75f);
        Assert.Equal(7, location.Id);
        Assert.Equal(0.75f, location.Pressure);
        Assert.False(location.TryGetPreviousLocation(out _));

        TouchCollection collection = TouchPanel.GetState();
        Assert.False(collection.FindById(7, out _));
    }

    [Fact]
    public void AccelerometerZeroReadingYieldsIdentityRotation()
    {
        var state = new AccelerometerState(Vector3.Zero);

        Assert.True(state.IsConnected);
        Assert.Equal(Matrix.Identity, state.GetRotation());

        var rotated = new AccelerometerState(new Vector3(0f, 1f, 0f));
        Assert.NotEqual(Matrix.Identity, rotated.GetRotation());
    }

    [Fact]
    public void OfficialCalculatorRunsWhenTheExternalCorpusIsPresent()
    {
        string repoRoot = Fixtures.RepoRoot;
        string officialRoot = Environment.GetEnvironmentVariable("DORADO_OFFICIAL_APPS")
            ?? Path.GetFullPath(Path.Combine(repoRoot, "..", "Zune HD Apps (Decompiled)"));
        string appDirectory = Path.Combine(officialRoot, "calculator", "gametitle", "584E07D1");
        if (!Directory.Exists(appDirectory))
        {
            return;
        }

        string? zdkLibrary = Environment.GetEnvironmentVariable("DORADO_ZDK_LIB");
        if (string.IsNullOrWhiteSpace(zdkLibrary))
        {
            zdkLibrary = Path.Combine(repoRoot, "native", "zdk-bridge", "libZDK.so");
            Environment.SetEnvironmentVariable("DORADO_ZDK_LIB", zdkLibrary);
        }

        if (!File.Exists(zdkLibrary))
        {
            return;
        }

        ZunePackage package = ZunePackageReader.Read(appDirectory);
        var backend = new SoftwareGraphicsBackend();

        ZuneRunResult result = ZuneAppRunner.Run(package, new ZuneRunOptions
        {
            Graphics = backend,
            Input = EmptyInputSource.Instance,
            FrameLimit = 3,
        });

        Assert.Equal(3, result.FramesRendered);
        Assert.Contains("Calculator", result.EntryPoint, StringComparison.Ordinal);
    }

    private static void AssertNear(float expected, float actual)
    {
        Assert.True(
            MathF.Abs(expected - actual) <= 1e-4f,
            $"Expected {expected}, but was {actual}.");
    }

    /// <summary>Builds a minimal valid XNB wrapping a single string value.</summary>
    private static byte[] BuildStringAsset(string value)
    {
        using var stream = new MemoryStream();
        stream.Write(new byte[] { (byte)'X', (byte)'N', (byte)'B', (byte)'w', 5, 0, 0, 0, 0, 0 });

        Write7Bit(stream, 1);
        byte[] readerName = Encoding.ASCII.GetBytes("Microsoft.Xna.Framework.Content.StringReader");
        Write7Bit(stream, readerName.Length);
        stream.Write(readerName);
        stream.Write(new byte[4]);
        Write7Bit(stream, 0);
        Write7Bit(stream, 1);

        byte[] text = Encoding.UTF8.GetBytes(value);
        Write7Bit(stream, text.Length);
        stream.Write(text);
        return stream.ToArray();
    }

    private static void Write7Bit(Stream stream, int value)
    {
        int remaining = value;
        while (remaining >= 0x80)
        {
            stream.WriteByte((byte)((remaining & 0x7F) | 0x80));
            remaining >>= 7;
        }

        stream.WriteByte((byte)remaining);
    }
}
