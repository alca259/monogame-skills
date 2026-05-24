using MonoGame.Editor.Core.Events;

namespace MonoGame.Editor.Core.UnitTests.Assets;

public sealed class AssetClassifierTests
{
    #region Classify — single extension

    [Theory]
    [InlineData("player.png",  AssetType.Texture)]
    [InlineData("tile.jpg",    AssetType.Texture)]
    [InlineData("bg.JPEG",     AssetType.Texture)]
    [InlineData("icon.bmp",    AssetType.Texture)]
    [InlineData("cursor.tga",  AssetType.Texture)]
    [InlineData("hit.wav",     AssetType.Audio)]
    [InlineData("music.mp3",   AssetType.Audio)]
    [InlineData("sfx.ogg",     AssetType.Audio)]
    [InlineData("theme.wma",   AssetType.Audio)]
    [InlineData("ui.spritefont", AssetType.Font)]
    [InlineData("bitmap.fnt",  AssetType.Font)]
    [InlineData("world.tmx",   AssetType.TiledMap)]
    [InlineData("tiles.tsx",   AssetType.TiledMap)]
    [InlineData("player.cs",   AssetType.Script)]
    [InlineData("data.xnb",    AssetType.Unknown)]
    [InlineData("readme.txt",  AssetType.Unknown)]
    public void Classify_KnownExtension_ReturnsExpectedType(string fileName, AssetType expected)
    {
        AssetType result = AssetClassifier.Classify(fileName);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Classify — compound extensions (longest match first)

    [Theory]
    [InlineData("menu.scene.json",     AssetType.Scene)]
    [InlineData("enemy.prefab.json",   AssetType.Prefab)]
    [InlineData("fire.particles.json", AssetType.Particles)]
    [InlineData("run.anim.json",       AssetType.Animation)]
    [InlineData("actions.input.json",  AssetType.InputMap)]
    public void Classify_CompoundExtension_ReturnsExpectedType(string fileName, AssetType expected)
    {
        AssetType result = AssetClassifier.Classify(fileName);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Classify_PlainJson_ReturnsUnknown()
    {
        AssetType result = AssetClassifier.Classify("config.json");
        Assert.Equal(AssetType.Unknown, result);
    }

    #endregion

    #region Classify — case insensitivity

    [Theory]
    [InlineData("Image.PNG",   AssetType.Texture)]
    [InlineData("sound.WAV",   AssetType.Audio)]
    [InlineData("map.TMX",     AssetType.TiledMap)]
    [InlineData("bg.SCENE.JSON", AssetType.Scene)]
    public void Classify_UpperCaseExtension_ReturnsCorrectType(string fileName, AssetType expected)
    {
        AssetType result = AssetClassifier.Classify(fileName);
        Assert.Equal(expected, result);
    }

    #endregion

    #region CreateInfo

    [Fact]
    public void CreateInfo_ExistingFile_PopulatesAllFields()
    {
        string tempDir  = Path.GetTempPath();
        string filePath = Path.Combine(tempDir, "test_asset.png");
        File.WriteAllBytes(filePath, new byte[512]);

        try
        {
            AssetInfo info = AssetClassifier.CreateInfo(filePath, tempDir);

            Assert.Equal(filePath,        info.AbsolutePath);
            Assert.Equal("test_asset",    info.Name);
            Assert.Equal(".png",          info.Extension);
            Assert.Equal(AssetType.Texture, info.Type);
            Assert.Equal(512L,            info.SizeBytes);
            Assert.False(Path.IsPathRooted(info.RelativePath));
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void CreateInfo_MissingFile_ReturnsSizeZero()
    {
        string tempDir   = Path.GetTempPath();
        string missingPath = Path.Combine(tempDir, "does_not_exist.wav");

        AssetInfo info = AssetClassifier.CreateInfo(missingPath, tempDir);

        Assert.Equal(0L, info.SizeBytes);
        Assert.Equal(AssetType.Audio, info.Type);
    }

    [Fact]
    public void CreateInfo_RelativePath_IsRelativeToRoot()
    {
        string tempDir    = Path.Combine(Path.GetTempPath(), "root");
        string subDir     = Path.Combine(tempDir, "Textures");
        string filePath   = Path.Combine(subDir, "hero.png");
        Directory.CreateDirectory(subDir);
        File.WriteAllBytes(filePath, []);

        try
        {
            AssetInfo info = AssetClassifier.CreateInfo(filePath, tempDir);
            Assert.Equal(Path.Combine("Textures", "hero.png"), info.RelativePath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    #endregion
}
