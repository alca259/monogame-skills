namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class AddBehaviourCommandTests
{
    [Fact]
    public void Execute_AddsBehaviourToObject()
    {
        EditorGameObject obj = new() { Name = "Player" };
        EditorBehaviour behaviour = new() { TypeName = "SpriteBehaviour" };
        AddBehaviourCommand cmd = new(obj, behaviour);

        cmd.Execute();

        Assert.Contains(behaviour, obj.Behaviours);
    }

    [Fact]
    public void Undo_RemovesBehaviourFromObject()
    {
        EditorGameObject obj = new() { Name = "Player" };
        EditorBehaviour behaviour = new() { TypeName = "SpriteBehaviour" };
        AddBehaviourCommand cmd = new(obj, behaviour);
        cmd.Execute();

        cmd.Undo();

        Assert.DoesNotContain(behaviour, obj.Behaviours);
    }

    [Fact]
    public void Description_ContainsTypeNameAndObjectName()
    {
        EditorGameObject obj = new() { Name = "Hero" };
        EditorBehaviour behaviour = new() { TypeName = "AudioBehaviour" };
        AddBehaviourCommand cmd = new(obj, behaviour);

        Assert.Contains("AudioBehaviour", cmd.Description);
        Assert.Contains("Hero", cmd.Description);
    }
}
