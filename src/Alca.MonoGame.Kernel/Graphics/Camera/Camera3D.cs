namespace Alca.MonoGame.Kernel.Graphics.Camera;

/// <summary>Abstract base for all 3D camera modes. Provides View, Projection, and frustum culling.</summary>
public abstract class Camera3D
{
    private BoundingFrustum _frustum = new(Matrix.Identity);
    private bool _frustumDirty = true;

    /// <summary>Gets the current camera eye position in world space.</summary>
    public Vector3 Position { get; protected set; }

    /// <summary>Gets the point the camera is looking at in world space.</summary>
    public Vector3 Target { get; protected set; }

    /// <summary>Gets the View matrix for this frame.</summary>
    public abstract Matrix View { get; }

    /// <summary>Gets the Projection matrix.</summary>
    public abstract Matrix Projection { get; }

    /// <summary>Returns the <see cref="BoundingFrustum"/> for this frame, rebuilding it only when dirty.</summary>
    public BoundingFrustum GetFrustum()
    {
        if (_frustumDirty)
        {
            _frustum = new BoundingFrustum(View * Projection);
            _frustumDirty = false;
        }

        return _frustum;
    }

    /// <summary>Marks the frustum dirty so it is rebuilt on the next <see cref="GetFrustum"/> call.</summary>
    protected void InvalidateFrustum() => _frustumDirty = true;

    /// <summary>Builds a View matrix from a position looking at a target. Call from subclass updates.</summary>
    protected static Matrix BuildView(Vector3 position, Vector3 target) =>
        Matrix.CreateLookAt(position, target, Vector3.Up);
}
