namespace MonoGame.Editor.Core.UnitTests.Serialization;

public sealed class SceneSerializerTests
{
    [Fact]
    public void Serialize_ProducesNonEmptyJson()
    {
        EditorScene scene = new() { Name = "TestScene", ScenePath = "/test.json" };
        string json = SceneSerializer.Serialize(scene);
        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("TestScene", json);
    }

    [Fact]
    public void Deserialize_ReturnsNullForEmptyString()
    {
        Assert.Null(SceneSerializer.Deserialize(string.Empty));
    }

    [Fact]
    public void Deserialize_ReturnsNullForWhitespace()
    {
        Assert.Null(SceneSerializer.Deserialize("   "));
    }

    [Fact]
    public void RoundTrip_PreservesSceneNameAndPath()
    {
        EditorScene original = new() { Name = "Level01", ScenePath = "/scenes/Level01.json" };

        string json = SceneSerializer.Serialize(original);
        EditorScene? loaded = SceneSerializer.Deserialize(json);

        Assert.NotNull(loaded);
        Assert.Equal("Level01", loaded!.Name);
        Assert.Equal("/scenes/Level01.json", loaded.ScenePath);
    }

    [Fact]
    public void RoundTrip_PreservesRootGameObjectNames()
    {
        EditorScene scene = new() { Name = "S" };
        scene.RootGameObjects.Add(new EditorGameObject { Name = "Root1" });
        scene.RootGameObjects.Add(new EditorGameObject { Name = "Root2" });

        EditorScene? loaded = SceneSerializer.Deserialize(SceneSerializer.Serialize(scene));

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.RootGameObjects.Count);
        Assert.Equal("Root1", loaded.RootGameObjects[0].Name);
        Assert.Equal("Root2", loaded.RootGameObjects[1].Name);
    }

    [Fact]
    public void RoundTrip_PreservesNestedChildren()
    {
        EditorScene scene = new() { Name = "S" };
        EditorGameObject parent = new() { Name = "Parent" };
        EditorGameObject child  = new() { Name = "Child" };
        parent.Children.Add(child);
        scene.RootGameObjects.Add(parent);

        EditorScene? loaded = SceneSerializer.Deserialize(SceneSerializer.Serialize(scene));

        Assert.NotNull(loaded);
        Assert.Single(loaded!.RootGameObjects);
        Assert.Single(loaded.RootGameObjects[0].Children);
        Assert.Equal("Child", loaded.RootGameObjects[0].Children[0].Name);
    }

    [Fact]
    public void Deserialize_RestoresParentLinks()
    {
        EditorScene scene = new() { Name = "S" };
        EditorGameObject parent = new() { Name = "P" };
        EditorGameObject child  = new() { Name = "C" };
        parent.Children.Add(child);
        child.Parent = parent;
        scene.RootGameObjects.Add(parent);

        EditorScene? loaded = SceneSerializer.Deserialize(SceneSerializer.Serialize(scene));

        Assert.NotNull(loaded);
        EditorGameObject loadedParent = loaded!.RootGameObjects[0];
        EditorGameObject loadedChild  = loadedParent.Children[0];

        Assert.Null(loadedParent.Parent);
        Assert.Same(loadedParent, loadedChild.Parent);
    }

    [Fact]
    public void RoundTrip_PreservesTransformValues()
    {
        EditorScene scene = new() { Name = "S" };
        EditorGameObject obj = new()
        {
            Name     = "Obj",
            Active   = false,
            Position = new EditorVector2(10f, 20f),
            Rotation = 45f,
            Scale    = new EditorVector2(2f, 3f),
        };
        scene.RootGameObjects.Add(obj);

        EditorScene? loaded = SceneSerializer.Deserialize(SceneSerializer.Serialize(scene));

        Assert.NotNull(loaded);
        EditorGameObject loadedObj = loaded!.RootGameObjects[0];
        Assert.False(loadedObj.Active);
        Assert.Equal(10f, loadedObj.Position.X);
        Assert.Equal(20f, loadedObj.Position.Y);
        Assert.Equal(45f, loadedObj.Rotation);
        Assert.Equal(2f,  loadedObj.Scale.X);
        Assert.Equal(3f,  loadedObj.Scale.Y);
    }

    [Fact]
    public void RoundTrip_PreservesBehaviourTypeName()
    {
        EditorScene scene = new() { Name = "S" };
        EditorGameObject obj = new() { Name = "Obj" };
        obj.Behaviours.Add(new EditorBehaviour { TypeName = "MyGame.SpeedBehaviour", Enabled = true });
        scene.RootGameObjects.Add(obj);

        EditorScene? loaded = SceneSerializer.Deserialize(SceneSerializer.Serialize(scene));

        Assert.NotNull(loaded);
        Assert.Single(loaded!.RootGameObjects[0].Behaviours);
        Assert.Equal("MyGame.SpeedBehaviour", loaded.RootGameObjects[0].Behaviours[0].TypeName);
        Assert.True(loaded.RootGameObjects[0].Behaviours[0].Enabled);
    }

    [Fact]
    public void Deserialize_RestoresParentLinksForMultiLevelHierarchy()
    {
        EditorScene scene = new() { Name = "S" };
        EditorGameObject root  = new() { Name = "Root" };
        EditorGameObject mid   = new() { Name = "Mid" };
        EditorGameObject leaf  = new() { Name = "Leaf" };
        mid.Children.Add(leaf);
        leaf.Parent = mid;
        root.Children.Add(mid);
        mid.Parent = root;
        scene.RootGameObjects.Add(root);

        EditorScene? loaded = SceneSerializer.Deserialize(SceneSerializer.Serialize(scene));

        Assert.NotNull(loaded);
        EditorGameObject loadedRoot = loaded!.RootGameObjects[0];
        EditorGameObject loadedMid  = loadedRoot.Children[0];
        EditorGameObject loadedLeaf = loadedMid.Children[0];

        Assert.Null(loadedRoot.Parent);
        Assert.Same(loadedRoot, loadedMid.Parent);
        Assert.Same(loadedMid,  loadedLeaf.Parent);
    }
}
