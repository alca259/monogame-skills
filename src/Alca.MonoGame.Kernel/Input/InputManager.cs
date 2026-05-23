namespace Alca.MonoGame.Kernel.Input;

public sealed class InputManager
{
    /// <summary>The GamePads property is an array because MonoGame supports up to four gamepads simultaneously. Each gamepad is associated with a PlayerIndex (0-3).</summary>
    private const int MAX_GAMEPADS = 4;

    /// <summary>Gets the state information of keyboard input.</summary>
    public KeyboardInfo Keyboard { get; private set; }

    /// <summary>Gets the state information of mouse input.</summary>
    public MouseInfo Mouse { get; private set; }

    /// <summary>Gets the state information of a gamepad.</summary>
    public GamePadInfo[] GamePads { get; private set; }

    /// <summary>Initializes a new instance of the InputManager class.</summary>
    public InputManager()
    {
        Keyboard = new KeyboardInfo();
        Mouse = new MouseInfo();
        GamePads = new GamePadInfo[MAX_GAMEPADS];
        for (int i = 0; i < MAX_GAMEPADS; i++)
        {
            GamePads[i] = new GamePadInfo((PlayerIndex)i);
        }
    }

    /// <summary>Updates the state information of all input devices.</summary>
    public void Update(GameTime gameTime)
    {
        Keyboard.Update();
        Mouse.Update();

        for (int i = 0; i < MAX_GAMEPADS; i++)
        {
            GamePads[i].Update(gameTime);
        }
    }

}

