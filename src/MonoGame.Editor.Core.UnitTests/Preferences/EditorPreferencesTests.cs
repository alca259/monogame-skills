namespace MonoGame.Editor.Core.UnitTests.Preferences;

public sealed class EditorPreferencesTests
{
    [Fact]
    public void Constructor_DefaultValues_AreReasonable()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "prefs.json");
        EditorPreferences prefs = new(tempPath);

        Assert.True(prefs.LeftPanelWidth > 0);
        Assert.True(prefs.RightPanelWidth > 0);
        Assert.True(prefs.ConsolePanelHeight > 0);
        Assert.True(prefs.HierarchyVisible);
        Assert.True(prefs.InspectorVisible);
        Assert.True(prefs.AssetBrowserVisible);
        Assert.True(prefs.ConsoleVisible);
        Assert.Equal(string.Empty, prefs.LastProjectPath);
    }

    [Fact]
    public void Save_Load_RoundTripsAllProperties()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "prefs.json");

        EditorPreferences original = new(tempPath)
        {
            LeftPanelWidth = 300,
            RightPanelWidth = 250,
            ConsolePanelHeight = 200,
            HierarchyVisible = false,
            InspectorVisible = true,
            AssetBrowserVisible = false,
            ConsoleVisible = true,
            LastProjectPath = "C:/Games/MyGame",
        };
        original.Save();

        EditorPreferences loaded = new(tempPath);
        loaded.Load();

        Assert.Equal(300, loaded.LeftPanelWidth);
        Assert.Equal(250, loaded.RightPanelWidth);
        Assert.Equal(200, loaded.ConsolePanelHeight);
        Assert.False(loaded.HierarchyVisible);
        Assert.True(loaded.InspectorVisible);
        Assert.False(loaded.AssetBrowserVisible);
        Assert.True(loaded.ConsoleVisible);
        Assert.Equal("C:/Games/MyGame", loaded.LastProjectPath);

        File.Delete(tempPath);
    }

    [Fact]
    public void Load_FileDoesNotExist_KeepsDefaults()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "prefs.json");
        EditorPreferences prefs = new(tempPath);
        prefs.Load();

        Assert.Equal(220, prefs.LeftPanelWidth);
    }

    [Fact]
    public void Load_CorruptJson_KeepsDefaults()
    {
        string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "prefs.json");
        Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);
        File.WriteAllText(tempPath, "{ invalid json }}}");

        EditorPreferences prefs = new(tempPath);
        prefs.Load();

        Assert.Equal(220, prefs.LeftPanelWidth);

        File.Delete(tempPath);
    }
}
