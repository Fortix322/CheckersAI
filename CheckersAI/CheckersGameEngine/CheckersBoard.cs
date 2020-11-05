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

        private void MovesAnalysis((int x, int y) start, List<Move> moves,bool currentCheckerQueen,CheckersPlayerSide currentPlayerSide,(int ordinary,int queen) enemy)
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

        private void BeatsAnalysis((int x, int y) start, List<Move> moves, bool currentCheckerQueen, CheckersPlayerSide currentPlayerSide, (int ordinary, int queen) enemy)
        {
            if (moves == null) moves = new List<Move>();
            if (!currentCheckerQueen)
            {
                (int x, int y) localFinish = (-1, -1);
                (int x, int y) middlePoint = (-1, -1);
                List<(int x, int y)> checkersForRemove = new List<(int x, int y)>();
                List<(int x, int y)> underMoves = new List<(int x, int y)>();
                for (int i = 0, xDif = 2, yDif = 2; i < 4; i++, xDif *= -1)
                {
                    try
                    {
                        if (i % 2 == 0) yDif *= -1;
                        localFinish.x = start.x + xDif;
                        localFinish.y = start.y + yDif;
                        if (fieldArray[localFinish.y, localFinish.x] != (int)CheckersTypes.emptyField) continue;
                        middlePoint = ((start.x + localFinish.x) / 2, (start.y + localFinish.y) / 2);
                        if (fieldArray[middlePoint.y, middlePoint.x] != enemy.ordinary && fieldArray[middlePoint.y, middlePoint.x] != enemy.queen) continue;

                        if ((currentPlayerSide == CheckersPlayerSide.White && localFinish.y == 0) ||
                            (currentPlayerSide == CheckersPlayerSide.Black && localFinish.y == fieldSize - 1)) currentCheckerQueen = true;

                        checkersForRemove.Add(middlePoint);
                        underMoves.Add(localFinish);

                        BeatsAnalysis(start, localFinish, moves, currentCheckerQueen, currentPlayerSide, enemy, checkersForRemove, underMoves);

                        underMoves.RemoveAt(underMoves.Count - 1);
                        checkersForRemove.RemoveAt(checkersForRemove.Count - 1);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            else if (currentCheckerQueen)
            {
                QueenMovesAnalisys(start, start, moves, false ,currentPlayerSide, enemy, null, null);
            }

        }

        private void BeatsAnalysis((int x, int y) start, (int x, int y) localStart, List<Move> moves, bool currentCheckerQueen, 
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

                        if (underMoves.Count > 1 && underMoves[underMoves.Count - 2] == localFinish) continue;

                        if (fieldArray[localFinish.y, localFinish.x] != (int)CheckersTypes.emptyField && localFinish != start) continue;
                        

                        middlePoint = ((localStart.x + localFinish.x) / 2, (localStart.y + localFinish.y) / 2);

                        bool repeat = false;
                        if (checkersForRemove.Count != 0) 
                            for (int j = 0; j < checkersForRemove.Count; j++)
                            {
                                if (checkersForRemove[j] == middlePoint)
                                {
                                    repeat = true;
                                    break;
                                }
                            }
                        if (repeat) continue;

                        if (fieldArray[middlePoint.y, middlePoint.x] != enemy.ordinary && fieldArray[middlePoint.y, middlePoint.x] != enemy.queen) continue;

                        if ((currentPlayerSide == CheckersPlayerSide.White && localFinish.y == 0) ||
                            (currentPlayerSide == CheckersPlayerSide.Black && localFinish.y == fieldSize - 1)) currentCheckerQueen = true;

                        childCount++;

                        underMoves.Add(localFinish);
                        checkersForRemove.Add(middlePoint);

                        List<(int x, int y)> nextCheckerForRemove;
                        List<(int x, int y)> nextUnderMove;

                        CloneList(out nextUnderMove, ref underMoves);

                        CloneList(out nextCheckerForRemove, ref checkersForRemove);

                        underMoves.RemoveAt(underMoves.Count - 1);
                        checkersForRemove.RemoveAt(checkersForRemove.Count - 1);


                        BeatsAnalysis(start,localFinish, moves,currentCheckerQueen,currentPlayerSide,enemy,nextCheckerForRemove,nextUnderMove);
                    }
                    catch
                    {
                        continue;
                    }
                }
                if(childCount == 0)
                {
                    Move move = new Move(start,localStart,currentCheckerQueen,checkersForRemove,underMoves);
                    moves.Add(move);
                }
            }
            else if (currentCheckerQueen)
            {
                QueenMovesAnalisys(start, localStart, moves, true ,currentPlayerSide, enemy, checkersForRemove, underMoves);
            }
        }

        private void QueenMovesAnalisys((int x, int y) start,(int x, int y) localStart, List<Move> moves,bool IsAttacking,CheckersPlayerSide currentPlayerSide, 
                                        (int ordinary, int queen) enemy, List<(int x, int y)> checkersForRemove, List<(int x, int y)> underMoves)
        {
            if (moves == null) moves = new List<Move>();
            if (checkersForRemove == null) checkersForRemove = new List<(int x, int y)>();
            if (underMoves == null) underMoves = new List<(int x, int y)>();

            Dictionary<(int, int), int> diagonalInfo = new Dictionary<(int, int), int>();
            List<(int x, int y)> toUnderMoves = new List<(int x, int y)>();
            int xDif = 1;
            int yDif = 1;
            (int x, int y) localFinish = (-1, -1);

            for(int i = 1;i <= 4;i++,xDif *= -1)
            {
                if (4 % i == 0) yDif *= -1;
                int fieldPassed = 0;
                for(int j = 1;j <= fieldSize; j++)
                {
                    localFinish = (localStart.x + j * xDif, localStart.y + j * yDif);
                    if (InBounds(localFinish.x, localFinish.y, fieldSize))
                    {
                        if(fieldArray[localFinish.y,localFinish.x] == (int)CheckersTypes.emptyField)
                        {
                            if (underMoves.Count > 0 && underMoves[underMoves.Count - 1] == localFinish)
                            {
                                int dictValue;
                                if (diagonalInfo.TryGetValue((xDif,yDif),out dictValue) == false)
                                {
                                    diagonalInfo.Add((xDif, yDif), -1);
                                    break;
                                }
                                throw new InvalidOperationException();
                            }
                            fieldPassed++;
                            toUnderMoves.Add(localFinish);
                        }
                        else if(fieldArray[localFinish.y, localFinish.x] == enemy.ordinary || fieldArray[localFinish.y, localFinish.x] == enemy.queen)
                        {
                            if (checkersForRemove.Count > 0)
                            {
                                bool repeat = false;
                                for (int n = 0; n < checkersForRemove.Count; n++) if (checkersForRemove[n] == localFinish) repeat = true;
                                if (repeat)
                                {
                                    diagonalInfo.Add((xDif, yDif), -1);
                                    break;
                                }
                            }
                            if (InBounds(localFinish.x + j * xDif, localFinish.y + j * yDif, fieldSize) && fieldArray[localFinish.y + j * yDif, localFinish.x  + j * xDif] == (int)CheckersTypes.emptyField)
                            {
                                int value;
                                if (diagonalInfo.TryGetValue((xDif, yDif), out value) == false)
                                {
                                    diagonalInfo.Add((xDif, yDif), 1);
                                    IsAttacking = true;
                                    if (toUnderMoves.Count > 0) for (int c = 0; c < toUnderMoves.Count; c++) underMoves.Add(toUnderMoves[c]);
                                    toUnderMoves.Clear();
                                    checkersForRemove.Add(localFinish);
                                    underMoves.Add((localFinish.x + j * xDif, localFinish.y + j * yDif));
                                    List<(int x, int y)> newCheckersForRemove = new List<(int x, int y)>();
                                    List<(int x, int y)> newUnderMoves = new List<(int x, int y)>();
                                    CloneList(out newCheckersForRemove, ref checkersForRemove);
                                    CloneList(out newUnderMoves, ref newUnderMoves);
                                    checkersForRemove.RemoveAt(checkersForRemove.Count - 1);
                                    underMoves.RemoveAt(underMoves.Count - 1);
                                    fieldPassed++;
                                    QueenMovesAnalisys(start, (localFinish.x + 1, localFinish.y + 1), moves, IsAttacking ,currentPlayerSide, enemy, newCheckersForRemove, newUnderMoves);
                                    break;
                                }
                                throw new InvalidOperationException();
                            }
                            if (fieldPassed > 0)
                            {
                                int dictValue;
                                if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false)
                                {
                                    diagonalInfo.Add((xDif, yDif), 0);
                                    break;
                                }
                                throw new InvalidOperationException();
                            }
                            else
                            {
                                int dictValue;
                                if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false)
                                {
                                    diagonalInfo.Add((xDif, yDif), -1);
                                    break;
                                }
                                throw new InvalidOperationException();
                            }
                        }
                        else // fieldArray[localFinish.y, localFinish.x] == friendlyChecker
                        {
                            if(fieldPassed > 0)
                            {
                                int dictValue;
                                if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false)
                                {
                                    diagonalInfo.Add((xDif, yDif), 0);
                                    break;
                                }
                                throw new InvalidOperationException();
                            }
                            else
                            {
                                int dictValue;
                                if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false)
                                {
                                    diagonalInfo.Add((xDif, yDif), -1);
                                    break;
                                }
                                throw new InvalidOperationException();
                            }
                        }
                    }
                    else
                    {
                        if (fieldPassed > 0)
                        {
                            int dictValue;
                            if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false)
                            {
                                diagonalInfo.Add((xDif, yDif), 0);
                                break;
                            }
                            throw new InvalidOperationException();
                        }
                        else
                        {
                            int dictValue;
                            if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false)
                            {
                                diagonalInfo.Add((xDif, yDif), -1);
                                break;
                            }
                            throw new InvalidOperationException();
                        }
                    }
                }
            }

            xDif = 1;
            yDif = 1;
            for (int j = 1; j <= 4; j++, xDif *= -1)
            {
                if (4 % j == 0) yDif *= -1;
                int dictValue = 0;
                if (diagonalInfo.TryGetValue((xDif, yDif), out dictValue) == false) throw new InvalidDataException();
                else
                {
                    if (dictValue == -1) break;

                    if (dictValue == 0 && !IsAttacking)
                    {
                        toUnderMoves.Clear();
                        for (int c = 1; c <= fieldSize; c++)
                        {
                            localFinish = (localStart.x + c * xDif, localStart.y + c * yDif);
                            if (InBounds(localFinish.x, localFinish.y, fieldSize))
                            {
                                underMoves.Add(localFinish);
                            }
                            else
                            {
                                Move regMove = new Move(start, underMoves[underMoves.Count - 1], true, checkersForRemove, underMoves);
                                moves.Add(regMove);
                                break;
                            }
                        }
                    }
                    else if (dictValue == 0 && IsAttacking)
                    {
                        if ((xDif, yDif) == (Math.Sign(checkersForRemove[checkersForRemove.Count - 1].x - localStart.x), Math.Sign(checkersForRemove[checkersForRemove.Count - 1].y - localStart.y)))
                        {
                            toUnderMoves.Clear();
                            for (int c = 0; c < fieldSize; c++)
                            {
                                localFinish = (localStart.x + c * xDif, localStart.y + c * yDif);
                                if (InBounds(localFinish.x, localFinish.y, fieldSize))
                                {
                                    underMoves.Add(localFinish);
                                }
                                else
                                {
                                    Move regMove = new Move(start, underMoves[underMoves.Count - 1], true, checkersForRemove, underMoves);
                                    moves.Add(regMove);
                                    break;
                                }
                            }
                        }
                        continue;
                    }
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


            MovesAnalysis(start, availableMoves, currentCheckerQueen, currentPlayerSide, (enemyOrdinary, enemyQueen));

            BeatsAnalysis(start, availableMoves, currentCheckerQueen, currentPlayerSide, (enemyOrdinary, enemyQueen));


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
            //fieldArray[1, 0] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[1, 2] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[1, 4] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[1, 6] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[3, 2] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[3, 4] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[5, 2] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[5, 4] = (int)CheckersTypes.blackOrdinary;
            //fieldArray[5, 6] = (int)CheckersTypes.blackOrdinary;

            fieldArray[3, 2] = (int)CheckersTypes.blackOrdinary;
            fieldArray[1, 6] = (int)CheckersTypes.blackOrdinary;

            fieldArray[2, 7] = (int)CheckersTypes.whiteOrdinary;


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

        private bool InBounds(int xInd,int yInd, int arraySize)
        {
            return ((xInd >= 0) && (xInd < arraySize)) && ((yInd >= 0) && (yInd < arraySize));
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

//int xDif = 1;
//int yDif = 1;
//int childCount = 0;

//List<(int x, int y)> toUnderMoves = new List<(int x, int y)>();
//(int x, int y) localFinish = (-1, -1);
//for(int i = 1; i <= 4; i++, xDif *= -1)
//{
//    if (4 % i == 0) yDif *= -1;
//    for(int j = 1; j <= fieldSize; j++) 
//    {
//        localFinish = (localStart.x + j * xDif, localStart.y + j * yDif);
//        if (InBounds(localFinish.x, localFinish.y, fieldSize))
//        {
//            if(fieldArray[localFinish.y,localFinish.x] == (int)CheckersTypes.emptyField)
//            {
//                if (underMoves.Count > 0 && underMoves[underMoves.Count - 1] == localFinish) break;
//                toUnderMoves.Add(localFinish);

//            }
//            else if(fieldArray[localFinish.y, localFinish.x] == enemy.ordinary || fieldArray[localFinish.y, localFinish.x] == enemy.queen)
//            {
                //if(checkersForRemove.Count > 0)
                //{
                //    bool repeat = false;
                //    for (int n = 0; n<checkersForRemove.Count; n++) if (checkersForRemove[n] == localFinish) repeat = true;
                //    if (repeat) break;
                //}
//                if (InBounds(localFinish.x + 1, localFinish.y + 1, fieldSize) && fieldArray[localFinish.y + 1, localFinish.x + 1] == (int) CheckersTypes.emptyField)
//{
//    if (toUnderMoves.Count > 0) for (int c = 0; c < toUnderMoves.Count; c++) underMoves.Add(toUnderMoves[c]);
//    toUnderMoves.Clear();
//    checkersForRemove.Add(localFinish);
//    underMoves.Add((localFinish.x + 1, localFinish.y + 1));
//    List<(int x, int y)> newCheckersForRemove = new List<(int x, int y)>();
//    List<(int x, int y)> newUnderMoves = new List<(int x, int y)>();
//    CloneList(out newCheckersForRemove, ref checkersForRemove);
//    CloneList(out newUnderMoves, ref newUnderMoves);
//    checkersForRemove.RemoveAt(checkersForRemove.Count - 1);
//    underMoves.RemoveAt(underMoves.Count - 1);
//    childCount++;
//    QueenMovesAnalisys(start, (localFinish.x + 1, localFinish.y + 1), moves, currentPlayerSide, enemy, newCheckersForRemove, newUnderMoves);

//    j += 1;
//}
//                else break;
//            }
//            else
//            {
//                //if (childCount > 0)
//                //{
//                //    if (toUnderMoves.Count > 0) for (int c = 0; c < toUnderMoves.Count; c++) underMoves.Add(toUnderMoves[c]);
//                //}
//                //else
//                //{
//                //    toUnderMoves.Clear();
//                //}
//                break;
//            }
//        }
//        else
//        {
//            //if (toUnderMoves.Count > 0)
//            //{
//            //    if (toUnderMoves.Count > 0) for (int c = 0; c < toUnderMoves.Count; c++) underMoves.Add(toUnderMoves[c]);
//            //}
//            //else
//            //{
//            //    toUnderMoves.Clear();
//            //}
//            break;
//        }

//    }
//    if (toUnderMoves.Count > 0)
//    {
//        for (int c = 0; c < toUnderMoves.Count; c++) underMoves.Add(toUnderMoves[c]);
//        Move move = new Move(start, localStart, true, checkersForRemove, underMoves);
//        moves.Add(move);
//    }
//}