using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;


/// <summary>
/// This project will demonstrate the use of model free reinforcement learning using a tabular solution.
/// Reinforcement learning works by being rewarded for achieving a goal.  It then randomly
/// takes actions, according to the rules of the game, and gains experience, observing what rewards
/// it gets from taking what actions.  The amount of time it spends exploring new actions vs the time
/// it leverages its own experience is the exploration/exploitation tradeoff.  As we train we will
/// reduce the exploration to only some small amount, so it mostly uses its experience.
/// 
/// 
/// </summary>

//The Q table is simple.  It stores the average reward for taking a move, given a state.
//Each cell is a running average.  It would costly from a space aspect to store the sum of its rewards and the total times it received them.
//But if we can update our average reward incrementally then we can avoid storing those.
//One such method which doesn't require storing totals is the EWMA.
//If n is the window of samples to average over, e.g. 100.
//avg = avg* (n - 1) / n + sample / n
//avg = avg * (1 - 0.01) + sample * 0.01

//So we can see the parallels to the Q function
//Q = Q* (1 - learnRate) + (reward + future_reward * discountRate) * learnRate
//It's simply the moving average of this moves reward plus a discounted reward of the best next move.
//This has the effect of pulling forward the future rewards into the past actions, ultimately to the first move.
//It should average those future rewards into all past actions which were taken in a given game.
//Actually the future_reward is the reward of taking the optimal action on the next state.
//Restating the Bellman Equation.
//Q(s, a) = Q(s, a) * (1-alpha) + (reward + argmax(Q(s', a')) * gamma) * alpha
//Where alpha is the (n-1/n), n being the number of samples to average over.
//and gamma is the discount rate, which defines the rate of decay of future rewards, essentially controls how far
//in the future rewards are measured.  argmax(Q(s', a')) is the Q of taking the optimal action from the next state.
//This is essentially a recursive function, however it doesn't necessarily need to be recursively evaluated because the
//Q table is tracking each recursions value, vis-a-vis dynamic programming.

//Regarding Gamma (learnRate):  If the solution requires 10 steps to achieve, the reward will be Gamma^10, and so the first move
//will have a relatively small value.  However, if there is a shorter path to the solution it should result in much 
//higher Q values reaching the first move.  So this has the effect of prioritizing quicker solutions.

//If the same solution is achieved via multiple states, then that move will gain more value than other Q states.
//This is overall good, because it will pursue positions of strength.  This is the reason why we see the agent
//sometimes not take immediately take the 3-in-a-row, because they make another move which results in a win in 2
//different ways.  The agent amasses more reward by winning two different ways than it does with a single way.


//So for tic-tac-toe, the next state will vary randomly by player2's move.  However no reward is ever given on P2's moves
//So my Q table never learns.So instead I will store all moves in a single game, and push down the reward
//to each state of the board, applying a discount at each step.  Here "applying" means averaging.
//This way, at game 2, we already have a best move to play, if we were to consult the Q table.
//10,000 games later, I should have a fairly stable Q table.


//I suppose one way to still retain the incremental update technique is to obtain argmax(Q(s',a')) from the step after
//the opponent takes their random move.  This would yield a state s' that has values.  And since p2 is random the values
//should converge over time.

//Now suppose P2 is an intelligent agent.  We would need to obtain a argmax(Q(s',a')) from the future, which we should 
//not be able to know.  Unless we memorize their moves, and they continue to play the same.  So we clearly cannot 
//assign Q values from the future, so we therefore must assign them in arrears.  In a game with an end we can easily do
//that at the end, such as this example, but in a continuous game when do we assess rewards?  Well, if we keep a fixed
//but rolling history of moves, e.g. 100, then we can assess the rewards when we receive them.  The only limitation
//being that we must achieve the reward within n steps of the originating move.  Depending on the scenario this may
//or may not be a problem.
//

//Consider a different game, a single player navigating a non-deterministic environment, where the environment changes.
//I believe our existing reasoning holds.  However, does memory based play work in a non-deterministic environment?
//Can a non-deterministic environment be solved with RL?


//Let's refactor this code and abstract the agent class.
//Agent has now be abstracted

namespace AIPlaysTicTacToe
{
    class Program
    {
        static void Main(string[] args)
        {
            //First we define the training variables
            int trainingEpochs = 90000;
            bool interactive = false;
            int winCount = 0;
            int gameCount = 0;

            //Player 1
            var agent1 = new Agent();
            var agent2 = new Agent();


            //Loop for a number of training epochs
            for (int epoch=1; epoch < trainingEpochs; epoch++)
            {
                //Reset win count every 10k games
                if (epoch % 10000 == 0)
                {
                    winCount = 0;
                    gameCount = 0;
                }

                //Create a new game
                var board = new Board(3, 3);
                agent1.NewGame();
                agent2.NewGame();

                
                while (true)
                {
                    //Ask player 1 to move
                    int action = agent1.DecideMove(board);

                    //Step 2: Make move, returns observation of environment
                    var P1_won = board.Move(action, 1);
                    var P1_cat = board.GetAvailableMoves().Count == 0;

                    //In this flavor of RL I will only assess rewards at the end of the game.

                    if (interactive)
                    {
                        Console.Clear();
                        DrawBoard(board, agent1);

                        if (P1_won)
                            Console.WriteLine("Won");
                        else if (P1_cat)
                            Console.WriteLine("Cat");
                        else
                            Console.ReadKey(true);
                    }

                    //If game is over then end
                    if (P1_won)
                    {
                        agent1.AssignReward(1.0);
                        agent2.AssignReward(-1.0);
                        winCount++;
                        break;
                    } else if (P1_cat)
                    {
                        agent1.AssignReward(0.0);
                        agent2.AssignReward(0.0);
                        break;
                    }

                    //Now have player 2 play
                    //Player 2 plays randomly.
                    //var movesP2 = board.GetAvailableMoves();
                    //int selection2 = rnd.Next(movesP2.Count - 1);
                    //var P2_won = board.Move(movesP2[selection2], 2);
                    //var P2_cat = movesP2.Count == 1;

                    //Player 2 is also an RL agent
                    int action2 = agent2.DecideMove(board);
                    var P2_won = board.Move(action2, 2);
                    var P2_cat = board.GetAvailableMoves().Count == 0;

                    if (P2_won)
                    {
                        agent1.AssignReward(-1.0);
                        agent2.AssignReward(1.0);
                    }
                    else if (P2_cat)
                    {
                        agent1.AssignReward(0.0);
                        agent2.AssignReward(1.0);
                    }

                    if (interactive)
                    {
                        Console.Clear();
                        DrawBoard(board, agent2);

                        if (P2_won)
                            Console.WriteLine("Lost");
                        else if (P2_cat)
                            Console.WriteLine("Cat");
                        else
                            Console.ReadKey(true);
                    }

                    //End of game
                    if (P2_won || P2_cat)
                        break;

                } // end of epoch

                

                gameCount++;
                Console.WriteLine($"Epoch {epoch}   Explor: {agent1.Exploration:0.0000}   win ratio: {(double)winCount / gameCount:0.000}");

                if (interactive)
                    Console.ReadKey(true);

                if (!interactive && epoch > 89950)
                    interactive = true;
            }

            Console.ReadKey();
        }

        


        /// <summary>
        /// Utility method to draw the board.  Also shows the Q learning table rewards.
        /// </summary>
        /// <param name="board"></param>
        public static void DrawBoard(Board board, Agent agent)
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
                        var reward = agent.Q[board.GetHashCode(), board.XYToAction(x, y)];
                        Console.Write("{0:0.0000}    ", reward);
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
