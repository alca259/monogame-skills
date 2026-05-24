namespace Alca.MonoGame.Demo.Scenes;

/// <summary>Main menu — clickable list of all demo scenes.</summary>
public sealed class UIScene_Menu : Scene
{
    private readonly UIRoot _uiRoot = new();
    private readonly UIInteractionManager _interactionManager = new();

    private Texture2D _pixel = null!;
    private SpriteFont _font = null!;

    public override void LoadContent()
    {
        _pixel = new Texture2D(Core.GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _font = Content.Load<SpriteFont>("DefaultFont");
        BuildUI();
    }

    private void BuildUI()
    {
        var root = new StackPanel { Orientation = Orientation.Vertical, Spacing = 8 };

        root.Add(new Label { Font = _font, Text = "MonoGame UI Demo — Selecciona una escena", Color = Color.Yellow, HAlign = HAlign.Center });

        AddEntry(root, "1.  Basic Controls    (Button, Label, Checkbox, Panel)", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_BasicControls>()));
        AddEntry(root, "2.  Input Text        (TextBox, NumericBox, PasswordBox)", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_InputText>()));
        AddEntry(root, "3.  TextArea", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_TextArea>()));
        AddEntry(root, "4.  Sliders & Progress Bars", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_Sliders>()));
        AddEntry(root, "5.  Selection         (Dropdown, RadioButton)", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_Selection>()));
        AddEntry(root, "6.  Color Picker      (RGB + HSV)", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_ColorPicker>()));
        AddEntry(root, "7.  Layout            (StackPanel, Flow, Grid, Anchor, Canvas)", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_Layout>()));
        AddEntry(root, "8.  ScrollView", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_ScrollView>()));
        AddEntry(root, "9.  Tooltip", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_Tooltip>()));
        AddEntry(root, "10. Focus Manager     (Tab navigation)", () => Core.SceneManager.RequestChange(Core.GetService<UIScene_Focus>()));
        AddEntry(root, "—   ECS Hierarchy Demo", () => Core.SceneManager.RequestChange(Core.GetService<EcsDemoScene>()));

        _uiRoot.Add(root);
    }

    private void AddEntry(StackPanel parent, string text, Action onClick)
    {
        var btn = new Button(_font, text);
        btn.Clicked += onClick;
        parent.Add(btn);
    }

    public override void Update(GameTime gameTime)
    {
        _uiRoot.Update(gameTime);
        Rectangle screen = new(0, 0, Core.GraphicsDevice.Viewport.Width, Core.GraphicsDevice.Viewport.Height);
        _uiRoot.Measure(new Vector2(screen.Width, screen.Height));
        _uiRoot.Arrange(screen);
        _interactionManager.Update(_uiRoot, Core.Input.Mouse);
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(new Color(15, 15, 25));
        _uiRoot.DrawAll(Core.SpriteBatch);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _pixel?.Dispose();
        base.Dispose(disposing);
    }
}
