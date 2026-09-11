using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>A bitmap font: glyph atlas, metrics and kerning.</summary>
public class SpriteFont : GraphicsResource
{
    private readonly ReadOnlyCollection<Rectangle> _glyphs;
    private readonly ReadOnlyCollection<Rectangle> _cropping;
    private readonly ReadOnlyCollection<char> _characterMap;
    private readonly ReadOnlyCollection<Vector3> _kerning;

    internal SpriteFont(
        Texture2D texture,
        IEnumerable<Rectangle> glyphs,
        IEnumerable<Rectangle> cropping,
        IEnumerable<char> characterMap,
        int lineSpacing,
        float spacing,
        IEnumerable<Vector3> kerning,
        char? defaultCharacter)
    {
        Texture = texture;
        _glyphs = new ReadOnlyCollection<Rectangle>(glyphs.ToList());
        _cropping = new ReadOnlyCollection<Rectangle>(cropping.ToList());
        _characterMap = new ReadOnlyCollection<char>(characterMap.ToList());
        LineSpacing = lineSpacing;
        Spacing = spacing;
        _kerning = new ReadOnlyCollection<Vector3>(kerning.ToList());
        DefaultCharacter = defaultCharacter;
        GraphicsDevice = texture.GraphicsDevice;
    }

    public Texture2D Texture { get; }

    public ReadOnlyCollection<Rectangle> Glyphs => _glyphs;

    public ReadOnlyCollection<Rectangle> Cropping => _cropping;

    public ReadOnlyCollection<char> CharacterMap => _characterMap;

    public ReadOnlyCollection<Vector3> Kerning => _kerning;

    public int LineSpacing { get; set; }

    public float Spacing { get; set; }

    public char? DefaultCharacter { get; set; }

    public Vector2 MeasureString(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Measure(text.AsSpan());
    }

    public Vector2 MeasureString(StringBuilder text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var builder = new StringBuilder(text.Length);
        for (int i = 0; i < text.Length; i++)
        {
            builder.Append(text[i]);
        }

        return Measure(builder.ToString().AsSpan());
    }

    internal float GetGlyphWidth(char character)
    {
        int index = FindGlyph(character);
        return index < 0 ? 0f : _kerning[index].Z;
    }

    internal int FindGlyph(char character)
    {
        int index = _characterMap.IndexOf(character);
        if (index >= 0)
        {
            return index;
        }

        return DefaultCharacter is { } fallback ? _characterMap.IndexOf(fallback) : -1;
    }

    private Vector2 Measure(ReadOnlySpan<char> text)
    {
        float lineWidth = 0f;
        float maxWidth = 0f;
        int lineCount = 1;
        foreach (char character in text)
        {
            if (character == '\n')
            {
                maxWidth = MathF.Max(maxWidth, lineWidth);
                lineWidth = 0f;
                lineCount++;
                continue;
            }

            if (character == '\r')
            {
                continue;
            }

            int index = FindGlyph(character);
            if (index >= 0)
            {
                lineWidth += _kerning[index].Z + Spacing;
            }
        }

        maxWidth = MathF.Max(maxWidth, lineWidth);
        return new Vector2(maxWidth, lineCount * LineSpacing);
    }
}
