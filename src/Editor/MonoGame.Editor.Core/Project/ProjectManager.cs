namespace MonoGame.Editor.Core.Project;

/// <summary>Creates and loads editor projects from disk.</summary>
public static class ProjectManager
{
    /// <summary>Name of the editor subdirectory inside the project root.</summary>
    public const string EditorFolderName = "Editor";

    /// <summary>Name of the project descriptor file written inside the <c>Editor/</c> subfolder.</summary>
    public const string ProjectFileName = "project.json";

    /// <summary>Returns the absolute path to the editor project descriptor file for a given root.</summary>
    public static string GetProjectFilePath(string rootPath) =>
        Path.Combine(rootPath, EditorFolderName, ProjectFileName);

    /// <summary>
    /// Creates a new project named <paramref name="name"/> inside <paramref name="parentPath"/>,
    /// scaffolding the required folder structure and writing <c>Editor/project.json</c>.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="parentPath"/> is empty.</exception>
    /// <exception cref="IOException">Thrown when the target folder already exists.</exception>
    public static EditorProject Create(string name, string parentPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(parentPath);

        string rootPath = Path.Combine(parentPath, name);

        if (Directory.Exists(rootPath))
            throw new IOException($"A folder named '{name}' already exists at '{parentPath}'.");

        EditorProject project = new(name, rootPath);

        Directory.CreateDirectory(project.EditorPath);
        Directory.CreateDirectory(project.ScenesPath);
        Directory.CreateDirectory(project.PrefabsPath);
        Directory.CreateDirectory(project.ContentPath);
        Directory.CreateDirectory(project.LocalizationPath);

        WriteProjectFile(project);

        return project;
    }

    /// <summary>
    /// Loads an existing project from <paramref name="projectPath"/> by reading <c>Editor/project.json</c>.
    /// Returns <c>null</c> if the folder does not contain a valid editor project file.
    /// </summary>
    public static EditorProject? Load(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        string jsonPath = GetProjectFilePath(projectPath);
        if (!File.Exists(jsonPath))
            return null;

        try
        {
            string json = File.ReadAllText(jsonPath);
            ProjectFileData? data = JsonSerializer.Deserialize<ProjectFileData>(json);

            if (data is null || string.IsNullOrWhiteSpace(data.Name))
                return null;

            return new EditorProject(
                data.Name,
                projectPath,
                string.IsNullOrWhiteSpace(data.ContentPath) ? "Content" : data.ContentPath,
                string.IsNullOrWhiteSpace(data.LocalizationPath) ? "Localization" : data.LocalizationPath);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Looks for a <c>.sln</c> or <c>.slnx</c> file in <paramref name="projectPath"/>.
    /// Returns the solution name (filename without extension), or <c>null</c> if none is found.
    /// </summary>
    public static string? FindSolutionName(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        string? sln = Directory.GetFiles(projectPath, "*.sln").FirstOrDefault()
                   ?? Directory.GetFiles(projectPath, "*.slnx").FirstOrDefault();

        return sln is null ? null : Path.GetFileNameWithoutExtension(sln);
    }

    /// <summary>
    /// Initializes an existing MonoGame solution folder as an editor project:
    /// writes <c>Editor/project.json</c> (name inferred from the solution) and creates
    /// any missing editor folders (<c>Editor/Scenes/</c>, <c>Editor/Prefabs/</c>) plus
    /// standard game folders (<c>Content/</c>, <c>Localization/</c>) if absent.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no solution file is found.</exception>
    public static EditorProject Initialize(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);

        string name = FindSolutionName(projectPath)
            ?? throw new InvalidOperationException($"No .sln or .slnx file found in '{projectPath}'.");

        EditorProject project = new(name, projectPath);

        Directory.CreateDirectory(project.EditorPath);
        Directory.CreateDirectory(project.ScenesPath);
        Directory.CreateDirectory(project.PrefabsPath);
        Directory.CreateDirectory(project.ContentPath);
        Directory.CreateDirectory(project.LocalizationPath);

        WriteProjectFile(project);

        return project;
    }

    private static void WriteProjectFile(EditorProject project)
    {
        string jsonPath = GetProjectFilePath(project.RootPath);
        ProjectFileData data = new()
        {
            Name             = project.Name,
            ContentPath      = Path.GetRelativePath(project.RootPath, project.ContentPath),
            LocalizationPath = Path.GetRelativePath(project.RootPath, project.LocalizationPath),
        };
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(jsonPath, json);
    }

    private sealed class ProjectFileData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("contentPath")]
        public string ContentPath { get; set; } = "Content";

        [JsonPropertyName("localizationPath")]
        public string LocalizationPath { get; set; } = "Localization";
    }
}
