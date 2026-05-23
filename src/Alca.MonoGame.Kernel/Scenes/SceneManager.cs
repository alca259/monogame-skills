namespace Alca.MonoGame.Kernel.Scenes;

public class SceneManager
{
    private enum FadeState { None, FadingOut, FadingIn }

    private readonly Game _game;
    private Scene? _currentScene;
    private Scene? _queuedScene;

    private FadeState _fadeState = FadeState.None;
    private float _fadeTimer;
    private float _fadeAlpha;

    private const float FadeDuration = 0.3f;

    public Scene? CurrentScene => _currentScene;

    public SceneManager(Game game)
    {
        _game = game;
    }

    // Internal constructor for unit tests — bypasses MonoGame infrastructure
    internal SceneManager()
    {
        _game = null!;
    }

    public void RequestChange(Scene scene)
    {
        _queuedScene = scene;
        if (_fadeState == FadeState.None)
        {
            _fadeState = FadeState.FadingOut;
            _fadeTimer = 0f;
        }
    }

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        switch (_fadeState)
        {
            case FadeState.FadingOut:
                _fadeTimer += dt;
                _fadeAlpha = Math.Min(1f, _fadeTimer / FadeDuration);
                if (_fadeTimer >= FadeDuration)
                {
                    _fadeAlpha = 1f;
                    ApplyPendingChange();
                    _fadeState = FadeState.FadingIn;
                    _fadeTimer = 0f;
                }
                break;

            case FadeState.FadingIn:
                _fadeTimer += dt;
                _fadeAlpha = 1f - Math.Min(1f, _fadeTimer / FadeDuration);
                if (_fadeTimer >= FadeDuration)
                {
                    _fadeAlpha = 0f;
                    // If another scene was queued during fade-in, start fading out immediately
                    if (_queuedScene != null)
                    {
                        _fadeState = FadeState.FadingOut;
                        _fadeTimer = 0f;
                    }
                    else
                    {
                        _fadeState = FadeState.None;
                    }
                }
                break;
        }

        _currentScene?.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        _currentScene?.Draw(gameTime);
    }

    public void DrawFadeOverlay(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Texture2D texture)
    {
        if (_fadeAlpha <= 0f) return;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        spriteBatch.Draw(texture, graphicsDevice.Viewport.Bounds, Color.Black * _fadeAlpha);
        spriteBatch.End();
    }

    private void ApplyPendingChange()
    {
        _currentScene?.UnloadContent();

        _currentScene = _queuedScene!;
        _queuedScene = null;

        SetupAndStartScene(_currentScene);
    }

    protected virtual void SetupAndStartScene(Scene scene)
    {
        var content = new ContentManager(_game.Services, "Content");
        //scene.Setup(content, _game.GraphicsDevice, this, _game.Window);
        scene.Initialize();
        scene.LoadContent();
    }
}
