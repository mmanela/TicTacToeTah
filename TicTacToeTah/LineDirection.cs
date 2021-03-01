using System;

namespace TicTacToeTah
{
    /// <summary>
    /// represents a direction in 3d space
    /// </summary>
    public class LineDirection : IComparable
    {
        public int dx, dy, dz;

        private int uniqueHashCode;

        public int ID
        {
            get
            {
                return uniqueHashCode;
            }
        }


        public static int CalcID(int dx, int dy, int dz)
        {
            return dx + dy * 10 + dz * 100;
        }

        public LineDirection(int x, int y, int z)
        {
            dx = x;
            dy = y;
            dz = z;
            uniqueHashCode = CalcID(dx, dy, dz);
        }


        public static bool operator ==(LineDirection l1, LineDirection l2)
        {
            return (l1.ID == l2.ID);
        }
        public static bool operator !=(LineDirection l1, LineDirection l2)
        {
            return l1.ID != l2.ID;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return uniqueHashCode;
        }

        int IComparable.CompareTo(object obj)
        {
            var lineDir = obj as LineDirection;
            if (lineDir == null)
            {
                throw new Exception("Not a valid type");
            }
            else
            {
                return uniqueHashCode.CompareTo(lineDir.ID);
            }
        }

    }
}