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

//Bug: It seems as though hits on the Q table are nearly always missed.  This is essentially data sparsity.
// The theoretical combinations of the board being filled is 9! because There are 9 possible moves for the first play,
// 8 possible moves for the 2nd play, so on and so forth.  


//TODO: Rewrite the Q table in terms of boardhash and action.  Action will now be 1-9.
//

namespace AIPlaysTicTacToe
{
    class Program
    {
        //Agent Q table - boards X actions
        public static double[,] Q;

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
            Q = new double[1<<18, 9];

            int trainingEpochs = 40000;
            double exploration = 1.0;  //epsillon greedy
            double min_exploration = 0.01;
            double explorationDecay = 0.9999;
            double learnRate = 0.01;  //Effectively a window of 100 samples.
            double discountRate = 0.99;
            bool interactive = false;
            int winCount = 0;
            int gameCount = 0;

            var rnd = new Random();


            //Loop a number of epochs
            for (int epoch=1; epoch < trainingEpochs; epoch++)
            {
                //Reset wincount every 10k games
                if (epoch % 10000 == 0)
                {
                    winCount = 0;
                    gameCount = 0;
                }

                //Create a new game
                var board = new Board(3, 3);
                //Store state, move history (only store history of player 1)
                var history = new List<Tuple<int, int>>();  

                //Reduce exploration rate exponentially until it reaches min_exploration.
                if (exploration > min_exploration)
                    exploration = exploration * explorationDecay;

                var reward = 0.0;

                while (true)
                {
                    int action;

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
                    //var previousBoardHash = board.GetHashCode();
                    history.Add(new Tuple<int,int>(board.GetHashCode(), action));
                    var P1_won = board.Move(action, 1);
                    var P1_cat = board.GetAvailableMoves().Count == 0;

                    //In this flavor of RL I will only assess rewards at the end of the game.



                    //Generate a reward if we won.
                    //double reward = won ? 1.0 : cat ? 0.5: 0.0;
                    //Do we need a negative reward for losing?

                    //Step 3: Update the Q policy
                    //The Q table is simple.  It stores the average reward for taking a move, given a state.
                    //Each cell is a running average.  Thus, it should be updated incrementally.
                    //One such method which doesn't require storing totals is the EWMA.
                    //If n is the window of samples to average over, e.g. 100.
                    //avg = avg * (n-1)/n + sample / n
                    //avg = avg * (1-0.01) + sample * 0.01

                    //So we can see the parallels to the Q function
                    //Q = Q * (1-learnRate) + (reward + best_future_reward * discountRate) * learnRate
                    //Its simply the moving average of this moves reward plus a discounted reward of the best next move.
                    //This has the effect of pulling forward the future rewards into the past actions, ultimately to the first move.
                    //It should average those future rewards into all past actions which were taken in a given game.

                    //So for tic-tac-toe, the next state will vary randomly by player2's move.  However no reward is ever given on P2's moves
                    //So my Q table never learns.  So instead I will store all moves in the game, and push down the reward
                    //to each state of the board, applying a discount at each step.  Here, applying means averaging.
                    //This way, at game 2, we already have a best move to play, if we were to consult the Q table.
                    //10,000 games later, I should have a fairly stable Q table.


                    //agent1.UpdatePolicy(board1, action, reward);
                    //var maxReward = GetMaxRewardForBoard(board, 1, Q);
                    //var previousQ = Q.ContainsKey(previousBoardHash) ? Q[previousBoardHash] : 0.0;
                    //Q[previousBoardHash] = previousQ * (1-learnRate) + learnRate * (reward + discountRate * maxReward.Item1);



                    if (interactive)
                    {
                        Console.Clear();
                        DrawBoard(board);

                        if (P1_won)
                            Console.WriteLine("Won");
                        else if (P1_cat)
                            Console.WriteLine("Cat");

                        Console.ReadKey(true);
                    }

                    //If game is over then end
                    if (P1_won)
                    {
                        reward = 1.0;
                        //Console.WriteLine("won");
                        winCount++;
                        break;
                    } else if (P1_cat)
                    {
                        reward = 0.5;
                        //Console.WriteLine("cat");
                        break;
                    }


                    //Store these variables after player 1 moved, to update it negatively if player 2 wins.
                    //maxReward = GetMaxRewardForBoard(board, 1, Q);
                    //previousBoardHash = board.GetHashCode();  
                    //previousQ = Q.ContainsKey(previousBoardHash) ? Q[previousBoardHash] : 0.0;

                    //Now have player 2 play
                    //Maybe player 2 plays randomly.
                    var movesP2 = board.GetAvailableMoves();
                    int selection2 = rnd.Next(movesP2.Count - 1);
                    var P2_won = board.Move(movesP2[selection2], 2);
                    var P2_cat = movesP2.Count == 1;

                    if (P2_won)
                    {
                        reward = -1.0;
                    }
                    else if (P2_cat)
                    {
                        reward = 0.5;
                    }

                    //Notice we don't store history of p2 moves.


                    //I think I should update agent1 Q table even though it hasn't moved because the board has been updated
                    //if (player2_won)
                    //    reward = -1.0;
                    //else if (cat)
                    //     = -0.1;
                    //else
                    //    reward = 0.0;

                    //if (player2_won || cat)
                    //{
                    //    var boardHash = board.GetHashCode();
                    //    Q[boardHash] = reward;
                    //}

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

                        if (P2_won)
                        {
                            Console.WriteLine("lose");
                        } else if (P2_cat)
                        {
                            Console.WriteLine("cat");
                        }

                        Console.ReadKey(true);
                    }

                    //End of game
                    if (P2_won || P2_cat)
                        break;

                } // end of epoch

                gameCount++;
                Console.WriteLine($"Epoch {epoch}   Explor: {exploration:0.0000}   win ratio: {(double)winCount / gameCount:0.00}");


                //Now propagate backwards the rewards from the games plays, now that we have the reward
                history.Reverse();
                foreach(var play in history)
                {
                    //Assign Q table
                    Q[play.Item1, play.Item2] = Q[play.Item1, play.Item2] * (1 - learnRate) + (reward * learnRate);
                    reward = reward * discountRate;
                }





                //if (interactive)
                //{
                //    //DrawBoard(board);
                //    Console.ReadKey(true);
                //    Console.Clear();
                //}


                //Thread.Sleep(10);

                if (!interactive && epoch > 39950)
                    interactive = true;
                
                
            }


            



            Console.ReadKey();
        }

        //TODO: This is probably an agent method

        //Find the maximum reward for all possible moves from the given board, by the given player
        //returns the action and its reward.
        public static Tuple<double, int> GetMaxRewardForBoard(Board board, int player, double[,] Q)
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
                } else if (r == r_max)
                {
                    candidates.Add(move);
                }
            }
            //If multiple actions have the same reward we should randomly select from those.
            if (candidates.Count > 1)
            {
                var rnd = new Random();
                int randomSelection = rnd.Next(candidates.Count - 1);
                action_max = moves[randomSelection];
            }

            return new Tuple<double, int>(r_max, action_max);
        }


        public static void DrawBoard(Board board)
        {
            var map = " XO".ToCharArray();
            
            Console.WriteLine(" {0} | {1} | {2}", map[board.Get(0, 0)], map[board.Get(1, 0)], map[board.Get(2, 0)]);
            Console.WriteLine("-----------");
            Console.WriteLine(" {0} | {1} | {2}", map[board.Get(0, 1)], map[board.Get(1, 1)], map[board.Get(2, 1)]);
            Console.WriteLine("-----------");
            Console.WriteLine(" {0} | {1} | {2}", map[board.Get(0, 2)], map[board.Get(1, 2)], map[board.Get(2, 2)]);
            Console.WriteLine("\n");

            //Display a heatmap

            for (int y = 0; y < board.Height; y++)
            { 
                for (int x = 0; x < board.Width; x++)
                {
                    if (board.Get(x,y) == 0)
                    {
                        //get Q reward for each board, action
                        var reward = Q[board.GetHashCode(), board.XYToAction(x, y)];
                        Console.Write("{0:0.0000}   ", reward);
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
