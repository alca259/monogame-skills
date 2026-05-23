using Alca.MonoGame.Kernel.Graphics;

namespace Alca.MonoGame.Kernel.UI.Controls;

/// <summary>A color picker that exposes three R/G/B sliders plus a hex text input and a preview swatch.</summary>
public sealed class ColorPickerRGB : UIContainer
{
    #region Constants

    private const int SliderHeight = 28;
    private const int SwatchSize = 40;
    private const int HexBoxHeight = 28;
    private const int Spacing = 6;
    private const int LabelWidth = 20;

    #endregion

    #region Fields

    private readonly Slider _sliderR;
    private readonly Slider _sliderG;
    private readonly Slider _sliderB;
    private readonly TextBox _hexInput;
    private readonly Label _labelR;
    private readonly Label _labelG;
    private readonly Label _labelB;

    private Color _selectedColor;
    private bool _suppressSliderSync;
    private bool _suppressHexCallback;

    #endregion

    #region Properties

    /// <summary>1×1 white pixel texture forwarded to sliders and the preview swatch.</summary>
    public Texture2D? Pixel { get; set; }

    /// <summary>The currently selected color. Changing this property pushes values to all sliders.</summary>
    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            _selectedColor = value;
            SyncSlidersFromColor();
        }
    }

    /// <summary>Color of the preview swatch background border.</summary>
    public Color SwatchBorderColor { get; set; } = new Color(120, 120, 120);

    /// <summary>Fired whenever the color changes due to slider interaction.</summary>
    public event Action<Color>? ColorChanged;

    /// <summary>Fired when the user releases a slider or confirms the hex input.</summary>
    public event Action<Color>? ColorCommitted;

    #endregion

    #region Constructor

    /// <summary>Creates a ColorPickerRGB. Pass a 1×1 white pixel texture for rendering.</summary>
    public ColorPickerRGB(SpriteFont? font, Texture2D? pixel)
    {
        Pixel = pixel;

        _labelR = new Label { Text = "R", Font = font, Color = new Color(230, 80, 80) };
        _labelG = new Label { Text = "G", Font = font, Color = new Color(80, 200, 80) };
        _labelB = new Label { Text = "B", Font = font, Color = new Color(80, 130, 230) };

        _sliderR = new Slider(pixel) { MinValue = 0f, MaxValue = 255f, Step = 1f };
        _sliderG = new Slider(pixel) { MinValue = 0f, MaxValue = 255f, Step = 1f };
        _sliderB = new Slider(pixel) { MinValue = 0f, MaxValue = 255f, Step = 1f };

        _sliderR.FillColor = new Color(200, 60, 60);
        _sliderG.FillColor = new Color(60, 180, 60);
        _sliderB.FillColor = new Color(60, 110, 200);

        _sliderR.ValueChanged += _ => OnSliderChanged();
        _sliderG.ValueChanged += _ => OnSliderChanged();
        _sliderB.ValueChanged += _ => OnSliderChanged();

        _hexInput = new TextBox(font, pixel, null) { MaxLength = 7 };
        _hexInput.TextChanged += OnHexChanged;

        Add(_labelR);
        Add(_sliderR);
        Add(_labelG);
        Add(_sliderG);
        Add(_labelB);
        Add(_sliderB);
        Add(_hexInput);

        _selectedColor = Color.White;
        SyncSlidersFromColor();
    }

    #endregion

    #region Layout

    /// <inheritdoc/>
    public override void Measure(Vector2 availableSize)
    {
        int w = (int)availableSize.X;
        int rows = 3; // R, G, B
        int h = rows * (SliderHeight + Spacing) + SwatchSize + Spacing + HexBoxHeight + Spacing;
        DesiredSize = new Vector2(w, h);
    }

    /// <inheritdoc/>
    public override void Arrange(Rectangle finalBounds)
    {
        Bounds = finalBounds;

        int x = finalBounds.X;
        int y = finalBounds.Y;
        int w = finalBounds.Width;
        int sliderX = x + LabelWidth + Spacing;
        int sliderW = w - LabelWidth - Spacing - SwatchSize - Spacing;

        // Row R
        _labelR.Arrange(new Rectangle(x, y + (SliderHeight - 16) / 2, LabelWidth, 16));
        _sliderR.Arrange(new Rectangle(sliderX, y, sliderW, SliderHeight));
        y += SliderHeight + Spacing;

        // Row G
        _labelG.Arrange(new Rectangle(x, y + (SliderHeight - 16) / 2, LabelWidth, 16));
        _sliderG.Arrange(new Rectangle(sliderX, y, sliderW, SliderHeight));
        y += SliderHeight + Spacing;

        // Row B
        _labelB.Arrange(new Rectangle(x, y + (SliderHeight - 16) / 2, LabelWidth, 16));
        _sliderB.Arrange(new Rectangle(sliderX, y, sliderW, SliderHeight));
        y += SliderHeight + Spacing;

        // Hex input
        _hexInput.Arrange(new Rectangle(x, y, w - SwatchSize - Spacing, HexBoxHeight));

        // Preview swatch is drawn inline in Draw — no separate UIElement needed
    }

    #endregion

    #region Color sync

    private void SyncSlidersFromColor()
    {
        _suppressSliderSync = true;
        _sliderR.Value = _selectedColor.R;
        _sliderG.Value = _selectedColor.G;
        _sliderB.Value = _selectedColor.B;
        _suppressSliderSync = false;
        SyncHexFromColor();
    }

    private void SyncHexFromColor()
    {
        string hex = ColorPickerUtils.ColorToHex(_selectedColor);
        if (_hexInput.Text == hex) return;
        _suppressHexCallback = true;
        _hexInput.SetText(hex);
        _suppressHexCallback = false;
    }

    private void OnSliderChanged()
    {
        if (_suppressSliderSync) return;
        _selectedColor = new Color((byte)_sliderR.Value, (byte)_sliderG.Value, (byte)_sliderB.Value);
        SyncHexFromColor();
        ColorChanged?.Invoke(_selectedColor);
    }

    private void OnHexChanged(string text)
    {
        if (_suppressHexCallback) return;
        Color? parsed = ColorPickerUtils.HexToColor(text);
        if (parsed is null) return;
        _selectedColor = parsed.Value;
        _suppressSliderSync = true;
        _sliderR.Value = _selectedColor.R;
        _sliderG.Value = _selectedColor.G;
        _sliderB.Value = _selectedColor.B;
        _suppressSliderSync = false;
        ColorChanged?.Invoke(_selectedColor);
        ColorCommitted?.Invoke(_selectedColor);
    }

    #endregion

    #region Draw

    /// <inheritdoc/>
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        base.Draw(spriteBatch);

        // Preview swatch — drawn in the right column next to the sliders
        if (Pixel is not null)
        {
            int swatchX = Bounds.Right - SwatchSize;
            int swatchY = Bounds.Y;
            var swatchRect = new Rectangle(swatchX, swatchY, SwatchSize, SwatchSize * 3 + Spacing * 2);
            spriteBatch.Draw(Pixel, swatchRect, _selectedColor * EffectiveOpacity);
            DrawHelper.DrawBorder(Pixel, spriteBatch, swatchRect, SwatchBorderColor * EffectiveOpacity, 1);
        }
    }

    #endregion
}
