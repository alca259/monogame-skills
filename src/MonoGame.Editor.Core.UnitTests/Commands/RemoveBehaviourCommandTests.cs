namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class RemoveBehaviourCommandTests
{
    [Fact]
    public void Execute_RemovesBehaviourFromObject()
    {
        EditorGameObject obj = new() { Name = "Player" };
        EditorBehaviour behaviour = new() { TypeName = "SpriteBehaviour" };
        obj.Behaviours.Add(behaviour);
        RemoveBehaviourCommand cmd = new(obj, behaviour);

        cmd.Execute();

        Assert.DoesNotContain(behaviour, obj.Behaviours);
    }

    [Fact]
    public void Undo_RestoresBehaviourAtOriginalIndex()
    {
        EditorGameObject obj = new() { Name = "Player" };
        EditorBehaviour first = new() { TypeName = "SpriteBehaviour" };
        EditorBehaviour second = new() { TypeName = "AudioBehaviour" };
        obj.Behaviours.Add(first);
        obj.Behaviours.Add(second);
        RemoveBehaviourCommand cmd = new(obj, first);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, obj.Behaviours.IndexOf(first));
    }

    [Fact]
    public void Description_ContainsTypeNameAndObjectName()
    {
        EditorGameObject obj = new() { Name = "Enemy" };
        EditorBehaviour behaviour = new() { TypeName = "AiBehaviour" };
        obj.Behaviours.Add(behaviour);
        RemoveBehaviourCommand cmd = new(obj, behaviour);

        Assert.Contains("AiBehaviour", cmd.Description);
        Assert.Contains("Enemy", cmd.Description);
    }
}
