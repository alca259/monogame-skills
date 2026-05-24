namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class RevertPrefabCommandTests
{
    [Fact]
    public void Execute_CopiesPrefabPropertiesToInstance()
    {
        EditorGameObject prefab = new() { Name = "PrefabVersion", Position = new EditorVector2(1f, 2f), Rotation = 45f };
        FakePrefabProvider provider = new(prefab);
        EditorGameObject instance = new() { Name = "Modified" };
        RevertPrefabCommand cmd = new(instance, "prefabs/obj.prefab.json", provider);

        cmd.Execute();

        Assert.Equal("PrefabVersion", instance.Name);
        Assert.Equal(new EditorVector2(1f, 2f), instance.Position);
        Assert.Equal(45f, instance.Rotation, 4);
    }

    [Fact]
    public void Undo_RestoresInstancePropertiesBeforeRevert()
    {
        EditorGameObject prefab = new() { Name = "PrefabVersion" };
        FakePrefabProvider provider = new(prefab);
        EditorGameObject instance = new() { Name = "OriginalName", Rotation = 30f };
        RevertPrefabCommand cmd = new(instance, "prefabs/obj.prefab.json", provider);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal("OriginalName", instance.Name);
        Assert.Equal(30f, instance.Rotation, 4);
    }

    [Fact]
    public void Execute_PrefabNotFound_ThrowsInvalidOperationException()
    {
        FakePrefabProvider provider = new(null);
        RevertPrefabCommand cmd = new(new EditorGameObject(), "missing.prefab.json", provider);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    [Fact]
    public void Description_ContainsPrefabFileName()
    {
        EditorGameObject prefab = new();
        RevertPrefabCommand cmd = new(new EditorGameObject(), "prefabs/hero.prefab.json", new FakePrefabProvider(prefab));
        Assert.Contains("hero.prefab.json", cmd.Description);
    }

    private sealed class FakePrefabProvider : IPrefabProvider
    {
        private readonly EditorGameObject? _prefab;

        public FakePrefabProvider(EditorGameObject? prefab) => _prefab = prefab;

        public EditorGameObject? LoadPrefab(string prefabPath) => _prefab;

        public void SavePrefab(EditorGameObject obj, string prefabPath) { }
    }
}
