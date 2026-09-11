namespace Microsoft.Xna.Framework.Media;

public enum MediaSourceType
{
    LocalDevice = 0,
    WindowsMediaConnect = 4,
}

/// <summary>A source of media. Dorado exposes only the on-device library.</summary>
public sealed class MediaSource
{
    internal MediaSource()
    {
        MediaSourceType = MediaSourceType.LocalDevice;
        Name = "Zune";
    }

    public MediaSourceType MediaSourceType { get; }

    public string Name { get; }

    public static IList<MediaSource> GetAvailableMediaSources() => [new MediaSource()];

    public override string ToString() => Name;
}

/// <summary>An offline view of the Zune media library; every collection is empty.</summary>
public sealed class MediaLibrary : IDisposable
{
    private bool isDisposed;

    public MediaLibrary()
        : this(new MediaSource())
    {
    }

    public MediaLibrary(MediaSource mediaSource)
    {
        ArgumentNullException.ThrowIfNull(mediaSource);
        MediaSource = mediaSource;
    }

    public MediaSource MediaSource { get; }

    public SongCollection Songs
    {
        get
        {
            ThrowIfDisposed();
            return SongCollection.Empty;
        }
    }

    public ArtistCollection Artists
    {
        get
        {
            ThrowIfDisposed();
            return ArtistCollection.Empty;
        }
    }

    public AlbumCollection Albums
    {
        get
        {
            ThrowIfDisposed();
            return AlbumCollection.Empty;
        }
    }

    public PlaylistCollection Playlists
    {
        get
        {
            ThrowIfDisposed();
            return PlaylistCollection.Empty;
        }
    }

    public GenreCollection Genres
    {
        get
        {
            ThrowIfDisposed();
            return GenreCollection.Empty;
        }
    }

    public PictureCollection Pictures
    {
        get
        {
            ThrowIfDisposed();
            return PictureCollection.Empty;
        }
    }

    public PictureAlbum RootPictureAlbum
    {
        get
        {
            ThrowIfDisposed();
            return PictureAlbum.Empty;
        }
    }

    public void Dispose() => isDisposed = true;

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(isDisposed, this);
}
