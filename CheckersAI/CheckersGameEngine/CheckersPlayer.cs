using System;
using System.Collections.Generic;
using System.Text;

namespace CheckersAI.CheckersGameEngine
{
    internal abstract class CheckersPlayer : IPrintable , ICloneable
    {
        public readonly CheckerSide checkerSide;
        public CheckersBoard boardOwner;
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

        public abstract CheckerSide DoMove();

    }

    internal class Player : CheckersPlayer
    {
        public Player(CheckerSide side) : base(side)
        {
        }

        public override CheckerSide DoMove()
        {
            try
            {
                string coordinates = Console.ReadLine();
                if (boardOwner != null)
                {
                    CheckerSide retSide = boardOwner.DoMove((CheckersPlayer)this, coordinates);
                    return retSide;
                }
                else throw new Exception("No board were claimed");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    internal class AI : CheckersPlayer
    {
        public AI(CheckerSide side) : base(side)
        {
        }

        public override CheckerSide DoMove()
        {
            throw new NotImplementedException();
        }
    }
}
