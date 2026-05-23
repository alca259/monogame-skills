namespace Alca.MonoGame.Kernel.ECS;

/// <summary>Base class for all component logic. Override only the lifecycle hooks you need.</summary>
public abstract class GameBehaviour
{
    /// <summary>Gets the entity this behaviour is attached to.</summary>
    public GameEntity Entity { get; internal set; } = null!;

    /// <summary>Gets or sets a value indicating whether this behaviour participates in Update and Draw.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Called immediately when added to an entity. Cache sibling component references here.</summary>
    public virtual void Awake() { }

    /// <summary>Called before the first Update. Use for logic that depends on all entities being initialized.</summary>
    public virtual void Start() { }

    /// <summary>Called every frame. Only invoked if this class overrides it.</summary>
    public virtual void Update(GameTime gameTime) { }

    /// <summary>Called every draw frame. Only invoked if this class overrides it.</summary>
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch) { }

    /// <summary>Called when the entity is destroyed.</summary>
    public virtual void OnDestroy() { }
}
