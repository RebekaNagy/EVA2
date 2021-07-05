using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Escape.Persistence;

public class DbPersistence : IPersistence
{
    public int MapSize { get; set; }

    public int[,] Table { get; set; }

    public int Secs { get; set; }
    public int Mins { get; set; }

    private Context ConText;

    public DbPersistence(String connection)
    {
        ConText = new Context(connection);
        ConText.Database.CreateIfNotExists();
    }

    public async Task Load(string name)
    {
        try
        {
            Game game = await ConText.Games
                .Include(g => g.Fields)
                .SingleAsync(g => g.Name == name);

            this.MapSize = game.TableSize;

            this.Table = new int[MapSize, MapSize];

            this.Mins = game.TableMins;
            this.Secs = game.TableSecs;

            foreach (Field field in game.Fields)
            {
                this.Table[field.X, field.Y] = field.Value;
            }
        }
        catch
        {
            throw new DataException();
        }
    }

    public async Task Save(string name, int size, Tile[,] tileTable, int givenSecs, int givenMins)
    {
        try
        {
            Game OverWriteGame = await ConText.Games
                .Include(g => g.Fields)
                .SingleOrDefaultAsync(g => g.Name == name);

            if (OverWriteGame != null)
            {
                ConText.Games.Remove(OverWriteGame);
            }

            Game ActualGame = new Game
            {
                TableSecs = givenSecs,
                TableMins = givenMins,
                TableSize = size,
                Name = name,
            };

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    Field field = new Field
                    {
                        X = i,
                        Y = j,
                        Value = (int)tileTable[i,j].Type
                    };
                    ActualGame.Fields.Add(field);
                }
            }

            ConText.Games.Add(ActualGame);
            await ConText.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new DataException();
        }
    }

    public async Task<ICollection<SaveEntry>> ListAsync()
    {
        try
        {
            return await ConText.Games
                .OrderByDescending(g => g.Time)
                .Select(g => new SaveEntry { Name = g.Name, Time = g.Time })
                .ToListAsync();
        }
        catch
        {
            throw new DataException();
        }
    }
    
}

