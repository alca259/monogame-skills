using Alca.MonoGame.Kernel.ECS;

namespace Alca.MonoGame.Kernel.UnitTests.ECS;

public sealed class GameEntityTests
{
    private static GameTime AnyGameTime() =>
        new(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(0.016));

    // ── Identity ───────────────────────────────────────────────────────────────

    [Fact]
    public void Name_IsPreserved_FromCreateEntity()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("Hero");

        Assert.Equal("Hero", entity.Name);
    }

    [Fact]
    public void Id_IsUnique_AcrossEntities()
    {
        var world = new GameWorld();
        var a = world.CreateEntity("A");
        var b = world.CreateEntity("B");

        Assert.NotEqual(a.Id, b.Id);
    }

    // ── Transform auto-attach ──────────────────────────────────────────────────

    [Fact]
    public void Transform_IsNotNull_AfterCreateEntity()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E", new Vector2(10, 20));

        Assert.NotNull(entity.Transform);
    }

    [Fact]
    public void Transform_Position_MatchesCreateEntity2dPosition()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E", new Vector2(42, 99));

        Assert.Equal(new Vector3(42, 99, 0), entity.Transform.Position);
    }

    [Fact]
    public void Transform_Position_MatchesCreateEntity3dPosition()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E", new Vector3(1, 2, 3));

        Assert.Equal(new Vector3(1, 2, 3), entity.Transform.Position);
    }

    [Fact]
    public void Transform_Position2d_ReturnsXY()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E", new Vector3(7, 8, 9));

        Assert.Equal(new Vector2(7, 8), entity.Transform.Position2d);
    }

    [Fact]
    public void Transform_Position2d_Setter_PreservesZ()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E", new Vector3(0, 0, 5));

        entity.Transform.Position2d = new Vector2(3, 4);

        Assert.Equal(new Vector3(3, 4, 5), entity.Transform.Position);
    }

    [Fact]
    public void Transform_Rotation2d_MapsToRotationZ()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");

        entity.Transform.Rotation2d = 1.57f;

        Assert.Equal(1.57f, entity.Transform.Rotation.Z);
        Assert.Equal(1.57f, entity.Transform.Rotation2d);
    }

    [Fact]
    public void Transform_IsAccessible_WithoutGetComponent()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");

        entity.Transform.Position = new Vector3(5, 5, 0);

        Assert.Equal(new Vector3(5, 5, 0), entity.Transform.Position);
    }

    // ── GetComponent / HasComponent / TryGetComponent ─────────────────────────

    [Fact]
    public void GetComponent_ReturnsBehaviour_ByConcreteType()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");
        var b = new CounterBehaviour();
        entity.Add(b);

        var result = entity.GetComponent<CounterBehaviour>();

        Assert.Same(b, result);
    }

    [Fact]
    public void GetComponent_ReturnsBehaviour_ByInterface()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");
        var b = new CounterBehaviour();
        entity.Add(b);

        var result = entity.GetComponent<ICounter>();

        Assert.Same(b, result);
    }

    [Fact]
    public void GetComponent_ReturnsNull_WhenNotPresent()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");

        var result = entity.GetComponent<CounterBehaviour>();

        Assert.Null(result);
    }

    [Fact]
    public void HasComponent_ReturnsTrue_WhenPresent()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");
        entity.Add(new CounterBehaviour());

        Assert.True(entity.HasComponent<CounterBehaviour>());
        Assert.True(entity.HasComponent<ICounter>());
    }

    [Fact]
    public void HasComponent_ReturnsFalse_WhenAbsent()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");

        Assert.False(entity.HasComponent<CounterBehaviour>());
    }

    [Fact]
    public void TryGetComponent_ReturnsTrue_AndSetsOut_WhenPresent()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");
        var b = new CounterBehaviour();
        entity.Add(b);

        bool found = entity.TryGetComponent<CounterBehaviour>(out var result);

        Assert.True(found);
        Assert.Same(b, result);
    }

    [Fact]
    public void TryGetComponent_ReturnsFalse_WhenAbsent()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");

        bool found = entity.TryGetComponent<CounterBehaviour>(out var result);

        Assert.False(found);
        Assert.Null(result);
    }

    [Fact]
    public void GetAllComponents_IncludesAllAddedBehaviours()
    {
        var world = new GameWorld();
        var entity = world.CreateEntity("E");
        var b1 = new CounterBehaviour();
        var b2 = new AnotherBehaviour();
        entity.Add(b1);
        entity.Add(b2);

        var all = entity.GetAllComponents().ToList();

        // TransformBehaviour is pre-attached, plus b1 and b2
        Assert.Contains(b1, all);
        Assert.Contains(b2, all);
    }

    // ── Active flag ────────────────────────────────────────────────────────────

    [Fact]
    public void Update_IsSkipped_WhenEntityIsInactive()
    {
        var world = new GameWorld();
        var spy = new CounterBehaviour();
        var entity = world.CreateEntity("E");
        entity.Add(spy);
        entity.Active = false;

        world.Update(AnyGameTime());
        world.Update(AnyGameTime());

        Assert.Equal(0, spy.UpdateCount);
    }

    [Fact]
    public void Update_Resumes_WhenEntityIsReActivated()
    {
        var world = new GameWorld();
        var spy = new CounterBehaviour();
        var entity = world.CreateEntity("E");
        entity.Add(spy);
        entity.Active = false;

        world.Update(AnyGameTime());
        entity.Active = true;
        world.Update(AnyGameTime());

        Assert.Equal(1, spy.UpdateCount);
    }

    // ── Reflection hot-path lists ──────────────────────────────────────────────

    [Fact]
    public void TransformBehaviour_IsNotInUpdateList_ZeroCostPerFrame()
    {
        // TransformBehaviour does not override Update or Draw, so it
        // must NOT appear in the entity's updatable/drawable hot-path lists.
        // We verify indirectly: the TransformBehaviour's Update method's
        // DeclaringType should be GameBehaviour (not overridden).
        var method = typeof(TransformBehaviour).GetMethod(
            "Update",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
            null, [typeof(GameTime)], null);

        Assert.Equal(typeof(GameBehaviour), method?.DeclaringType);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private interface ICounter
    {
        int UpdateCount { get; }
    }

    private sealed class CounterBehaviour : GameBehaviour, ICounter
    {
        public int UpdateCount { get; private set; }
        public override void Update(GameTime gameTime) => UpdateCount++;
    }

    private sealed class AnotherBehaviour : GameBehaviour { }
}
