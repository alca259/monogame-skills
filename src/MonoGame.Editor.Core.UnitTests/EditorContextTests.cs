namespace MonoGame.Editor.Core.UnitTests;

public sealed class EditorContextTests
{
    private static EditorContext CreateContext()
    {
        EditorEventBus bus = new();
        return new EditorContext(bus);
    }

    [Fact]
    public void Constructor_DefaultState_IsEditing()
    {
        EditorContext ctx = CreateContext();
        Assert.Equal(EditorState.Editing, ctx.State);
    }

    [Fact]
    public void Constructor_NoSelection_IsNull()
    {
        EditorContext ctx = CreateContext();
        Assert.Null(ctx.SelectedObject);
        Assert.Empty(ctx.MultiSelection);
    }

    [Fact]
    public void SetState_Playing_StateChanges()
    {
        EditorContext ctx = CreateContext();
        ctx.SetState(EditorState.Playing);
        Assert.Equal(EditorState.Playing, ctx.State);
    }

    [Fact]
    public void SetState_PublishesEditorStateChangedEvent()
    {
        EditorEventBus bus = new();
        EditorContext ctx = new(bus);
        EditorStateChangedEvent? received = null;
        bus.Subscribe<EditorStateChangedEvent>(e => received = e);

        ctx.SetState(EditorState.Playing);

        Assert.NotNull(received);
        Assert.Equal(EditorState.Editing, received.OldState);
        Assert.Equal(EditorState.Playing, received.NewState);
    }

    [Fact]
    public void SetState_SameState_StillPublishesEvent()
    {
        EditorEventBus bus = new();
        EditorContext ctx = new(bus);
        int eventCount = 0;
        bus.Subscribe<EditorStateChangedEvent>(_ => eventCount++);

        ctx.SetState(EditorState.Editing);

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void SetSelection_PublishesGameObjectSelectedEvent()
    {
        EditorEventBus bus = new();
        EditorContext ctx = new(bus);
        GameObjectSelectedEvent? received = null;
        bus.Subscribe<GameObjectSelectedEvent>(e => received = e);

        EditorGameObject obj = new() { Name = "Player" };
        ctx.SetSelection(obj);

        Assert.NotNull(received);
        Assert.Same(obj, received.GameObject);
    }

    [Fact]
    public void SetSelection_SetsSelectedObjectAndMultiSelection()
    {
        EditorContext ctx = CreateContext();
        EditorGameObject obj = new() { Name = "Enemy" };
        ctx.SetSelection(obj);

        Assert.Same(obj, ctx.SelectedObject);
        Assert.Single(ctx.MultiSelection);
        Assert.Same(obj, ctx.MultiSelection[0]);
    }

    [Fact]
    public void SetSelection_Null_ClearsSelectionAndPublishesEvent()
    {
        EditorEventBus bus = new();
        EditorContext ctx = new(bus);
        ctx.SetSelection(new EditorGameObject());

        GameObjectSelectedEvent? received = null;
        bus.Subscribe<GameObjectSelectedEvent>(e => received = e);
        ctx.SetSelection(null);

        Assert.Null(ctx.SelectedObject);
        Assert.Empty(ctx.MultiSelection);
        Assert.NotNull(received);
        Assert.Null(received.GameObject);
    }

    [Fact]
    public void SetMultiSelection_SetsFirstAsSelectedObject()
    {
        EditorContext ctx = CreateContext();
        EditorGameObject obj1 = new() { Name = "A" };
        EditorGameObject obj2 = new() { Name = "B" };
        ctx.SetMultiSelection([obj1, obj2]);

        Assert.Same(obj1, ctx.SelectedObject);
        Assert.Equal(2, ctx.MultiSelection.Count);
    }

    [Fact]
    public void SetMultiSelection_Empty_ClearsSelection()
    {
        EditorContext ctx = CreateContext();
        ctx.SetSelection(new EditorGameObject());
        ctx.SetMultiSelection([]);

        Assert.Null(ctx.SelectedObject);
        Assert.Empty(ctx.MultiSelection);
    }

    [Fact]
    public void SetActiveScene_PublishesSceneLoadedEvent()
    {
        EditorEventBus bus = new();
        EditorContext ctx = new(bus);
        SceneLoadedEvent? received = null;
        bus.Subscribe<SceneLoadedEvent>(e => received = e);

        EditorScene scene = new() { Name = "Level1" };
        ctx.SetActiveScene(scene);

        Assert.NotNull(received);
        Assert.Same(scene, received.Scene);
        Assert.Same(scene, ctx.ActiveScene);
    }

    [Fact]
    public void SetActiveProject_SetsProject()
    {
        EditorContext ctx = CreateContext();
        EditorProject project = new("MyGame", "C:/Projects/MyGame");
        ctx.SetActiveProject(project);

        Assert.Same(project, ctx.ActiveProject);
    }
}
