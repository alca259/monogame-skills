namespace Alca.MonoGame.Kernel.UI.Controls;

/// <summary>
/// A scrollable container that clips its visible area using a ScissorRectangle.
/// The ScissorRectangle is always intersected with the current device scissor to support nesting.
/// </summary>
public sealed class ScrollView : UIContainer
{
    private readonly GraphicsDevice _graphicsDevice;

    /// <summary>Created once in the constructor; never recreated in Draw.</summary>
    private readonly RasterizerState _scissorState;

    private Vector2 _scrollOffset;

    /// <summary>Total content size measured by the last layout pass.</summary>
    public Vector2 ContentSize { get; private set; }

    /// <summary>Current scroll position in pixels (X, Y), clamped to valid range.</summary>
    public Vector2 ScrollOffset => _scrollOffset;

    /// <summary>Creates a ScrollView bound to the given GraphicsDevice for scissor clipping.</summary>
    public ScrollView(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _scissorState = new RasterizerState { ScissorTestEnable = true };
    }

    #region Layout

    /// <inheritdoc/>
    public override void Arrange(Rectangle finalBounds)
    {
        Bounds = finalBounds;

        // Stack children vertically, shifted by scroll offset
        int cursor = finalBounds.Y - (int)_scrollOffset.Y;

        for (int i = 0; i < ChildrenReadOnly.Count; i++)
        {
            Vector2 ds = ChildrenReadOnly[i].DesiredSize;
            int childH = (int)ds.Y;
            ChildrenReadOnly[i].Arrange(new Rectangle(finalBounds.X, cursor, finalBounds.Width, childH));
            cursor += childH;
        }

        float contentHeight = cursor - (finalBounds.Y - (int)_scrollOffset.Y);
        ContentSize = new Vector2(finalBounds.Width, contentHeight);
        ClampScrollOffset();
    }

    #endregion

    #region Update

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;

        int wheel = Mouse.GetState().ScrollWheelValue;

        // We track delta scroll each frame via a cached field
        int delta = _lastWheelValue - wheel;
        _lastWheelValue = wheel;

        if (delta != 0)
            ScrollBy(new Vector2(0f, delta));

        base.Update(gameTime);
    }

    private int _lastWheelValue;

    /// <summary>Scrolls by the given delta in pixels, clamped to content bounds.</summary>
    public void ScrollBy(Vector2 delta)
    {
        _scrollOffset += delta;
        ClampScrollOffset();
        Invalidate();
    }

    private void ClampScrollOffset()
    {
        float maxY = MathF.Max(0f, ContentSize.Y - Bounds.Height);
        float maxX = MathF.Max(0f, ContentSize.X - Bounds.Width);
        _scrollOffset = new Vector2(
            MathHelper.Clamp(_scrollOffset.X, 0f, maxX),
            MathHelper.Clamp(_scrollOffset.Y, 0f, maxY));
    }

    #endregion

    #region Draw

    /// <inheritdoc/>
    public override void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible) return;

        // End current batch, switch to scissor, draw children, restore.
        spriteBatch.End();

        Rectangle savedScissor = _graphicsDevice.ScissorRectangle;
        Rectangle clip = Rectangle.Intersect(Bounds, savedScissor);
        _graphicsDevice.ScissorRectangle = clip;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, null, _scissorState);

        for (int i = 0; i < ChildrenReadOnly.Count; i++)
        {
            if (ChildrenReadOnly[i].IsVisible)
                ChildrenReadOnly[i].Draw(spriteBatch);
        }

        spriteBatch.End();

        _graphicsDevice.ScissorRectangle = savedScissor;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            SamplerState.LinearClamp, null, null);
    }

    #endregion
}
