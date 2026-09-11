using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>An offline, detached network session: all gamer lists are empty and no traffic flows.</summary>
public sealed class NetworkSession : IDisposable
{
    public const int MaxSupportedGamers = 8;

    public const int MaxPreviousGamers = 25;

    private readonly GamerCollection<NetworkGamer> allGamers = new();
    private readonly GamerCollection<LocalNetworkGamer> localGamers = new();
    private readonly GamerCollection<NetworkGamer> remoteGamers = new();
    private readonly GamerCollection<NetworkGamer> previousGamers = new();
    private NetworkSessionState sessionState = NetworkSessionState.Lobby;

    internal NetworkSession(
        NetworkSessionType sessionType,
        int maxGamers,
        int privateGamerSlots,
        NetworkSessionProperties? sessionProperties)
    {
        SessionType = sessionType;
        MaxGamers = maxGamers;
        PrivateGamerSlots = privateGamerSlots;
        SessionProperties = sessionProperties ?? new NetworkSessionProperties();
    }

    public static bool IsAvailable => false;

    public NetworkSessionType SessionType { get; }

    public NetworkSessionState SessionState => sessionState;

    public bool IsHost => false;

    public NetworkGamer? Host => null;

    public GamerCollection<NetworkGamer> AllGamers => allGamers;

    public GamerCollection<LocalNetworkGamer> LocalGamers => localGamers;

    public GamerCollection<NetworkGamer> RemoteGamers => remoteGamers;

    public GamerCollection<NetworkGamer> PreviousGamers => previousGamers;

    public bool IsEveryoneReady => false;

    public int MaxGamers { get; set; }

    public int PrivateGamerSlots { get; set; }

    public float SimulatedPacketLoss { get; set; }

    public TimeSpan SimulatedLatency { get; set; }

    public int BytesPerSecondSent => 0;

    public int BytesPerSecondReceived => 0;

    public NetworkSessionProperties SessionProperties { get; }

    public bool AllowJoinInProgress { get; set; }

    public bool AllowHostMigration { get; set; }

    public bool IsDisposed { get; private set; }

    public event EventHandler<GameStartedEventArgs>? GameStarted;

    public event EventHandler<GameEndedEventArgs>? GameEnded;

    public event EventHandler<NetworkSessionEndedEventArgs>? SessionEnded
    {
        add { }
        remove { }
    }

    public event EventHandler<GamerJoinedEventArgs>? GamerJoined
    {
        add { }
        remove { }
    }

    public event EventHandler<GamerLeftEventArgs>? GamerLeft
    {
        add { }
        remove { }
    }

    public event EventHandler<HostChangedEventArgs>? HostChanged
    {
        add { }
        remove { }
    }

    public static event EventHandler<InviteAcceptedEventArgs>? InviteAccepted
    {
        add { }
        remove { }
    }

    public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers) =>
        new(sessionType, maxGamers, 0, null);

    public static NetworkSession Create(
        NetworkSessionType sessionType,
        int maxLocalGamers,
        int maxGamers,
        int privateGamerSlots,
        NetworkSessionProperties? sessionProperties) =>
        new(sessionType, maxGamers, privateGamerSlots, sessionProperties);

    public static NetworkSession Create(
        NetworkSessionType sessionType,
        IEnumerable<SignedInGamer>? localGamers,
        int maxGamers,
        int privateGamerSlots,
        NetworkSessionProperties? sessionProperties) =>
        new(sessionType, maxGamers, privateGamerSlots, sessionProperties);

    public static IAsyncResult BeginCreate(
        NetworkSessionType sessionType,
        int maxLocalGamers,
        int maxGamers,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(Create(sessionType, maxLocalGamers, maxGamers), callback, asyncState);

    public static IAsyncResult BeginCreate(
        NetworkSessionType sessionType,
        int maxLocalGamers,
        int maxGamers,
        int privateGamerSlots,
        NetworkSessionProperties? sessionProperties,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(Create(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties), callback, asyncState);

    public static IAsyncResult BeginCreate(
        NetworkSessionType sessionType,
        IEnumerable<SignedInGamer>? localGamers,
        int maxGamers,
        int privateGamerSlots,
        NetworkSessionProperties? sessionProperties,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(Create(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties), callback, asyncState);

    public static NetworkSession EndCreate(IAsyncResult result) => Unwrap<NetworkSession>(result);

    public static AvailableNetworkSessionCollection Find(
        NetworkSessionType sessionType,
        int maxLocalGamers,
        NetworkSessionProperties? searchProperties) =>
        new();

    public static AvailableNetworkSessionCollection Find(
        NetworkSessionType sessionType,
        IEnumerable<SignedInGamer>? localGamers,
        NetworkSessionProperties? searchProperties) =>
        new();

    public static IAsyncResult BeginFind(
        NetworkSessionType sessionType,
        int maxLocalGamers,
        NetworkSessionProperties? searchProperties,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(Find(sessionType, maxLocalGamers, searchProperties), callback, asyncState);

    public static IAsyncResult BeginFind(
        NetworkSessionType sessionType,
        IEnumerable<SignedInGamer>? localGamers,
        NetworkSessionProperties? searchProperties,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(Find(sessionType, localGamers, searchProperties), callback, asyncState);

    public static AvailableNetworkSessionCollection EndFind(IAsyncResult result) =>
        Unwrap<AvailableNetworkSessionCollection>(result);

    public static NetworkSession Join(AvailableNetworkSession availableSession)
    {
        ArgumentNullException.ThrowIfNull(availableSession);
        return new NetworkSession(NetworkSessionType.Local, MaxSupportedGamers, 0, null);
    }

    public static IAsyncResult BeginJoin(
        AvailableNetworkSession availableSession,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(Join(availableSession), callback, asyncState);

    public static NetworkSession EndJoin(IAsyncResult result) => Unwrap<NetworkSession>(result);

    public static NetworkSession JoinInvited(int maxLocalGamers) =>
        new(NetworkSessionType.Local, MaxSupportedGamers, 0, null);

    public static NetworkSession JoinInvited(IEnumerable<SignedInGamer>? localGamers) =>
        new(NetworkSessionType.Local, MaxSupportedGamers, 0, null);

    public static IAsyncResult BeginJoinInvited(int maxLocalGamers, AsyncCallback? callback, object? asyncState) =>
        Complete(JoinInvited(maxLocalGamers), callback, asyncState);

    public static IAsyncResult BeginJoinInvited(
        IEnumerable<SignedInGamer>? localGamers,
        AsyncCallback? callback,
        object? asyncState) =>
        Complete(JoinInvited(localGamers), callback, asyncState);

    public static NetworkSession EndJoinInvited(IAsyncResult result) => Unwrap<NetworkSession>(result);

    public void Dispose()
    {
        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    public void Update()
    {
    }

    public void StartGame()
    {
        sessionState = NetworkSessionState.Playing;
        GameStarted?.Invoke(this, new GameStartedEventArgs());
    }

    public void EndGame()
    {
        sessionState = NetworkSessionState.Lobby;
        GameEnded?.Invoke(this, new GameEndedEventArgs());
    }

    public void ResetReady()
    {
    }

    public NetworkGamer? FindGamerById(byte gamerId)
    {
        foreach (NetworkGamer gamer in allGamers)
        {
            if (gamer.Id == gamerId)
            {
                return gamer;
            }
        }

        return null;
    }

    public void AddLocalGamer(SignedInGamer gamer)
    {
        ArgumentNullException.ThrowIfNull(gamer);
    }

    private static IAsyncResult Complete<T>(T value, AsyncCallback? callback, object? asyncState)
    {
        var result = new CompletedAsyncResult<T>(value, asyncState);
        callback?.Invoke(result);
        return result;
    }

    private static T Unwrap<T>(IAsyncResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result is CompletedAsyncResult<T> completed)
        {
            return completed.Value;
        }

        throw new ArgumentException("The async result was not produced by NetworkSession.", nameof(result));
    }
}

/// <summary>An already-completed <see cref="IAsyncResult"/> carrying a value.</summary>
internal sealed class CompletedAsyncResult<T> : IAsyncResult
{
    public CompletedAsyncResult(T value, object? asyncState)
    {
        Value = value;
        AsyncState = asyncState;
        AsyncWaitHandle = new ManualResetEvent(initialState: true);
    }

    public T Value { get; }

    public object? AsyncState { get; }

    public WaitHandle AsyncWaitHandle { get; }

    public bool CompletedSynchronously => true;

    public bool IsCompleted => true;
}
