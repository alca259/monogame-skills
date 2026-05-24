namespace MonoGame.Editor.Core.UnitTests.Tilemaps;

public sealed class TilemapImporterTests
{
    private const string SimpleTmx =
        """
        <?xml version="1.0" encoding="UTF-8"?>
        <map version="1.10" width="4" height="3" tilewidth="32" tileheight="32">
          <tileset firstgid="1" name="Tiles" tilewidth="32" tileheight="32" columns="8" tilecount="64">
            <image source="tiles.png"/>
          </tileset>
          <layer name="Ground" width="4" height="3">
            <data encoding="csv">
        1,2,3,4,
        5,6,7,8,
        0,1,0,1
            </data>
          </layer>
        </map>
        """;

    [Fact]
    public void ParseFromString_ValidTmx_ParsesDimensions()
    {
        var asset = TilemapImporter.ParseFromString(SimpleTmx);
        Assert.Equal(4, asset.MapWidth);
        Assert.Equal(3, asset.MapHeight);
        Assert.Equal(32, asset.TileWidth);
        Assert.Equal(32, asset.TileHeight);
    }

    [Fact]
    public void ParseFromString_ValidTmx_ParsesTileset()
    {
        var asset = TilemapImporter.ParseFromString(SimpleTmx);
        Assert.Single(asset.Tilesets);
        var ts = asset.Tilesets[0];
        Assert.Equal(1, ts.FirstGid);
        Assert.Equal("Tiles", ts.Name);
        Assert.Equal(8, ts.Columns);
        Assert.Equal(64, ts.TileCount);
        Assert.Equal("tiles.png", ts.ImagePath);
    }

    [Fact]
    public void ParseFromString_ValidTmx_ParsesSingleLayer()
    {
        var asset = TilemapImporter.ParseFromString(SimpleTmx);
        Assert.Single(asset.Layers);
        Assert.Equal("Ground", asset.Layers[0].Name);
    }

    [Fact]
    public void ParseFromString_CsvData_PopulatesLayerTiles()
    {
        var asset = TilemapImporter.ParseFromString(SimpleTmx);
        var layer = asset.Layers[0];
        Assert.Equal(1, layer.GetTile(0, 0));
        Assert.Equal(2, layer.GetTile(1, 0));
        Assert.Equal(4, layer.GetTile(3, 0));
        Assert.Equal(5, layer.GetTile(0, 1));
        Assert.Equal(8, layer.GetTile(3, 1));
        Assert.Null(layer.GetTile(0, 2));  // gid 0 → null
        Assert.Equal(1, layer.GetTile(1, 2));
    }

    [Fact]
    public void ParseFromString_EmptyTilesAsZero_MappedToNull()
    {
        const string tmx =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <map version="1.10" width="2" height="2" tilewidth="16" tileheight="16">
              <layer name="L" width="2" height="2">
                <data encoding="csv">0,0,0,0</data>
              </layer>
            </map>
            """;
        var asset = TilemapImporter.ParseFromString(tmx);
        Assert.Null(asset.Layers[0].GetTile(0, 0));
        Assert.Null(asset.Layers[0].GetTile(1, 1));
    }

    [Fact]
    public void ParseFromString_MultipleLayers_ParsesAll()
    {
        const string tmx =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <map version="1.10" width="2" height="2" tilewidth="16" tileheight="16">
              <layer name="Ground" width="2" height="2">
                <data encoding="csv">1,2,3,4</data>
              </layer>
              <layer name="Decoration" width="2" height="2">
                <data encoding="csv">0,0,0,5</data>
              </layer>
            </map>
            """;
        var asset = TilemapImporter.ParseFromString(tmx);
        Assert.Equal(2, asset.Layers.Count);
        Assert.Equal("Ground", asset.Layers[0].Name);
        Assert.Equal("Decoration", asset.Layers[1].Name);
        Assert.Equal(5, asset.Layers[1].GetTile(1, 1));
    }

    [Fact]
    public void ParseFromString_XmlEncoding_PopulatesLayerTiles()
    {
        const string tmx =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <map version="1.10" width="2" height="2" tilewidth="16" tileheight="16">
              <layer name="L" width="2" height="2">
                <data>
                  <tile gid="3"/>
                  <tile gid="7"/>
                  <tile gid="0"/>
                  <tile gid="2"/>
                </data>
              </layer>
            </map>
            """;
        var asset = TilemapImporter.ParseFromString(tmx);
        Assert.Equal(3, asset.Layers[0].GetTile(0, 0));
        Assert.Equal(7, asset.Layers[0].GetTile(1, 0));
        Assert.Null(asset.Layers[0].GetTile(0, 1));
        Assert.Equal(2, asset.Layers[0].GetTile(1, 1));
    }

    [Fact]
    public void ParseFromString_NoLayers_ReturnsEmptyLayerList()
    {
        const string tmx =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <map version="1.10" width="5" height="5" tilewidth="32" tileheight="32"/>
            """;
        var asset = TilemapImporter.ParseFromString(tmx);
        Assert.Empty(asset.Layers);
    }

    [Fact]
    public void ParseFromString_ExternalTilesetNotFound_CreatesTilesetStub()
    {
        const string tmx =
            """
            <?xml version="1.0" encoding="UTF-8"?>
            <map version="1.10" width="2" height="2" tilewidth="16" tileheight="16">
              <tileset firstgid="1" source="missing.tsx"/>
            </map>
            """;
        var asset = TilemapImporter.ParseFromString(tmx, "");
        Assert.Single(asset.Tilesets);
        Assert.Equal(1, asset.Tilesets[0].FirstGid);
        Assert.Equal("missing", asset.Tilesets[0].Name);
    }

    [Fact]
    public void ParseFromString_StoresFilePath()
    {
        var asset = TilemapImporter.ParseFromString(SimpleTmx, "/maps/world.tmx");
        Assert.Equal("/maps/world.tmx", asset.FilePath);
    }
}
