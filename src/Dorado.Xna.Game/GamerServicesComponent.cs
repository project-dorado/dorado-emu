namespace Microsoft.Xna.Framework.GamerServices;

/// <summary>Headless gamer services; nothing to initialize, nothing to pump.</summary>
public class GamerServicesComponent : GameComponent
{
    public GamerServicesComponent(Game game)
        : base(game)
    {
    }

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }
}
