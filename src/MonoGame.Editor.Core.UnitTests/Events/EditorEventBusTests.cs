namespace MonoGame.Editor.Core.UnitTests.Events;

public sealed class EditorEventBusTests
{
    [Fact]
    public void Subscribe_Publish_HandlerReceivesEvent()
    {
        EditorEventBus bus = new();
        GameObjectSelectedEvent? received = null;
        bus.Subscribe<GameObjectSelectedEvent>(e => received = e);

        GameObjectSelectedEvent evt = new(null);
        bus.Publish(evt);

        Assert.Same(evt, received);
    }

    [Fact]
    public void Unsubscribe_AfterUnsubscribe_HandlerNotCalled()
    {
        EditorEventBus bus = new();
        int callCount = 0;
        Action<AssetImportedEvent> handler = _ => callCount++;

        bus.Subscribe(handler);
        bus.Publish(new AssetImportedEvent(new AssetInfo("/c/test.png", "test.png", "test", AssetType.Texture, ".png", 0)));
        bus.Unsubscribe(handler);
        bus.Publish(new AssetImportedEvent(new AssetInfo("/c/test2.png", "test2.png", "test2", AssetType.Texture, ".png", 0)));

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Publish_NoSubscribers_DoesNotThrow()
    {
        EditorEventBus bus = new();
        bus.Publish(new EditorStateChangedEvent(EditorState.Editing, EditorState.Playing));
    }

    [Fact]
    public void Subscribe_MultipleHandlers_AllReceiveEvent()
    {
        EditorEventBus bus = new();
        int count = 0;
        bus.Subscribe<UndoPerformedEvent>(_ => count++);
        bus.Subscribe<UndoPerformedEvent>(_ => count++);

        bus.Publish(new UndoPerformedEvent("Move"));

        Assert.Equal(2, count);
    }

    [Fact]
    public void Subscribe_DifferentEventTypes_DoNotInterfere()
    {
        EditorEventBus bus = new();
        bool undoReceived = false;
        bool redoReceived = false;

        bus.Subscribe<UndoPerformedEvent>(_ => undoReceived = true);
        bus.Subscribe<RedoPerformedEvent>(_ => redoReceived = true);

        bus.Publish(new UndoPerformedEvent("Move"));

        Assert.True(undoReceived);
        Assert.False(redoReceived);
    }

    [Fact]
    public void Unsubscribe_HandlerNotRegistered_DoesNotThrow()
    {
        EditorEventBus bus = new();
        Action<UndoPerformedEvent> handler = _ => { };
        bus.Unsubscribe(handler);
    }
}
