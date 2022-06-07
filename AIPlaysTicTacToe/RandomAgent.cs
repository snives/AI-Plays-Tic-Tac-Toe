using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlaysTicTacToe
{
    /// <summary>
    /// The RandomAgent plays random moves.  Serves as a benchmark.
    /// </summary>
    class RandomAgent : IAgent
    {
        public int PlayerId { get; set; }

        private Random _rnd;

        public RandomAgent()
        {
            _rnd = new Random();
        }

        public void AssignReward(double reward)
        {
            //NOP
        }

        public int DecideMove(Board board)
        {
            var moves = board.GetAvailableMoves();
            int selection = _rnd.Next(moves.Count);
            return moves[selection];
        }

        public void NewGame()
        {
            //NOP
        }
    }
}
