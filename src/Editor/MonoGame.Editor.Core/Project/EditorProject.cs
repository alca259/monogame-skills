namespace MonoGame.Editor.Core.Project;

/// <summary>Represents a game project open in the editor.</summary>
public sealed class EditorProject
{
    /// <summary>Project name.</summary>
    public string Name { get; }

    /// <summary>Absolute path to the project root folder (where the .sln/.slnx lives).</summary>
    public string RootPath { get; }

    /// <summary>Absolute path to the <c>Editor/</c> subfolder — all editor-generated files live here.</summary>
    public string EditorPath { get; }

    /// <summary>Absolute path to the game's <c>Content/</c> folder (configurable, default <c>{RootPath}/Content</c>).</summary>
    public string ContentPath { get; }

    /// <summary>Absolute path to the <c>Editor/Scenes/</c> folder where scene JSON files are stored.</summary>
    public string ScenesPath { get; }

    /// <summary>Absolute path to the <c>Editor/Prefabs/</c> folder where prefab JSON files are stored.</summary>
    public string PrefabsPath { get; }

    /// <summary>Absolute path to the game's <c>Localization/</c> folder (configurable, default <c>{RootPath}/Localization</c>).</summary>
    public string LocalizationPath { get; }

    /// <summary>
    /// Initializes the project with computed sub-paths derived from <paramref name="rootPath"/>.
    /// </summary>
    /// <param name="name">Project name.</param>
    /// <param name="rootPath">Absolute path to the project root.</param>
    /// <param name="contentRelativePath">Relative path to the game content folder (default: <c>Content</c>).</param>
    /// <param name="localizationRelativePath">Relative path to the localization folder (default: <c>Localization</c>).</param>
    public EditorProject(
        string name,
        string rootPath,
        string contentRelativePath = "Content",
        string localizationRelativePath = "Localization")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        Name             = name;
        RootPath         = rootPath;
        EditorPath       = Path.Combine(rootPath, "Editor");
        ScenesPath       = Path.Combine(EditorPath, "Scenes");
        PrefabsPath      = Path.Combine(EditorPath, "Prefabs");
        ContentPath      = Path.Combine(rootPath, contentRelativePath);
        LocalizationPath = Path.Combine(rootPath, localizationRelativePath);
    }
}
