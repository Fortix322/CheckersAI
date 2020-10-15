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
            CheckersBoard checkers = new CheckersBoard(CheckersRulesEnum.rus,playersArray);
            CheckersPrinter print = new CheckersPrinter(checkers);
            checkers.StartGame();
            //checkers.Move(playersArray[1], "f2;e3");
            //print.Print();

            //checkers.Move(playersArray[0], "c3;d2");
            //print.Print();

            //checkers.Move(playersArray[1], "f6;e7");
            //print.Print();

            //checkers.Move(playersArray[0], "c1;d0");
            //print.Print();

            //checkers.Move(playersArray[1], "e3;c1");
            print.Print();

            int a = 1;
            int ind = 1;

            while (true)
            {
                try
                {
                    playersArray[ind].DoMove();
                    print.Print();
                    a *= -1;
                    ind += a;
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
