using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Escape.Persistence
{
    public class TextFilePersistence : IPersistence
    {
        private String _saveDirectory;
        public int MapSize { get; set; }

        public int[,] Table { get; set; }

        public int Secs { get; set; }
        public int Mins { get; set; }

        public async Task Load(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string[] numbers = reader.ReadToEnd().Split();

                    MapSize = int.Parse(numbers[0]);
                    Secs = int.Parse(numbers[1]);
                    Mins = int.Parse(numbers[2]);

                    Table = new int[MapSize, MapSize];

                    if (MapSize < 11) throw new DataException();
                    for (int i = 3; i < numbers.Length-1; i++)
                    {
                        Table[(i - 3) / MapSize, (i - 3) % MapSize] = int.Parse(numbers[i]);
                    }
                }

            }
            catch
            {
                throw new DataException();
            }

        }

        public async Task Save(string path, int mapsize, Tile[,] tileTable, int givenSecs, int givenMins)
        {
            Table = new int[mapsize, mapsize];

            for (var i = 0; i < mapsize; i++)
            {
                for (var j = 0; j < mapsize; j++)
                {
                    Table[i, j] = (int)tileTable[i, j].Type;
                }
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(path)) 
                {
                    writer.Write(mapsize + " ");

                    writer.Write(givenSecs + " ");

                    writer.Write(givenMins + " ");

                    for (var i = 0; i < mapsize; i++)
                    {
                        for (var j = 0; j < mapsize; j++)
                        {
                            writer.Write(Table[i, j] + " "); 
                        }
                    }
                    
                }
            }
            catch
            {
                throw new DataException();
            }

        }

        public async Task<ICollection<SaveEntry>> ListAsync()
        {
            try
            {
                return Directory.GetFiles(_saveDirectory, "*.stl").Select(path => new SaveEntry
                    {
                        Name = Path.GetFileNameWithoutExtension(path),
                        Time = File.GetCreationTime(path)
                    }).ToList();
            }
            catch
            {
                throw new DataException();
            }
        }

    }
}
