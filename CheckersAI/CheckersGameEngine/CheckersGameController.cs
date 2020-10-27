using System;

namespace CheckersAI.CheckersGameEngine
{
    class CheckersGameController
    {
        static void Main(string[] args)
        {
            CheckersBoard board = new CheckersBoard(CheckersFieldSize.Medium, CheckersPlayerType.Player, CheckersPlayerType.Player);
            board.MainGameLoopStart(true);
        }
    }
}
