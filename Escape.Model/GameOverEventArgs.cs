using System;

namespace Escape.Model
{
    public class GameOverEventArgs : EventArgs
    {
        public bool GameWon { get; private set; }

        public GameOverEventArgs(bool gameWon)
        {
            GameWon = gameWon;
        }
    }
}
