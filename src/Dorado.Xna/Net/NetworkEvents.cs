using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

public enum NetworkSessionType
{
    Local = 0,
    SystemLink = 1,
    PlayerMatch = 2,
    Ranked = 3,
}

public enum NetworkSessionState
{
    Lobby = 0,
    Playing = 1,
    Ended = 2,
}

public enum NetworkSessionEndReason
{
    ClientSignedOut = 0,
    HostEndedSession = 1,
    RemovedByHost = 2,
    Disconnected = 3,
}

public enum NetworkSessionJoinError
{
    SessionNotFound = 0,
    SessionNotJoinable = 1,
    SessionFull = 2,
}

[Flags]
public enum SendDataOptions
{
    None = 0,
    Reliable = 1,
    InOrder = 2,
    ReliableInOrder = Reliable | InOrder,
    Chat = 4,
}

/// <summary>Reports a gamer that joined a session.</summary>
public class GamerJoinedEventArgs : EventArgs
{
    public GamerJoinedEventArgs(NetworkGamer gamer) => Gamer = gamer;

    public NetworkGamer Gamer { get; }
}

/// <summary>Reports a gamer that left a session.</summary>
public class GamerLeftEventArgs : EventArgs
{
    public GamerLeftEventArgs(NetworkGamer gamer) => Gamer = gamer;

    public NetworkGamer Gamer { get; }
}

/// <summary>Reports a change of session host.</summary>
public class HostChangedEventArgs : EventArgs
{
    public HostChangedEventArgs(NetworkGamer oldHost, NetworkGamer newHost)
    {
        OldHost = oldHost;
        NewHost = newHost;
    }

    public NetworkGamer OldHost { get; }

    public NetworkGamer NewHost { get; }
}

/// <summary>Reports an accepted game invitation.</summary>
public class InviteAcceptedEventArgs : EventArgs
{
    public InviteAcceptedEventArgs(SignedInGamer gamer) => Gamer = gamer;

    public SignedInGamer Gamer { get; }

    public bool IsCurrentSession => false;
}

/// <summary>Reports why a network session ended.</summary>
public class NetworkSessionEndedEventArgs : EventArgs
{
    public NetworkSessionEndedEventArgs(NetworkSessionEndReason endReason) => EndReason = endReason;

    public NetworkSessionEndReason EndReason { get; }
}

public class GameStartedEventArgs : EventArgs
{
}

public class GameEndedEventArgs : EventArgs
{
}

/// <summary>Reports a network gamer event.</summary>
public class NetworkGamerEventArgs : EventArgs
{
    public NetworkGamerEventArgs(NetworkGamer gamer) => Gamer = gamer;

    public NetworkGamer Gamer { get; }
}

/// <summary>Reports a network machine event.</summary>
public class NetworkMachineEventArgs : EventArgs
{
    public NetworkMachineEventArgs(NetworkMachine machine) => Machine = machine;

    public NetworkMachine Machine { get; }
}
