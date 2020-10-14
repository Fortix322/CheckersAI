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
            playersArray[1] = new AI(CheckerSide.white);
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

            int ind = 1;
            string coord;

            while (true)
            {
                coord = Console.ReadLine();
                try
                {
                    CheckerSide winner = checkers.DoMove(playersArray[ind], coord);
                    if (winner == CheckerSide.none) Console.WriteLine($"{playersArray[ind].checkerSide} - winner");
                    else if(winner == CheckerSide.black) ind = 0;
                    else ind = 1;
                    print.Print();
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
