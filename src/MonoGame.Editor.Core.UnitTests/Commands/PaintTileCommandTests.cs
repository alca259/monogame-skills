namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class PaintTileCommandTests
{
    [Fact]
    public void Execute_SetsTileOnLayer()
    {
        FakeTileLayer layer = new();
        PaintTileCommand cmd = new(layer, 2, 3, 7);

        cmd.Execute();

        Assert.Equal(7, layer.GetTile(2, 3));
    }

    [Fact]
    public void Undo_RestoresPreviousTile()
    {
        FakeTileLayer layer = new();
        layer.SetTile(2, 3, 5);
        PaintTileCommand cmd = new(layer, 2, 3, 7);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(5, layer.GetTile(2, 3));
    }

    [Fact]
    public void Undo_WhenTileWasEmpty_RestoresNull()
    {
        FakeTileLayer layer = new();
        PaintTileCommand cmd = new(layer, 0, 0, 1);
        cmd.Execute();

        cmd.Undo();

        Assert.Null(layer.GetTile(0, 0));
    }

    [Fact]
    public void Description_IsPaintTile()
    {
        Assert.Equal("Paint Tile", new PaintTileCommand(new FakeTileLayer(), 0, 0, 1).Description);
    }

    // Fake layer backed by a dictionary
    private sealed class FakeTileLayer : ITileLayer
    {
        private readonly Dictionary<(int, int), int> _tiles = [];

        public int? GetTile(int column, int row)
            => _tiles.TryGetValue((column, row), out int id) ? id : null;

        public void SetTile(int column, int row, int? tileId)
        {
            if (tileId is null)
                _tiles.Remove((column, row));
            else
                _tiles[(column, row)] = tileId.Value;
        }
    }
}
