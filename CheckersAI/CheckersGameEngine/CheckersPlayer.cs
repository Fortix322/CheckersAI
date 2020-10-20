using System;
using System.Collections.Generic;
using System.Text;

namespace CheckersAI.CheckersGameEngine
{
    internal abstract class CheckersPlayer : IPrintable , ICloneable
    {
        public readonly CheckerSide checkerSide;
        public CheckersBoard boardOwner;
        private List<Checker> _movableCheckers;
        private List<Checker> _checkerCanBeat;
        public bool IsWinner = false;

        protected CheckersPlayer(CheckerSide side)
        {
            checkerSide = side;
            _movableCheckers = new List<Checker>();
            _checkerCanBeat = new List<Checker>();
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

        public void EditMovableChecker(Checker checker, bool AddOrRemove)
        {
            if(AddOrRemove)
                _movableCheckers.Add(checker);
            else _movableCheckers.Remove(checker);
        }

        public void EditCheckerCanBeat(Checker checker,bool AddOrRemove)
        {
            if (AddOrRemove)
                _checkerCanBeat.Add(checker);
            else _checkerCanBeat.Remove(checker);
        }

        public void RemoveAllMovableChecker()
        {
            _movableCheckers.Clear();
        }

        public void RemoveAllCheckerCanBeat()
        {
            _checkerCanBeat.Clear();
        }

        public int getCheckersCanBeatCount()
        {
            return _checkerCanBeat.Count;
        }

        public int geMovableCheckerCount()
        {
            return _movableCheckers.Count;
        }
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
