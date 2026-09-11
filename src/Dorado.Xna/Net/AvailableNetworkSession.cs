using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Net;

/// <summary>A session advertised by the offline session browser.</summary>
public sealed class AvailableNetworkSession
{
    internal AvailableNetworkSession()
    {
    }

    public string HostGamertag { get; internal set; } = string.Empty;

    public int CurrentGamerCount { get; internal set; }

    public int OpenPublicGamerSlots { get; internal set; }

    public int OpenPrivateGamerSlots { get; internal set; }

    public NetworkSessionProperties SessionProperties { get; } = new();

    public QualityOfService QualityOfService { get; } = new();
}

/// <summary>A read-only list of available network sessions.</summary>
public sealed class AvailableNetworkSessionCollection : ReadOnlyCollection<AvailableNetworkSession>, IDisposable
{
    internal AvailableNetworkSessionCollection()
        : base(new List<AvailableNetworkSession>())
    {
    }

    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

/// <summary>Connection quality measurements for a remote session.</summary>
public sealed class QualityOfService
{
    internal QualityOfService()
    {
    }

    public bool IsAvailable => false;

    public int BytesPerSecondUpstream => 0;

    public int BytesPerSecondDownstream => 0;

    public TimeSpan AverageRoundtripTime => TimeSpan.Zero;

    public TimeSpan MinimumRoundtripTime => TimeSpan.Zero;
}
