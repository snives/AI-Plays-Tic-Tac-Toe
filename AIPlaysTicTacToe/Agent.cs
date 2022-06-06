using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlaysTicTacToe
{
    public class Agent
    {
        //Agent Q table - boards X actions
        public double[,] Q;

        private Random _rnd;

        //Assigns an id to a player. e.g. player 1. 
        public int PlayerId { get; set; }

        /// <summary>
        /// The window of observations to average over. =1/n
        /// Works like an EWMA.
        /// </summary>
        public double Alpha { get; set; } = 0.01;
        
        /// <summary>
        /// The ratio of play being exploration or exploitation.  1.0=always explore, 0.0 = always exploit
        /// </summary>
        public double Exploration { get; set; } = 1.0;

        /// <summary>
        /// The minimum exploration rate
        /// </summary>
        public double MinExploration { get; set; } = 0.01;

        /// <summary>
        /// The rate of decay of the exploration rate.  (1-1/n)
        /// </summary>
        public double ExplorationDecay { get; set; } = 0.99994;

        /// <summary>
        /// The rate rewards are discounted in its history of moves.
        /// </summary>
        public double RewardDiscountRate { get; set; } = 0.9;

        //Store state, move history
        private List<Tuple<int, int>> _history { get; set; }


        public Agent()
        {
            //Agent Q table
            Q = new double[1 << 18, 9];

            _rnd = new Random();

            //Initialize history
            _history = new List<Tuple<int, int>>();
        }

        /// <summary>
        /// Asks the agent to make a move
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public int DecideMove(Board board)
        {
            int action;

            //Step 1: Decide between exploration and exploitation
            if (_rnd.NextDouble() < Exploration)
            {
                //explore - Select a random move from available moves
                var moves = board.GetAvailableMoves();
                int selection = _rnd.Next(moves.Count);
                action = moves[selection];
            }
            else
            {
                //exploit
                //Choose the next move that maximizes the reward
                var max_Reward = GetMaxRewardForBoard(board);
                action = max_Reward.Item2;
            }

            //Store this move in history
            _history.Add(new Tuple<int, int>(board.GetHashCode(), action));

            return action;
        }

        public void NewGame()
        {
            _history.Clear();

            //Adjust our exploration/exploitation ratio
            //Reduce exploration rate exponentially until it reaches min exploration rate.
            if (Exploration > MinExploration)
                Exploration *= ExplorationDecay;
        }

        //Find the maximum reward for all possible moves from the given board, by the given player
        //returns the action and its reward.
        private Tuple<double, int> GetMaxRewardForBoard(Board board)
        {
            double r_max = double.MinValue;
            int action_max = 0;
            var moves = board.GetAvailableMoves();

            //List of candidate actions if they all have the same reward.
            var candidates = new List<int>();

            //Iterate the moves, and retrieve the reward
            foreach (var move in moves)
            {
                //Get the reward for each board, action combination.
                var r = Q[board.GetHashCode(), move];
                if (r > r_max)
                {
                    r_max = r;
                    action_max = move;
                    candidates.Clear();
                    candidates.Add(move);
                }
                else if (r == r_max)
                {
                    candidates.Add(move);
                }
            }
            //If multiple actions have the same reward we should randomly select from those.
            if (candidates.Count > 1)
            {
                int randomSelection = _rnd.Next(candidates.Count - 1);
                action_max = moves[randomSelection];
            }

            return new Tuple<double, int>(r_max, action_max);
        }


        public void AssignReward(double reward)
        {
            //Now propagate backwards the rewards from the games plays, now that we have the reward
            _history.Reverse();
            foreach (var play in _history)
            {
                //Assign Q table
                Q[play.Item1, play.Item2] = Q[play.Item1, play.Item2] * (1 - Alpha) + (reward * Alpha);
                reward = reward * RewardDiscountRate;
            }
        }
    }
}
