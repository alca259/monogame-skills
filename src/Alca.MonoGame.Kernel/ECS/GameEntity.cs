namespace Alca.MonoGame.Kernel.ECS;

public class GameEntity
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsActive { get; set; } = true;
    public GameWorld World { get; internal set; } = null!;

    private readonly List<GameBehaviour> _behaviours = [];
    private bool _started;

    public T AddComponent<T>() where T : GameBehaviour, new()
    {
        var b = new T
        {
            Entity = this
        };
        _behaviours.Add(b);
        b.Awake();
        return b;
    }

    public T? GetComponent<T>() where T : GameBehaviour
    {
        for (int i = 0; i < _behaviours.Count; i++)
            if (_behaviours[i] is T result) return result;
        return null;
    }

    internal void Start()
    {
        if (_started) return;
        _started = true;
        for (int i = 0; i < _behaviours.Count; i++)
            if (_behaviours[i].IsEnabled) _behaviours[i].Start();
    }

    internal void Update(GameTime gameTime)
    {
        if (!IsActive) return;
        for (int i = 0; i < _behaviours.Count; i++)
            if (_behaviours[i].IsEnabled) _behaviours[i].Update(gameTime);
    }

    internal void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        if (!IsActive) return;
        for (int i = 0; i < _behaviours.Count; i++)
            if (_behaviours[i].IsEnabled) _behaviours[i].Draw(gameTime, spriteBatch);
    }

    internal void Destroy()
    {
        for (int i = 0; i < _behaviours.Count; i++)
            _behaviours[i].OnDestroy();
    }
}
