using System.Collections;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A music artist in the local media library.</summary>
public sealed class Artist : IEquatable<Artist>, IDisposable
{
    internal static readonly Artist Empty = new();

    internal Artist()
    {
    }

    public string Name { get; internal set; } = string.Empty;

    public SongCollection Songs { get; internal set; } = SongCollection.Empty;

    public AlbumCollection Albums { get; internal set; } = AlbumCollection.Empty;

    public void Dispose()
    {
    }

    public static bool operator ==(Artist? first, Artist? second) => ReferenceEquals(first, second);

    public static bool operator !=(Artist? first, Artist? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(Artist? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of artists.</summary>
public sealed class ArtistCollection : IEnumerable<Artist>, IEnumerable, IDisposable
{
    internal static readonly ArtistCollection Empty = new();

    private readonly Artist[] artists;

    internal ArtistCollection()
        : this([])
    {
    }

    internal ArtistCollection(IEnumerable<Artist> artists) => this.artists = artists.ToArray();

    public int Count => artists.Length;

    public Artist this[int index] => artists[index];

    public void Dispose()
    {
    }

    public IEnumerator<Artist> GetEnumerator() => ((IEnumerable<Artist>)artists).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => artists.GetEnumerator();
}
