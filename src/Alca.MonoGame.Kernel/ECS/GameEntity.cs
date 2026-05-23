using System.Reflection;

namespace Alca.MonoGame.Kernel.ECS;

/// <summary>Named container of GameBehaviours. Equivalent to Unity's GameObject.</summary>
public sealed class GameEntity
{
    private static readonly Type[] _updateParamTypes = [typeof(GameTime)];
    private static readonly Type[] _drawParamTypes = [typeof(GameTime), typeof(SpriteBatch)];

    private readonly Dictionary<Type, GameBehaviour> _behaviours = [];
    private readonly List<GameBehaviour> _updatables = [];
    private readonly List<GameBehaviour> _drawables = [];
    private bool _started;

    /// <summary>Gets the unique identifier for this entity.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Gets the display name of this entity.</summary>
    public string Name { get; }

    /// <summary>Gets or sets a value indicating whether this entity participates in Update and Draw.</summary>
    public bool Active { get; set; } = true;

    /// <summary>Gets the world that owns this entity.</summary>
    public GameWorld World { get; internal set; } = null!;

    /// <summary>
    /// Gets the always-present spatial component. Equivalent to Unity's <c>gameObject.transform</c>.
    /// Set automatically when a <see cref="TransformBehaviour"/> is added.
    /// </summary>
    public TransformBehaviour Transform { get; private set; } = null!;

    internal GameEntity(string name) => Name = name;

    // ── Fluent builder ─────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a behaviour to this entity, calls <see cref="GameBehaviour.Awake"/> immediately,
    /// and returns this entity for fluent chaining.
    /// </summary>
    public GameEntity Add<T>(T behaviour) where T : GameBehaviour
    {
        _behaviours[typeof(T)] = behaviour;
        behaviour.Entity = this;

        if (behaviour is TransformBehaviour t)
            Transform = t;

        // One-time reflection check — never repeated per frame.
        var type = behaviour.GetType();
        if (OverridesMethod(type, nameof(GameBehaviour.Update), _updateParamTypes))
            _updatables.Add(behaviour);
        if (OverridesMethod(type, nameof(GameBehaviour.Draw), _drawParamTypes))
            _drawables.Add(behaviour);

        behaviour.Awake();
        return this;
    }

    // ── Component access ───────────────────────────────────────────────────────

    /// <summary>Returns the component by concrete type or interface. Returns null if not found.</summary>
    public T? GetComponent<T>() where T : class
    {
        // Fast path: exact concrete type match
        if (_behaviours.TryGetValue(typeof(T), out var exact))
            return (T)(object)exact;

        // Slow path: search by interface or base type
        foreach (var b in _behaviours.Values)
            if (b is T match) return match;

        return null;
    }

    /// <summary>Attempts to get a component. Returns true and sets <paramref name="component"/> if found.</summary>
    public bool TryGetComponent<T>(out T? component) where T : class
    {
        component = GetComponent<T>();
        return component is not null;
    }

    /// <summary>Returns true if this entity has a component of type T (concrete or interface).</summary>
    public bool HasComponent<T>() where T : class => GetComponent<T>() is not null;

    /// <summary>Returns all behaviours attached to this entity.</summary>
    public IEnumerable<GameBehaviour> GetAllComponents() => _behaviours.Values;

    // ── Internal loop ──────────────────────────────────────────────────────────

    internal void Update(GameTime gameTime)
    {
        if (!Active) return;

        if (!_started)
        {
            StartAll();
            _started = true;
        }

        for (int i = 0; i < _updatables.Count; i++)
            if (_updatables[i].Enabled) _updatables[i].Update(gameTime);
    }

    internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (!Active) return;

        for (int i = 0; i < _drawables.Count; i++)
            if (_drawables[i].Enabled) _drawables[i].Draw(gameTime, spriteBatch);
    }

    internal void Destroy()
    {
        foreach (var b in _behaviours.Values)
            b.OnDestroy();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private void StartAll()
    {
        foreach (var b in _behaviours.Values)
            b.Start();
    }

    private static bool OverridesMethod(Type type, string methodName, Type[] paramTypes)
    {
        var method = type.GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public,
            null, paramTypes, null);
        return method?.DeclaringType != typeof(GameBehaviour);
    }
}
