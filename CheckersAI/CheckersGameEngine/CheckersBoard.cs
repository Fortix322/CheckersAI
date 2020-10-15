using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace CheckersAI.CheckersGameEngine
{
    internal class CheckersBoard : IPrintable
    {
        /*** FIELDS ***/

        // PRIVATE FIELDS

        private List<Checker> _checkers = new List<Checker>();

        private readonly (sbyte x, sbyte y) _fieldsize;

        private CheckersPlayer[] _players;

        private CheckersPlayer _currentPlayer;
        private CheckersPlayer _nextPlayer;

        private CheckersRulesEnum _rulesType;

        private sbyte[,] _field;

        /*** METHODS ***/

        // CONSTUCTORS

        public CheckersBoard(CheckersRulesEnum rules, CheckersPlayer[] playersArray)
        {
            _rulesType = rules;

            _fieldsize = ((sbyte)_rulesType, (sbyte)_rulesType);

            _field = new sbyte[_fieldsize.y, _fieldsize.x];

            _players = new CheckersPlayer[CheckersGameController.amountOfPlayers];

            for (int i = 0; i < CheckersGameController.amountOfPlayers; i++)
            {
                _players[i] = (CheckersPlayer)playersArray[i];
                _players[i].boardOwner = this;
                if (_players[i].checkerSide == CheckerSide.white) _currentPlayer = _players[i];
                else _nextPlayer = _players[i];
            }


        }

        // PUBLIC METHODS

        public bool CanMove(Coordinates startPos, Coordinates finishPos)
        {
            try
            {
                if (_field[finishPos.y, finishPos.x] != (sbyte)CheckerSide.none) return false;

                int yPosInterval = (CheckerSide)_field[startPos.y, startPos.x] == CheckerSide.black ? 1 : -1;

                if (finishPos.y - startPos.y == yPosInterval && Math.Abs(finishPos.x - startPos.x) == 1)
                {
                    return true;
                }
                return false;
            }
            catch (IndexOutOfRangeException ex)
            {
                return false;
            }
        }

        public bool CanBeat(Coordinates startPos, Coordinates finishPos)
        {
            try
            {
                if (_field[finishPos.y, finishPos.x] != (sbyte)CheckerSide.none) return false;

                int beatInterval = 2;

                Coordinates middlePoint = new Coordinates((sbyte)(0.5 * (startPos.y + finishPos.y)), (sbyte)(0.5 * (startPos.x + finishPos.x)));

                if (Math.Abs(finishPos.y - startPos.y) == beatInterval && Math.Abs(finishPos.x - startPos.x) == beatInterval)
                {
                    CheckerSide enemySide = _nextPlayer.checkerSide == CheckerSide.black ? CheckerSide.white : CheckerSide.black;
                    if (_field[middlePoint.y, middlePoint.x] == (sbyte)enemySide)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (IndexOutOfRangeException ex)
            {
                return false;
            }
        }

        public void Print()
        {
            Console.Write("  ");
            for (int i = 0; i < _fieldsize.x; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(i);
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.ResetColor();
            for (int i = 0; i < _fieldsize.y; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{(char)('A' + i)} ");
                Console.ResetColor();

                for (int j = 0; j < _fieldsize.x; j++) Console.Write(_field[i, j]);
                Console.Write('\n');
            }
        }

        public CheckersPlayer GetCurrentPlayer()
        {
            return _currentPlayer;
        }

        /// <summary>
        /// Returns array of players
        /// </summary>
        /// <returns> CheckersPlayer array with 2 players </returns>
        public CheckersPlayer[] GetPlayers()
        {
            CheckersPlayer[] players = new CheckersPlayer[CheckersGameController.amountOfPlayers];
            for (int i = 0; i < players.Length; i++) players[i] = (CheckersPlayer)_players[i].Clone();
            return players;
        }

        public CheckerSide DoMove(CheckersPlayer caller, string coordinates)
        {
            Exception WrongMoveException = new Exception("Wrong move");
            Exception WrongPlayerException = new Exception("Not your queue or player don't play");
            Coordinates startPos = new Coordinates(-1, -1);
            Coordinates finishPos = new Coordinates(-1, -1);
            Checker currentChecker;
            try
            {
                if (!ReferenceEquals(caller, _currentPlayer)) throw WrongMoveException;

                ParseCoordinates(coordinates, ref startPos, ref finishPos);

                if((currentChecker = FindChecker(startPos)) != null)
                {
                    if (_currentPlayer.checkerSide == currentChecker.checkerSide)
                    {
                        Move(startPos, finishPos, currentChecker);

                        CheckerSide winSide = CheckWin();
                        if (winSide != CheckerSide.none)
                        {
                            return winSide;
                        }
                        
                        MovesDelete();

                        CheckersListUpdate(currentChecker, finishPos);

                        MovesUpdate();

                        QueueUpdate();

                        return CheckerSide.none;
                    }
                   

                }

                throw WrongMoveException;

            }
            catch (IndexOutOfRangeException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public void StartGame()
        {
            FieldPrep();
            CheckerPrep(_checkers);
            MovesUpdate();

        }

        // PRIVATE METHODS

        /// <summary>
        ///     Moves checkers on board
        /// </summary>
        /// <return> Return side whick must move next </return>
        ///  <param name = "caller"> Object which does func call </param>
        ///  <param name = "coordinates"> What and where checker must be placed </param>
        private CheckerSide Move(Coordinates startPos, Coordinates finishPos,Checker currentChecker)
        {
            Exception WrongMoveException = new Exception("Wrong move");
            Exception WrongPlayerException = new Exception("Not your queue or player don't play");

            try
            {
                Coordinates dictValueCoord = new Coordinates(-1, -1);
                if (currentChecker.availableMoves.TryGetValue(finishPos, out dictValueCoord) &&
                    startPos.Equals(dictValueCoord))
                {
                    _field[finishPos.y, finishPos.x] = (sbyte)_currentPlayer.checkerSide;
                    _field[startPos.y, startPos.x] = (sbyte)CheckerSide.none;

                    return _nextPlayer.checkerSide;
                }
                else if (currentChecker.availableBeat.TryGetValue(finishPos, out dictValueCoord))
                {
                    if (startPos.Equals(dictValueCoord))
                    {
                        Coordinates middlePoint = new Coordinates((sbyte)(0.5 * (startPos.y + finishPos.y)), (sbyte)(0.5 * (startPos.x + finishPos.x)));

                        _field[finishPos.y, finishPos.x] = (sbyte)_currentPlayer.checkerSide;
                        _field[middlePoint.y, middlePoint.x] = (sbyte)CheckerSide.none;
                        _field[startPos.y, startPos.x] = (sbyte)CheckerSide.none;

                        CheckersListUpdate(middlePoint);
                        return _nextPlayer.checkerSide;
                    }
                    else
                    {
                        Coordinates localStartPos;
                        Coordinates localFinishPos = finishPos;
                        Stack<Coordinates> finishCoordinates = new Stack<Coordinates>();
                        while (currentChecker.availableBeat.TryGetValue(localFinishPos, out localStartPos))
                        {
                            if (localStartPos.Equals(startPos))
                            {
                                while(currentChecker.availableBeat.TryGetValue(localFinishPos, out localStartPos))
                                {
                                    Coordinates middlePoint = new Coordinates((sbyte)(0.5 * (localStartPos.y + localFinishPos.y)), (sbyte)(0.5 * (localStartPos.x + localFinishPos.x)));

                                    
                                    _field[middlePoint.y, middlePoint.x] = (sbyte)CheckerSide.none;
                                    _field[localStartPos.y, localStartPos.x] = (sbyte)CheckerSide.none;

                                    CheckersListUpdate(middlePoint);
                                    if (localFinishPos.Equals(finishPos))
                                    {
                                        _field[localFinishPos.y, localFinishPos.x] = (sbyte)_currentPlayer.checkerSide;
                                        return _nextPlayer.checkerSide;

                                    }
                                    localFinishPos = finishCoordinates.Pop();
                                }
                            }
                            finishCoordinates.Push(localFinishPos);
                            localFinishPos = localStartPos;
                        }
                        throw WrongMoveException;

                    }

                }
                else throw WrongMoveException;

                throw WrongMoveException;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw WrongMoveException;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private void MovesUpdate()
        {
            foreach(Checker ch in _checkers)
            {
                sbyte directionY = ch.checkerSide == CheckerSide.black ? (sbyte)1 : (sbyte)-1;
                Coordinates finalPos = new Coordinates(-1, -1);

                finalPos.y = (sbyte)(ch.checkerCoordinates.y + directionY);
                for (int i = 0,directionX = 1; i < 2; i++,directionX *= -1)
                {
                    finalPos.x = (sbyte)(ch.checkerCoordinates.x + directionX);
                    if (CanMove(ch.checkerCoordinates, finalPos))
                    {
                        ch.availableMoves.Add(finalPos, ch.checkerCoordinates);
                    }
                }

                FindAvailableBeats(ch.checkerCoordinates, ch.availableBeat);
            }
        }

        private void MovesUpdate(Checker checker)
        {
            sbyte directionY = checker.checkerSide == CheckerSide.black ? (sbyte)1 : (sbyte)-1;
            Coordinates finalPos = new Coordinates(-1, -1);

            finalPos.y = (sbyte)(checker.checkerCoordinates.y + directionY);
            for (int i = 0, directionX = 1; i < 2; i++, directionX *= -1)
            {
                finalPos.x = (sbyte)(checker.checkerCoordinates.x + directionX);
                if (CanMove(checker.checkerCoordinates, finalPos))
                {
                    checker.availableMoves.Add(finalPos, checker.checkerCoordinates);
                }
            }


            FindAvailableBeats(checker.checkerCoordinates, checker.availableBeat);
        }

        private void FindAvailableBeats(Coordinates startPos,Dictionary<Coordinates,Coordinates> availableBeat)
        {
            sbyte beatInterval = 2;
            Coordinates finalPos = new Coordinates(-1, -1);

            for (int i = 0, directionX = 2; i < 4; i++, directionX *= -1)
            {
                if (i % 2 == 0) beatInterval *= -1;
                finalPos.y = (sbyte)(startPos.y + beatInterval);
                finalPos.x = (sbyte)(startPos.x + directionX);
                if (CanBeat(startPos, finalPos))
                {
                    Coordinates oldStartPos;
                    if (availableBeat.TryGetValue(startPos, out oldStartPos) == false) oldStartPos = new Coordinates(-1, -1);
                    if (!(finalPos.Equals(oldStartPos)))
                    {
                        availableBeat.Add(finalPos, startPos);
                        FindAvailableBeats(finalPos, availableBeat);
                    }
                }
            }
        }
       
        private void MovesDelete()
        {
            foreach(Checker ch in _checkers)
            {
                ch.availableMoves.Clear();
                ch.availableBeat.Clear();
            }
            
        }
        
        private Checker FindChecker(Coordinates coordinates)
        {
            foreach(Checker ch in _checkers)
            {
                if(ch.checkerCoordinates.x == coordinates.x && ch.checkerCoordinates.y == coordinates.y)
                {
                    return ch;
                }
            }
            return null;
        }

        private CheckerSide CheckWin()
        {
            int blackCount = 0;
            int whiteCount = 0;

            foreach(Checker checker in _checkers)
            {
                if (checker.checkerSide == CheckerSide.black) blackCount++;
                else whiteCount++;
            }

            if (blackCount == 0) return CheckerSide.white;
            else if (whiteCount == 0) return CheckerSide.black;
            else return CheckerSide.none;
        }

        // replace checker in list;
        private bool CheckersListUpdate(Coordinates startPos, Coordinates finishPos)
        {
            Checker checker = FindChecker(startPos);
            if (checker != null)
            {
                checker.checkerCoordinates = finishPos;
                return true;
            }
            return false;
        }

        private bool CheckersListUpdate(Checker checker,Coordinates finishPos)
        {
   
            if (checker != null)
            {
                checker.checkerCoordinates = finishPos;
                return true;
            }
            return false;
        }
        // delete checker from list;
        private bool CheckersListUpdate(Coordinates pos)
        {
            Checker checker = FindChecker(pos);
            if (checker != null)
                if(_checkers.Remove(checker)) return true;
            
            return false;
        }

        private bool CheckersListUpdate(Checker checker)
        {
            if (checker != null)
                if (_checkers.Remove(checker)) return true;

            return false;
        }

        private bool ParseCoordinates(string coordinates,ref Coordinates startPos,ref Coordinates finishPos)
        {
            Exception WrongCoordinatesException = new Exception("Coordinates example : A1;B0");

            int YPos = -1;
            int XPos = -1;

            int firstNumInd = -1;
            int secondNumInd = -1;

            bool anotherPos = false;

            for (int i = 0; i < coordinates.Length; i++)
            {
                if (Char.IsLetter(coordinates[i]))
                {
                    YPos = coordinates.ToUpper()[i] - 'A';
                    continue;
                }
                else if (Char.IsDigit(coordinates[i]))
                {
                    if (firstNumInd == -1)
                    {
                        firstNumInd = i;
                    }
                    if( !(i + 1 < coordinates.Length) || !Char.IsDigit(coordinates[i + 1]))
                    {
                        secondNumInd = i;
                    }
                    if (secondNumInd != -1)
                    {
                        int substrLen = secondNumInd - firstNumInd + 1;
                        if(XPos != -1) throw WrongCoordinatesException;
                        XPos = int.Parse(coordinates.Substring(firstNumInd, substrLen));
                        firstNumInd = -1;
                        secondNumInd = -1;
                    }
                }
                else if (coordinates[i].Equals(';'))
                {
                    if (YPos == -1 || XPos == -1) break;
                    anotherPos = true;
                    startPos.y = (sbyte)YPos;
                    startPos.x = (sbyte)XPos;
                    YPos = XPos = -1;
                }
                else if(coordinates[i] != ' ')
                {
                    throw new Exception($"Unexpected symbol : {coordinates[i]}");
                }
            }

            if (anotherPos == false || YPos == -1 || XPos == -1)
            {
                throw WrongCoordinatesException;
            }
            else
            {
                finishPos.y = (sbyte)YPos;
                finishPos.x = (sbyte)XPos;
            }
            
            return true;
            
        }

        private void FieldPrep()
        {
            sbyte playerRows = (sbyte)((_fieldsize.y - 2) / 2);


            bool set;
            sbyte playerIndex = (sbyte)CheckerSide.black;
            for (int i = 0; i < _fieldsize.y; i++)
            {
                set = !(i % 2 == 0);

                if (i == playerRows)
                {
                    i += 2;
                    playerIndex = (sbyte)CheckerSide.white;
                }
                for (int j = 0; j < _fieldsize.x; j++)
                {
                    if (set)
                    {
                        _field[i, j] = playerIndex;
                        set = false;
                    }
                    else set = true;
                }

            }
        }

        private void CheckerPrep(List<Checker> checkersList)
        {
            for (sbyte i = 0; i < _fieldsize.y; i++)
            {
                for (sbyte j = 0; j < _fieldsize.x; j++)
                {
                    if (_field[i, j] != 0)
                    {
                        checkersList.Add(new Checker(CheckerType.ordinary, (CheckerSide)_field[i, j], new Coordinates(i, j)));
                    }
                }

            }
        }

        private void QueueUpdate()
        {
            CheckersPlayer tempPlayer = _currentPlayer;
            _currentPlayer = _nextPlayer;
            _nextPlayer = tempPlayer;
        }

    }

    internal class Checker
    {
        private CheckerType _checkerType;
        private CheckerSide _checkerSide;
        private Coordinates _coordinates;
        public Dictionary<Coordinates, Coordinates> availableMoves;
        public Dictionary<Coordinates, Coordinates> availableBeat;


        public Coordinates checkerCoordinates
        {
            get
            {
                return _coordinates;
            }
            set
            {
                 _coordinates = value;
            }
        }
        public CheckerType checkerType
        {
            get
            {
                return _checkerType;
            }
            set
            {
                if (_checkerType != value) _checkerType = value;
            }
        }

        public CheckerSide checkerSide
        {
            get
            {
                return _checkerSide;
            }
            set
            {
                if (_checkerSide != value) _checkerSide = value;
            }
        }

        public Checker(CheckerType type, CheckerSide side, Coordinates coordinates)
        {
            checkerType = type;
            checkerSide = side;
            checkerCoordinates = coordinates;
            availableBeat = new Dictionary<Coordinates, Coordinates>();
            availableMoves = new Dictionary<Coordinates, Coordinates>();
        }

    }

    internal struct Coordinates
    {
        public sbyte x;
        public sbyte y;

        public Coordinates(sbyte y, sbyte x) 
        {
            this.x = x;
            this.y = y;
        }

    }

}