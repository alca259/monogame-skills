namespace MonoGame.Editor.Core.UnitTests.Tilemaps;

public sealed class EditorTilemapAssetTests
{
    private static EditorTilemapAsset BuildAsset(IEnumerable<EditorTileset>? tilesets = null, IEnumerable<EditorTileLayer>? layers = null)
    {
        return new EditorTilemapAsset(
            filePath: "map.tmx",
            mapWidth: 10,
            mapHeight: 10,
            tileWidth: 32,
            tileHeight: 32,
            tilesets: tilesets ?? [],
            layers: layers ?? []);
    }

    [Fact]
    public void Constructor_SetsPropertiesCorrectly()
    {
        var asset = BuildAsset();
        Assert.Equal("map.tmx", asset.FilePath);
        Assert.Equal(10, asset.MapWidth);
        Assert.Equal(10, asset.MapHeight);
        Assert.Equal(32, asset.TileWidth);
        Assert.Equal(32, asset.TileHeight);
    }

    [Fact]
    public void Tilesets_AreReadOnly()
    {
        var ts = new EditorTileset { FirstGid = 1, Name = "Tileset", TileWidth = 32, TileHeight = 32, Columns = 8, TileCount = 64 };
        var asset = BuildAsset(tilesets: [ts]);
        Assert.Single(asset.Tilesets);
        Assert.IsAssignableFrom<IReadOnlyList<EditorTileset>>(asset.Tilesets);
    }

    [Fact]
    public void Layers_AreReadOnly()
    {
        var layer = new EditorTileLayer("Ground", 10, 10);
        var asset = BuildAsset(layers: [layer]);
        Assert.Single(asset.Layers);
        Assert.IsAssignableFrom<IReadOnlyList<EditorTileLayer>>(asset.Layers);
    }

    [Fact]
    public void GetTilesetForGid_ReturnsCorrectTileset()
    {
        var ts1 = new EditorTileset { FirstGid = 1, Name = "A", Columns = 8, TileCount = 64, TileWidth = 32, TileHeight = 32 };
        var ts2 = new EditorTileset { FirstGid = 65, Name = "B", Columns = 8, TileCount = 32, TileWidth = 32, TileHeight = 32 };
        var asset = BuildAsset(tilesets: [ts1, ts2]);

        Assert.Equal(ts1, asset.GetTilesetForGid(1));
        Assert.Equal(ts1, asset.GetTilesetForGid(64));
        Assert.Equal(ts2, asset.GetTilesetForGid(65));
        Assert.Equal(ts2, asset.GetTilesetForGid(96));
    }

    [Fact]
    public void GetTilesetForGid_BelowFirstGid_ReturnsNull()
    {
        var ts = new EditorTileset { FirstGid = 5, Name = "A", Columns = 8, TileCount = 64, TileWidth = 32, TileHeight = 32 };
        var asset = BuildAsset(tilesets: [ts]);
        Assert.Null(asset.GetTilesetForGid(4));
        Assert.Null(asset.GetTilesetForGid(0));
    }

    [Fact]
    public void GetTilesetForGid_NoTilesets_ReturnsNull()
    {
        var asset = BuildAsset();
        Assert.Null(asset.GetTilesetForGid(1));
    }

    [Fact]
    public void GetLocalId_SubtractsFirstGid()
    {
        var ts = new EditorTileset { FirstGid = 10, Name = "A", Columns = 8, TileCount = 64, TileWidth = 32, TileHeight = 32 };
        Assert.Equal(0, EditorTilemapAsset.GetLocalId(10, ts));
        Assert.Equal(5, EditorTilemapAsset.GetLocalId(15, ts));
    }

    [Fact]
    public void GetTilesetForGid_MultipleMatches_ReturnsHighestFirstGid()
    {
        var ts1 = new EditorTileset { FirstGid = 1, Name = "A", Columns = 8, TileCount = 64, TileWidth = 32, TileHeight = 32 };
        var ts2 = new EditorTileset { FirstGid = 50, Name = "B", Columns = 4, TileCount = 16, TileWidth = 32, TileHeight = 32 };
        var asset = BuildAsset(tilesets: [ts1, ts2]);

        Assert.Equal(ts2, asset.GetTilesetForGid(50));
    }

    [Fact]
    public void EditorTileset_GetTileSourceRect_ReturnsCorrectBounds()
    {
        var ts = new EditorTileset { FirstGid = 1, Name = "A", TileWidth = 16, TileHeight = 16, Columns = 4, TileCount = 16 };
        var rect = ts.GetTileSourceRect(5); // col=1, row=1
        Assert.Equal(16, rect.X);
        Assert.Equal(16, rect.Y);
        Assert.Equal(16, rect.Width);
        Assert.Equal(16, rect.Height);
    }

    [Fact]
    public void EditorTileset_GetTileSourceRect_ZeroColumns_ReturnsEmpty()
    {
        var ts = new EditorTileset { FirstGid = 1, Name = "A", TileWidth = 16, TileHeight = 16, Columns = 0, TileCount = 0 };
        var rect = ts.GetTileSourceRect(0);
        Assert.Equal(System.Drawing.Rectangle.Empty, rect);
    }
}
