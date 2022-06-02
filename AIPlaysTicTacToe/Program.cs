using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


/// <summary>
/// This project will demonstrate the use of model free reinforcement learning using a tabular solution.
/// 
/// </summary>



namespace AIPlaysTicTacToe
{
    class Program
    {
        //Agent Q table
        public static Dictionary<int, double> Q = new Dictionary<int, double>();

        static void Main(string[] args)
        {
            //First we define the board environment, and define a method that observes its state.
            //var board1 = new Board(3, 3);
            //board1.Set(0, 0, 1);
            //board1.Set(1, 1, 1);
            //board1.Set(2, 2, 1);
            //board1.Set(1, 0, 2);

            //DrawBoard(board1);

            //var moves = board1.GetAvailableMoves();
            //foreach (var move in moves)
            //    Console.Write($"({move.Item1}, {move.Item2}) ");

            //Agent Q table
            //var Q = new Dictionary<int, double>();

            int trainingEpochs = 10000;
            double exploration = 1.0;  //epsillon greedy
            double min_exploration = 0.05;
            double explorationDecay = 0.9997;
            double learnRate = 0.2;
            double discountRate = 0.99;
            bool interactive = false;
            int winCount = 0;

            var rnd = new Random();


            //Loop a number of epochs
            for (int epoch=1; epoch <= trainingEpochs; epoch++)
            {
                

                //Create a new game
                var board = new Board(3, 3);

                //Reduce exploration rate exponentially until it reaches min_exploration.
                if (exploration > min_exploration)
                    exploration = exploration * explorationDecay;

                Console.WriteLine($"Epoch {epoch}   Explor: {exploration:0.0000}   win ratio: {(double)winCount / epoch:0.00}");

                while (true)
                {
                    Tuple<int, int> action = null;

                    //Step 1: Decide between exploration and exploitation
                    if (rnd.NextDouble() < exploration)
                    {
                        //explore - Select a random move from available moves
                        var moves = board.GetAvailableMoves();
                        int selection = rnd.Next(moves.Count - 1);
                        action = moves[selection];
                    }
                    else
                    {
                        //exploit
                        //Choose the next move that maximizes the reward
                        var max_Reward = GetMaxRewardForBoard(board, 1, Q);
                        action = max_Reward.Item2;
                    }

                    //Step 2: Make move
                    var previousBoardHash = board.GetHashCode();
                    var won = board.Move(action.Item1, action.Item2, 1);
                    var cat = board.GetAvailableMoves().Count == 0;

                    //Generate a reward if we won.
                    double reward = won ? 1.0 : cat ? 0.5: 0.0;
                    //Do we need a negative reward for losing?

                    //Step 3: Update the Q policy
                    //agent1.UpdatePolicy(board1, action, reward);
                    var maxReward = GetMaxRewardForBoard(board, 1, Q);
                    var previousQ = Q.ContainsKey(previousBoardHash) ? Q[previousBoardHash] : 0.0;
                    Q[previousBoardHash] = (1-learnRate) * previousQ + learnRate * (reward + discountRate * maxReward.Item1);



                    //if (interactive)
                    //{
                    //    Console.Clear();
                    //    DrawBoard(board);

                    //    if (won)
                    //        Console.WriteLine("Won");
                    //    if (!won && cat)
                    //        Console.WriteLine("Cat");

                    //    Console.ReadKey(true);
                    //}

                    //If game is over then end
                    if (won)
                    {
                        //Console.WriteLine("won");
                        winCount++;
                        break;
                    }
                    if (cat)
                    {
                        //Console.WriteLine("cat");
                        break;
                    }


                    //Store these variables after player 1 moved, to update it negatively if player 2 wins.
                    //maxReward = GetMaxRewardForBoard(board, 1, Q);
                    //previousBoardHash = board.GetHashCode();  
                    //previousQ = Q.ContainsKey(previousBoardHash) ? Q[previousBoardHash] : 0.0;

                    //Now have player 2 play
                    //Maybe player 2 plays randomly.
                    var moves2 = board.GetAvailableMoves();
                    int selection2 = rnd.Next(moves2.Count - 1);
                    var player2_won = board.Move(moves2[selection2].Item1, moves2[selection2].Item2, 2);
                    cat = moves2.Count == 1;

                    //I think I should update agent1 Q table even though it hasn't moved because the board has been updated
                    if (player2_won)
                        reward = -1.0;
                    else if (cat)
                        reward = -0.1;
                    else
                        reward = 0.0;

                    if (player2_won || cat)
                    {
                        var boardHash = board.GetHashCode();
                        Q[boardHash] = reward;
                    }
                    
                    //What is it learning?  It's learning that the board state is a loss.  It's learning that by not going to where player 2 went, it is losing.
                    //So agent1 should receive a positive reward for having gone where player2 went to win.  Except this board represents player2 having gone there
                    //only.  Honestly, I don't think I implemented the Q learning table correctly.  Storing only the environment doesn't allow us to evaluate the
                    //reward associated with each possible action.  

                    //Continuing with this parlance of negative reward.  If this position resulted in a loss then this board reward should definitely be -1 all the time.
                    //So if its a cat, the game also ends, so its score can be slightly negative.
                    //But if the game continues how does this cell approach its limit?  Should this board state change?



                    if (interactive)
                    {
                        Console.Clear();
                        DrawBoard(board);

                        if (player2_won)
                            Console.WriteLine("lose");
                        if (!player2_won && cat)
                            Console.WriteLine("cat");

                        Console.ReadKey(true);
                    }

                    //End of game
                    if (player2_won || cat)
                        break;

                } // end of epoch


                //if (interactive)
                //{
                //    //DrawBoard(board);
                //    Console.ReadKey(true);
                //    Console.Clear();
                //}

               
                //Thread.Sleep(10);

                if (!interactive && epoch > 9950)
                    interactive = true;
                
                
            }


            



            Console.ReadKey();
        }

        //TODO: This is probably an agent method
        //Find the maximum reward for all possible moves from the given board, by the given player
        public static Tuple<double, Tuple<int,int>> GetMaxRewardForBoard(Board board, int player, Dictionary<int, double> Q)
        {
            double r_max = double.MinValue;
            Tuple<int, int> move_max = null;
            var moves = board.GetAvailableMoves();

            //List of candidate moves if they all have the same reward.
            var candidates = new List<Tuple<int, int>>();

            //TODO: Handle a no moves available condition
            foreach (var move in moves)
            {
                var nextBoard = new Board(board);
                nextBoard.Move(move.Item1, move.Item2, 1);
                //Lookup historical reward of this board layout
                int boardHash = nextBoard.GetHashCode();
                var r = Q.ContainsKey(boardHash) ? Q[boardHash] : 0.0;
                if (r > r_max)
                {
                    r_max = r;
                    move_max = move;
                    candidates.Clear();
                    candidates.Add(move);
                } else if (r == r_max)
                {
                    candidates.Add(move);
                }
            }
            //TODO: If multiple moves have the same reward we should randomly select from those.
            if (candidates.Count > 1)
            {
                var rnd = new Random();
                int randomSelection = rnd.Next(candidates.Count - 1);
                move_max = moves[randomSelection];
            }

            return new Tuple<double, Tuple<int, int>>(r_max, move_max);
        }


        public static void DrawBoard(Board board)
        {
            var map = " XO".ToCharArray();
            Console.WriteLine(" {0} | {1} | {2}", map[board.Get(0, 0)], map[board.Get(1, 0)], map[board.Get(2, 0)]);
            Console.WriteLine("-----------");
            Console.WriteLine(" {0} | {1} | {2}", map[board.Get(0, 1)], map[board.Get(1, 1)], map[board.Get(2, 1)]);
            Console.WriteLine("-----------");
            Console.WriteLine(" {0} | {1} | {2}", map[board.Get(0, 2)], map[board.Get(1, 2)], map[board.Get(2, 2)]);

            //Display a heatmap
            for (int y = 0; y < board.Height; y++)
            { 
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.Get(x,y) == 0)
                    {
                        var next = new Board(board);
                        next.Move(x, y, 1);
                        int boardHash = next.GetHashCode();
                        //get Q reward for each board
                        if (Q.ContainsKey(boardHash))
                        {
                            var reward = Q[next.GetHashCode()];
                            Console.Write("{0:0.0000}   ", reward);
                        } else
                        {
                            Console.Write("null      ");
                        }
                        
                    } else
                    {
                        Console.Write("n/a       ");
                    }
                    
                }
                Console.WriteLine("\n--------- --------- --------- ");
            }
        }
    }
}
