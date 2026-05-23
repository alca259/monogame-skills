using Alca.MonoGame.Kernel.UI.Focus;
using Alca.MonoGame.Kernel.UI.Interaction;

namespace Alca.MonoGame.Kernel.UI.Controls;

/// <summary>A pressable button with hover/press animation and focus support.</summary>
public sealed class Button : UIElement, IUIInteractable, IFocusable
{
    #region Constants

    private const float ScaleNormal = 1.0f;
    private const float ScaleHovered = 1.05f;
    private const float ScalePressed = 0.97f;
    private const float ScaleLerpSpeed = 12f;
    private const int PaddingH = 16;
    private const int PaddingV = 10;

    private const int StateNormal = 0;
    private const int StateHovered = 1;
    private const int StatePressed = 2;

    #endregion

    #region Fields

    private readonly SpriteFont? _font;
    private readonly string _text;
    private Vector2 _textSize;

    /// <summary>Pre-allocated color array indexed by button state (Normal=0, Hovered=1, Pressed=2).</summary>
    private readonly Color[] _stateColors = new Color[3];

    private float _currentScale = ScaleNormal;
    private float _targetScale = ScaleNormal;
    private int _state = StateNormal;

    // IFocusable backing fields
    private bool _isFocused;

    #endregion

    #region Properties

    /// <summary>Color used when the button is in its normal state.</summary>
    public Color NormalColor
    {
        get => _stateColors[StateNormal];
        set => _stateColors[StateNormal] = value;
    }

    /// <summary>Color used when the pointer is over the button.</summary>
    public Color HoveredColor
    {
        get => _stateColors[StateHovered];
        set => _stateColors[StateHovered] = value;
    }

    /// <summary>Color used when the button is being pressed.</summary>
    public Color PressedColor
    {
        get => _stateColors[StatePressed];
        set => _stateColors[StatePressed] = value;
    }

    /// <summary>Color used when the button is disabled.</summary>
    public Color DisabledColor { get; set; } = Color.Gray;

    /// <summary>Optional background texture. When null, the button renders as a colored rectangle.</summary>
    public Texture2D? Texture { get; set; }

    /// <summary>Fired when the button is clicked (pointer up while hovered).</summary>
    public event Action? Clicked;

    #endregion

    #region Constructor

    /// <summary>Creates a new Button.</summary>
    /// <param name="font">Font for the label text. Can be null for an icon-only button.</param>
    /// <param name="text">Label text.</param>
    public Button(SpriteFont? font, string text)
    {
        _font = font;
        _text = text;
        _textSize = font is not null ? font.MeasureString(text) : Vector2.Zero;

        _stateColors[StateNormal] = Color.White;
        _stateColors[StateHovered] = Color.LightYellow;
        _stateColors[StatePressed] = new Color(180, 180, 180);
    }

    #endregion

    #region Layout

    /// <inheritdoc/>
    public override void Measure(Vector2 availableSize)
    {
        DesiredSize = new Vector2(_textSize.X + PaddingH, _textSize.Y + PaddingV);
    }

    #endregion

    #region Update / Draw

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;

        _targetScale = _state switch
        {
            StatePressed => ScalePressed,
            StateHovered => ScaleHovered,
            _ => ScaleNormal,
        };

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _currentScale += (_targetScale - _currentScale) * ScaleLerpSpeed * dt;
    }

    /// <inheritdoc/>
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        Color tint = !IsEnabled ? DisabledColor : _stateColors[_state];
        tint *= EffectiveOpacity;

        Vector2 center = Bounds.Center.ToVector2();

        if (Texture is not null)
        {
            Vector2 origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
            spriteBatch.Draw(Texture, center, null, tint, 0f, origin, _currentScale, SpriteEffects.None, 0f);
        }

        if (_font is not null && _text.Length > 0)
        {
            Vector2 halfText = _textSize / 2f;
            spriteBatch.DrawString(_font, _text,
                center - halfText * _currentScale,
                Color.Black * EffectiveOpacity,
                0f, Vector2.Zero, _currentScale, SpriteEffects.None, 0f);
        }
    }

    #endregion

    #region IUIInteractable

    /// <inheritdoc/>
    public bool IsHovered => _state is StateHovered or StatePressed;

    /// <inheritdoc/>
    public void OnPointerEnter()
    {
        if (_state == StateNormal)
            _state = StateHovered;
    }

    /// <inheritdoc/>
    public void OnPointerLeave()
    {
        _state = StateNormal;
    }

    /// <inheritdoc/>
    public void OnPointerDown(ref UIPointerEventArgs args)
    {
        if (!IsEnabled) return;
        _state = StatePressed;
    }

    /// <inheritdoc/>
    public void OnPointerUp(ref UIPointerEventArgs args)
    {
        if (!IsEnabled) return;

        bool wasPressed = _state == StatePressed;
        _state = StateHovered;

        if (wasPressed)
        {
            Clicked?.Invoke();
            args.Handled = true;
        }
    }

    #endregion

    #region IFocusable

    /// <inheritdoc/>
    public int TabIndex { get; set; }

    /// <inheritdoc/>
    public int? FocusNeighborUp { get; set; }

    /// <inheritdoc/>
    public int? FocusNeighborDown { get; set; }

    /// <inheritdoc/>
    public int? FocusNeighborLeft { get; set; }

    /// <inheritdoc/>
    public int? FocusNeighborRight { get; set; }

    /// <inheritdoc/>
    public bool IsFocused => _isFocused;

    /// <inheritdoc/>
    public void OnFocusGained()
    {
        _isFocused = true;
        if (_state == StateNormal) _state = StateHovered;
    }

    /// <inheritdoc/>
    public void OnFocusLost()
    {
        _isFocused = false;
        _state = StateNormal;
    }

    #endregion
}
