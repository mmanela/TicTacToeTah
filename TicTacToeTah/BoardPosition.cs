using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacToeTah
{
    /// <summary>
    /// A single cell in the board
    /// </summary>
    public class BoardPosition
    {
        private PlayerNumber playerNumber;


        //all lines through this point
        private Dictionary<LineDirection, List<BoardPosition>> linesThroughPoint = new Dictionary<LineDirection, List<BoardPosition>>();

        private readonly int uniqueHashCode;


        //how important is this spot in the board
        //this is used by AI to determine important moves
        // value range from 0 up
        private int importance;

        public int ID
        {
            get
            {
                return uniqueHashCode;
            }
        }

        public int ImportanceFactor
        {
            get
            {
                return importance;
            }
            set
            {
                if (value >= 0)
                {
                    importance = value;
                }

            }

        }

        public Dictionary<LineDirection, List<BoardPosition>> ThroughLines
        {
            get
            {
                return linesThroughPoint;
            }

        }

        public PlayerNumber Player
        {
            get
            {
                return playerNumber;
            }
            set
            {
                playerNumber = value;
            }
        }

        public int X { get; private set; }

        public int Y { get; private set; }

        public int Z { get; private set; }

        public Move Move
        {
            get
            {
                return new Move(X, Y, Z, playerNumber);
            }

        }



        public BoardPosition(int x, int y, int z, PlayerNumber pNum)
        {
            X = x;
            Y = y;
            Z = z;
            playerNumber = pNum;
            uniqueHashCode = CalcID(x, y, z);
        }
        public BoardPosition(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
            playerNumber = PlayerNumber.None;
            uniqueHashCode = CalcID(x, y, z);
        }


        public static int CalcID(int x, int y, int z)
        {
            return x + y * 10 + z * 100;
        }



        public new String ToString()
        {
            return X.ToString() + Y.ToString() + Z.ToString() + ((int)playerNumber).ToString();
        }
    }
}
