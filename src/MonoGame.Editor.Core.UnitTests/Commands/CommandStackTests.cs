namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class CommandStackTests
{
    [Fact]
    public void Execute_SingleCommand_CommandIsExecuted()
    {
        bool executed = false;
        CommandStack stack = new();
        FakeCommand cmd = new("Test", execute: () => executed = true);

        stack.Execute(cmd);

        Assert.True(executed);
    }

    [Fact]
    public void Execute_AfterExecution_UndoDescriptionMatchesCommand()
    {
        CommandStack stack = new();
        stack.Execute(new FakeCommand("My Op"));

        Assert.Equal("My Op", stack.UndoDescription);
    }

    [Fact]
    public void Execute_AfterExecution_RedoDescriptionIsNull()
    {
        CommandStack stack = new();
        stack.Execute(new FakeCommand("My Op"));

        Assert.Null(stack.RedoDescription);
    }

    [Fact]
    public void Undo_AfterExecute_CallsUndo()
    {
        bool undone = false;
        CommandStack stack = new();
        stack.Execute(new FakeCommand("Op", undo: () => undone = true));

        stack.Undo();

        Assert.True(undone);
    }

    [Fact]
    public void Undo_AfterExecute_MovesDescriptionToRedo()
    {
        CommandStack stack = new();
        stack.Execute(new FakeCommand("Op"));

        stack.Undo();

        Assert.Null(stack.UndoDescription);
        Assert.Equal("Op", stack.RedoDescription);
    }

    [Fact]
    public void Redo_AfterUndo_ReExecutesCommand()
    {
        int executeCount = 0;
        CommandStack stack = new();
        stack.Execute(new FakeCommand("Op", execute: () => executeCount++));

        stack.Undo();
        stack.Redo();

        Assert.Equal(2, executeCount);
    }

    [Fact]
    public void Redo_AfterUndo_MovesDescriptionBackToUndo()
    {
        CommandStack stack = new();
        stack.Execute(new FakeCommand("Op"));
        stack.Undo();

        stack.Redo();

        Assert.Equal("Op", stack.UndoDescription);
        Assert.Null(stack.RedoDescription);
    }

    [Fact]
    public void Execute_NewCommand_ClearsRedoStack()
    {
        CommandStack stack = new();
        stack.Execute(new FakeCommand("Op1"));
        stack.Undo();
        stack.Execute(new FakeCommand("Op2"));

        Assert.Null(stack.RedoDescription);
    }

    [Fact]
    public void Execute_ExceedsMaxHistory_OldestEntryIsDropped()
    {
        CommandStack stack = new(maxHistory: 3);
        stack.Execute(new FakeCommand("A"));
        stack.Execute(new FakeCommand("B"));
        stack.Execute(new FakeCommand("C"));
        stack.Execute(new FakeCommand("D"));

        // Undo three times — should land on B, C, D descriptions (A was dropped)
        stack.Undo(); // D
        stack.Undo(); // C
        stack.Undo(); // B
        Assert.Null(stack.UndoDescription); // A was evicted
    }

    [Fact]
    public void Undo_EmptyStack_DoesNotThrow()
    {
        CommandStack stack = new();
        Exception? ex = Record.Exception(() => stack.Undo());
        Assert.Null(ex);
    }

    [Fact]
    public void Redo_EmptyStack_DoesNotThrow()
    {
        CommandStack stack = new();
        Exception? ex = Record.Exception(() => stack.Redo());
        Assert.Null(ex);
    }

    [Fact]
    public void Clear_AfterExecute_BothDescriptionsAreNull()
    {
        CommandStack stack = new();
        stack.Execute(new FakeCommand("Op"));
        stack.Undo();

        stack.Clear();

        Assert.Null(stack.UndoDescription);
        Assert.Null(stack.RedoDescription);
    }

    [Fact]
    public void Undo_PublishesUndoPerformedEvent()
    {
        List<UndoPerformedEvent> events = [];
        EditorEventBus bus = new();
        bus.Subscribe<UndoPerformedEvent>(e => events.Add(e));
        CommandStack stack = new(eventBus: bus);
        stack.Execute(new FakeCommand("Op"));

        stack.Undo();

        Assert.Single(events);
        Assert.Equal("Op", events[0].Description);
    }

    [Fact]
    public void Redo_PublishesRedoPerformedEvent()
    {
        List<RedoPerformedEvent> events = [];
        EditorEventBus bus = new();
        bus.Subscribe<RedoPerformedEvent>(e => events.Add(e));
        CommandStack stack = new(eventBus: bus);
        stack.Execute(new FakeCommand("Op"));
        stack.Undo();

        stack.Redo();

        Assert.Single(events);
        Assert.Equal("Op", events[0].Description);
    }

    // ---------------------------------------------------------------------------
    private sealed class FakeCommand : IEditorCommand
    {
        private readonly Action? _execute;
        private readonly Action? _undo;

        public FakeCommand(string description, Action? execute = null, Action? undo = null)
        {
            Description = description;
            _execute = execute;
            _undo = undo;
        }

        public string Description { get; }
        public void Execute() => _execute?.Invoke();
        public void Undo() => _undo?.Invoke();
    }
}
