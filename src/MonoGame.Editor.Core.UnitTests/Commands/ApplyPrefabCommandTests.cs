namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class ApplyPrefabCommandTests
{
    [Fact]
    public void Execute_SavesPrefabWithInstanceData()
    {
        FakePrefabProvider provider = new();
        EditorGameObject instance = new() { Name = "Hero", Position = new EditorVector2(5f, 10f) };
        ApplyPrefabCommand cmd = new(instance, "prefabs/hero.prefab.json", provider);

        cmd.Execute();

        Assert.Equal(instance, provider.LastSaved);
        Assert.Equal("prefabs/hero.prefab.json", provider.LastSavedPath);
    }

    [Fact]
    public void Undo_RestoresPreviousPrefabContent()
    {
        EditorGameObject originalPrefab = new() { Name = "Original" };
        FakePrefabProvider provider = new(originalPrefab);
        EditorGameObject instance = new() { Name = "Modified" };
        ApplyPrefabCommand cmd = new(instance, "prefabs/hero.prefab.json", provider);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(originalPrefab, provider.LastSaved);
    }

    [Fact]
    public void Description_ContainsPrefabFileName()
    {
        ApplyPrefabCommand cmd = new(new EditorGameObject(), "prefabs/enemy.prefab.json", new FakePrefabProvider());
        Assert.Contains("enemy.prefab.json", cmd.Description);
    }

    private sealed class FakePrefabProvider : IPrefabProvider
    {
        private readonly EditorGameObject? _existingPrefab;
        public EditorGameObject? LastSaved { get; private set; }
        public string? LastSavedPath { get; private set; }

        public FakePrefabProvider(EditorGameObject? existing = null) => _existingPrefab = existing;

        public EditorGameObject? LoadPrefab(string prefabPath) => _existingPrefab;

        public void SavePrefab(EditorGameObject obj, string prefabPath)
        {
            LastSaved = obj;
            LastSavedPath = prefabPath;
        }
    }
}
