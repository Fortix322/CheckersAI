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
        private readonly int fieldSize;
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
            List<Move> availableMoves = new List<Move>();
            if (FindAllPossibleMoves(start, ref availableMoves, currentPlayer.playerSide))
            {
                for (int i = 0; i < availableMoves.Count; i++)
                {
                    if (availableMoves[i].finish == finish)
                    {
                        if (availableMoves[i].checkersForRemove == null && availableMoves[i].underMoves == null)
                        {
                            if (availableMoves[i].turnsQueen)
                            {
                                fieldArray[start.y, start.x] = (int)CheckersTypes.emptyField;
                                fieldArray[finish.y, finish.x] = currentPlayer.playerSide == CheckersPlayerSide.White ? (int)CheckersTypes.whiteQueen : (int)CheckersTypes.blackQueen;
                            }
                            else
                            {
                                fieldArray[finish.y, finish.x] = fieldArray[start.y, start.x];
                                fieldArray[start.y, start.x] = (int)CheckersTypes.emptyField;
                            }
                            return true;

                        }
                    }
                }
            }
            return false;

        }

        private void CanMove((int x, int y) start,ref List<Move> moves,bool currentCheckerQueen,CheckersPlayerSide currentPlayerSide,(int ordinary,int queen) enemy)
        {
            if (!currentCheckerQueen)
            {
                int yDif = currentPlayerSide == CheckersPlayerSide.White ? -1 : 1;
                (int x, int y) localFinish = (-1, start.y + yDif);
                for (int i = 0, xDif = 1; i < 2; i++, xDif *= -1)
                {
                    try
                    {
                        localFinish.x = start.x + xDif;
                        if (fieldArray[localFinish.y, localFinish.x] != enemy.queen && fieldArray[localFinish.y, localFinish.x] != enemy.ordinary
                           && fieldArray[localFinish.y, localFinish.x] == (int)CheckersTypes.emptyField)
                        {
                            bool toQueen = false;
                            if ((currentPlayerSide == CheckersPlayerSide.White && localFinish.y == 0) ||
                                (currentPlayerSide == CheckersPlayerSide.Black && localFinish.y == fieldSize - 1)) toQueen = true;
                            moves.Add(new Move(start, localFinish, toQueen, null, null));
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        private void CanBeat((int x, int y) start, ref List<Move> moves, bool currentCheckerQueen, CheckersPlayerSide currentPlayerSide, (int ordinary, int queen) enemy)
        {

            if (!currentCheckerQueen)
            {
                (int x, int y) localFinish = (-1, -1);
                (int x, int y) middlePoint = (-1, -1);
                List<(int x, int y)> checkersForRemove = new List<(int x, int y)>();
                List<(int x, int y)> underMoves = new List<(int x, int y)>();
                for (int i = 0,xDif = 2,yDif = 2; i < 4; i++,xDif *= -1)
                {
                    try
                    {
                        if (i % 2 == 0) yDif *= -1;
                        localFinish.x = start.x + xDif;
                        localFinish.y = start.y + yDif;
                        if (fieldArray[localFinish.y, localFinish.x] != (int)CheckersTypes.emptyField) continue;
                        middlePoint = ((start.x + localFinish.x) / 2, (start.y + localFinish.y) / 2);
                        if (fieldArray[middlePoint.y, middlePoint.x] != enemy.ordinary && fieldArray[middlePoint.y, middlePoint.x] != enemy.queen) continue;
                        checkersForRemove.Add(middlePoint);
                        CanBeat(start, localFinish, ref moves, currentCheckerQueen, currentPlayerSide, enemy, checkersForRemove, underMoves);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        private void CanBeat((int x, int y) start, (int x, int y) localStart, ref List<Move> moves, bool currentCheckerQueen, 
                             CheckersPlayerSide currentPlayerSide, (int ordinary, int queen) enemy, List<(int x, int y)> checkersForRemove, List<(int x, int y)> underMoves)
        {
            if (!currentCheckerQueen)
            {
                (int x, int y) localFinish = (-1, -1);
                (int x, int y) middlePoint = (-1, -1);
                int childCount = 0;
                for (int i = 0, xDif = 2, yDif = 2; i < 4; i++, xDif *= -1)
                {
                    try
                    {
                        if (i % 2 == 0) yDif *= -1;
                        localFinish.x = localStart.x + xDif;
                        localFinish.y = localStart.y + yDif;

                        if (fieldArray[localFinish.y, localFinish.x] != (int)CheckersTypes.emptyField) continue;

                        if (underMoves.Count != 0)
                        {
                            if (localFinish == underMoves[underMoves.Count - 1]) continue;
                            bool repeat = false;
                            for (int j = 0; j < underMoves.Count; j++) if (underMoves[j] == localFinish) repeat = true;
                            if (repeat) continue;
                        }
                        
                        middlePoint = ((localStart.x + localFinish.x) / 2, (localStart.y + localFinish.y) / 2);
                        if (fieldArray[middlePoint.y, middlePoint.x] != enemy.ordinary && fieldArray[middlePoint.y, middlePoint.x] != enemy.queen) continue;
                        childCount++;


                        if(underMoves.Count == 0 || underMoves[underMoves.Count - 1] != localFinish)
                            underMoves.Add(localStart);
                        checkersForRemove.Add(middlePoint);

                        List<(int x, int y)> nextCheckerForRemove;
                        List<(int x, int y)> nextUnderMove;

                        CloneList(out nextUnderMove, ref underMoves);

                        CloneList(out nextCheckerForRemove, ref checkersForRemove);

                        CanBeat(start,localFinish,ref moves,currentCheckerQueen,currentPlayerSide,enemy,nextCheckerForRemove,nextUnderMove);
                    }
                    catch
                    {
                        continue;
                    }
                }
                if(childCount == 0)
                {
                    Move move = new Move(start,localFinish,currentCheckerQueen,checkersForRemove,underMoves);
                    moves.Add(move);
                }
            }
        }

        private void CloneList(out List<(int x, int y)> newQueue,ref List<(int x, int y)> oldQueue)
        {
            (int x, int y)[] temp = new (int x, int y)[oldQueue.Count];
            oldQueue.CopyTo(temp, 0);
            newQueue = new List<(int x, int y)>();
            for (int i = 0; i < temp.Length; i++)
            {
                newQueue.Add(temp[i]);
            }
        }

        private bool FindAllPossibleMoves((int x, int y) start,ref List<Move> availableMoves,CheckersPlayerSide currentPlayerSide)
        { 
            if (currentPlayerSide == CheckersPlayerSide.None) return false;

            if (availableMoves == null) availableMoves = new List<Move>();

            int enemyOrdinary = currentPlayerSide == CheckersPlayerSide.White ? (int)CheckersTypes.blackOrdinary : (int)CheckersTypes.whiteOrdinary;
            int enemyQueen = currentPlayerSide == CheckersPlayerSide.White ? (int)CheckersTypes.blackQueen : (int)CheckersTypes.whiteQueen;
            bool currentCheckerQueen = false;

            if(currentPlayerSide == CheckersPlayerSide.White) if (fieldArray[start.y, start.x] == (int)CheckersTypes.whiteQueen) currentCheckerQueen = true;
            else if(currentPlayerSide == CheckersPlayerSide.Black) if (fieldArray[start.y, start.x] == (int)CheckersTypes.blackQueen) currentCheckerQueen = true;


            CanMove(start, ref availableMoves, currentCheckerQueen, currentPlayerSide, (enemyOrdinary, enemyQueen));

            CanBeat(start, ref availableMoves, currentCheckerQueen, currentPlayerSide, (enemyOrdinary, enemyQueen));


            if (availableMoves.Count > 0) return true;
            return false;
        }


        private int[,] FieldBuilding(int fieldSize)
        {
            bool oddRow;
            int teamDistance = (fieldSize - 2) / 2;
            int[,] fieldArray = new int[fieldSize,fieldSize];
            //int checkers = (int)CheckersTypes.blackOrdinary;
            //for(int i = 0;i < fieldSize; i++)
            //{
            //    if (i == teamDistance)
            //    {
            //        i++;
            //        checkers = (int)CheckersTypes.whiteOrdinary;
            //        continue;
            //    }
            //    oddRow = (i % 2 != 0);
            //    for(int j = 0;j < fieldSize; j++)
            //    {
            //        if (oddRow) fieldArray[i, j++] = checkers;
            //        else fieldArray[i, ++j] = checkers;
            //    }
            //}
            fieldArray[1, 2] = (int)CheckersTypes.blackOrdinary;
            fieldArray[1, 4] = (int)CheckersTypes.blackOrdinary;
            fieldArray[1, 6] = (int)CheckersTypes.blackOrdinary;
            fieldArray[3, 4] = (int)CheckersTypes.blackOrdinary;
            fieldArray[3, 6] = (int)CheckersTypes.blackOrdinary;
            fieldArray[5, 4] = (int)CheckersTypes.blackOrdinary;
            fieldArray[6, 3] = (int)CheckersTypes.whiteOrdinary;


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

        struct Move
        {
            public readonly (int x, int y) start;
            public (int x, int y) finish;
            public bool turnsQueen;

            public List<(int x, int y)> checkersForRemove;
            public List<(int x, int y)> underMoves;

            public Move((int x, int y) startPoint)
            {
                start = startPoint;
                finish = (-1, -1);
                turnsQueen = false;
                checkersForRemove = new List<(int x, int y)>();
                underMoves = new List<(int x, int y)>();
            }

            public Move((int x, int y) startPoint, (int x, int y) finishPoint, bool TurnsQueen, List<(int x, int y)> CheckersForRemove,List<(int x, int y)> UnderMoves)
            {
                start = startPoint;
                finish = finishPoint;
                turnsQueen = TurnsQueen;
                checkersForRemove = CheckersForRemove;
                underMoves = UnderMoves;
            }
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