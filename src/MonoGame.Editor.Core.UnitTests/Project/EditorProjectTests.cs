namespace MonoGame.Editor.Core.UnitTests.Project;

public sealed class EditorProjectTests
{
    [Fact]
    public void Constructor_ValidArgs_SetsDefaultPaths()
    {
        EditorProject project = new("MyGame", "C:/Projects/MyGame");

        Assert.Equal("MyGame", project.Name);
        Assert.Equal("C:/Projects/MyGame", project.RootPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Editor"), project.EditorPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Editor", "Scenes"), project.ScenesPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Editor", "Prefabs"), project.PrefabsPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Content"), project.ContentPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Localization"), project.LocalizationPath);
    }

    [Fact]
    public void Constructor_CustomContentAndLocalizationPaths_UsesProvided()
    {
        EditorProject project = new("MyGame", "C:/Projects/MyGame", "Assets", "Lang");

        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Assets"), project.ContentPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Lang"), project.LocalizationPath);
    }

    [Theory]
    [InlineData("", "C:/path")]
    [InlineData("   ", "C:/path")]
    [InlineData("Name", "")]
    [InlineData("Name", "   ")]
    public void Constructor_InvalidArgs_ThrowsArgumentException(string name, string rootPath)
    {
        Assert.Throws<ArgumentException>(() => new EditorProject(name, rootPath));
    }
}
