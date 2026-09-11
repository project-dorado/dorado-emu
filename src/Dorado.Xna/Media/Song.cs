using System.Collections;
using System.Runtime.CompilerServices;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A song from the local media library.</summary>
public sealed class Song : IEquatable<Song>, IDisposable
{
    internal static readonly Song Empty = new();

    private Song()
    {
    }

    internal Song(string name, string fileName, int durationMilliseconds)
    {
        Name = name;
        FileName = fileName;
        Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
    }

    public string Name { get; } = string.Empty;

    public Artist Artist { get; internal set; } = Artist.Empty;

    public Album Album { get; internal set; } = Album.Empty;

    public Genre Genre { get; internal set; } = Genre.Empty;

    public TimeSpan Duration { get; } = TimeSpan.Zero;

    public bool IsRated => Rating > 0;

    public int Rating { get; internal set; }

    public int PlayCount => 0;

    public int TrackNumber { get; internal set; }

    public bool IsProtected => false;

    internal string FileName { get; } = string.Empty;

    public void Dispose()
    {
    }

    public static bool operator ==(Song? first, Song? second) => ReferenceEquals(first, second);

    public static bool operator !=(Song? first, Song? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(Song? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of songs.</summary>
public sealed class SongCollection : IEnumerable<Song>, IEnumerable, IDisposable
{
    internal static readonly SongCollection Empty = new();

    private readonly Song[] songs;

    internal SongCollection()
        : this([])
    {
    }

    internal SongCollection(IEnumerable<Song> songs) => this.songs = songs.ToArray();

    public int Count => songs.Length;

    public Song this[int index] => songs[index];

    public void Dispose()
    {
    }

    public IEnumerator<Song> GetEnumerator() => ((IEnumerable<Song>)songs).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => songs.GetEnumerator();
}
