using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A music album in the local media library.</summary>
public sealed class Album : IEquatable<Album>, IDisposable
{
    internal static readonly Album Empty = new();

    internal Album()
    {
    }

    public string Name { get; internal set; } = string.Empty;

    public Artist Artist { get; internal set; } = Artist.Empty;

    public SongCollection Songs { get; internal set; } = SongCollection.Empty;

    public Genre Genre { get; internal set; } = Genre.Empty;

    public TimeSpan Duration => TimeSpan.Zero;

    public bool HasArt => false;

    public void Dispose()
    {
    }

    public Texture2D? GetAlbumArt(IServiceProvider serviceProvider) => null;

    public Texture2D? GetThumbnail(IServiceProvider serviceProvider) => null;

    public static bool operator ==(Album? first, Album? second) => ReferenceEquals(first, second);

    public static bool operator !=(Album? first, Album? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(Album? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of albums.</summary>
public sealed class AlbumCollection : IEnumerable<Album>, IEnumerable, IDisposable
{
    internal static readonly AlbumCollection Empty = new();

    private readonly Album[] albums;

    internal AlbumCollection()
        : this([])
    {
    }

    internal AlbumCollection(IEnumerable<Album> albums) => this.albums = albums.ToArray();

    public int Count => albums.Length;

    public Album this[int index] => albums[index];

    public void Dispose()
    {
    }

    public IEnumerator<Album> GetEnumerator() => ((IEnumerable<Album>)albums).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => albums.GetEnumerator();
}
