using System.Text.Json;
using Dorado.Containers;
using Dorado.Platform.Desktop.Software;
using Dorado.Plugins.Protocol.Rpc;
using Dorado.Runtime;

namespace Dorado.Cli.Ipc;

/// <summary>
/// Stdio-hosted JSON-RPC 2.0 server that exposes the Dorado emulator's
/// <c>inspect</c>, <c>unpack</c>, <c>refs</c>, and <c>run</c> commands over a
/// line-oriented transport. Spawning the CLI with <c>--ipc</c> switches the
/// main program into this mode instead of the human-friendly CLI.
///
/// The wire shape is JSON-RPC 2.0 — the same framing the Dorado desktop uses
/// for its plugin host (<c>Dorado.Plugins.Protocol.Rpc.JsonRpcChannel</c>) and
/// its LAN sync protocol (<c>Dorado-hd/sync/SyncProtocol.kt</c>).
///
/// Transport: the server reads inbound frames from <paramref name="pluginTransport"/>
/// (stdin in production, paired in-memory in tests) and writes outbound frames
/// back to it (stdout in production). The host pumps its own paired transport.
/// </summary>
public sealed class EmulatorRpcServer : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly JsonRpcChannel _channel;
    private readonly ILineTransport _transport;

    public EmulatorRpcServer(ILineTransport pluginTransport)
    {
        _transport = pluginTransport;
        _channel = new JsonRpcChannel(pluginTransport);
        _channel.RequestHandler = HandleRequestAsync;
    }

    /// <summary>
    /// Runs the JSON-RPC read loop until the transport reports exit (stdin EOF
    /// or peer disconnect) or <paramref name="cancellationToken"/> fires.
    /// </summary>
    public async Task ServeAsync(CancellationToken cancellationToken)
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnExited(object? sender, int? code) => completed.TrySetResult();

        // Subscribe BEFORE starting the read loop: stdin can reach EOF the
        // instant the loop starts, and an Exited raised before subscription
        // would be lost and leave this method awaiting forever.
        _transport.Exited += OnExited;
        try
        {
            await _transport.StartAsync(cancellationToken).ConfigureAwait(false);
            _channel.Start();

            using var registration = cancellationToken.Register(() => completed.TrySetResult());
            await completed.Task.ConfigureAwait(false);
        }
        finally
        {
            _transport.Exited -= OnExited;
            await _channel.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task<object?> HandleRequestAsync(string method, JsonElement? parameters)
    {
        return method switch
        {
            "inspect" => Inspect(RequireString(parameters, "package")),
            "unpack" => Unpack(
                RequireString(parameters, "package"),
                RequireString(parameters, "outputDir")),
            "refs" => Refs(RequireString(parameters, "assembly")),
            "run" => await RunAsync(parameters),
            _ => throw new InvalidOperationException($"unknown method '{method}'."),
        };
    }

    private static string RequireString(JsonElement? parameters, string propertyName)
    {
        if (parameters is null || parameters.Value.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException($"Parameters must be an object with a '{propertyName}' string.");
        }
        if (!parameters.Value.TryGetProperty(propertyName, out var element) ||
            element.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException($"'{propertyName}' (string) is required.");
        }
        return element.GetString()!;
    }

    private static object Inspect(string packagePath)
    {
        var package = ZunePackageReader.Read(packagePath);
        return new
        {
            kind = package.Kind.ToString(),
            title = package.Metadata.Title,
            description = package.Metadata.Description,
            executable = package.Metadata.Executable,
            startupAssembly = package.Metadata.StartupAssembly,
            platform = package.Metadata.Platform,
            guid = package.Metadata.GameGuid?.ToString(),
            runtimeProfile = package.Metadata.RuntimeProfile,
            ccgameVersion = package.Metadata.CcgameVersion,
            encrypted = package.IsEncrypted,
            entryCount = package.EntryCount,
            fileCount = package.Files.Count,
            files = package.Files.Select(f => new
            {
                path = f.Path,
                container = f.ContainerName,
                size = f.Size,
            }),
        };
    }

    private static object Unpack(string packagePath, string outputDir)
    {
        var package = ZunePackageReader.Read(packagePath);
        if (package.IsEncrypted || package.EntryCount == 0)
        {
            return new
            {
                written = 0,
                error = package.IsEncrypted
                    ? "package payload is DRM-encrypted; no key provider supplied (see docs/drm-key-import.md)."
                    : "package contains no extractable entries.",
            };
        }

        Directory.CreateDirectory(outputDir);
        int written = 0;
        foreach (var file in package.Files)
        {
            if (!package.TryGetEntry(file.Path, out var payload))
            {
                continue;
            }
            var destination = Path.Combine(outputDir, Sanitize(file.Path));
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? outputDir);
            File.WriteAllBytes(destination, payload);
            written++;
        }

        return new { written, outputDir };
    }

    private static object Refs(string assemblyPath)
    {
        var report = AssemblyInspector.Inspect(assemblyPath);
        return new
        {
            assemblyReferences = report.AssemblyReferences,
            typeReferences = report.TypeReferences,
            xnaDerivedTypes = report.XnaDerivedTypes,
            memberReferences = report.MemberReferences.ToDictionary(
                kv => kv.Key,
                kv => (IReadOnlyList<string>)kv.Value.ToList()),
        };
    }

    private static async Task<object> RunAsync(JsonElement? parameters)
    {
        var args = parameters is null
            ? new RunArgs(null, null, null, null)
            : parameters.Value.Deserialize<RunArgs>(JsonOptions) ?? new RunArgs(null, null, null, null);
        if (string.IsNullOrWhiteSpace(args.Package))
        {
            throw new ArgumentException("'package' is required.");
        }
        var frames = args.Frames ?? 60;
        var outPath = args.Out;
        var hash = args.Hash ?? false;

        var package = ZunePackageReader.Read(args.Package);
        var backend = new SoftwareGraphicsBackend();
        var result = await Task.Run(() => ZuneAppRunner.Run(package, new ZuneRunOptions
        {
            Graphics = backend,
            Input = ScriptedInputSource.Empty,
            FrameLimit = frames,
            OnFrameRendered = frame =>
            {
                if (outPath is not null && frame == frames - 1)
                {
                    backend.SavePng(outPath);
                }
            },
        })).ConfigureAwait(false);

        var response = new Dictionary<string, object?>
        {
            ["entryPoint"] = result.EntryPoint,
            ["framesRendered"] = result.FramesRendered,
        };
        if (hash)
        {
            response["frameSha256"] = FrameHash(backend.SnapshotBackbuffer());
        }
        return response;
    }

    private static string Sanitize(string path)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var parts = path.Replace('\\', '/').Split('/').Select(p =>
            string.Concat(p.Where(c => !invalid.Contains(c) && c != ':' && c != '*' && c != '?' && c != '"' && c != '<' && c != '>' && c != '|'))
            .TrimStart('.', '_'));
        var joined = Path.Combine(parts.Where(p => p.Length > 0).ToArray());
        return joined.Length == 0 ? "entry" : joined;
    }

    private static string FrameHash(uint[] pixels)
    {
        var bytes = new byte[pixels.Length * 4];
        for (var i = 0; i < pixels.Length; i++)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), pixels[i]);
        }
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
    }

    public sealed record RunArgs(string? Package, int? Frames, string? Out, bool? Hash);

    public async ValueTask DisposeAsync()
    {
        await _channel.DisposeAsync().ConfigureAwait(false);
    }
}
