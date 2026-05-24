namespace MonoGame.Editor.Core.UnitTests.Project;

public sealed class ProjectManagerTests : IDisposable
{
    private readonly string _tempDir;

    public ProjectManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "PMTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidArgs_ReturnsProjectWithCorrectPaths()
    {
        EditorProject project = ProjectManager.Create("MyGame", _tempDir);

        Assert.Equal("MyGame", project.Name);
        Assert.Equal(Path.Combine(_tempDir, "MyGame"), project.RootPath);
        Assert.Equal(Path.Combine(_tempDir, "MyGame", "Editor"), project.EditorPath);
        Assert.Equal(Path.Combine(_tempDir, "MyGame", "Editor", "Scenes"), project.ScenesPath);
        Assert.Equal(Path.Combine(_tempDir, "MyGame", "Editor", "Prefabs"), project.PrefabsPath);
    }

    [Fact]
    public void Create_ValidArgs_CreatesExpectedFolders()
    {
        EditorProject project = ProjectManager.Create("MyGame", _tempDir);

        Assert.True(Directory.Exists(project.RootPath));
        Assert.True(Directory.Exists(project.EditorPath));
        Assert.True(Directory.Exists(project.ScenesPath));
        Assert.True(Directory.Exists(project.PrefabsPath));
        Assert.True(Directory.Exists(project.ContentPath));
        Assert.True(Directory.Exists(project.LocalizationPath));
    }

    [Fact]
    public void Create_ValidArgs_WritesProjectJsonInsideEditorFolder()
    {
        EditorProject project = ProjectManager.Create("MyGame", _tempDir);

        string jsonPath = ProjectManager.GetProjectFilePath(project.RootPath);
        Assert.True(File.Exists(jsonPath));
        Assert.StartsWith(project.EditorPath, jsonPath);
    }

    [Fact]
    public void Create_FolderAlreadyExists_ThrowsIOException()
    {
        Directory.CreateDirectory(Path.Combine(_tempDir, "Existing"));

        Assert.Throws<IOException>(() => ProjectManager.Create("Existing", _tempDir));
    }

    [Theory]
    [InlineData("", "C:/path")]
    [InlineData("   ", "C:/path")]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    public void Create_InvalidArgs_ThrowsArgumentException(string name, string parentPath)
    {
        Assert.Throws<ArgumentException>(() => ProjectManager.Create(name, parentPath));
    }

    // ── Load ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Load_ValidProject_ReturnsProjectWithCorrectName()
    {
        EditorProject created = ProjectManager.Create("TestGame", _tempDir);

        EditorProject? loaded = ProjectManager.Load(created.RootPath);

        Assert.NotNull(loaded);
        Assert.Equal("TestGame", loaded.Name);
        Assert.Equal(created.RootPath, loaded.RootPath);
    }

    [Fact]
    public void Load_ValidProject_RestoresCustomContentPath()
    {
        EditorProject created = ProjectManager.Create("TestGame", _tempDir);
        string root = created.RootPath;

        // Overwrite project.json with a custom contentPath
        string json = """{ "name": "TestGame", "version": "1.0", "contentPath": "Assets", "localizationPath": "Lang" }""";
        File.WriteAllText(ProjectManager.GetProjectFilePath(root), json);

        EditorProject? loaded = ProjectManager.Load(root);

        Assert.NotNull(loaded);
        Assert.Equal(Path.Combine(root, "Assets"), loaded.ContentPath);
        Assert.Equal(Path.Combine(root, "Lang"), loaded.LocalizationPath);
    }

    [Fact]
    public void Load_FolderWithoutEditorSubfolder_ReturnsNull()
    {
        string emptyFolder = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(emptyFolder);

        EditorProject? result = ProjectManager.Load(emptyFolder);

        Assert.Null(result);
    }

    [Fact]
    public void Load_FolderWithMalformedJson_ReturnsNull()
    {
        string folder = Path.Combine(_tempDir, "bad");
        string editorFolder = Path.Combine(folder, ProjectManager.EditorFolderName);
        Directory.CreateDirectory(editorFolder);
        File.WriteAllText(Path.Combine(editorFolder, ProjectManager.ProjectFileName), "not-json");

        EditorProject? result = ProjectManager.Load(folder);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Load_InvalidPath_ThrowsArgumentException(string path)
    {
        Assert.Throws<ArgumentException>(() => ProjectManager.Load(path));
    }

    // ── FindSolutionName ─────────────────────────────────────────────────────

    [Fact]
    public void FindSolutionName_SlnPresent_ReturnsName()
    {
        File.WriteAllText(Path.Combine(_tempDir, "MyGame.sln"), string.Empty);

        string? name = ProjectManager.FindSolutionName(_tempDir);

        Assert.Equal("MyGame", name);
    }

    [Fact]
    public void FindSolutionName_SlnxPresent_ReturnsName()
    {
        File.WriteAllText(Path.Combine(_tempDir, "MyGame.slnx"), string.Empty);

        string? name = ProjectManager.FindSolutionName(_tempDir);

        Assert.Equal("MyGame", name);
    }

    [Fact]
    public void FindSolutionName_NoSolutionFile_ReturnsNull()
    {
        string? name = ProjectManager.FindSolutionName(_tempDir);

        Assert.Null(name);
    }

    // ── Initialize ───────────────────────────────────────────────────────────

    [Fact]
    public void Initialize_WithSln_WritesProjectJsonInsideEditorFolder()
    {
        File.WriteAllText(Path.Combine(_tempDir, "MyGame.sln"), string.Empty);

        EditorProject project = ProjectManager.Initialize(_tempDir);

        Assert.Equal("MyGame", project.Name);
        Assert.Equal(_tempDir, project.RootPath);
        string jsonPath = ProjectManager.GetProjectFilePath(_tempDir);
        Assert.True(File.Exists(jsonPath));
        Assert.StartsWith(project.EditorPath, jsonPath);
    }

    [Fact]
    public void Initialize_WithSln_CreatesEditorSubfolders()
    {
        File.WriteAllText(Path.Combine(_tempDir, "MyGame.sln"), string.Empty);

        EditorProject project = ProjectManager.Initialize(_tempDir);

        Assert.True(Directory.Exists(project.EditorPath));
        Assert.True(Directory.Exists(project.ScenesPath));
        Assert.True(Directory.Exists(project.PrefabsPath));
    }

    [Fact]
    public void Initialize_WithoutSolutionFile_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => ProjectManager.Initialize(_tempDir));
    }

    [Fact]
    public void Initialize_ExistingEditorFilesNotOverwritten()
    {
        File.WriteAllText(Path.Combine(_tempDir, "MyGame.sln"), string.Empty);
        string editorScenes = Path.Combine(_tempDir, ProjectManager.EditorFolderName, "Scenes");
        Directory.CreateDirectory(editorScenes);
        File.WriteAllText(Path.Combine(editorScenes, "level1.json"), "{}");

        ProjectManager.Initialize(_tempDir);

        Assert.True(File.Exists(Path.Combine(editorScenes, "level1.json")));
    }

    [Fact]
    public void Initialize_ExistingGameFoldersNotTouched()
    {
        File.WriteAllText(Path.Combine(_tempDir, "MyGame.sln"), string.Empty);
        string contentFolder = Path.Combine(_tempDir, "Content");
        Directory.CreateDirectory(contentFolder);
        File.WriteAllText(Path.Combine(contentFolder, "Content.mgcb"), "existing");

        ProjectManager.Initialize(_tempDir);

        Assert.True(File.Exists(Path.Combine(contentFolder, "Content.mgcb")));
    }
}
