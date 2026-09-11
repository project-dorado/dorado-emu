using System.Text;
using Dorado.Platform;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>A 2-D sprite batcher. Draw calls are rasterized immediately by the backend.</summary>
public class SpriteBatch : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;

    public SpriteBatch(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
    }

    public GraphicsDevice GraphicsDevice => _graphicsDevice;

    public bool IsDisposed { get; private set; }

    public string Name { get; set; } = string.Empty;

    public object? Tag { get; set; }

    public event EventHandler? Disposing;

    public void Begin()
    {
    }

    public void Begin(SpriteBlendMode blendMode)
    {
    }

    public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, SaveStateMode saveStateMode)
    {
    }

    public void Begin(
        SpriteBlendMode blendMode,
        SpriteSortMode sortMode,
        SaveStateMode saveStateMode,
        Matrix transformMatrix)
    {
    }

    public void End()
    {
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color) =>
        Draw(texture, destinationRectangle, null, color);

    public void Draw(
        Texture2D texture,
        Rectangle destinationRectangle,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Rectangle source = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
        var request = new SpriteDrawRequest
        {
            Texture = texture.Backend,
            HasSource = true,
            SourceX = source.X,
            SourceY = source.Y,
            SourceWidth = source.Width,
            SourceHeight = source.Height,
            X = destinationRectangle.X,
            Y = destinationRectangle.Y,
            OriginX = origin.X,
            OriginY = origin.Y,
            ScaleX = source.Width == 0 ? 0f : destinationRectangle.Width / (float)source.Width,
            ScaleY = source.Height == 0 ? 0f : destinationRectangle.Height / (float)source.Height,
            Rotation = rotation,
            Tint = ToRgba(color),
            FlipX = (effects & SpriteEffects.FlipHorizontally) != 0,
            FlipY = (effects & SpriteEffects.FlipVertically) != 0,
            LayerDepth = layerDepth,
        };

        _graphicsDevice.Backend.DrawSprite(in request);
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Rectangle source = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);

        var request = new SpriteDrawRequest
        {
            Texture = texture.Backend,
            HasSource = true,
            SourceX = source.X,
            SourceY = source.Y,
            SourceWidth = source.Width,
            SourceHeight = source.Height,
            X = destinationRectangle.X,
            Y = destinationRectangle.Y,
            OriginX = 0f,
            OriginY = 0f,
            ScaleX = source.Width == 0 ? 0f : destinationRectangle.Width / (float)source.Width,
            ScaleY = source.Height == 0 ? 0f : destinationRectangle.Height / (float)source.Height,
            Rotation = 0f,
            Tint = ToRgba(color),
        };

        _graphicsDevice.Backend.DrawSprite(in request);
    }

    public void Draw(Texture2D texture, Vector2 position, Color color) =>
        Draw(texture, position, null, color);

    public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) =>
        Draw(texture, position, sourceRectangle, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

    public void Draw(
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Rectangle source = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);

        var request = new SpriteDrawRequest
        {
            Texture = texture.Backend,
            HasSource = true,
            SourceX = source.X,
            SourceY = source.Y,
            SourceWidth = source.Width,
            SourceHeight = source.Height,
            X = position.X,
            Y = position.Y,
            OriginX = origin.X,
            OriginY = origin.Y,
            ScaleX = scale.X,
            ScaleY = scale.Y,
            Rotation = rotation,
            Tint = ToRgba(color),
            FlipX = (effects & SpriteEffects.FlipHorizontally) != 0,
            FlipY = (effects & SpriteEffects.FlipVertically) != 0,
            LayerDepth = layerDepth,
        };

        _graphicsDevice.Backend.DrawSprite(in request);
    }

    public void Draw(
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth) =>
        Draw(texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth);

    public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color) =>
        DrawString(spriteFont, text, position, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

    public void DrawString(
        SpriteFont spriteFont,
        string text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth) =>
        DrawString(spriteFont, text, position, color, rotation, origin, new Vector2(scale, scale), effects, layerDepth);

    public void DrawString(
        SpriteFont spriteFont,
        string text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth)
    {
        ArgumentNullException.ThrowIfNull(spriteFont);
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        float x = 0f;
        float y = 0f;
        foreach (char character in text)
        {
            if (character == '\n')
            {
                x = 0f;
                y += spriteFont.LineSpacing;
                continue;
            }

            if (character == '\r')
            {
                continue;
            }

            int index = spriteFont.FindGlyph(character);
            if (index < 0)
            {
                continue;
            }

            Rectangle glyph = spriteFont.Glyphs[index];
            Vector3 kerning = spriteFont.Kerning[index];
            var offset = new Vector2(x + kerning.X, y + kerning.Y);
            Draw(
                spriteFont.Texture,
                position + offset,
                glyph,
                color,
                rotation,
                origin,
                scale,
                effects,
                layerDepth);
            x += kerning.Z + spriteFont.Spacing;
        }
    }

    public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color) =>
        DrawString(spriteFont, text?.ToString() ?? string.Empty, position, color);

    public void DrawString(
        SpriteFont spriteFont,
        StringBuilder text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth) =>
        DrawString(spriteFont, text?.ToString() ?? string.Empty, position, color, rotation, origin, scale, effects, layerDepth);

    public void DrawString(
        SpriteFont spriteFont,
        StringBuilder text,
        Vector2 position,
        Color color,
        float rotation,
        Vector2 origin,
        Vector2 scale,
        SpriteEffects effects,
        float layerDepth) =>
        DrawString(spriteFont, text?.ToString() ?? string.Empty, position, color, rotation, origin, scale, effects, layerDepth);

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        Disposing?.Invoke(this, EventArgs.Empty);
        GC.SuppressFinalize(this);
    }

    private static Rgba32 ToRgba(Color color) => new(color.R, color.G, color.B, color.A);
}
