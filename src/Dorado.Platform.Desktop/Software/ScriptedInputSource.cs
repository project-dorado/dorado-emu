using Dorado.Platform;

namespace Dorado.Platform.Desktop.Software;

/// <summary>Deterministic input for headless runs: a per-frame script.</summary>
public sealed class ScriptedInputSource : IInputSource
{
    private readonly Func<long, InputSnapshot> _script;

    public ScriptedInputSource(Func<long, InputSnapshot> script) => _script = script;

    public static ScriptedInputSource Empty { get; } = new(_ => InputSnapshot.Empty);

    public static ScriptedInputSource Touches(IEnumerable<(long Frame, TouchPoint Point)> points)
    {
        var byFrame = points
            .GroupBy(p => p.Frame)
            .ToDictionary(g => g.Key, g => g.Select(p => p.Point).ToArray());

        return new ScriptedInputSource(frame =>
            byFrame.TryGetValue(frame, out TouchPoint[]? touches)
                ? new InputSnapshot { Touches = touches }
                : InputSnapshot.Empty);
    }

    public InputSnapshot Snapshot(long frame) => _script(frame);
}
