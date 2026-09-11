using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net;

/// <summary>A gamer inside a network session.</summary>
public class NetworkGamer : Gamer
{
    internal NetworkGamer(NetworkSession session, byte id, string gamertag, bool isHost, bool isLocal)
        : base(gamertag)
    {
        Session = session;
        Id = id;
        IsHost = isHost;
        IsLocal = isLocal;
    }

    public NetworkSession Session { get; }

    public NetworkMachine? Machine { get; internal set; }

    public bool IsHost { get; internal set; }

    public bool IsLocal { get; }

    public bool IsPrivateSlot => false;

    public bool IsReady { get; set; }

    public bool HasVoice => false;

    public bool IsTalking => false;

    public bool IsMutedByLocalUser => false;

    public bool IsGuest => false;

    public TimeSpan RoundtripTime => TimeSpan.Zero;

    public byte Id { get; }

    public bool HasLeftSession { get; internal set; }
}

/// <summary>A gamer on this device inside a network session.</summary>
public sealed class LocalNetworkGamer : NetworkGamer
{
    internal LocalNetworkGamer(NetworkSession session, byte id, string gamertag, bool isHost)
        : base(session, id, gamertag, isHost, isLocal: true)
    {
    }

    public SignedInGamer? SignedInGamer => null;

    public bool IsDataAvailable => false;

    public void EnableSendVoice(NetworkGamer remoteGamer, bool enable)
    {
    }

    public void SendData(byte[] data, SendDataOptions options)
    {
    }

    public void SendData(byte[] data, SendDataOptions options, NetworkGamer recipient)
    {
    }

    public void SendData(byte[] data, int offset, int count, SendDataOptions options)
    {
    }

    public void SendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)
    {
    }

    public void SendData(PacketWriter data, SendDataOptions options)
    {
    }

    public void SendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
    {
    }

    public int ReceiveData(byte[] data, out NetworkGamer? sender)
    {
        sender = null;
        return 0;
    }

    public int ReceiveData(byte[] data, int offset, out NetworkGamer? sender)
    {
        sender = null;
        return 0;
    }

    public int ReceiveData(PacketReader data, out NetworkGamer? sender)
    {
        sender = null;
        return 0;
    }

    public void SendPartyInvites()
    {
    }
}

/// <summary>One machine participating in a network session.</summary>
public sealed class NetworkMachine
{
    private readonly GamerCollection<NetworkGamer> gamers = new();

    internal NetworkMachine()
    {
    }

    public GamerCollection<NetworkGamer> Gamers => gamers;

    public void RemoveFromSession()
    {
    }
}
