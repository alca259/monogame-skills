namespace Alca.MonoGame.Kernel.ECS;

/// <summary>
/// Pure-data spatial component automatically attached to every entity.
/// Uses <see cref="Vector3"/> for all spatial data — the Z component is simply ignored
/// in 2D contexts. Equivalent to Unity's Transform.
/// </summary>
public sealed class TransformBehaviour : GameBehaviour
{
    /// <summary>Gets or sets the world-space position. For 2D use X and Y; Z can carry depth/layer.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Gets or sets the velocity used by movement behaviours.</summary>
    public Vector3 Velocity { get; set; }

    /// <summary>
    /// Gets or sets the rotation as Euler angles in radians (pitch = X, yaw = Y, roll = Z).
    /// For 2D, only Z (roll) is used.
    /// </summary>
    public Vector3 Rotation { get; set; }

    /// <summary>Gets or sets the scale factor. Defaults to (1, 1, 1).</summary>
    public Vector3 Scale { get; set; } = Vector3.One;

    // ── 2D convenience ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the XY plane position. Z is preserved on set.</summary>
    public Vector2 Position2d
    {
        get => new(Position.X, Position.Y);
        set => Position = new Vector3(value, Position.Z);
    }

    /// <summary>Gets or sets the XY plane velocity. Z is preserved on set.</summary>
    public Vector2 Velocity2d
    {
        get => new(Velocity.X, Velocity.Y);
        set => Velocity = new Vector3(value, Velocity.Z);
    }

    /// <summary>Gets or sets the Z-axis rotation (roll) in radians — the standard 2D rotation angle.</summary>
    public float Rotation2d
    {
        get => Rotation.Z;
        set => Rotation = new Vector3(Rotation.X, Rotation.Y, value);
    }

    /// <summary>Gets or sets the XY plane scale. Z is preserved on set.</summary>
    public Vector2 Scale2d
    {
        get => new(Scale.X, Scale.Y);
        set => Scale = new Vector3(value, Scale.Z);
    }

    // ── Constructors ───────────────────────────────────────────────────────────

    /// <summary>Creates a TransformBehaviour at the specified 3D position.</summary>
    public TransformBehaviour(Vector3 position = default) => Position = position;

    /// <summary>Creates a TransformBehaviour at the specified 2D position (Z = 0).</summary>
    public TransformBehaviour(Vector2 position) => Position = new Vector3(position, 0f);
}
