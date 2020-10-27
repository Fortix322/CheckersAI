using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace CheckersAI.CheckersGameEngine
{
    public enum CheckersFieldSize
    {
        Small = 6,
        Medium = 8,
        Large = 10
    }

    public enum CheckersPlayerType
    {
        Ai,
        Player,
        NetworkPlayer
    }

    public enum CheckersPlayerSide
    {
        None = 0,
        White,
        Black 
    }

    public enum CheckersTypes
    {
        whiteOrdinary = 1,
        blackOrdinary = 2,
        whiteQueen = 3,
        blackQueen = 4,
        emptyField = 0
    }
    public class CheckersBoard
    {
        private int fieldSize;
        private int[,] fieldArray;
        private CheckersPlayer whitePlayer;
        private CheckersPlayer blackPlayer;
        public CheckersBoard(CheckersFieldSize size,CheckersPlayerType white, CheckersPlayerType black)
        {
            fieldSize = (int)size;
            fieldArray = FieldBuilding(fieldSize);
            SetPlayers( out whitePlayer, white, out blackPlayer, black);
        }

        public CheckersPlayerSide MainGameLoopStart(bool necessaryBeat)
        {
            CheckersPlayer currentPlayer = whitePlayer;
            (int x, int y) start,finish;


            while (true)
            {
                currentPlayer.DoMove(out start, out finish);
                Console.WriteLine($"{ProcessMove(start, finish, currentPlayer)},{currentPlayer.playerSide}");
                currentPlayer = ChangeMove(currentPlayer);
            }
        }

        private bool ProcessMove((int x, int y) start,(int x, int y) finish,CheckersPlayer currentPlayer)
        {
            if(CanMove(fieldArray,start,finish,currentPlayer.playerSide))
            {
                return true;
            }
            return false;
        }

        public bool CanMove(int[,] field,(int x, int y) start, (int x, int y) finish, CheckersPlayerSide currentPlayerSide)
        {
            if (currentPlayerSide == CheckersPlayerSide.None) return false;
            else if (field[finish.y, finish.x] != (int)CheckersTypes.emptyField) return false;
            try
            {
                int yDifference = currentPlayerSide == CheckersPlayerSide.White ? -1 : 1;
                if (finish.y - start.y == yDifference && Math.Abs(finish.x - start.x) == 1)
                {
                    if ((yDifference == -1 && (field[start.y, start.x] == (int)CheckersTypes.whiteOrdinary || field[start.y, start.x] == (int)CheckersTypes.whiteQueen)) ||
                       (yDifference == 1 && (field[start.y, start.x] == (int)CheckersTypes.blackOrdinary || field[start.y, start.x] == (int)CheckersTypes.blackQueen)))
                    {
                        return true;
                    }
                }

            }
            catch (Exception ex) { 
                return false;
            }
            return false;
            
        }

        private int[,] FieldBuilding(int fieldSize)
        {
            bool oddRow;
            int teamDistance = (fieldSize - 2) / 2;
            int[,] fieldArray = new int[fieldSize,fieldSize];
            int checkers = (int)CheckersTypes.blackOrdinary;
            for(int i = 0;i < fieldSize; i++)
            {
                if (i == teamDistance)
                {
                    i++;
                    checkers = (int)CheckersTypes.whiteOrdinary;
                    continue;
                }
                oddRow = (i % 2 != 0);
                for(int j = 0;j < fieldSize; j++)
                {
                    if (oddRow) fieldArray[i, j++] = checkers;
                    else fieldArray[i, ++j] = checkers;
                }
            }
            return fieldArray;
        }

        private void SetPlayers(out CheckersPlayer whitePlayer, CheckersPlayerType whiteType, out CheckersPlayer blackPlayer, CheckersPlayerType blackType)
        {
            CheckersPlayerType playerType = whiteType;
            CheckersPlayerSide playerSide = CheckersPlayerSide.White;
            CheckersPlayer currentPlayer = null;
            whitePlayer = blackPlayer = null;
            while(whitePlayer == null || blackPlayer == null) { 

                switch (playerType)
                {
                    case CheckersPlayerType.Ai :
                        // IN FUTURE 
                        break;
                    case CheckersPlayerType.Player :
                        currentPlayer = new Player(playerSide);
                        break;
                    case CheckersPlayerType.NetworkPlayer:
                        // IN FUTURE
                        break;
                }
                if (whitePlayer == null)
                {
                    whitePlayer = currentPlayer;
                    playerSide = CheckersPlayerSide.Black;
                    playerType = blackType;
                }
                else blackPlayer = currentPlayer;
            }
        }

        private CheckersPlayer ChangeMove(CheckersPlayer currentPlayer)
        {
            if (currentPlayer == null) return null;
            CheckersPlayer nextPlayer;
            nextPlayer = currentPlayer == whitePlayer ? blackPlayer : whitePlayer;
            return nextPlayer;
        }

        private abstract class CheckersPlayer
        {
            public readonly CheckersPlayerSide playerSide;
            public abstract bool DoMove(out (int x, int y) startCoordinates, out (int x, int y) finishCoordinates);

            public CheckersPlayer(CheckersPlayerSide playerSide)
            {
                this.playerSide = playerSide;
            }
        }

        private class Player : CheckersPlayer
        {
            public Player(CheckersPlayerSide playerSide) : base(playerSide) { }
            
            public override bool DoMove(out (int x, int y) startCoordinates, out (int x, int y) finishCoordinates)
            {
                string coordinates;
                if (RequestInput(out coordinates))
                {
                    if (ParseCoordinates(coordinates, out startCoordinates, out finishCoordinates)) return true;
                    return false;
                }
                else
                {
                    startCoordinates = (-1,-1);
                    finishCoordinates = (-1, -1);
                    return false;
                }
            }

            public bool RequestInput(out string coordinates)
            {
                coordinates = null;
                string coordinateString = Console.ReadLine();
                if (coordinateString != null)
                {
                    coordinates = coordinateString;
                    return true;
                }
                else return false;
            }

            private bool ParseCoordinates(string coordinateString, out (int x, int y) startCoordinates, out (int x, int y) finishCoordinates)
            {
                startCoordinates = (-1, -1);
                finishCoordinates = (-1, -1);
                bool punctMark = false;

                for(int i = 0;i < coordinateString.Length; i++)
                {
                    if (char.IsLetter(coordinateString[i]))
                    {
                        if (startCoordinates.x == -1) startCoordinates.x = char.ToUpper(coordinateString[i]) - 'A';

                        else if (finishCoordinates.x == -1)
                        {
                            if (punctMark)
                                finishCoordinates.x = char.ToUpper(coordinateString[i]) - 'A';
                            else return false;
                        }

                        else return false;
                    }
                    else if (char.IsDigit(coordinateString[i]))
                    {
                        if (startCoordinates.y == -1)
                        {
                            startCoordinates.y = coordinateString[i] - '0';
                        }

                        else if (finishCoordinates.y == -1)
                        {
                            if (punctMark)
                                finishCoordinates.y = coordinateString[i] - '0';
                            else return false;
                        }
                        else return false;

                    }
                    else if (char.IsWhiteSpace(coordinateString[i])) continue;
                    else if (char.IsPunctuation(coordinateString[i])) punctMark = true;
                    else return false;
                }
                return Math.Min(startCoordinates.x, startCoordinates.y) >= 0 && Math.Min(finishCoordinates.x, finishCoordinates.y) >= 0;
            }

            
        }
    }

}