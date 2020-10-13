using System;
using System.Collections.Generic;
using System.Text;

namespace CheckersAI.CheckersGameEngine
{
    public abstract class CheckersPlayer : IPrintable , ICloneable
    {
        public readonly CheckerSide checkerSide;
        public List<Checker> movableCheckers;

        protected CheckersPlayer(CheckerSide side)
        {
            checkerSide = side;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public void Print()
        {
            Console.WriteLine($"Now it's time to {(sbyte)checkerSide} - {checkerSide}");
        }

    }

    public class Player : CheckersPlayer
    {
        public Player(CheckerSide side) : base(side)
        {
        }

    }

    public class AI : CheckersPlayer
    {
        public AI(CheckerSide side) : base(side)
        {
        }

    }
}
