using System.Collections;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A folder of pictures in the local media library.</summary>
public sealed class PictureAlbum : IEquatable<PictureAlbum>, IDisposable
{
    internal static readonly PictureAlbum Empty = new();

    internal PictureAlbum()
    {
    }

    public string Name { get; internal set; } = string.Empty;

    public PictureAlbumCollection Albums { get; internal set; } = PictureAlbumCollection.Empty;

    public PictureCollection Pictures { get; internal set; } = PictureCollection.Empty;

    public PictureAlbum? Parent { get; internal set; }

    public void Dispose()
    {
    }

    public static bool operator ==(PictureAlbum? first, PictureAlbum? second) => ReferenceEquals(first, second);

    public static bool operator !=(PictureAlbum? first, PictureAlbum? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(PictureAlbum? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of picture albums.</summary>
public sealed class PictureAlbumCollection : IEnumerable<PictureAlbum>, IEnumerable, IDisposable
{
    internal static readonly PictureAlbumCollection Empty = new();

    private readonly PictureAlbum[] albums;

    internal PictureAlbumCollection()
        : this([])
    {
    }

    internal PictureAlbumCollection(IEnumerable<PictureAlbum> albums) => this.albums = albums.ToArray();

    public int Count => albums.Length;

    public PictureAlbum this[int index] => albums[index];

    public void Dispose()
    {
    }

    public IEnumerator<PictureAlbum> GetEnumerator() => ((IEnumerable<PictureAlbum>)albums).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => albums.GetEnumerator();
}
