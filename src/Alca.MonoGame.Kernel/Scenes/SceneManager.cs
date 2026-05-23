namespace Alca.MonoGame.Kernel.Scenes;

/// <summary>Manages scene transitions with fade in/out effects.</summary>
public sealed class SceneManager
{
    private enum FadeState { None, FadingOut, FadingIn }

    // Retained for future lifecycle hooks in Milestone 3.5
#pragma warning disable IDE0052
    private readonly Game _game;
#pragma warning restore IDE0052
    private Scene? _currentScene;
    private Scene? _queuedScene;

    private FadeState _fadeState = FadeState.None;
    private float _fadeTimer;
    private float _fadeAlpha;

    private const float FadeDuration = 0.3f;

    /// <summary>Gets the currently active scene.</summary>
    public Scene? CurrentScene => _currentScene;

    /// <summary>Exposed for unit testing only. Returns the current fade alpha value.</summary>
    internal float FadeAlpha => _fadeAlpha;

    /// <summary>Creates a new SceneManager bound to the given game instance.</summary>
    public SceneManager(Game game)
    {
        _game = game;
    }

    internal SceneManager()
    {
        _game = null!;
    }

    /// <summary>Requests a transition to the given scene with a fade effect.</summary>
    public void RequestChange(Scene scene)
    {
        _queuedScene = scene;
        if (_fadeState == FadeState.None)
        {
            _fadeState = FadeState.FadingOut;
            _fadeTimer = 0f;
        }
    }

    /// <summary>Updates fade state and the current scene.</summary>
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

    /// <summary>Draws the current scene.</summary>
    public void Draw(GameTime gameTime)
    {
        _currentScene?.Draw(gameTime);
    }

    /// <summary>Draws a full-screen black overlay at the current fade alpha.
    /// Must be called explicitly by the game after base.Draw().</summary>
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

    private void SetupAndStartScene(Scene scene)
    {
        scene.Initialize();
        scene.LoadContent();
    }
}
