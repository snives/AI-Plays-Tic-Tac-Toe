using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlaysTicTacToe
{
    class PlayVsRandom
    {

        static void Main(string[] args)
        {
            //First we define the training variables
            int trainingEpochs = 90000;
            bool interactive = false;
            int winCount = 0;
            int gameCount = 0;

            //Player 1
            IAgent agent1 = new Agent();
            IAgent agent2 = new RandomAgent();


            //Loop for a number of training epochs
            for (int epoch = 1; epoch < trainingEpochs; epoch++)
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
                    //Step 1: Ask player 1 to move
                    int action = agent1.DecideMove(board);

                    //Step 2: Make move, returns observation of environment
                    var P1_won = board.Move(action, 1);
                    var P1_cat = board.GetAvailableMoves().Count == 0;

                    if (interactive)
                    {
                        Console.Clear();
                        board.DrawBoard(board);

                        if (P1_won)
                            Console.WriteLine("Won");
                        else if (P1_cat)
                            Console.WriteLine("Cat");
                        else
                            Console.ReadKey(true);
                    }

                    //Step 3: Assign rewards based upon the outcome of the game.
                    //Notice that in this impelementation we assign rewards at the end of the game.
                    if (P1_won)
                    {
                        agent1.AssignReward(1.0);
                        agent2.AssignReward(-1.0);
                        winCount++;
                        break;
                    }
                    else if (P1_cat)
                    {
                        agent1.AssignReward(0.0);
                        agent2.AssignReward(0.0);
                        break;
                    }

                    //Now have player 2 play
                    //Player 2 plays randomly.
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
                        board.DrawBoard(board);
                        ((Agent)agent1).DisplayQTable(board);

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
                Console.WriteLine($"Epoch {epoch}   Explor: {((Agent)agent1).Exploration:0.0000}   win ratio: {(double)winCount / gameCount:0.000}");

                if (interactive)
                    Console.ReadKey(true);

                if (!interactive && epoch > 89950)
                    interactive = true;
            }

            Console.ReadKey();
        }


    }
}
