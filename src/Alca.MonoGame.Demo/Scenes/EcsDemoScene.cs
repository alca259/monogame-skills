namespace Alca.MonoGame.Demo.Scenes;

/// <summary>
/// Demo scene showcasing the ECS hierarchy system.
/// Creates a parent entity and child entities to demonstrate Transform hierarchy.
/// </summary>
public sealed class EcsDemoScene : Scene
{
    private SpriteFont? _font;
    private readonly GameWorld _world = new();
    private GameEntity _parentEntity = null!;
    private GameEntity _childEntity = null!;
    private float _angle;

    public override void LoadContent()
    {
        try { _font = Content.Load<SpriteFont>("DefaultFont"); }
        catch { /* No font available */ }
    }

    protected override void PostInitialize()
    {
        base.PostInitialize();
        _parentEntity = _world.CreateEntity("Parent", new Vector2(640, 360));
        _childEntity = _world.CreateEntity("Child", new Vector2(100, 0));
        _childEntity.SetParent(_parentEntity);
    }

    public override void Update(GameTime gameTime)
    {
        _angle += (float)gameTime.ElapsedGameTime.TotalSeconds;
        _parentEntity.Transform.Rotation2d = _angle;

        _world.Update(gameTime);

        if (Core.Input.IsKeyReleased(Keys.Space))
            Core.SceneManager.RequestChange(Core.GetService<UIDemoScene>());
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(20, 20, 40));

        Core.SpriteBatch.Begin();

        if (_font is not null)
        {
            var parentPos = _parentEntity.Transform.Position;
            var childPos = _childEntity.Transform.Position;

            Core.SpriteBatch.DrawString(_font, "ECS Hierarchy Demo", new Vector2(20, 20), Color.White);
            Core.SpriteBatch.DrawString(_font, $"Parent world pos: {parentPos.X:F0}, {parentPos.Y:F0}", new Vector2(20, 50), Color.LightGreen);
            Core.SpriteBatch.DrawString(_font, $"Child world pos:  {childPos.X:F0}, {childPos.Y:F0}", new Vector2(20, 75), Color.LightBlue);
            Core.SpriteBatch.DrawString(_font, $"Child local pos:  {_childEntity.Transform.LocalPosition.X:F0}, {_childEntity.Transform.LocalPosition.Y:F0}", new Vector2(20, 100), Color.LightYellow);
            Core.SpriteBatch.DrawString(_font, "Press [Space] to switch to UI Demo", new Vector2(20, 140), Color.LightGray);
        }

        Core.SpriteBatch.End();

        _world.Draw(gameTime, Core.SpriteBatch);
    }
}
