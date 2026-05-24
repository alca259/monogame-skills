namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class RotateEntityCommandTests
{
    [Fact]
    public void Execute_SetsNewRotation()
    {
        EditorGameObject obj = new() { Name = "Wheel", Rotation = 0f };
        RotateEntityCommand cmd = new(obj, 90f);

        cmd.Execute();

        Assert.Equal(90f, obj.Rotation, 4);
    }

    [Fact]
    public void Undo_RestoresPreviousRotation()
    {
        EditorGameObject obj = new() { Name = "Wheel", Rotation = 45f };
        RotateEntityCommand cmd = new(obj, 90f);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(45f, obj.Rotation, 4);
    }

    [Fact]
    public void Description_ContainsObjectName()
    {
        EditorGameObject obj = new() { Name = "Gear" };
        Assert.Contains("Gear", new RotateEntityCommand(obj, 0f).Description);
    }
}
