using Alca.MonoGame.Kernel.Audio;
using Alca.MonoGame.Kernel.Input;
using Alca.MonoGame.Kernel.Scenes;

namespace Alca.MonoGame.Kernel;

/// <summary>Core extended Game class for MonoGame applications.</summary>
public abstract class Core : Game
{
    private static Core _instance;

    // The scene that is currently active.
    private static Scene? _activeScene;

    // The next scene to switch to, if there is one.
    private static Scene? _nextScene;

    /// <summary>Indicates if the game is currently in fullscreen mode.</summary>
    private bool _isFullScreen;

    /// <summary>Gets reference to the Core instance.</summary>
    public static Core Instance => _instance;
    /// <summary>Gets the graphics device manager to control the presentationof graphics.</summary>
    public static GraphicsDeviceManager Graphics { get; private set; }
    /// <summary>Get the graphics device used to create graphical resources and perform primitive rendering.</summary>
    public static new GraphicsDevice GraphicsDevice { get; private set; }
    public static GraphicsDevice GameGraphicsDevice { get; private set; }
    /// <summary>Gets the sprite batch used for all 2D rendering.</summary>
    public static SpriteBatch SpriteBatch { get; private set; }
    /// <summary>Gets the content manager used to load global assets.</summary>
    public static new ContentManager Content { get; private set; }
    /// <summary>Gets a reference to the input management system.</summary>
    public static InputManager Input { get; private set; }
    /// <summary>Gets or Sets a value that indicates if the game should exit when the esc key on the keyboard is pressed.</summary>
    public static bool ExitOnEscape { get; set; }
    /// <summary>Gets a reference to the audio control system.</summary>
    public static AudioController Audio { get; private set; }

    /// <summary>Creates a new Core instance.</summary>
    /// <param name="title">The title to display in the title bar of the game window.</param>
    /// <param name="width">The initial width, in pixels, of the game window.</param>
    /// <param name="height">The initial height, in pixels, of the game window.</param>
    /// <param name="fullScreen">Indicates if the game should start in fullscreen mode.</param>
    protected Core(string title, int width, int height, bool fullScreen)
    {
        // Ensure only one instance of Core exists
        if (_instance != null)
        {
            throw new InvalidOperationException("Only a single Core instance can be created");
        }

        _instance = this;

        // Create a new graphics device manager and set the graphics defaults.
        _isFullScreen = fullScreen;
        Graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = width,
            PreferredBackBufferHeight = height,
            IsFullScreen = fullScreen
        };

        // Apply the graphics settings.
        Graphics.ApplyChanges();

        // Set the game window title.
        Window.Title = title;

        // Set the core's content manager to a reference of the base Game's content manager.
        Content = base.Content;

        // Set the root directory for content.
        Content.RootDirectory = "Content";

        // Mouse is visible by default.
        IsMouseVisible = true;

        // Exit on escape is true by default
        ExitOnEscape = true;
    }

    /// <summary>Called before the game is initialized.</summary>
    protected virtual void PreInitialize()
    {
        GameGraphicsDevice = base.GraphicsDevice;
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
        PreInitialize();

        base.Initialize();

        // Set the core's graphics device to a reference of the base Game's graphics device.
        GraphicsDevice = base.GraphicsDevice;

        // Create the sprite batch instance.
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a new input manager.
        Input = new InputManager();

        // Create a new audio controller.
        Audio = new AudioController();

        PostInitialize();
    }

    /// <summary>Called after the game has been initialized but before the first Update method is called.</summary>
    protected virtual void PostInitialize()
    {
    }

    /// <inheritdoc/>
    protected override void Update(GameTime gameTime)
    {
        // Update the input manager.
        Input.Update(gameTime);

        // Update the audio controller.
        Audio.Update();

        if (ExitOnEscape && Input.Keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        if (Input.Keyboard.IsKeyDown(Keys.F11))
        {
            // Toggle fullscreen mode when F11 is pressed.
            _isFullScreen = !_isFullScreen;
            Graphics.IsFullScreen = _isFullScreen;
            Graphics.ApplyChanges();
        }

        // if there is a next scene waiting to be switch to, then transition
        // to that scene.
        if (_nextScene != null)
        {
            TransitionScene();
        }

        // If there is an active scene, update it.
        _activeScene?.Update(gameTime);

        base.Update(gameTime);
    }

    /// <inheritdoc/>
    protected override void Draw(GameTime gameTime)
    {
        // If there is an active scene, draw it.
        _activeScene?.Draw(gameTime);

        base.Draw(gameTime);
    }

    /// <inheritdoc/>
    protected override void UnloadContent()
    {
        // Dispose of the audio controller.
        Audio.Dispose();

        base.UnloadContent();
    }

    public static void ChangeScene(Scene next)
    {
        // Only set the next scene value if it is not the same
        // instance as the currently active scene.
        if (_activeScene != next)
        {
            _nextScene = next;
        }
    }

    private static void TransitionScene()
    {
        // If there is an active scene, dispose of it.
        _activeScene?.Dispose();

        // Force the garbage collector to collect to ensure memory is cleared.
        GC.Collect();

        // Change the currently active scene to the new scene.
        _activeScene = _nextScene;

        // Null out the next scene value so it does not trigger a change over and over.
        _nextScene = null;

        // If the active scene now is not null, initialize it.
        // Remember, just like with Game, the Initialize call also calls the
        // Scene.LoadContent
        _activeScene?.Initialize();
    }
}
