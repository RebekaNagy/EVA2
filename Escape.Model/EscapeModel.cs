using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Timers;
using Escape.Persistence;

namespace Escape.Model
{
    public class EscapeModel
    {
        public int Size { get; set; }
        public Tile[,] Tiles { get; set; }
        public Enemy[] Enemies { get; set; }
        public Entity Player { get; set; }

        public int[,] table;

        public int tmp;

        private Timer EnemyTimer;

        public bool loaded;

        public bool isWon;

        public int secs;
        public int mins;

        private IPersistence persistence;

        public EscapeModel(IPersistence dataAccess)
        {
            persistence = dataAccess;
        }

        public async Task Load(string name) 
        {
            if (persistence == null)
                throw new InvalidOperationException("No data access is provided.");

            await persistence.Load(name);
            loaded = true;

            LoadGame(persistence.MapSize, persistence.Table, persistence.Secs, persistence.Mins);
        }

        public async Task Save(string name)
        {
            if (persistence == null)
                throw new InvalidOperationException("No data access is provided.");

            await persistence.Save(name, Size, Tiles, secs, mins);
            
        }

        public async Task<ICollection<SaveEntry>> ListGamesAsync()
        {
            if (persistence == null)
                throw new InvalidOperationException("No data access is provided.");

            return await persistence.ListAsync();
        }

        public void LoadGame(int size, int[,] table, int givensecs, int givenmins)
        {
            Pause(true);

            secs = givensecs;
            mins = givenmins;
            tmp = 0;

            EnemyTimer = new Timer(500);
            EnemyTimer.Elapsed += new ElapsedEventHandler(OnTick);

            Size = size;

            Tiles = new Tile[Size, Size];

            OnMapChanged(Size);

            Enemies = new Enemy[2];

            bool firstEnemy = true;

            for (var i = 0; i < Size; ++i)
            {
                for (var j = 0; j < Size; ++j)
                {
                    Tiles[i, j] = new Tile(i, j);
                    Tiles[i, j].Type = (TileType)table[i, j];
                    if (Tiles[i, j].Type == TileType.Player)
                    {
                        Player = new Entity(Tiles[i, j], TileType.Player);
                    }
                    if (Tiles[i, j].Type == TileType.Enemy)
                    {
                        if(firstEnemy)
                        {
                            Enemies[0] = new Enemy(Tiles[i, j], TileType.Enemy);
                            firstEnemy = false;
                        }
                        else
                        {
                            Enemies[1] = new Enemy(Tiles[i, j], TileType.Enemy);
                            firstEnemy = true;
                        }
                    }

                    OnTileChanged(Tiles[i, j]);
                }
            }

            if (!firstEnemy)
            {
                Enemies[1] = new Enemy(Tiles[1,1], TileType.Enemy);
                Enemies[1].Dead = true;
            }

            Pause(false);
        }

        #region pause
        public void Pause(bool pause)
        {
            if (pause)
            {
                EnemyTimer?.Stop();
            }
            else
            {
                EnemyTimer?.Start();
            }
        }

        public bool Paused
        {
            get
            {
                if (EnemyTimer != null)
                    return !EnemyTimer.Enabled;
                else
                    return true;
            }
        }


        #endregion

        public event EventHandler<MapChangedEventArgs> MapChanged;
        public event EventHandler<TileChangedEventArgs> TileChanged;
        public event EventHandler<GameOverEventArgs> GameOver;

        #region moves
        public void MoveEnemies()
        {
            var areAllEnemiesDead = true;

            foreach (var enemy in Enemies)
            {
                areAllEnemiesDead = areAllEnemiesDead && enemy.Dead;

                if (enemy.Dead)
                {
                    continue;
                }

                var currentTile = enemy.Tile;
                var distanceX = Player.Tile.X - currentTile.X;
                var distanceY = Player.Tile.Y - currentTile.Y;
                Tile destinationTile;

                if (Math.Abs(distanceY) > Math.Abs(distanceX))
                {
                    if (distanceY > 0)
                    {
                        destinationTile = Tiles[currentTile.X, currentTile.Y + 1];
                    }
                    else
                    {
                        destinationTile = Tiles[currentTile.X, currentTile.Y - 1];
                    }
                }
                else
                {
                    if (distanceX > 0)
                    {
                        destinationTile = Tiles[currentTile.X + 1, currentTile.Y];
                    }
                    else
                    {
                        destinationTile = Tiles[currentTile.X - 1, currentTile.Y];
                    }
                }

                currentTile.Type = TileType.Empty;
                OnTileChanged(currentTile);

                if (destinationTile.Type == TileType.Mine || destinationTile.Type == TileType.Enemy)
                {
                    enemy.Dead = true;
                }
                else if (destinationTile.Type == TileType.Empty)
                {
                    destinationTile.Type = TileType.Enemy;
                    OnTileChanged(destinationTile);
                    enemy.Tile = destinationTile;
                }
                else
                {
                    isWon = false;
                    OnGameOver(isWon);
                }

            }

            if (areAllEnemiesDead)
            {
                isWon = true;
                OnGameOver(isWon);
            }
        }

        public void MovePlayer(Direction direction)
        {
            if (Paused)
            {
                return;
            }

            Tile destinationTile;

            if (direction == Direction.Left && Player.Tile.Y != 0)
            {
                destinationTile = Tiles[Player.Tile.X, Player.Tile.Y - 1];
            }
            else if (direction == Direction.Right && Player.Tile.Y != Size - 1)
            {
                destinationTile = Tiles[Player.Tile.X, Player.Tile.Y + 1];
            }
            else if (direction == Direction.Down && Player.Tile.X != Size - 1)
            {
                destinationTile = Tiles[Player.Tile.X + 1, Player.Tile.Y];
            }
            else if (direction == Direction.Up && Player.Tile.X != 0)
            {
                destinationTile = Tiles[Player.Tile.X - 1, Player.Tile.Y];
            }
            else
            {
                return;
            }

            if (destinationTile.Type == TileType.Enemy || destinationTile.Type == TileType.Mine)
            {
                isWon = false;
                OnGameOver(isWon);
            }
            else
            {
                Player.Tile.Type = TileType.Empty;
                destinationTile.Type = TileType.Player;
                OnTileChanged(Player.Tile);
                OnTileChanged(destinationTile);
                Player.Tile = destinationTile;
            }
        }
        
        #endregion


        public void NewGame(int size)
        {
            loaded = false;

            secs = 0;
            mins = 0;
            tmp = 0;

            Pause(true);

            EnemyTimer = new Timer(500);
            EnemyTimer.Elapsed += new ElapsedEventHandler(OnTick);

            Size = size;

            Tiles = new Tile[Size, Size];

            for (var i = 0; i < Size; i++)
            {
                for (var j = 0; j < Size; j++)
                {
                    Tiles[i, j] = new Tile(i, j);
                }
            }

            OnMapChanged(Size);

            foreach (Tile tile in Tiles)
            {
                if (tile.Type != TileType.Empty)
                { 
                    tile.Type = TileType.Empty;
                    OnTileChanged(tile);
                }
            }

            var middle = (int)Math.Floor(Size / 2.0);

            Player = new Entity(Tiles[0, middle], TileType.Player);

            Enemies = new Enemy[2];

            Enemies[0] = new Enemy(Tiles[Size - 1, 0], TileType.Enemy);
            Enemies[1] = new Enemy(Tiles[Size - 1, Size - 1], TileType.Enemy);

            var random = new Random();

            for (var i = 0; i < middle + 3; i++)
            {
                var randomX = random.Next(1, Size - 1);
                var randomY = random.Next(1, Size - 1);
                Tiles[randomX, randomY].Type = TileType.Mine;
            }

            for (var i = 0; i < Size; i++)
            {
                for (var j = 0; j < Size; j++)
                {
                    OnTileChanged(Tiles[i, j]);
                }
            }

            Pause(false);
        }
        
        #region Events

        private void OnTick(object sender, ElapsedEventArgs e)
        {
            MoveEnemies();
            if (tmp % 2 == 0)
            {
                secs++;
                if (secs == 60)
                {
                    mins++;
                    secs = 0;
                }
            }
            
            tmp = tmp + 1;

        }
        private void OnMapChanged(int n)
        {
            MapChanged?.Invoke(this, new MapChangedEventArgs(n));
        }
        private void OnGameOver(bool n) 
        {
            Pause(true);
            GameOver?.Invoke(this, new GameOverEventArgs(n));
        }

        private void OnTileChanged(Tile tile)
        {
            TileChanged?.Invoke(this, new TileChangedEventArgs(tile.X, tile.Y, tile.Type));
        }
        
        #endregion

    }
}
