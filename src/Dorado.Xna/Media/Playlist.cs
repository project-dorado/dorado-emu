using System.Collections;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A saved playlist in the local media library.</summary>
public sealed class Playlist : IEquatable<Playlist>, IDisposable
{
    internal static readonly Playlist Empty = new();

    internal Playlist()
    {
    }

    public string Name { get; internal set; } = string.Empty;

    public SongCollection Songs { get; internal set; } = SongCollection.Empty;

    public TimeSpan Duration => TimeSpan.Zero;

    public void Dispose()
    {
    }

    public static bool operator ==(Playlist? first, Playlist? second) => ReferenceEquals(first, second);

    public static bool operator !=(Playlist? first, Playlist? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(Playlist? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of playlists.</summary>
public sealed class PlaylistCollection : IEnumerable<Playlist>, IEnumerable, IDisposable
{
    internal static readonly PlaylistCollection Empty = new();

    private readonly Playlist[] playlists;

    internal PlaylistCollection()
        : this([])
    {
    }

    internal PlaylistCollection(IEnumerable<Playlist> playlists) => this.playlists = playlists.ToArray();

    public int Count => playlists.Length;

    public Playlist this[int index] => playlists[index];

    public void Dispose()
    {
    }

    public IEnumerator<Playlist> GetEnumerator() => ((IEnumerable<Playlist>)playlists).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => playlists.GetEnumerator();
}
