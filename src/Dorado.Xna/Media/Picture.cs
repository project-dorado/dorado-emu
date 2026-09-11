using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Media;

/// <summary>A picture in the local media library.</summary>
public sealed class Picture : IEquatable<Picture>, IDisposable
{
    internal static readonly Picture Empty = new();

    internal Picture()
    {
    }

    public string Name { get; internal set; } = string.Empty;

    public PictureAlbum Album { get; internal set; } = PictureAlbum.Empty;

    public int Width => 0;

    public int Height => 0;

    public DateTime Date => DateTime.MinValue;

    public void Dispose()
    {
    }

    public Texture2D? GetTexture(IServiceProvider serviceProvider) => null;

    public Texture2D? GetThumbnail(IServiceProvider serviceProvider) => null;

    public static bool operator ==(Picture? first, Picture? second) => ReferenceEquals(first, second);

    public static bool operator !=(Picture? first, Picture? second) => !ReferenceEquals(first, second);

    public override bool Equals(object? obj) => ReferenceEquals(this, obj);

    public bool Equals(Picture? other) => ReferenceEquals(this, other);

    public override string ToString() => Name;

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
}

/// <summary>A read-only sequence of pictures.</summary>
public sealed class PictureCollection : IEnumerable<Picture>, IEnumerable, IDisposable
{
    internal static readonly PictureCollection Empty = new();

    private readonly Picture[] pictures;

    internal PictureCollection()
        : this([])
    {
    }

    internal PictureCollection(IEnumerable<Picture> pictures) => this.pictures = pictures.ToArray();

    public int Count => pictures.Length;

    public Picture this[int index] => pictures[index];

    public void Dispose()
    {
    }

    public IEnumerator<Picture> GetEnumerator() => ((IEnumerable<Picture>)pictures).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => pictures.GetEnumerator();
}
