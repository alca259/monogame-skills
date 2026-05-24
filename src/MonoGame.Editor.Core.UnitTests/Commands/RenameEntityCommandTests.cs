namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class RenameEntityCommandTests
{
    [Fact]
    public void Execute_SetsNewName()
    {
        EditorGameObject obj = new() { Name = "OldName" };
        RenameEntityCommand cmd = new(obj, "NewName");

        cmd.Execute();

        Assert.Equal("NewName", obj.Name);
    }

    [Fact]
    public void Undo_RestoresPreviousName()
    {
        EditorGameObject obj = new() { Name = "OldName" };
        RenameEntityCommand cmd = new(obj, "NewName");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal("OldName", obj.Name);
    }

    [Fact]
    public void Description_ContainsBothNames()
    {
        EditorGameObject obj = new() { Name = "Before" };
        RenameEntityCommand cmd = new(obj, "After");

        Assert.Contains("Before", cmd.Description);
        Assert.Contains("After", cmd.Description);
    }
}
