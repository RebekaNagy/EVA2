using System.Collections.Generic;
using System.Threading.Tasks;

namespace Escape.Persistence
{
    public interface IPersistence
    {
        int MapSize { get; set; }

        int[,] Table { get; set; }

        int Secs { get; set; }

        int Mins { get; set; }
        Task Load(string name);

        Task Save(string name, int mapsize, Tile[,] tileTable, int givenSecs, int givenMins);

        Task<ICollection<SaveEntry>> ListAsync();
    }
}
