namespace MonoGame.Editor.Core.Project;

/// <summary>Represents a game project open in the editor.</summary>
public sealed class EditorProject
{
    /// <summary>Project name.</summary>
    public string Name { get; }

    /// <summary>Absolute path to the project root folder.</summary>
    public string RootPath { get; }

    /// <summary>Absolute path to the <c>Content/</c> folder.</summary>
    public string ContentPath { get; }

    /// <summary>Absolute path to the <c>Scenes/</c> folder.</summary>
    public string ScenesPath { get; }

    /// <summary>Absolute path to the <c>Content/Prefabs/</c> folder.</summary>
    public string PrefabsPath { get; }

    /// <summary>Absolute path to the <c>Localization/</c> folder.</summary>
    public string LocalizationPath { get; }

    /// <summary>Initializes the project with computed sub-paths derived from <paramref name="rootPath"/>.</summary>
    public EditorProject(string name, string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        Name = name;
        RootPath = rootPath;
        ContentPath = Path.Combine(rootPath, "Content");
        ScenesPath = Path.Combine(rootPath, "Scenes");
        PrefabsPath = Path.Combine(rootPath, "Content", "Prefabs");
        LocalizationPath = Path.Combine(rootPath, "Localization");
    }
}
