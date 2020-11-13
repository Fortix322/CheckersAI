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
        public readonly bool beatRule;
        private CPlayer whitePlayer;
        private CPlayer blackPlayer;
        private bool IsMaximizerMove = true;

        public Board(BoardSize size,bool whiteAI, bool blackAI,bool beatRule)
        {
            this.beatRule = beatRule;
            fieldSize = (sbyte)size;
            field = new CType[fieldSize, fieldSize];
            field = fieldFill(fieldSize);
            whitePlayer = whiteAI ? (CPlayer)(new AI(fieldSize,true,beatRule)) : (CPlayer)(new Player());
            blackPlayer = blackAI ? (CPlayer)(new AI(fieldSize,false,beatRule)) : (CPlayer)(new Player());

        }
        
        public void GameLoop()
        {
            while (IsWinner(field,fieldSize,beatRule) == CType.EmptyField)
            {
                CPlayer currentPlayer = IsMaximizerMove ? whitePlayer : blackPlayer;
                CType[,] pos = currentPlayer.DoMove(GetAllPossiblePosition(beatRule, IsMaximizerMove, field, fieldSize));
                field = pos;
                IsMaximizerMove = !IsMaximizerMove;
            }

            int a = 0;
            foreach(CType check in field)
            {
                Console.Write((int)check);
                a++;
                if (a % 8 == 0) Console.WriteLine();
            }
            Console.ReadLine();
        }


        public CType[,] fieldFill(sbyte fieldSize)
        {
            CType[,] resultField = new CType[fieldSize, fieldSize];

            bool oddRow;
            int teamDistance = (fieldSize - 2) / 2;
            CType checkers = CType.BlackOrdinary;
            for (int i = 0; i < fieldSize; i++)
            {
                if (i == teamDistance)
                {
                    i++;
                    checkers = CType.WhiteOrdinary;
                    continue;
                }
                oddRow = (i % 2 != 0);
                for (int j = 0; j < fieldSize; j++)
                {
                    if (oddRow)
                    {
                        resultField[i, j++] = checkers;
                        resultField[i, j] = CType.EmptyField;

                    }
                    else
                    {
                        resultField[i, j] = CType.EmptyField;
                        resultField[i, ++j] = checkers;
                    }

                }

            }
            
            return resultField;
        }

        public static List<CType[,]> GetAllPossiblePosition(bool beatRule, bool IsMaximizingPlayer ,CType[,] currentField,sbyte currentFieldSize)
        {
            List<CType[,]> result = new List<CType[,]>();
            (CType ordinary, CType queen) enemyCheck = IsMaximizingPlayer ? (CType.BlackOrdinary, CType.BlackQueen) : (CType.WhiteOrdinary, CType.WhiteQueen);
            (CType ordinary, CType queen) yourCheck = IsMaximizingPlayer ? (CType.WhiteOrdinary, CType.WhiteQueen) : (CType.BlackOrdinary, CType.BlackQueen);

            (sbyte y,sbyte x)checkCoordinates = (-1, -1);
            for(int i = 0;i < currentFieldSize; i++)
            {
                for(int j = 0; j < currentFieldSize; j++)
                {
                    if (currentField[i,j] == enemyCheck.ordinary || currentField[i, j] == enemyCheck.queen ||
                        currentField[i, j] == CType.EmptyField) continue;

                    checkCoordinates = ((sbyte)i, (sbyte)j);
                    int movesCount = result.Count;
                    result.AddRange(GetAttacks(checkCoordinates, enemyCheck, yourCheck, currentField, currentFieldSize));

                    if (!beatRule || movesCount.Equals(result.Count))
                        result.AddRange(GetMoves(checkCoordinates, enemyCheck, yourCheck, currentField, currentFieldSize));
                }
            }
           

            for (int i = 0; i < result.Count; i++)
            {
                for (int j = 0; j < currentFieldSize; j++)
                {
                    for (int c = 0; c < currentFieldSize; c++)
                    {
                        if (result[i][j, c] == CType.Beaten) result[i][j, c] = CType.EmptyField;
                    }
                }
            }

            return result;
        }
        
        private static List<CType[,]> GetMoves((sbyte y, sbyte x) checkCoordinates,(CType odrinary,CType queen) enemyCheck, (CType odrinary, CType queen) yourCheck,
            CType[,] currentField,sbyte currentFieldSize)
        {
            List<CType[,]> result = new List<CType[,]>();
            if(currentField[checkCoordinates.y,checkCoordinates.x] == yourCheck.odrinary)
            {
                sbyte yDist = yourCheck.odrinary == CType.WhiteOrdinary ? (sbyte)-1 : (sbyte)1;
                for (int i = 0, xDist = 1; i < 2; i++, xDist *= -1)
                {
                    if(IsBound(checkCoordinates.x + xDist, checkCoordinates.y + yDist, currentFieldSize))
                    {
                        if(currentField[checkCoordinates.y + yDist, checkCoordinates.x + xDist] == CType.EmptyField)
                        {
                            int queenField = yourCheck.odrinary == CType.WhiteOrdinary ? 0 : currentFieldSize - 1;
                            CType[,] resField = (CType[,])currentField.Clone();
                            if (checkCoordinates.y + yDist == queenField)
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
            else if(currentField[checkCoordinates.y, checkCoordinates.x] == yourCheck.queen)
            {
                (sbyte y, sbyte x) finishCoord = (-1, -1);
                foreach((sbyte xDir,sbyte yDir) dir in directions)
                {
                    for(int i = 1; i <= currentFieldSize; i++)
                    {
                        finishCoord = ((sbyte)(checkCoordinates.y + i * dir.yDir), (sbyte)(checkCoordinates.x + i * dir.xDir));
                        if (!IsBound(finishCoord.x, finishCoord.y, currentFieldSize)) break;

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

        private static List<CType[,]> GetAttacks((sbyte y, sbyte x) checkCoordinates, (CType odrinary,CType queen) enemyCheck, (CType odrinary,CType queen) yourCheck, 
            CType[,] currentField,sbyte currentFieldSize)
        {
            List<CType[,]> result = new List<CType[,]>();
            (sbyte y, sbyte x) finishCoord = (-1, -1);
            if (currentField[checkCoordinates.y, checkCoordinates.x] == yourCheck.odrinary)
            {
                foreach((sbyte xDir,sbyte yDir) dir in directions)
                {
                    finishCoord = ((sbyte)(checkCoordinates.y + 2 * dir.yDir), (sbyte)(checkCoordinates.x + 2 * dir.xDir));
                    if (!IsBound(finishCoord.x, finishCoord.y, currentFieldSize)) continue;
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

                    int queenField = yourCheck.odrinary == CType.WhiteOrdinary ? 0 : currentFieldSize - 1;
                    if (finishCoord.y == queenField)
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
                    listFromRecursion = GetAttacks(finishCoord, enemyCheck, yourCheck, resField, currentFieldSize);
                    if (listFromRecursion.Count == 0) result.Add(resField);
                    else result.AddRange(listFromRecursion);
                }
            }
            else if (currentField[checkCoordinates.y, checkCoordinates.x] == yourCheck.queen)
            {
                foreach((sbyte xDir,sbyte yDir) dir in directions)
                {
                    for (int i = 1; i <= currentFieldSize; i++)
                    {
                        finishCoord = ((sbyte)(checkCoordinates.y + i * dir.yDir), (sbyte)(checkCoordinates.x + i * dir.xDir));
                        if (!IsBound(finishCoord.x, finishCoord.y, currentFieldSize) ) break;
                        if (currentField[finishCoord.y, finishCoord.x] == CType.Beaten) break;

                        if(currentField[finishCoord.y,finishCoord.x] == enemyCheck.odrinary || currentField[finishCoord.y, finishCoord.x] == enemyCheck.queen)
                        {
                            if (!IsBound(finishCoord.y + dir.yDir, finishCoord.x + dir.xDir, currentFieldSize)) break;
                            (sbyte y, sbyte x) newFinish = ((sbyte)(finishCoord.y + dir.yDir), (sbyte)(finishCoord.x + dir.xDir));

                            CType[,] resField = (CType[,])currentField.Clone();
                            resField[finishCoord.y, finishCoord.x] = CType.Beaten;

                            resField[newFinish.y, newFinish.x] = resField[checkCoordinates.y, checkCoordinates.x];
                            resField[checkCoordinates.y, checkCoordinates.x] = CType.EmptyField;

                            List<CType[,]> listFromRecursion = new List<CType[,]>();
                            listFromRecursion = GetAttacks(newFinish, enemyCheck, yourCheck, resField, currentFieldSize);
                            if (listFromRecursion.Count == 0)
                            {
                                result.Add(resField);
                                int c = 1;
                                while (IsBound(newFinish.x + c * dir.xDir, newFinish.y + c * dir.yDir, currentFieldSize) &&
                                      resField[newFinish.y + c * dir.yDir, newFinish.x + c * dir.xDir] == CType.EmptyField)
                                {
                                    CType[,] field = (CType[,])resField.Clone();
                                    field[newFinish.y + c * dir.yDir, newFinish.x + c * dir.xDir] = field[newFinish.y, newFinish.x];
                                    field[newFinish.y, newFinish.x] = CType.EmptyField;
                                    result.Add(field);
                                    c++;
                                }
                                break;
                            }
                            else
                            {
                                result.AddRange(listFromRecursion);
                                break;
                            }
                        } 
                    }
                }
            }

            return result;
        }


        public static bool IsBound(int x,int y,int fieldSize)
        {
            return (x >= 0 && x < fieldSize) && (y >= 0 && y < fieldSize);
        }

        public static CType IsWinner(CType[,] pos,sbyte fieldSize,bool beatRule)
        {
            int blackCount = 0;
            int whiteCount = 0;
            if (GetAllPossiblePosition(beatRule, true, pos, fieldSize).Count == 0) return CType.BlackOrdinary;
            else if(GetAllPossiblePosition(beatRule, false, pos, fieldSize).Count == 0) return CType.WhiteOrdinary;

            for (int i = 0; i < fieldSize; i++)
            {
                for (int j = 0; j < fieldSize; j++)
                {
                    if (pos[i, j] == CType.BlackOrdinary || pos[i, j] == CType.BlackQueen) blackCount++;
                    else if (pos[i, j] == CType.WhiteOrdinary || pos[i, j] == CType.WhiteQueen) whiteCount++;
                }
            }
            if (blackCount == 0) return CType.WhiteOrdinary;
            else if (whiteCount == 0) return CType.BlackOrdinary;
            else return CType.EmptyField;
        }

        private abstract class CPlayer 
        {

            public abstract CType[,] DoMove(List<CType[,]> availableMoves);
        }

        private class Player : CPlayer
        {
           

            public Player()
            {
                Console.WriteLine("Created new Player");
            }
            public override CType[,] DoMove(List<CType[,]> availableMoves)
            {
                throw new NotImplementedException();
            }
        }

        private class AI : CPlayer
        {
            private static readonly (sbyte max, sbyte min) evaluations = (sbyte.MaxValue, sbyte.MinValue);
            private readonly sbyte fieldSize;
            private readonly bool IsBeatRule;
            private readonly bool IsMaxPlayer;
            public AI(sbyte size,bool IsMaximizerPlayer,bool BeatRule)
            {
                Console.WriteLine("Created new AI");
                fieldSize = size;
                IsMaxPlayer = IsMaximizerPlayer;
                IsBeatRule = BeatRule;
            }
            public override CType[,] DoMove(List<CType[,]> availableMoves)
            {
                (CType[,] pos, int eval) bestMove = (availableMoves[0], -1);
                if (IsMaxPlayer) bestMove.eval = evaluations.min;
                else bestMove.eval = evaluations.max;

                foreach (CType[,] pos in availableMoves)
                {
                    int evaluation = Minimax(pos,7,IsMaxPlayer,evaluations.min,evaluations.max);
                    if (IsMaxPlayer)
                    {
                        if(evaluation > bestMove.eval)
                        {
                            bestMove.eval = evaluation;
                            bestMove.pos = pos;
                        }
                    }
                    else
                    {
                        if (evaluation < bestMove.eval)
                        {
                            bestMove.eval = evaluation;
                            bestMove.pos = pos;
                        }
                    }
                }

                int a = 0;
                foreach (CType check in bestMove.pos)
                {
                    Console.Write((int)check);
                    a++;
                    if (a % 8 == 0) Console.WriteLine();
                }
                Console.WriteLine();
                return bestMove.pos;
            }

            private int Minimax(CType[,] pos,int depth,bool IsMaximizingPlayer,int alpha,int beta)
            {
                if(depth == 0 || IsWinner(pos,fieldSize, IsBeatRule) != CType.EmptyField)
                {
                    return StaticEvaluation(pos, fieldSize);
                }
                else if (IsMaximizingPlayer)
                {
                    int MaxEval = evaluations.min;

                    List<CType[,]> childPosList = GetAllPossiblePosition(IsBeatRule, IsMaximizingPlayer, pos, fieldSize);
                    if (childPosList.Count > 0)
                    {
                        foreach(CType[,] childPos in childPosList)
                        {
                            int NextPosEval = Minimax(childPos, depth - 1, false, alpha, beta);
                            MaxEval = Math.Max(MaxEval, NextPosEval);
                            alpha = Math.Max(alpha, NextPosEval);
                            if (beta <= alpha) break;
                        }
                        return MaxEval;
                    }
                    else throw new ArgumentException();
                }
                else if (!IsMaximizingPlayer)
                {
                    int MinEval = evaluations.max;

                    List<CType[,]> childPosList = GetAllPossiblePosition(IsBeatRule, IsMaximizingPlayer, pos, fieldSize);
                    if (childPosList.Count > 0)
                    {
                        foreach (CType[,] childPos in childPosList)
                        {
                            int NextPosEval = Minimax(childPos, depth - 1, true, alpha, beta);
                            MinEval = Math.Min(MinEval, NextPosEval);
                            beta = Math.Min(beta, NextPosEval);
                            if (beta <= alpha) break;
                        }
                        return MinEval;
                    }
                    else throw new ArgumentException();
                }

                throw new ArgumentNullException();
            }

            private int StaticEvaluation(CType[,] pos,sbyte fieldSize)
            {
                int whiteCount = 0;
                int blackCount = 0;
                foreach(CType check in pos)
                {
                    switch (check)
                    {
                        case CType.BlackOrdinary:
                            blackCount++;
                            break;

                        case CType.WhiteOrdinary:
                            whiteCount++;
                            break;

                        case CType.BlackQueen:
                            blackCount += 2;
                            break;

                        case CType.WhiteQueen:
                            whiteCount += 2;
                            break;
                    }
                }

                return whiteCount - blackCount;
            }

        }
    }
}