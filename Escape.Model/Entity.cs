using Escape.Persistence;

namespace Escape.Model
{
    public class Entity
    {
        public Tile Tile { get; set; }

        public Entity(Tile tile, TileType tileType)
        {
            this.Tile = tile;
            this.Tile.Type = tileType;
        }
    }

    public class Enemy : Entity
    {
        public bool Dead { get; set; }

        public Enemy(Tile tile, TileType tileType) : base(tile, tileType) { }
    }
}
