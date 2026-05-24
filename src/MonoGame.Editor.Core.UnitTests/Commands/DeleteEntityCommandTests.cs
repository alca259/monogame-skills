namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class DeleteEntityCommandTests
{
    [Fact]
    public void Execute_RootObject_RemovesFromSceneRoot()
    {
        EditorScene scene = new();
        EditorGameObject obj = new() { Name = "Enemy" };
        scene.RootGameObjects.Add(obj);
        DeleteEntityCommand cmd = new(obj, scene);

        cmd.Execute();

        Assert.DoesNotContain(obj, scene.RootGameObjects);
    }

    [Fact]
    public void Execute_ChildObject_RemovesFromParentChildren()
    {
        EditorScene scene = new();
        EditorGameObject parent = new() { Name = "Root" };
        EditorGameObject child = new() { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        scene.RootGameObjects.Add(parent);
        DeleteEntityCommand cmd = new(child, scene);

        cmd.Execute();

        Assert.DoesNotContain(child, parent.Children);
    }

    [Fact]
    public void Execute_SetsParentToNull()
    {
        EditorScene scene = new();
        EditorGameObject parent = new() { Name = "Root" };
        EditorGameObject child = new() { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        scene.RootGameObjects.Add(parent);
        DeleteEntityCommand cmd = new(child, scene);

        cmd.Execute();

        Assert.Null(child.Parent);
    }

    [Fact]
    public void Undo_RootObject_RestoresAtOriginalIndex()
    {
        EditorScene scene = new();
        EditorGameObject obj1 = new() { Name = "A" };
        EditorGameObject obj2 = new() { Name = "B" };
        scene.RootGameObjects.Add(obj1);
        scene.RootGameObjects.Add(obj2);
        DeleteEntityCommand cmd = new(obj1, scene);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, scene.RootGameObjects.IndexOf(obj1));
    }

    [Fact]
    public void Undo_ChildObject_RestoresParentRelationship()
    {
        EditorScene scene = new();
        EditorGameObject parent = new() { Name = "Root" };
        EditorGameObject child = new() { Name = "Child", Parent = parent };
        parent.Children.Add(child);
        scene.RootGameObjects.Add(parent);
        DeleteEntityCommand cmd = new(child, scene);
        cmd.Execute();

        cmd.Undo();

        Assert.Contains(child, parent.Children);
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void Description_ContainsObjectName()
    {
        EditorScene scene = new();
        EditorGameObject obj = new() { Name = "Goblin" };
        scene.RootGameObjects.Add(obj);

        Assert.Contains("Goblin", new DeleteEntityCommand(obj, scene).Description);
    }
}
