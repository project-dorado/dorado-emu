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

    public void Begin()
    {
    }

    public void Begin(SpriteBlendMode blendMode)
    {
    }

    public void Begin(SpriteBlendMode blendMode, SpriteSortMode sortMode, SaveStateMode saveStateMode)
    {
    }

    public void End()
    {
    }

    public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color) =>
        Draw(texture, destinationRectangle, null, color);

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

    public void Dispose()
    {
    }

    private static Rgba32 ToRgba(Color color) => new(color.R, color.G, color.B, color.A);
}
