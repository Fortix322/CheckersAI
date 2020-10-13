using System;
using System.Collections.Generic;
using System.Text;

namespace CheckersAI.CheckersGameEngine
{
    //TODO: Сделать хороший интерфейс управления,в котором будет печататься консоль а ниже будут отображаться последние 15 
    // выведенных сообщений 
    public class CheckersPrinter : IPrintable
    {
        private CheckersBoard mainBoard;
        private CheckersPlayer[] players;

        public CheckersPrinter(CheckersBoard board)
        {
            mainBoard = board;
            players = mainBoard.GetPlayers();

        }


        public void Print()
        {
            Console.Clear();
            mainBoard.Print();
            Console.Write("\n\n");
            mainBoard.GetCurrentPlayer().Print();
        }
    }
}
