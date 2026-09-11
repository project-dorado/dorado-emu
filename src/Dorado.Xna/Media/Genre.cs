using System.Collections;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A music genre in the local media library.</summary>
public sealed class Genre : IEquatable<Genre>, IDisposable
{
    internal static readonly Genre Empty = new();

    internal Genre()
    {
    }

    public string Name { get; internal set; } = string.Empty;

    public SongCollection Songs { get; internal set; } = SongCollection.Empty;

    public AlbumCollection Albums { get; internal set; } = AlbumCollection.Empty;

    public void Dispose()
    {
    }

    public static bool operator ==(Genre? first, Genre? second) => ReferenceEquals(first, second);

    public static bool operator !=(Genre? first, Genre? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(Genre? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of genres.</summary>
public sealed class GenreCollection : IEnumerable<Genre>, IEnumerable, IDisposable
{
    internal static readonly GenreCollection Empty = new();

    private readonly Genre[] genres;

    internal GenreCollection()
        : this([])
    {
    }

    internal GenreCollection(IEnumerable<Genre> genres) => this.genres = genres.ToArray();

    public int Count => genres.Length;

    public Genre this[int index] => genres[index];

    public void Dispose()
    {
    }

    public IEnumerator<Genre> GetEnumerator() => ((IEnumerable<Genre>)genres).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => genres.GetEnumerator();
}
