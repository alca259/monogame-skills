namespace MonoGame.Editor.Core.UnitTests.Commands;

public sealed class EraseTileCommandTests
{
    [Fact]
    public void Execute_ClearsTileOnLayer()
    {
        FakeTileLayer layer = new();
        layer.SetTile(1, 1, 9);
        EraseTileCommand cmd = new(layer, 1, 1);

        cmd.Execute();

        Assert.Null(layer.GetTile(1, 1));
    }

    [Fact]
    public void Undo_RestoresPreviousTile()
    {
        FakeTileLayer layer = new();
        layer.SetTile(1, 1, 9);
        EraseTileCommand cmd = new(layer, 1, 1);
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(9, layer.GetTile(1, 1));
    }

    [Fact]
    public void Description_IsEraseTile()
    {
        Assert.Equal("Erase Tile", new EraseTileCommand(new FakeTileLayer(), 0, 0).Description);
    }

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
