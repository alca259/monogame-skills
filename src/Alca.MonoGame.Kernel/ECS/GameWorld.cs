namespace Alca.MonoGame.Kernel.ECS;

public class GameWorld
{
    public bool IsEnabled { get; set; } = true;

    private readonly List<GameEntity> _entities = [];
    private readonly List<GameEntity> _pendingAdd = [];
    private readonly List<GameEntity> _pendingDestroy = [];

    public GameEntity CreateEntity()
    {
        var entity = new GameEntity
        {
            World = this
        };
        _pendingAdd.Add(entity);
        return entity;
    }

    public void Destroy(GameEntity entity)
    {
        _pendingDestroy.Add(entity);
    }

    public void Update(GameTime gameTime)
    {
        FlushPendingAdds();

        if (!IsEnabled) return;

        for (int i = 0; i < _entities.Count; i++)
            _entities[i].Update(gameTime);

        FlushPendingDestroys();
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Always draw regardless of IsEnabled — pausing only stops updates, not rendering
        for (int i = 0; i < _entities.Count; i++)
            _entities[i].Draw(gameTime, spriteBatch);
    }

    private void FlushPendingAdds()
    {
        for (int i = 0; i < _pendingAdd.Count; i++)
        {
            var entity = _pendingAdd[i];
            _entities.Add(entity);
            entity.Start();
        }
        _pendingAdd.Clear();
    }

    public T? GetBehaviour<T>() where T : GameBehaviour
    {
        for (int i = 0; i < _entities.Count; i++)
        {
            var b = _entities[i].GetComponent<T>();
            if (b != null) return b;
        }
        return null;
    }

    public List<T> GetBehaviours<T>() where T : GameBehaviour
    {
        var result = new List<T>();
        for (int i = 0; i < _entities.Count; i++)
        {
            var b = _entities[i].GetComponent<T>();
            if (b != null) result.Add(b);
        }
        return result;
    }

    private void FlushPendingDestroys()
    {
        for (int i = 0; i < _pendingDestroy.Count; i++)
        {
            var entity = _pendingDestroy[i];
            entity.Destroy();
            _entities.Remove(entity);
        }
        _pendingDestroy.Clear();
    }
}
