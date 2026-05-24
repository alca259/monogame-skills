namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class MoveEntityCommandTests
{
    [Fact]
    public void Execute_SetsNewPosition()
    {
        EditorGameObject obj = new() { Name = "Player", Position = new EditorVector2(0f, 0f) };
        MoveEntityCommand cmd = new(obj, new EditorVector2(10f, 20f));

        cmd.Execute();

        Assert.Equal(new EditorVector2(10f, 20f), obj.Position);
    }

    [Fact]
    public void Undo_RestoresPreviousPosition()
    {
        EditorGameObject obj = new() { Name = "Player", Position = new EditorVector2(5f, 5f) };
        MoveEntityCommand cmd = new(obj, new EditorVector2(10f, 20f));
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(new EditorVector2(5f, 5f), obj.Position);
    }

    [Fact]
    public void Description_ContainsObjectName()
    {
        EditorGameObject obj = new() { Name = "Bullet" };
        Assert.Contains("Bullet", new MoveEntityCommand(obj, EditorVector2.Zero).Description);
    }
}
