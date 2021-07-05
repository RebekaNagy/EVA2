using System;
using Escape.Persistence;

namespace Escape.Model
{
    public class TileChangedEventArgs : EventArgs
    {
        public int X { get; private set; }
        
        public int Y { get; private set; }
        
        public TileType Type { get; private set; }
        
        public TileChangedEventArgs(int x, int y, TileType type)
        {
            X = x;
            Y = y;
            Type = type;
        }
    }
}
