using System.Text.Json;
using Dorado.Cli.Ipc;
using Dorado.Plugins.Protocol.Rpc;
using Xunit;

namespace Dorado.Tests;

/// <summary>
/// Drives <see cref="EmulatorRpcServer"/> through paired in-memory
/// <see cref="InMemoryLineTransport"/>s — one for the host, one for the plugin
/// — so the JSON-RPC surface can be exercised without spawning a sub-process.
/// Each test writes a request frame and asserts on the response frame.
///
/// Corpus-dependent tests no-op when the fixture is absent so CI stays green
/// without copyrighted content (same pattern as <see cref="CcgameReaderTests"/>).
/// </summary>
public sealed class EmulatorRpcServerTests
{
    [Fact]
    public Task Inspect_ReturnsPackageMetadata() => WithServerAsync(async (host) =>
    {
        var packagePath = Fixtures.CorpusFile("XNA Pong.ccgame");
        if (packagePath is null)
        {
            return;
        }

        var result = await CallAsync(host, "inspect", new { package = packagePath });

        Assert.Equal("Ccgame", result.GetProperty("kind").GetString());
        Assert.Equal("Pong", result.GetProperty("title").GetString());
        Assert.Equal("ZunePong.exe", result.GetProperty("startupAssembly").GetString());
        Assert.Equal("Zune.v3.1", result.GetProperty("runtimeProfile").GetString());
        Assert.Equal(7, result.GetProperty("entryCount").GetInt32());
        Assert.False(result.GetProperty("encrypted").GetBoolean());
        Assert.True(result.GetProperty("files").GetArrayLength() >= 7);
    });

    [Fact]
    public Task Inspect_UnknownPackage_ReturnsJsonRpcError() => WithServerAsync(async (host) =>
    {
        var (_, error) = await CallWithErrorAsync(host, "inspect", new { package = "/nonexistent.zcp" });
        Assert.True(error.HasValue);
    });

    [Fact]
    public Task Unpack_WritesExtractedFiles() => WithServerAsync(async (host) =>
    {
        var packagePath = Fixtures.CorpusFile("XNA Pong.ccgame");
        if (packagePath is null)
        {
            return;
        }

        var outputDir = Path.Combine(Path.GetTempPath(), "dorado-emu-ipc-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var result = await CallAsync(host, "unpack", new { package = packagePath, outputDir });

            Assert.Equal(7, result.GetProperty("written").GetInt32());
            Assert.True(File.Exists(Path.Combine(outputDir, "ZunePong.exe")));
        }
        finally
        {
            try { if (Directory.Exists(outputDir)) Directory.Delete(outputDir, recursive: true); } catch { }
        }
    });

    [Fact]
    public Task Refs_ReturnsInspectorReport() => WithServerAsync(async (host) =>
    {
        var result = await CallAsync(host, "refs", new { assembly = typeof(EmulatorRpcServer).Assembly.Location });

        Assert.True(result.TryGetProperty("assemblyReferences", out _));
        Assert.True(result.TryGetProperty("xnaDerivedTypes", out _));
    });

    [Fact]
    public Task Run_ReturnsDeterministicFrameHash() => WithServerAsync(async (host) =>
    {
        var packagePath = Fixtures.CorpusFile("XNA Pong.ccgame");
        if (packagePath is null)
        {
            return;
        }

        var result = await CallAsync(host, "run", new { package = packagePath, frames = 1, hash = true });

        Assert.Equal(1, result.GetProperty("framesRendered").GetInt32());
        var hash = result.GetProperty("frameSha256").GetString();
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.Equal(64, hash!.Length);
    });

    [Fact]
    public Task UnknownMethod_ReturnsJsonRpcError() => WithServerAsync(async (host) =>
    {
        var (_, error) = await CallWithErrorAsync(host, "bogus", new { });
        Assert.True(error.HasValue);
    });

    // ---- synthetic fixtures: run offline, no corpus required -------------

    [Fact]
    public Task Inspect_SyntheticCcgame_ReturnsEntryMetadata() => WithServerAsync(async (host) =>
    {
        var packagePath = WriteTemp("ccgame", SyntheticPackages.BuildCcgame());
        try
        {
            var result = await CallAsync(host, "inspect", new { package = packagePath });

            Assert.Equal("Ccgame", result.GetProperty("kind").GetString());
            Assert.False(result.GetProperty("encrypted").GetBoolean());
            Assert.Equal(2, result.GetProperty("entryCount").GetInt32());
            Assert.Equal(2, result.GetProperty("files").GetArrayLength());
        }
        finally
        {
            TryDelete(packagePath);
        }
    });

    [Fact]
    public Task Unpack_SyntheticCcgame_ExtractsBothEntries() => WithServerAsync(async (host) =>
    {
        var packagePath = WriteTemp("ccgame", SyntheticPackages.BuildCcgame());
        var outputDir = Path.Combine(Path.GetTempPath(), "dorado-emu-ipc-tests", Guid.NewGuid().ToString("N"));
        try
        {
            var result = await CallAsync(host, "unpack", new { package = packagePath, outputDir });

            Assert.Equal(2, result.GetProperty("written").GetInt32());
            var exe = Path.Combine(outputDir, "0.exe");
            var xnb = Path.Combine(outputDir, "1.xnb");
            Assert.True(File.Exists(exe));
            Assert.True(File.Exists(xnb));
            var exeBytes = File.ReadAllBytes(exe);
            Assert.Equal((byte)'M', exeBytes[0]);
            Assert.Equal((byte)'Z', exeBytes[1]);
            var xnbBytes = File.ReadAllBytes(xnb);
            Assert.Equal("XNB", System.Text.Encoding.ASCII.GetString(xnbBytes, 0, 3));
        }
        finally
        {
            TryDelete(packagePath);
            try { if (Directory.Exists(outputDir)) Directory.Delete(outputDir, recursive: true); } catch { }
        }
    });

    [Fact]
    public Task Inspect_SyntheticZcp_ReportsManifestMetadata() => WithServerAsync(async (host) =>
    {
        var packagePath = WriteTemp("zcp", SyntheticPackages.BuildZcp());
        try
        {
            var result = await CallAsync(host, "inspect", new { package = packagePath });

            Assert.Equal("Zcp", result.GetProperty("kind").GetString());
            Assert.Equal("Calculator", result.GetProperty("title").GetString());
            Assert.Equal("Calculator.exe", result.GetProperty("startupAssembly").GetString());
            Assert.Equal("Zune.v3.1", result.GetProperty("platform").GetString());
            Assert.True(result.GetProperty("encrypted").GetBoolean());
        }
        finally
        {
            TryDelete(packagePath);
        }
    });

    private static string WriteTemp(string extension, byte[] bytes)
    {
        var path = Path.Combine(Path.GetTempPath(), "dorado-emu-ipc-tests", $"{Guid.NewGuid():N}.{extension}");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private static async Task WithServerAsync(Func<ILineTransport, Task> body)
    {
        var (host, plugin) = InMemoryLineTransport.CreatePair();
        await using var server = new EmulatorRpcServer(plugin);
        using var cts = new CancellationTokenSource();
        var serveTask = server.ServeAsync(cts.Token);
        try
        {
            await body(host).ConfigureAwait(false);
        }
        finally
        {
            cts.Cancel();
            try { await plugin.StopAsync(); } catch { }
            try { await serveTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
        }
    }

    private static async Task<JsonElement> CallAsync(ILineTransport host, string method, object parameters)
    {
        var request = new { jsonrpc = "2.0", id = "1", method, @params = parameters };
        await host.WriteLineAsync(JsonSerializer.Serialize(request), default).ConfigureAwait(false);
        var line = await host.ReadLineAsync(default).ConfigureAwait(false);
        Assert.NotNull(line);
        using var doc = JsonDocument.Parse(line!);
        var root = doc.RootElement;
        Assert.False(root.TryGetProperty("error", out var err) && err.ValueKind != JsonValueKind.Null,
            $"Unexpected JSON-RPC error: {root}");
        Assert.True(root.TryGetProperty("result", out var result));
        return result.Clone();
    }

    private static async Task<(JsonElement result, JsonElement? error)> CallWithErrorAsync(
        ILineTransport host, string method, object parameters)
    {
        var request = new { jsonrpc = "2.0", id = "1", method, @params = parameters };
        await host.WriteLineAsync(JsonSerializer.Serialize(request), default).ConfigureAwait(false);
        var line = await host.ReadLineAsync(default).ConfigureAwait(false);
        Assert.NotNull(line);
        using var doc = JsonDocument.Parse(line!);
        var root = doc.RootElement.Clone();
        if (root.TryGetProperty("error", out var error) && error.ValueKind != JsonValueKind.Null)
        {
            return (root.TryGetProperty("result", out var r) ? r.Clone() : default, error.Clone());
        }
        return (root.GetProperty("result").Clone(), null);
    }
}
