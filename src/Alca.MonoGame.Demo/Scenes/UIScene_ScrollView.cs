namespace Alca.MonoGame.Demo.Scenes;

/// <summary>Scene 8/10 — demos ScrollView (vertical and horizontal).</summary>
public sealed class UIScene_ScrollView : Scene
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
        var root = new StackPanel { Orientation = Orientation.Vertical, Spacing = 16 };

        var backBtn = new Button(_font, "← Menú");
        backBtn.Clicked += () => Core.SceneManager.RequestChange(Core.GetService<UIScene_Menu>());
        root.Add(backBtn);
        root.Add(new Label { Font = _font, Text = "Scene 8/10: ScrollView", Color = Color.DimGray });
        root.Add(new Label { Font = _font, Text = "ScrollView Demo  (mouse wheel to scroll)", Color = Color.Yellow, HAlign = HAlign.Center });

        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20 };

        // Vertical scroll with 25 items
        var scrollV = new ScrollView(Core.GraphicsDevice);
        var contentV = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        for (int i = 1; i <= 25; i++)
            contentV.Add(new Label { Font = _font, Text = $"Ítem {i:00} — vertical list entry" });
        scrollV.Add(contentV);
        row.Add(scrollV);

        // Horizontal scroll with 10 wide items
        var scrollH = new ScrollView(Core.GraphicsDevice);
        var contentH = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        for (int i = 1; i <= 10; i++)
            contentH.Add(new Label { Font = _font, Text = $"  Categoría Muy Larga {i}  ", Color = Color.LightCyan });
        scrollH.Add(contentH);
        row.Add(scrollH);

        root.Add(row);
        _uiRoot.Add(root);
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
        Core.GraphicsDevice.Clear(new Color(20, 20, 30));
        _uiRoot.DrawAll(Core.SpriteBatch);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _pixel?.Dispose();
        base.Dispose(disposing);
    }
}
