namespace MonoGame.Editor.Core.UnitTests.Project;

public sealed class EditorProjectTests
{
    [Fact]
    public void Constructor_ValidArgs_SetsAllPaths()
    {
        EditorProject project = new("MyGame", "C:/Projects/MyGame");

        Assert.Equal("MyGame", project.Name);
        Assert.Equal("C:/Projects/MyGame", project.RootPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Content"), project.ContentPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Scenes"), project.ScenesPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Content", "Prefabs"), project.PrefabsPath);
        Assert.Equal(Path.Combine("C:/Projects/MyGame", "Localization"), project.LocalizationPath);
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
