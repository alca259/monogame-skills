namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class ReparentEntityCommandTests
{
    [Fact]
    public void Execute_MovesObjectToNewParent()
    {
        EditorScene scene = new();
        EditorGameObject oldParent = new() { Name = "OldParent" };
        EditorGameObject newParent = new() { Name = "NewParent" };
        EditorGameObject child = new() { Name = "Child", Parent = oldParent };
        oldParent.Children.Add(child);
        scene.RootGameObjects.Add(oldParent);
        scene.RootGameObjects.Add(newParent);
        ReparentEntityCommand cmd = new(child, scene, newParent);

        cmd.Execute();

        Assert.DoesNotContain(child, oldParent.Children);
        Assert.Contains(child, newParent.Children);
        Assert.Equal(newParent, child.Parent);
    }

    [Fact]
    public void Execute_MoveToRoot_SetsParentNull()
    {
        EditorScene scene = new();
        EditorGameObject parent = new() { Name = "Parent" };
        EditorGameObject child = new() { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        scene.RootGameObjects.Add(parent);
        ReparentEntityCommand cmd = new(child, scene, null);

        cmd.Execute();

        Assert.Contains(child, scene.RootGameObjects);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void Undo_RestoresPreviousParent()
    {
        EditorScene scene = new();
        EditorGameObject oldParent = new() { Name = "OldParent" };
        EditorGameObject newParent = new() { Name = "NewParent" };
        EditorGameObject child = new() { Name = "Child", Parent = oldParent };
        oldParent.Children.Add(child);
        scene.RootGameObjects.Add(oldParent);
        scene.RootGameObjects.Add(newParent);
        ReparentEntityCommand cmd = new(child, scene, newParent);
        cmd.Execute();

        cmd.Undo();

        Assert.Contains(child, oldParent.Children);
        Assert.DoesNotContain(child, newParent.Children);
        Assert.Equal(oldParent, child.Parent);
    }

    [Fact]
    public void Description_ContainsObjectName()
    {
        EditorScene scene = new();
        EditorGameObject obj = new() { Name = "Hero" };
        scene.RootGameObjects.Add(obj);

        Assert.Contains("Hero", new ReparentEntityCommand(obj, scene, null).Description);
    }
}
