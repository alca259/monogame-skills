namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class ScaleEntityCommandTests
{
    [Fact]
    public void Execute_SetsNewScale()
    {
        EditorGameObject obj = new() { Name = "Rock", Scale = EditorVector2.One };
        ScaleEntityCommand cmd = new(obj, new EditorVector2(2f, 3f));

        cmd.Execute();

        Assert.Equal(new EditorVector2(2f, 3f), obj.Scale);
    }

    [Fact]
    public void Undo_RestoresPreviousScale()
    {
        EditorGameObject obj = new() { Name = "Rock", Scale = new EditorVector2(1.5f, 1.5f) };
        ScaleEntityCommand cmd = new(obj, new EditorVector2(2f, 3f));
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(new EditorVector2(1.5f, 1.5f), obj.Scale);
    }

    [Fact]
    public void Description_ContainsObjectName()
    {
        EditorGameObject obj = new() { Name = "Tree" };
        Assert.Contains("Tree", new ScaleEntityCommand(obj, EditorVector2.One).Description);
    }
}
