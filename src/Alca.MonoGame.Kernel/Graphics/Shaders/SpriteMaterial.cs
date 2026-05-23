namespace Alca.MonoGame.Kernel.Graphics.Shaders;

/// <summary>A sprite-compatible material that exposes Alpha and TintColor as shader parameters.</summary>
public sealed class SpriteMaterial : Material
{
    private float _alpha = 1f;
    private Color _tintColor = Color.White;

    /// <summary>Gets or sets the global alpha multiplier sent to the shader (0–1).</summary>
    public float Alpha
    {
        get => _alpha;
        set => _alpha = MathHelper.Clamp(value, 0f, 1f);
    }

    /// <summary>Gets or sets the tint color multiplied onto each pixel by the shader.</summary>
    public Color TintColor
    {
        get => _tintColor;
        set => _tintColor = value;
    }

    /// <summary>Initializes the sprite material with the given effect.</summary>
    public SpriteMaterial(Effect effect) : base(effect) { }

    /// <inheritdoc/>
    public override void Apply()
    {
        EffectParameter? alphaParam = Effect.Parameters["Alpha"];
        if (alphaParam is not null)
            alphaParam.SetValue(_alpha);

        EffectParameter? tintParam = Effect.Parameters["TintColor"];
        if (tintParam is not null)
            tintParam.SetValue(_tintColor.ToVector4());
    }
}
