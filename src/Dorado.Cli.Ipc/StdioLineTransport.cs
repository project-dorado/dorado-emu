using System.Text;
using Dorado.Plugins.Protocol.Rpc;

namespace Dorado.Cli.Ipc;

/// <summary>
/// Std-in / std-out <see cref="ILineTransport"/>. Used when the CLI is invoked
/// with <c>--ipc</c> so the host (the parent Dorado process) can drive the
/// emulator over JSON-RPC frames. Cancellation is the only stop signal —
/// the parent process closing stdin flips the read loop to <c>null</c>.
/// </summary>
public sealed class StdioLineTransport : ILineTransport
{
    private readonly TextReader _reader;
    private readonly TextWriter _writer;
    private bool _running;

    public StdioLineTransport(TextReader reader, TextWriter writer)
    {
        _reader = reader;
        _writer = writer;
    }

    public bool IsRunning => _running;

    public event EventHandler<int?>? Exited;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _running = true;
        return Task.CompletedTask;
    }

    public async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null && _running)
            {
                // EOF means the parent closed stdin: signal exit so ServeAsync
                // returns and the process terminates cleanly.
                await StopAsync().ConfigureAwait(false);
            }
            return line;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        await _writer.WriteLineAsync(line.AsMemory(), cancellationToken).ConfigureAwait(false);
        await _writer.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync()
    {
        _running = false;
        Exited?.Invoke(this, 0);
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _running = false;
        return ValueTask.CompletedTask;
    }
}
