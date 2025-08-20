public class CpuSettings
{
    private static int depth;
    private static Player player = Player.None;
    static Player[,] Board;
    static Player CurrentPlayer;

    public void SetBoard(Player[,] setBoard)
    {
        Board = new Player[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Board[i, j] = setBoard[i, j];
            }
        }
    }

    public void SetCurrentPlayer(Player player)
    {
        CurrentPlayer = player;
    }


    public void None()
    {
        depth = 0;
        player = Player.None;
    }

    public void Easy()
    {
        depth = 1;
    }

    public void Medium()
    {
        depth = 3;
    }

    public void Hard()
    {
        depth = 5;
    }

    public void SetDepth(int n)
    {
        depth = n;
    }

    public int GetDepth()
    {
        return depth;
    }

    public void Black()
    {
        player = Player.Black;
    }

    public void White()
    {
        player = Player.White;
    }

    public Player GetPlayer()
    {
        return player;
    }

    public Player[,] GetBoard()
    {
        return Board;
    }
    public Player GetCurrentPlayer()
    {
        return CurrentPlayer;
    }
}
