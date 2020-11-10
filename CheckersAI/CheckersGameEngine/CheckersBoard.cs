using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace CheckersAI.CheckersGameEngine
{
    public enum CType 
    {
        EmptyField,
        Beaten,
        WhiteOrdinary,
        WhiteQueen,
        BlackOrdinary,
        BlackQueen
    }

    public enum BoardSize
    {
        Small = 6,
        Medium = 8,
        Large = 10
    }

    
    public class Board
    {
        public static (sbyte, sbyte)[] directions =
        {
            (-1,-1),
            (1,1),
            (-1,1),
            (1,-1)

        };

        private CType[,] field;
        public readonly sbyte fieldSize;
        private CPlayer whitePlayer;
        private CPlayer blackPlayer;
        private bool IsMaximizerMove = true;

        public Board(BoardSize size,bool whiteAI, bool blackAI)
        {
            fieldSize = (sbyte)size;
            field = new CType[fieldSize, fieldSize];
            field = fieldFill(fieldSize);
            whitePlayer = whiteAI ? (CPlayer)(new AI()) : (CPlayer)(new Player());
            blackPlayer = blackAI ? (CPlayer)(new AI()) : (CPlayer)(new Player());

        }
        public Board(CType[,] board,Board creator)
        {
            field = board;
            fieldSize = creator.fieldSize;
            whitePlayer = creator.whitePlayer;
            blackPlayer = creator.blackPlayer;

        }
        public CType[,] fieldFill(sbyte fieldSize)
        {
            CType[,] resultField = new CType[fieldSize, fieldSize];

            //bool oddRow;
            //int teamDistance = (fieldSize - 2) / 2;
            //CType checkers = CType.BlackOrdinary;
            //for (int i = 0; i < fieldSize; i++)
            //{
            //    if (i == teamDistance)
            //    {
            //        i++;
            //        checkers = CType.WhiteOrdinary;
            //        continue;
            //    }
            //    oddRow = (i % 2 != 0);
            //    for (int j = 0; j < fieldSize; j++)
            //    {
            //        if (oddRow)
            //        {
            //            resultField[i, j++] = checkers;
            //            resultField[i, j] = CType.EmptyField;

            //        }
            //        else
            //        {
            //            resultField[i, j] = CType.EmptyField;
            //            resultField[i, ++j] = checkers;
            //        }

            //    }

            //}
            for(int i = 0; i < fieldSize; i++)
            {
                for(int j = 0; j < fieldSize; j++)
                {
                    resultField[i, j] = CType.EmptyField;
                }
            }

            resultField[4, 3] = CType.WhiteOrdinary;
            resultField[3, 2] = CType.BlackOrdinary;
            resultField[3, 4] = CType.BlackQueen;
            resultField[1, 0] = CType.BlackQueen;
            resultField[1, 6] = CType.BlackQueen;
            resultField[1, 4] = CType.BlackQueen;



            return resultField;
        }

        public List<CType[,]> GetAllPossiblePosition(bool ForAllCheckers, bool IsMaximizingPlayer, (sbyte y, sbyte x) checkCoordinates)
        {
            if (!ForAllCheckers && !IsBound(checkCoordinates.y,checkCoordinates.x,fieldSize)) throw new ArgumentException();

            List<CType[,]> result = new List<CType[,]>();
            (CType ordinary, CType queen) enemyCheck = IsMaximizingPlayer ? (CType.BlackOrdinary, CType.BlackQueen) : (CType.WhiteOrdinary, CType.WhiteQueen);
            (CType ordinary, CType queen) yourCheck = IsMaximizingPlayer ? (CType.WhiteOrdinary, CType.WhiteQueen) : (CType.BlackOrdinary, CType.BlackQueen);

            if (!ForAllCheckers)
            {
                if (field[checkCoordinates.y, checkCoordinates.x] == enemyCheck.ordinary || field[checkCoordinates.y, checkCoordinates.x] == enemyCheck.queen ||
                    field[checkCoordinates.y, checkCoordinates.x] == CType.EmptyField) return null;

                result.AddRange(GetMoves(checkCoordinates, enemyCheck, yourCheck,field));
                result.AddRange(GetAttacks(checkCoordinates, enemyCheck, yourCheck,field));
            }
            else if (ForAllCheckers)
            {
                checkCoordinates = (-1, -1);
                for(int i = 0;i < fieldSize; i++)
                {
                    for(int j = 0; j < fieldSize; j++)
                    {
                        if (field[i,j] == enemyCheck.ordinary || field[i, j] == enemyCheck.queen ||
                            field[i, j] == CType.EmptyField) continue;

                        checkCoordinates = ((sbyte)i, (sbyte)j);
                        result.AddRange(GetMoves(checkCoordinates, enemyCheck, yourCheck,field));
                        result.AddRange(GetAttacks(checkCoordinates, enemyCheck, yourCheck,field));
                    }
                }
            }

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 0; j < fieldSize; j++)
                {
                    for (int c = 0; c < fieldSize; c++)
                    {
                        Console.Write((int)(result[i][j, c]));
                    }
                    Console.WriteLine();
                }
                Console.WriteLine('\n');
            }

            return result;
        }
        
        private List<CType[,]> GetMoves((sbyte y, sbyte x) checkCoordinates,(CType odrinary,CType queen) enemyCheck, (CType odrinary, CType queen) yourCheck,
            CType[,] currentField)
        {
            List<CType[,]> result = new List<CType[,]>();
            if(currentField[checkCoordinates.y,checkCoordinates.x] == yourCheck.odrinary)
            {
                sbyte yDist = yourCheck.odrinary == CType.WhiteOrdinary ? (sbyte)-1 : (sbyte)1;
                for (int i = 0, xDist = 1; i < 2; i++, xDist *= -1)
                {
                    if(IsBound(checkCoordinates.x + xDist, checkCoordinates.y + yDist, fieldSize))
                    {
                        if(currentField[checkCoordinates.y + yDist, checkCoordinates.x + xDist] == CType.EmptyField)
                        {
                            int queenField = yourCheck.odrinary == CType.WhiteOrdinary ? 0 : fieldSize - 1;
                            CType[,] resField = (CType[,])currentField.Clone();
                            if (checkCoordinates.y == queenField)
                            {
                                CType queen = yourCheck.odrinary == CType.WhiteOrdinary ? CType.WhiteQueen : CType.BlackQueen;
                                resField[checkCoordinates.y + yDist, checkCoordinates.x + xDist] = queen;
                            }
                            else
                            {
                                resField[checkCoordinates.y + yDist, checkCoordinates.x + xDist] = resField[checkCoordinates.y, checkCoordinates.x];
                            }
                            resField[checkCoordinates.y, checkCoordinates.x] = CType.EmptyField;
                            result.Add(resField);
                        }
                    }
                }
            }
            else if(field[checkCoordinates.y, checkCoordinates.x] == yourCheck.queen)
            {
                (sbyte y, sbyte x) finishCoord = (-1, -1);
                foreach((sbyte xDir,sbyte yDir) dir in directions)
                {
                    for(int i = 1; i <= fieldSize; i++)
                    {
                        finishCoord = ((sbyte)(checkCoordinates.y + i * dir.yDir), (sbyte)(checkCoordinates.x + i * dir.xDir));
                        if (!IsBound(finishCoord.x, finishCoord.y, fieldSize)) break;

                        if (currentField[finishCoord.y, finishCoord.x] == CType.EmptyField)
                        {
                            CType[,] resField = (CType[,])currentField.Clone();
                            resField[finishCoord.y, finishCoord.x] = resField[checkCoordinates.y, checkCoordinates.x];
                            resField[checkCoordinates.y, checkCoordinates.x] = CType.EmptyField;
                            result.Add(resField);
                        }
                        else if (currentField[finishCoord.y, finishCoord.x] != CType.EmptyField) break;
                    }
                }
            }
            return result;
        }

        private List<CType[,]> GetAttacks((sbyte y, sbyte x) checkCoordinates, (CType odrinary,CType queen) enemyCheck, (CType odrinary,CType queen) yourCheck, 
            CType[,] currentField)
        {
            List<CType[,]> result = new List<CType[,]>();
            (sbyte y, sbyte x) finishCoord = (-1, -1);
            if (currentField[checkCoordinates.y, checkCoordinates.x] == yourCheck.odrinary)
            {
                foreach((sbyte xDir,sbyte yDir) dir in directions)
                {
                    finishCoord = ((sbyte)(checkCoordinates.y + 2 * dir.yDir), (sbyte)(checkCoordinates.x + 2 * dir.xDir));
                    if (!IsBound(finishCoord.x, finishCoord.y, fieldSize)) continue;
                    (sbyte y, sbyte x) middlePoint = ((sbyte)((finishCoord.y + checkCoordinates.y) * 0.5), 
                                                      (sbyte)((finishCoord.x + checkCoordinates.x) * 0.5));

                    if (currentField[finishCoord.y, finishCoord.x] != CType.EmptyField || (currentField[middlePoint.y, middlePoint.x] != enemyCheck.odrinary &&
                                                                                           currentField[middlePoint.y, middlePoint.x] != enemyCheck.queen))
                    {
                        continue;
                    }

                    if (currentField[middlePoint.y, middlePoint.x] == CType.Beaten) continue;

                    CType[,] resField = (CType[,])currentField.Clone();
                    resField[middlePoint.y, middlePoint.x] = CType.Beaten;

                    int queenField = yourCheck.odrinary == CType.WhiteOrdinary ? 0 : fieldSize - 1;
                    if (checkCoordinates.y == queenField)
                    {
                        CType queen = yourCheck.odrinary == CType.WhiteOrdinary ? CType.WhiteQueen : CType.BlackQueen;
                        resField[finishCoord.y, finishCoord.x] = queen;
                    }
                    else
                    {
                        resField[finishCoord.y, finishCoord.x] = resField[checkCoordinates.y, checkCoordinates.x];
                    }
                    resField[checkCoordinates.y, checkCoordinates.x] = CType.EmptyField;

                    List<CType[,]> listFromRecursion = new List<CType[,]>();
                    listFromRecursion = GetAttacks(finishCoord, enemyCheck, yourCheck, resField);
                    if (listFromRecursion.Count == 0) result.Add(resField);
                    else result.AddRange(listFromRecursion);
                }
            }
            else if (currentField[checkCoordinates.y, checkCoordinates.x] == yourCheck.queen)
            {
                // TODO : this shit must me done today
            }

            return result;
        }

        public static bool IsBound(int x,int y,int fieldSize)
        {
            return (x >= 0 && x < fieldSize) && (y >= 0 && y < fieldSize);
        }

        private abstract class CPlayer 
        {
            public abstract bool DoMove();
        }

        private class Player : CPlayer
        {
            public override bool DoMove()
            {
                throw new NotImplementedException();
            }
        }

        private class AI : CPlayer
        {
            public override bool DoMove()
            {
                throw new NotImplementedException();
            }
        }

    }
}