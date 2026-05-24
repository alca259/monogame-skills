namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class SetPropertyCommandTests
{
    [Fact]
    public void Execute_AppliesNewValue()
    {
        int value = 0;
        SetPropertyCommand<int> cmd = new("Set X", 0, 42, v => value = v);

        cmd.Execute();

        Assert.Equal(42, value);
    }

    [Fact]
    public void Undo_RestoresPreviousValue()
    {
        int value = 10;
        SetPropertyCommand<int> cmd = new("Set X", 10, 42, v => value = v);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(10, value);
    }

    [Fact]
    public void Description_MatchesProvided()
    {
        SetPropertyCommand<string> cmd = new("Set Name", "a", "b", _ => { });
        Assert.Equal("Set Name", cmd.Description);
    }

    [Fact]
    public void Execute_WithBoolProperty_TogglesCorrectly()
    {
        EditorGameObject obj = new() { Active = true };
        SetPropertyCommand<bool> cmd = new("Set Active", true, false, v => obj.Active = v);

        cmd.Execute();

        Assert.False(obj.Active);
    }

    [Fact]
    public void Undo_WithBoolProperty_RestoresOriginal()
    {
        EditorGameObject obj = new() { Active = true };
        SetPropertyCommand<bool> cmd = new("Set Active", true, false, v => obj.Active = v);
        cmd.Execute();

        cmd.Undo();

        Assert.True(obj.Active);
    }
}
