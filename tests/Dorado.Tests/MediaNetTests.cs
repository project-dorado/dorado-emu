using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;

namespace Dorado.Tests;

/// <summary>Smoke tests for the offline Media and Net surfaces of the XNA shim.</summary>
public sealed class MediaNetTests
{
    [Fact]
    public void MediaPlayerDefaultsToStoppedAtFullVolume()
    {
        MediaPlayer.Stop();
        MediaPlayer.Volume = 1f;

        Assert.Equal(MediaState.Stopped, MediaPlayer.State);
        Assert.Equal(1f, MediaPlayer.Volume);
        Assert.False(MediaPlayer.IsMuted);
        Assert.False(MediaPlayer.IsShuffled);
        Assert.False(MediaPlayer.IsRepeating);
        Assert.Null(MediaPlayer.Queue.ActiveSong);
    }

    [Fact]
    public void MediaLibraryExposesEmptyOfflineCollections()
    {
        using var library = new MediaLibrary();

        Assert.Equal("Zune", library.MediaSource.Name);
        Assert.Equal(MediaSourceType.LocalDevice, library.MediaSource.MediaSourceType);
        Assert.Empty(library.Songs);
        Assert.Empty(library.Albums);
        Assert.Empty(library.Artists);
        Assert.Empty(library.Genres);
        Assert.Empty(library.Pictures);
        Assert.Empty(library.Playlists);
        Assert.Empty(library.RootPictureAlbum.Albums);
    }

    [Fact]
    public void PlayingAnEmptyCollectionIsAnOfflineNoOp()
    {
        using var library = new MediaLibrary();

        MediaPlayer.Play(library.Songs);

        Assert.Equal(MediaState.Stopped, MediaPlayer.State);
        Assert.Equal(TimeSpan.Zero, MediaPlayer.PlayPosition);
        Assert.Null(MediaPlayer.Queue.ActiveSong);
    }

    [Fact]
    public void VolumeIsClampedToUnitRange()
    {
        MediaPlayer.Volume = 4f;
        Assert.Equal(1f, MediaPlayer.Volume);

        MediaPlayer.Volume = -2f;
        Assert.Equal(0f, MediaPlayer.Volume);

        MediaPlayer.Volume = 0.25f;
        Assert.Equal(0.25f, MediaPlayer.Volume);
    }

    [Fact]
    public void VisualizationDataHasFixedBuffers()
    {
        var data = new VisualizationData();

        Assert.Equal(256, data.Frequencies.Count);
        Assert.Equal(256, data.Samples.Count);
    }

    [Fact]
    public void PacketWriterRoundTripsPrimitiveAndXnaTypes()
    {
        var writer = new PacketWriter();
        writer.Write((byte)7);
        writer.Write(42);
        writer.Write(1.5f);
        writer.Write("dorado");
        writer.Write(new Vector3(1f, 2f, 3f));
        writer.Write(new Color(0x11223344));

        byte[] buffer = ((MemoryStream)writer.BaseStream).ToArray();
        var reader = new PacketReader();
        reader.BaseStream.Write(buffer, 0, buffer.Length);
        reader.Position = 0;

        Assert.Equal(7, reader.ReadByte());
        Assert.Equal(42, reader.ReadInt32());
        Assert.Equal(1.5f, reader.ReadSingle());
        Assert.Equal("dorado", reader.ReadString());
        Assert.Equal(new Vector3(1f, 2f, 3f), reader.ReadVector3());
        Assert.Equal(new Color(0x11223344), reader.ReadColor());
    }

    [Fact]
    public void NetworkSessionStartsOfflineAndDisposes()
    {
        using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 8);

        Assert.False(NetworkSession.IsAvailable);
        Assert.False(session.IsHost);
        Assert.Null(session.Host);
        Assert.Empty(session.AllGamers);
        Assert.Empty(session.LocalGamers);
        Assert.Empty(session.RemoteGamers);
        Assert.Equal(NetworkSessionState.Lobby, session.SessionState);

        session.StartGame();
        Assert.Equal(NetworkSessionState.Playing, session.SessionState);

        session.EndGame();
        Assert.Equal(NetworkSessionState.Lobby, session.SessionState);

        session.Update();
        session.Dispose();

        Assert.True(session.IsDisposed);
    }

    [Fact]
    public void NetworkSessionAsyncCreateCompletesSynchronously()
    {
        IAsyncResult? observed = null;
        IAsyncResult result = NetworkSession.BeginCreate(
            NetworkSessionType.Local,
            1,
            8,
            callbackResult => observed = callbackResult,
            "session-state");

        Assert.True(result.CompletedSynchronously);
        Assert.True(result.IsCompleted);
        Assert.Same(result, observed);
        Assert.Equal("session-state", result.AsyncState);

        using NetworkSession session = NetworkSession.EndCreate(result);
        Assert.NotNull(session);
    }

    [Fact]
    public void FindReturnsEmptyAvailableSessionList()
    {
        using AvailableNetworkSessionCollection sessions =
            NetworkSession.Find(NetworkSessionType.SystemLink, 1, new NetworkSessionProperties());

        Assert.Empty(sessions);
    }

    [Fact]
    public void SessionPropertiesExposeEightFixedSlots()
    {
        var properties = new NetworkSessionProperties();

        Assert.Equal(8, properties.Count);
        Assert.Null(properties[0]);

        properties[3] = 1234;
        Assert.Equal(1234, properties[3]);
        Assert.Equal(3, ((IList<int?>)properties).IndexOf(1234));

        Assert.Throws<ArgumentOutOfRangeException>(() => properties[8]);
        Assert.Throws<NotSupportedException>(() => ((IList<int?>)properties).Add(1));
    }

    [Fact]
    public void GamerCollectionIsReadOnly()
    {
        using NetworkSession session = NetworkSession.Create(NetworkSessionType.Local, 1, 8);
        GamerCollection<NetworkGamer> gamers = session.AllGamers;

        Assert.IsAssignableFrom<ReadOnlyCollection<NetworkGamer>>(gamers);
        Assert.IsAssignableFrom<IEnumerable<Gamer>>(gamers);
    }
}
