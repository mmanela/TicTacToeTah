using System;
using System.Collections.Generic;

namespace TicTacToeTah
{
    /// <summary>
    /// Represents the game board.  It controls all actions about the board.
    /// </summary>
    public class GameLogic
    {
        private int dimension;

        private bool gameOver;

        private int maxSpots;
        private int usedSpots;


        //internal store the board
        private BoardPosition[,,] gameMatrix;
        private readonly Dictionary<int, LineDirection> possibleDirections = new Dictionary<int, LineDirection>();

        private AI computerIntelligence;
        private DifficultyLevels difficulty = DifficultyLevels.Medium;

        private bool dimensionChange;


        public int Dimension
        {
            get { return dimension; }
            set
            {
                if (dimension != value)
                {
                    dimensionChange = true;
                    dimension = value;
                }
                else
                {
                    dimensionChange = false;
                }
            }
        }

        public AI AI
        {
            get { return computerIntelligence; }
        }


        public DifficultyLevels ComputerDifficulty
        {
            set { difficulty = value; }
        }

        public GameLogic()
        {
            LineDirection newLine = null;
            //build all possible directions a line of linegth d cubes can be in a dxdxd board
            //check the 9 3d directions
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    newLine = new LineDirection(i, j, 1);
                    possibleDirections[newLine.ID] = newLine;
                }
            }

            //the 4 2d directions

            //horizontal 2d check
            newLine = new LineDirection(1, 0, 0);
            possibleDirections[newLine.ID] = newLine;

            //vertical 2d check
            newLine = new LineDirection(0, 1, 0);
            possibleDirections[newLine.ID] = newLine;

            //diagonal 2d check
            newLine = new LineDirection(1, 1, 0);
            possibleDirections[newLine.ID] = newLine;

            //diagonal 2d check
            newLine = new LineDirection(1, -1, 0);
            possibleDirections[newLine.ID] = newLine;
        }


        public bool SetUpBoardLogic()
        {
            if (dimension < 3)
            {
                throw new ArgumentException("Dimension of game must be greater than 2");
            }

            if (dimensionChange)
            {
                maxSpots = dimension * dimension * dimension;
                gameMatrix = new BoardPosition[dimension,dimension,dimension];
            }

            gameOver = false;
            usedSpots = 0;
            //initilize board to empty
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    for (int k = 0; k < dimension; k++)
                    {
                        if (gameMatrix[i, j, k] == null)
                        {
                            gameMatrix[i, j, k] = new BoardPosition(i, j, k);
                        }
                        else
                        {
                            gameMatrix[i, j, k].Player = PlayerNumber.None;
                            gameMatrix[i, j, k].ImportanceFactor = gameMatrix[i, j, k].ThroughLines.Count;
                            ;
                        }
                    }
                }
            }

            if (dimensionChange)
            {
                //rebuild the BoardPosition linethrough dictionaries
                BuildBoardPositionInfo();
            }
            computerIntelligence = new AI(gameMatrix, dimension, difficulty);


            return true;
        }


        public void BuildBoardPositionInfo()
        {
            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    for (int z = 0; z < dimension; z++)
                    {
                        BoardPosition currentPos = gameMatrix[x, y, z];
                        foreach (LineDirection dir in possibleDirections.Values)
                        {
                            List<BoardPosition> cellInDir = GetLinePositions(currentPos, dir);
                            if (cellInDir.Count == dimension)
                            {
                                currentPos.ThroughLines[dir] = cellInDir;
                            }
                        }
                        currentPos.ImportanceFactor = currentPos.ThroughLines.Count;
                    }
                }
            }
        }


        private List<BoardPosition> GetLinePositions(BoardPosition move, LineDirection direction)
        {
            int posX = move.X;
            int posY = move.Y;
            int posZ = move.Z;

            List<BoardPosition> cellList = new List<BoardPosition>();

            while (posX >= 0 &&
                   posY >= 0 &&
                   posZ >= 0 &&
                   posX < dimension &&
                   posY < dimension &&
                   posZ < dimension
                )
            {
                cellList.Add(gameMatrix[posX, posY, posZ]);
                posX += direction.dx;
                posY += direction.dy;
                posZ += direction.dz;
            }

            posX = move.X - direction.dx;
            posY = move.Y - direction.dy;
            posZ = move.Z - direction.dz;

            while (posX >= 0 &&
                   posY >= 0 &&
                   posZ >= 0 &&
                   posX < dimension &&
                   posY < dimension &&
                   posZ < dimension)
            {
                cellList.Add(gameMatrix[posX, posY, posZ]);
                posX -= direction.dx;
                posY -= direction.dy;
                posZ -= direction.dz;
            }

            return cellList;
        }

        /// <summary>
        /// Place a new move on the board
        /// </summary>
        /// <param name="moveToPlace">the move object to place</param>
        /// <returns>The result of the move</returns>
        public MoveResult PlaceMove(Move moveToPlace)
        {
            var moveResult = new MoveResult();
            if (maxSpots <= usedSpots || gameOver)
            {
                moveResult.status = MoveStatus.GameOver;
                return moveResult;
            }

            if (gameMatrix[moveToPlace.X, moveToPlace.Y, moveToPlace.Z].Player == PlayerNumber.None)
            {
                //set this spot to the player who moved here
                gameMatrix[moveToPlace.X, moveToPlace.Y, moveToPlace.Z].Player = moveToPlace.Player;
                //this spot has no importance now since its taken
                //only empty spots can have importance
                gameMatrix[moveToPlace.X, moveToPlace.Y, moveToPlace.Z].ImportanceFactor = 0;

                if (IsWinningState(moveToPlace, out moveResult.moveList))
                {
                    gameOver = true;
                    moveResult.status = MoveStatus.WinningMove;
                }
                else
                {
                    usedSpots++;
                    if (maxSpots <= usedSpots)
                    {
                        moveResult.status = MoveStatus.TieGame;
                    }
                    else
                    {
                        moveResult.status = MoveStatus.TurnOver;
                    }
                }
            }
            else
            {
                moveResult.status = MoveStatus.InvalidMove;
            }

            return moveResult;
        }


        /// <summary>
        /// Give the next move see if it is the winning move
        /// </summary>
        /// <param name="lastMove"></param>
        /// <returns></returns>
        public bool IsWinningState(Move moveToPlace, out List<Move> resultList)
        {
            BoardPosition cellPos = gameMatrix[moveToPlace.X, moveToPlace.Y, moveToPlace.Z];


            foreach (LineDirection direction in cellPos.ThroughLines.Keys)
            {
                List<BoardPosition> cells = cellPos.ThroughLines[direction];
                resultList = new List<Move>();

                List<BoardPosition> emptyCells = new List<BoardPosition>();
                int emptyCount = 0;
                int playerFirstCount = 0;
                int playerSecondCount = 0;
                foreach (BoardPosition cell in cells)
                {
                    //build result move list
                    resultList.Add(cell.Move);

                    if (cell.Player == PlayerNumber.First)
                    {
                        playerFirstCount++;
                    }
                    else if (cell.Player == PlayerNumber.Second)
                    {
                        playerSecondCount++;
                    }
                    else
                    {
                        emptyCells.Add(cell);
                        emptyCount++;
                    }
                }


                if (playerFirstCount == dimension || playerSecondCount == dimension)
                {
                    return true;
                }


                if (playerFirstCount == 0 || playerSecondCount == 0)
                {
                    if (playerFirstCount == dimension - 1 || playerSecondCount == dimension - 1)
                    {
                        foreach (BoardPosition posCell in emptyCells)
                        {
                            posCell.ImportanceFactor = 99;
                        }
                    }
                    else if (playerFirstCount == dimension - 2 || playerSecondCount == dimension - 2)
                    {
                        foreach (BoardPosition posCell in emptyCells)
                        {
                            posCell.ImportanceFactor += 5;
                        }
                    }
                }
                else
                {
                    foreach (BoardPosition posCell in emptyCells)
                    {
                        posCell.ImportanceFactor -= 5;
                    }
                }
            }

            resultList = null;
            return false;
        }
    }
}