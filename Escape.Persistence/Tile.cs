
namespace Escape.Persistence
{
    public class Tile
    {
        public TileType Type { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public Tile(int x, int y)
        {
            X = x;
            Y = y;
            Type = TileType.Empty;
        }
    }
}
