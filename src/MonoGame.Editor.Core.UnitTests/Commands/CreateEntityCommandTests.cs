namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class CreateEntityCommandTests
{
    [Fact]
    public void Execute_NoParent_AddsObjectToSceneRoot()
    {
        EditorScene scene = new();
        EditorGameObject obj = new() { Name = "Player" };
        CreateEntityCommand cmd = new(obj, scene);

        cmd.Execute();

        Assert.Contains(obj, scene.RootGameObjects);
    }

    [Fact]
    public void Execute_WithParent_AddsObjectToParentChildren()
    {
        EditorScene scene = new();
        EditorGameObject parent = new() { Name = "Parent" };
        scene.RootGameObjects.Add(parent);
        EditorGameObject child = new() { Name = "Child" };
        CreateEntityCommand cmd = new(child, scene, parent);

        cmd.Execute();

        Assert.Contains(child, parent.Children);
        Assert.Equal(parent, child.Parent);
    }

    [Fact]
    public void Execute_NoParent_SetsParentToNull()
    {
        EditorScene scene = new();
        EditorGameObject obj = new() { Name = "Root" };
        CreateEntityCommand cmd = new(obj, scene);

        cmd.Execute();

        Assert.Null(obj.Parent);
    }

    [Fact]
    public void Undo_NoParent_RemovesObjectFromSceneRoot()
    {
        EditorScene scene = new();
        EditorGameObject obj = new() { Name = "Player" };
        CreateEntityCommand cmd = new(obj, scene);
        cmd.Execute();

        cmd.Undo();

        Assert.DoesNotContain(obj, scene.RootGameObjects);
    }

    [Fact]
    public void Undo_WithParent_RemovesObjectFromParentChildren()
    {
        EditorScene scene = new();
        EditorGameObject parent = new() { Name = "Parent" };
        scene.RootGameObjects.Add(parent);
        EditorGameObject child = new() { Name = "Child" };
        CreateEntityCommand cmd = new(child, scene, parent);
        cmd.Execute();

        cmd.Undo();

        Assert.DoesNotContain(child, parent.Children);
        Assert.Null(child.Parent);
    }

    [Fact]
    public void Description_ContainsObjectName()
    {
        EditorGameObject obj = new() { Name = "Hero" };
        CreateEntityCommand cmd = new(obj, new EditorScene());

        Assert.Contains("Hero", cmd.Description);
    }
}
