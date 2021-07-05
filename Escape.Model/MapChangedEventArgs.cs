using System;

namespace Escape.Model
{
    public class MapChangedEventArgs : EventArgs
    {
        public int Size { get; private set; }

        public MapChangedEventArgs(int size)
        {
            Size = size;
        }
    }
}
