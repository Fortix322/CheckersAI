using System;

namespace CheckersAI.CheckersGameEngine
{
    interface IPrintable
    {
        public void Print();
    }

    public enum CheckersRulesEnum : sbyte
    {
        rus = 8
    }

    public enum PlayerTypeEnum
    {
        ai,
        player
    }

    public enum CheckerType
    {
        ordinary,
        king
    }

    public enum CheckerSide
    {
        none = 0,
        black,
        white
    }
    class CheckersGameController
    {
        public const sbyte amountOfPlayers = 2;
        static void Main(string[] args)
        {
            CheckersPlayer[] playersArray = new CheckersPlayer[amountOfPlayers];
            playersArray[0] = new Player(CheckerSide.black);
            playersArray[1] = new Player(CheckerSide.white);
            CheckersBoard checkers = new CheckersBoard(CheckersRulesEnum.rus,playersArray,true);
            CheckersPrinter print = new CheckersPrinter(checkers);
            checkers.StartGame();
            
            print.Print();

            int ind = 1;

            while (true)
            {
                try
                {
                    CheckerSide moveNow = playersArray[ind].DoMove();
                    print.Print();
                    if (playersArray[0].checkerSide == moveNow) ind = 0;
                    else ind = 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
            

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
