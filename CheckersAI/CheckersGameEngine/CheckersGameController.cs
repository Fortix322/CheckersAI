using System;

namespace CheckersAI.CheckersGameEngine
{
    class CheckersGameController
    {
        static void Main(string[] args)
        {
            Board board = new Board(BoardSize.Medium,true,true,true);
            board.GameLoop();
            
        }
    }
}