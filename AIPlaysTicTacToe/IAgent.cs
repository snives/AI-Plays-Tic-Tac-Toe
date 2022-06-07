namespace AIPlaysTicTacToe
{
    public interface IAgent
    {
        int PlayerId { get; set; }

        void AssignReward(double reward);
        int DecideMove(Board board);
        void NewGame();
    }
}