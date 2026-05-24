using MonoGame.Editor.Core.Commands;

namespace MonoGame.Editor.Core.UnitTests.Tilemaps;

public sealed class EditorTileLayerTests
{
    [Fact]
    public void GetTile_Unset_ReturnsNull()
    {
        var layer = new EditorTileLayer("Ground", 4, 4);
        Assert.Null(layer.GetTile(2, 2));
    }

    [Fact]
    public void SetTile_ValidCoordinate_StoresTileId()
    {
        var layer = new EditorTileLayer("Ground", 4, 4);
        layer.SetTile(1, 2, 5);
        Assert.Equal(5, layer.GetTile(1, 2));
    }

    [Fact]
    public void SetTile_WithNullId_ClearsTile()
    {
        var layer = new EditorTileLayer("Ground", 4, 4);
        layer.SetTile(0, 0, 3);
        layer.SetTile(0, 0, null);
        Assert.Null(layer.GetTile(0, 0));
    }

    [Fact]
    public void GetTile_OutOfBounds_ReturnsNull()
    {
        var layer = new EditorTileLayer("Ground", 3, 3);
        Assert.Null(layer.GetTile(-1, 0));
        Assert.Null(layer.GetTile(0, -1));
        Assert.Null(layer.GetTile(3, 0));
        Assert.Null(layer.GetTile(0, 3));
    }

    [Fact]
    public void SetTile_OutOfBounds_DoesNotThrow()
    {
        var layer = new EditorTileLayer("Ground", 3, 3);
        var ex = Record.Exception(() => layer.SetTile(-1, 0, 1));
        Assert.Null(ex);
    }

    [Fact]
    public void Name_ReturnsConstructorValue()
    {
        var layer = new EditorTileLayer("Collision", 2, 2);
        Assert.Equal("Collision", layer.Name);
    }

    [Fact]
    public void Dimensions_ReflectConstructorValues()
    {
        var layer = new EditorTileLayer("Test", 10, 8);
        Assert.Equal(10, layer.Width);
        Assert.Equal(8, layer.Height);
    }

    [Fact]
    public void ToCsvData_EmptyLayer_ProducesZeros()
    {
        var layer = new EditorTileLayer("Ground", 2, 2);
        string csv = layer.ToCsvData();
        string[] tokens = csv.Split([',', '\n', '\r'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Equal(4, tokens.Length);
        Assert.All(tokens, t => Assert.Equal("0", t));
    }

    [Fact]
    public void ToCsvData_WithTiles_ProducesExpectedTokenCount()
    {
        var layer = new EditorTileLayer("Ground", 3, 2);
        layer.SetTile(0, 0, 1);
        layer.SetTile(1, 0, 2);
        layer.SetTile(2, 1, 5);
        string csv = layer.ToCsvData();
        string[] tokens = csv.Split([',', '\n', '\r'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Assert.Equal(6, tokens.Length);
    }

    [Fact]
    public void ToCsvData_RoundTrips_ViaParseCsvData()
    {
        var layer = new EditorTileLayer("Ground", 3, 3);
        layer.SetTile(0, 0, 1);
        layer.SetTile(1, 1, 7);
        layer.SetTile(2, 2, 3);

        // Re-parse via TilemapImporter using a minimal TMX with the CSV
        string csv = layer.ToCsvData();
        string tmx =
            $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <map version="1.10" width="3" height="3" tilewidth="32" tileheight="32">
              <layer name="Ground" width="3" height="3">
                <data encoding="csv">{csv}</data>
              </layer>
            </map>
            """;

        var asset = TilemapImporter.ParseFromString(tmx);
        var parsed = asset.Layers[0];

        Assert.Equal(1, parsed.GetTile(0, 0));
        Assert.Equal(7, parsed.GetTile(1, 1));
        Assert.Equal(3, parsed.GetTile(2, 2));
        Assert.Null(parsed.GetTile(0, 1));
    }

    [Fact]
    public void ImplementsITileLayer()
    {
        var layer = new EditorTileLayer("Ground", 2, 2);
        Assert.IsAssignableFrom<ITileLayer>(layer);
    }
}
