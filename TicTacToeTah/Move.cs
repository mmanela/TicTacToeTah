namespace TicTacToeTah
{
    /// <summary>
    /// Represents a desired move by a person
    /// </summary>
    public class Move
    {
        /// <summary>
        /// the value of this cell
        /// </summary>
        public PlayerNumber Player { get; set; }

        /// <summary>
        /// X coordinate
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Z coordinate
        /// </summary>
        public int Z { get; private set; }

        public Move(int posX, int posY, int posZ)
        {
            X = posX;
            Y = posY;
            Z = posZ;
            Player = PlayerNumber.None;
        }

        public Move(int posX, int posY, int posZ, PlayerNumber pl)
            : this(posX, posY, posZ)
        {
            Player = pl;
        }
    }
}