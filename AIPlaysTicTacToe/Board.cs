using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace AIPlaysTicTacToe
{
    public class Board: IEquatable<Board>
    {
        //Capture the board as two tables of X's and O's, oriented as a single bitarray.
        private BitArray ba;
        public int Width { get; set; }
        public int Height { get; set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;

            //The simplest way to represent a tic-tac-toe board is with two bit array for X's and an array for O's.
            ba = new BitArray(2 * Width * Height);
        }

        /// <summary>
        /// Creates a new board as a copy of another board
        /// </summary>
        /// <param name="board"></param>
        public Board(Board board)
        {
            Width = board.Width;
            Height = board.Height;
            ba = (BitArray) board.ba.Clone();
        }

        public void Set(int x, int y, int player)
        {
            if (x >= Width)
                throw new ArgumentException("X must be less than Width");

            if (y >= Height)
                throw new ArgumentException("Y must be less than Height");

            if (player > 2)
                throw new ArgumentException("Player must be 1 or 2");

            ba.Set(((player - 1) * Width * Height) + x * Width + y, true);
        }

        /// <summary>
        /// Returns a 1 if an X is placed, or 2 if an O is placed, returns 0 otherwise.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Get(int x, int y)
        {
            return ba.Get(x * Width + y) ? 1 : ba.Get((Width * Height) + x * Width + y) ? 2 : 0;
        }

        public List<Tuple<int,int>> GetAvailableMoves()
        {
            var tuples = new List<Tuple<int, int>>();
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (Get(x, y) == 0)
                        tuples.Add(new Tuple<int, int>(x, y));

            return tuples;
        }

        //Allow the player to make a move, returns true if player has won
        public bool Move(int x, int y, int player)
        {
            //Register player move
            Set(x, y, player);

            //Evaluate the board  
                        //Horizontals
            bool won = Get(0, 0) == player && Get(1, 0) == player && Get(2, 0) == player ||
                       Get(0, 1) == player && Get(1, 1) == player && Get(2, 1) == player ||
                       Get(0, 2) == player && Get(1, 2) == player && Get(2, 2) == player ||
                       //Verticals
                       Get(0, 0) == player && Get(0, 1) == player && Get(0, 2) == player ||
                       Get(1, 0) == player && Get(1, 1) == player && Get(1, 2) == player ||
                       Get(2, 0) == player && Get(2, 1) == player && Get(2, 2) == player ||
                       //Diagonals
                       Get(0, 0) == player && Get(1, 1) == player && Get(2, 2) == player ||
                       Get(0, 2) == player && Get(1, 1) == player && Get(2, 0) == player;

            return won;
        }
        


        //We will override the object.Equals and GetHashCode function so we can treat this object as a value type.
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.GetHashCode().Equals(obj.GetHashCode());
        }

        public bool Equals(Board other)
        {
            if (other == null)
                return false;

            return this.GetHashCode() == other.GetHashCode();
        }

       
        // override object.GetHashCode
        public override int GetHashCode()
        {
            //Produce a 32bit int from the bitarray.
            byte[] b = new byte[4];
            ba.CopyTo(b, 0);
            return BitConverter.ToInt32(b, 0);
        }

    }
}
