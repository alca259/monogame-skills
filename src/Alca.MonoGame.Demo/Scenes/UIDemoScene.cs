namespace Alca.MonoGame.Demo.Scenes;

/// <summary>
/// Demo scene showcasing the UI system controls.
/// Add a SpriteFont asset named "DefaultFont" to Content/Content.mgcb to see rendered text.
/// </summary>
public sealed class UIDemoScene : Scene
{
    private SpriteFont? _font;
    private string _statusText = "UI Demo — add Content/DefaultFont.spritefont to enable text rendering.";

    private readonly GameWorld _world = new();

    public override void LoadContent()
    {
        try
        {
            _font = Content.Load<SpriteFont>("DefaultFont");
            _statusText = "UI Demo loaded. Press Tab to navigate controls.";
        }
        catch
        {
            // No content loaded yet; demo still runs without a font.
        }
    }

    public override void Update(GameTime gameTime)
    {
        _world.Update(gameTime);

        if (Core.Input.IsKeyReleased(Keys.Space))
            Core.SceneManager.RequestChange(Core.GetService<EcsDemoScene>());
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(30, 30, 30));

        Core.SpriteBatch.Begin();

        if (_font is not null)
        {
            Core.SpriteBatch.DrawString(_font, _statusText, new Vector2(20, 20), Color.White);
            Core.SpriteBatch.DrawString(_font, "Press [Space] to switch to ECS Demo", new Vector2(20, 50), Color.LightGray);
        }

        Core.SpriteBatch.End();

        _world.Draw(gameTime, Core.SpriteBatch);
    }
}
