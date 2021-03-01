using System;
using System.Collections.Generic;

namespace TicTacToeTah
{
    /// <summary>
    /// AI for the computer, this decides what the next move should be
    /// </summary>
    public class AI
    {
        private readonly BoardPosition[,,] gameMatrix;
        private readonly int dimension;
        private readonly Random randomNumber = new Random();

        private readonly DifficultyLevels difficulty;

        //determines the difficuly settings for each difficulty level
        //this will map a level to a double array of size 2
        // the first value in the double array is the probablity of move to a spot of a line thats one from victory
        // the second value is the same for a line thats 2 from vicory
        private readonly Dictionary<DifficultyLevels, double[]> difficultyParameters = new Dictionary<DifficultyLevels, double[]>();

        public AI(BoardPosition[,,] grid, int dim)
        {
            gameMatrix = grid;
            dimension = dim;
            difficulty = DifficultyLevels.Hard;
            InitializeDifficultyParameters();
        }

        public AI(BoardPosition[,,] grid, int dim, DifficultyLevels dif)
        {
            gameMatrix = grid;
            dimension = dim;
            difficulty = dif;
            InitializeDifficultyParameters();
        }

        private void InitializeDifficultyParameters()
        {
            //The first double in the double array is the probability that we move into the last empty spot of a line
            //The second double is the probability that we move into one of the empty spots of a line with 2 
            difficultyParameters.Add(DifficultyLevels.Impossible, new double[] {1, 1});
            difficultyParameters.Add(DifficultyLevels.Hard, new[] {1, .5});
            difficultyParameters.Add(DifficultyLevels.Medium, new[] {.75, .4});
            difficultyParameters.Add(DifficultyLevels.Easy, new[] {.6, .3});
        }


        public Move NextAIMove(PlayerNumber computerPlayer)
        {
            int highestImportance = 0;
            Move desiredMove = null;
            bool oneAwayMoveAllowed = false;
            bool twoAwayMoveAllowed = false;

            double prob = randomNumber.NextDouble();
            if (prob <= difficultyParameters[difficulty][0])
            {
                oneAwayMoveAllowed = true;
            }
            prob = randomNumber.NextDouble();
            if (prob <= difficultyParameters[difficulty][1])
            {
                twoAwayMoveAllowed = true;
            }

            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    for (int k = 0; k < dimension; k++)
                    {
                        if (gameMatrix[i, j, k].ImportanceFactor >= 99 && oneAwayMoveAllowed)
                        {
                            desiredMove = gameMatrix[i, j, k].Move;
                            desiredMove.Player = computerPlayer;
                            return desiredMove;
                        }
                        else if (gameMatrix[i, j, k].ImportanceFactor < 99)
                        {
                            if (highestImportance < gameMatrix[i, j, k].ImportanceFactor)
                            {
                                highestImportance = gameMatrix[i, j, k].ImportanceFactor;
                                desiredMove = gameMatrix[i, j, k].Move;
                                desiredMove.Player = computerPlayer;
                            }
                        }
                    }
                }
            }

            if (twoAwayMoveAllowed)
            {
                return desiredMove;
            }
            else
            {
                return NextAIRandomMove(computerPlayer);
            }
        }

        public Move NextAIRandomMove(PlayerNumber computerPlayer)
        {
            int x = 0;
            int y = 0;
            int z = 0;

            do
            {
                x = randomNumber.Next(0, dimension);
                y = randomNumber.Next(0, dimension);
                z = randomNumber.Next(0, dimension);
            } while (gameMatrix[x, y, z].Player != PlayerNumber.None);

            Move desiredMove = gameMatrix[x, y, z].Move;
            desiredMove.Player = computerPlayer;

            return desiredMove;
        }
    }
}