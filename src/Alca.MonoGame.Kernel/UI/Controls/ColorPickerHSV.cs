using Alca.MonoGame.Kernel.Graphics;

namespace Alca.MonoGame.Kernel.UI.Controls;

/// <summary>A color picker that uses a hue bar and a saturation/value square for ergonomic color selection.</summary>
public sealed class ColorPickerHSV : UIContainer
{
    #region Constants

    private const int HueBarHeight = 20;
    private const int SwatchSize = 40;
    private const int HexBoxHeight = 28;
    private const int Spacing = 6;
    private const int CrosshairHalfSize = 6;

    #endregion

    #region Fields

    private readonly TextBox _hexInput;
    private readonly GraphicsDevice? _graphicsDevice;

    private Texture2D? _hueBarTexture;
    private Texture2D? _svSquareTexture;

    private float _hue;
    private float _saturation = 1f;
    private float _brightness = 1f;
    private Color _selectedColor;

    private Rectangle _hueBarBounds;
    private Rectangle _svSquareBounds;
    private Rectangle _swatchBounds;

    private bool _draggingHue;
    private bool _draggingSv;
    private bool _suppressHexCallback;

    #endregion

    #region Properties

    /// <summary>1×1 white pixel texture for borders and the preview swatch.</summary>
    public Texture2D? Pixel { get; set; }

    /// <summary>Border color used around the hue bar and SV square.</summary>
    public Color BorderColor { get; set; } = new Color(120, 120, 120);

    /// <summary>The currently selected color.</summary>
    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            _selectedColor = value;
            ColorPickerUtils.RgbToHsv(_selectedColor, out _hue, out _saturation, out _brightness);
            SyncHexFromColor();
            RegenerateSvTexture();
        }
    }

    /// <summary>Fired whenever the color changes during drag or hex input.</summary>
    public event Action<Color>? ColorChanged;

    /// <summary>Fired when the user releases the drag handle or commits the hex input.</summary>
    public event Action<Color>? ColorCommitted;

    #endregion

    #region Constructor

    /// <summary>Creates a ColorPickerHSV. Pass a GraphicsDevice to generate gradient textures on layout.</summary>
    public ColorPickerHSV(GraphicsDevice? graphicsDevice, SpriteFont? font, Texture2D? pixel)
    {
        _graphicsDevice = graphicsDevice;
        Pixel = pixel;

        _hexInput = new TextBox(font, pixel, null) { MaxLength = 7 };
        _hexInput.TextChanged += OnHexChanged;
        Add(_hexInput);

        _selectedColor = Color.Red;
        ColorPickerUtils.RgbToHsv(_selectedColor, out _hue, out _saturation, out _brightness);
    }

    #endregion

    #region Layout

    /// <inheritdoc/>
    public override void Measure(Vector2 availableSize)
    {
        int w = (int)availableSize.X;
        int svSize = w - SwatchSize - Spacing;
        int h = HueBarHeight + Spacing + svSize + Spacing + HexBoxHeight;
        DesiredSize = new Vector2(w, h);
    }

    /// <inheritdoc/>
    public override void Arrange(Rectangle finalBounds)
    {
        Bounds = finalBounds;

        int x = finalBounds.X;
        int y = finalBounds.Y;
        int w = finalBounds.Width;
        int svSize = w - SwatchSize - Spacing;

        _hueBarBounds = new Rectangle(x, y, svSize, HueBarHeight);
        y += HueBarHeight + Spacing;

        _svSquareBounds = new Rectangle(x, y, svSize, svSize);
        _swatchBounds = new Rectangle(x + svSize + Spacing, finalBounds.Y, SwatchSize, HueBarHeight + Spacing + svSize);
        y += svSize + Spacing;

        _hexInput.Arrange(new Rectangle(x, y, svSize, HexBoxHeight));

        GenerateHueBarTexture();
        RegenerateSvTexture();
        SyncHexFromColor();
    }

    #endregion

    #region Texture generation

    private void GenerateHueBarTexture()
    {
        if (_graphicsDevice is null || _hueBarBounds.Width <= 0 || _hueBarBounds.Height <= 0) return;

        _hueBarTexture?.Dispose();
        int w = _hueBarBounds.Width;
        int h = _hueBarBounds.Height;
        _hueBarTexture = new Texture2D(_graphicsDevice, w, h);

        Color[] data = new Color[w * h];
        for (int col = 0; col < w; col++)
        {
            Color c = ColorPickerUtils.HsvToRgb(col / (float)w * 360f, 1f, 1f);
            for (int row = 0; row < h; row++)
                data[row * w + col] = c;
        }

        _hueBarTexture.SetData(data);
    }

    private void RegenerateSvTexture()
    {
        if (_graphicsDevice is null || _svSquareBounds.Width <= 0 || _svSquareBounds.Height <= 0) return;

        _svSquareTexture?.Dispose();
        int w = _svSquareBounds.Width;
        int h = _svSquareBounds.Height;
        _svSquareTexture = new Texture2D(_graphicsDevice, w, h);

        Color[] data = new Color[w * h];
        for (int row = 0; row < h; row++)
        {
            float v = 1f - row / (float)(h - 1);
            for (int col = 0; col < w; col++)
            {
                float s = col / (float)(w - 1);
                data[row * w + col] = ColorPickerUtils.HsvToRgb(_hue, s, v);
            }
        }

        _svSquareTexture.SetData(data);
    }

    #endregion

    #region Update / Draw

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;
        base.Update(gameTime);

        MouseState ms = Mouse.GetState();
        Point mousePos = ms.Position;
        bool held = ms.LeftButton == ButtonState.Pressed;

        if (_draggingHue)
        {
            if (!held)
            {
                _draggingHue = false;
                ColorCommitted?.Invoke(_selectedColor);
            }
            else
            {
                ApplyHueDrag(mousePos.X);
            }
        }
        else if (_draggingSv)
        {
            if (!held)
            {
                _draggingSv = false;
                ColorCommitted?.Invoke(_selectedColor);
            }
            else
            {
                ApplySvDrag(mousePos);
            }
        }
        else if (ms.LeftButton == ButtonState.Pressed)
        {
            if (_hueBarBounds.Contains(mousePos))
            {
                _draggingHue = true;
                ApplyHueDrag(mousePos.X);
            }
            else if (_svSquareBounds.Contains(mousePos))
            {
                _draggingSv = true;
                ApplySvDrag(mousePos);
            }
        }
    }

    private void ApplyHueDrag(int screenX)
    {
        if (_hueBarBounds.Width <= 0) return;
        float t = MathHelper.Clamp((screenX - _hueBarBounds.X) / (float)_hueBarBounds.Width, 0f, 1f);
        _hue = t * 360f;
        RebuildColor();
        RegenerateSvTexture();
    }

    private void ApplySvDrag(Point mousePos)
    {
        if (_svSquareBounds.Width <= 0 || _svSquareBounds.Height <= 0) return;
        _saturation = MathHelper.Clamp((mousePos.X - _svSquareBounds.X) / (float)_svSquareBounds.Width, 0f, 1f);
        _brightness = 1f - MathHelper.Clamp((mousePos.Y - _svSquareBounds.Y) / (float)_svSquareBounds.Height, 0f, 1f);
        RebuildColor();
    }

    private void RebuildColor()
    {
        _selectedColor = ColorPickerUtils.HsvToRgb(_hue, _saturation, _brightness);
        SyncHexFromColor();
        ColorChanged?.Invoke(_selectedColor);
    }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        float opacity = EffectiveOpacity;

        // Hue bar
        if (_hueBarTexture is not null)
            spriteBatch.Draw(_hueBarTexture, _hueBarBounds, Color.White * opacity);

        if (Pixel is not null)
        {
            // Hue cursor line
            int hueCursorX = _hueBarBounds.X + (int)(_hue / 360f * _hueBarBounds.Width);
            spriteBatch.Draw(Pixel,
                new Rectangle(hueCursorX - 1, _hueBarBounds.Y, 2, _hueBarBounds.Height),
                Color.White * opacity);

            DrawHelper.DrawBorder(Pixel, spriteBatch, _hueBarBounds, BorderColor * opacity, 1);
        }

        // SV square
        if (_svSquareTexture is not null)
            spriteBatch.Draw(_svSquareTexture, _svSquareBounds, Color.White * opacity);

        if (Pixel is not null)
        {
            // Crosshair cursor
            int crossX = _svSquareBounds.X + (int)(_saturation * (_svSquareBounds.Width - 1));
            int crossY = _svSquareBounds.Y + (int)((1f - _brightness) * (_svSquareBounds.Height - 1));

            spriteBatch.Draw(Pixel,
                new Rectangle(crossX - CrosshairHalfSize, crossY - 1, CrosshairHalfSize * 2, 2),
                Color.White * opacity);
            spriteBatch.Draw(Pixel,
                new Rectangle(crossX - 1, crossY - CrosshairHalfSize, 2, CrosshairHalfSize * 2),
                Color.White * opacity);

            DrawHelper.DrawBorder(Pixel, spriteBatch, _svSquareBounds, BorderColor * opacity, 1);

            // Swatch
            spriteBatch.Draw(Pixel, _swatchBounds, _selectedColor * opacity);
            DrawHelper.DrawBorder(Pixel, spriteBatch, _swatchBounds, BorderColor * opacity, 1);
        }

        // Children (hex input)
        for (int i = 0; i < ChildrenReadOnly.Count; i++)
        {
            if (ChildrenReadOnly[i].IsVisible)
                ChildrenReadOnly[i].Draw(spriteBatch);
        }
    }

    #endregion

    #region Hex sync

    private void SyncHexFromColor()
    {
        string hex = ColorPickerUtils.ColorToHex(_selectedColor);
        if (_hexInput.Text == hex) return;
        _suppressHexCallback = true;
        _hexInput.SetText(hex);
        _suppressHexCallback = false;
    }

    private void OnHexChanged(string text)
    {
        if (_suppressHexCallback) return;
        Color? parsed = ColorPickerUtils.HexToColor(text);
        if (parsed is null) return;
        _selectedColor = parsed.Value;
        ColorPickerUtils.RgbToHsv(_selectedColor, out _hue, out _saturation, out _brightness);
        RegenerateSvTexture();
        ColorChanged?.Invoke(_selectedColor);
        ColorCommitted?.Invoke(_selectedColor);
    }

    #endregion

    #region Dispose

    /// <summary>Releases generated gradient textures.</summary>
    public void Dispose()
    {
        _hueBarTexture?.Dispose();
        _svSquareTexture?.Dispose();
    }

    #endregion
}
