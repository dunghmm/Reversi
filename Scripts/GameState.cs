using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public const int Rows = 8;
    public const int Cols = 8;

    public Player[,] Board { get; }
    public Dictionary<Player, int> DiscCount { get; }
    public Player CurrentPlayer { get; private set; }
    public bool GameOver { get; set; }
    public Player Winner { get; private set; }
    public Dictionary<Position, List<Position>> LegalMoves { get; private set; }
    public Stack<MoveInfo> MoveHistory { get; private set; }

    public Stack<MoveInfo> RedoHistory { get; private set; }

    public GameState()
    {
        Board = new Player[Rows, Cols];
        Board[3, 3] = Player.White;
        Board[3, 4] = Player.Black;
        Board[4, 3] = Player.Black;
        Board[4, 4] = Player.White;

        DiscCount = new Dictionary<Player, int>()
        {
            {Player.Black, 2 },
            {Player.White, 2 }
        };

        CurrentPlayer = Player.Black;
        LegalMoves = FindLegalMoves(CurrentPlayer);
        MoveHistory = new Stack<MoveInfo>();
        RedoHistory = new Stack<MoveInfo>();
    }

    public GameState(Player[,] board, Player player)
    {
        Board = new Player[Rows, Cols];
        DiscCount = new Dictionary<Player, int>()
        {
            {Player.Black, 0 },
            {Player.White, 0 }
        };

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Cols; j++)
            {
                Board[i, j] = board[i, j];
                if (board[i, j] != Player.None)
                {
                    DiscCount[board[i, j]]++;
                }
            }
        }
        CurrentPlayer = player.Opponent();
        PassTurn();
        MoveHistory = new Stack<MoveInfo>();
        RedoHistory = new Stack<MoveInfo>();
    }

    public bool MakeMove(Position pos, out MoveInfo moveInfo)
    {
        if (!LegalMoves.ContainsKey(pos))
        {
            Debug.Log("Wrong");
            moveInfo = null;
            return false;
        }

        Player movePlayer = CurrentPlayer;
        List<Position> outflanked = LegalMoves[pos];

        Board[pos.Row, pos.Col] = movePlayer;
        FlipDiscs(outflanked);
        UpdateDiscCounts(movePlayer, outflanked.Count);

        moveInfo = new MoveInfo { Player = movePlayer, Position = pos, Outflanked = outflanked };
        MoveHistory.Push(moveInfo);
        RedoHistory.Clear();
        PassTurn();
        return true;
    }

    public bool UndoMove()
    {
        if (MoveHistory.Count == 0)
        {
            return false;
        }

        MoveInfo moveInfo = MoveHistory.Pop();
        RedoHistory.Push(moveInfo);
        Player movePlayer = moveInfo.Player;
        Position pos = moveInfo.Position;
        List<Position> outflanked = moveInfo.Outflanked;

        Board[pos.Row, pos.Col] = Player.None;
        FlipDiscs(outflanked);
        DiscCount[movePlayer] -= (outflanked.Count + 1);
        DiscCount[movePlayer.Opponent()] += outflanked.Count;
        CurrentPlayer = movePlayer;
        LegalMoves = FindLegalMoves(movePlayer);
        
        return true;
    }

    public bool RedoMove()
    {
        if (RedoHistory.Count == 0)
        {
            return false;
        }

        MoveInfo moveInfo = RedoHistory.Pop();
        MoveHistory.Push(moveInfo);
        Player movePlayer = moveInfo.Player;
        Position pos = moveInfo.Position;
        List<Position> outflanked = moveInfo.Outflanked;

        CurrentPlayer = movePlayer;
        Board[pos.Row, pos.Col] = movePlayer;
        FlipDiscs(outflanked);
        UpdateDiscCounts(movePlayer, outflanked.Count);
        PassTurn();
        return true;

    }

    public IEnumerable<Position> OccupiedPositions()
    {
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Board[r, c] != Player.None)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }

    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position pos in positions)
        {
            Board[pos.Row, pos.Col] = Board[pos.Row, pos.Col].Opponent();
        }
    }

    private void UpdateDiscCounts(Player movePlayer, int outflankedCount)
    {
        DiscCount[movePlayer] += outflankedCount + 1;
        DiscCount[movePlayer.Opponent()] -= outflankedCount;
    }

    private void ChangePlayer()
    {
        CurrentPlayer = CurrentPlayer.Opponent();
        LegalMoves = FindLegalMoves(CurrentPlayer);
    }

    private Player FindWinner()
    {
        if (DiscCount[Player.Black] > DiscCount[Player.White])
        {
            return Player.Black;
        }
        if (DiscCount[Player.Black] < DiscCount[Player.White])
        {
            return Player.White;
        }

        return Player.None;
    }

    private void PassTurn()
    {
        ChangePlayer();

        if (LegalMoves.Count > 0)
        {
            return;
        }

        ChangePlayer();

        if (LegalMoves.Count == 0)
        {
            CurrentPlayer = Player.None;
            GameOver = true;
            Winner = FindWinner();
        }
    }

    private bool IsInsideBoard(int r, int c)
    {
        return r >= 0 && r < Rows && c >= 0 && c < Cols;
    }

    private List<Position> OutflankedInDir(Position pos, Player player, int rDelta, int cDelta)
    {
        List<Position> outflanked = new List<Position>();
        int r = pos.Row + rDelta;
        int c = pos.Col + cDelta;

        while (IsInsideBoard(r, c) && Board[r, c] != Player.None)
        {
            if (Board[r, c] == player.Opponent())
            {
                outflanked.Add(new Position(r, c));
                r += rDelta;
                c += cDelta;
            }
            else
            {
                return outflanked;
            }
        }

        return new List<Position>();
    }

    private List<Position> Outflanked(Position pos, Player player)
    {
        List<Position> outflanked = new List<Position>();

        for (int rDelta = -1; rDelta <= 1; rDelta++)
        {
            for (int cDelta = -1; cDelta <= 1; cDelta++)
            {
                if (rDelta == 0 && cDelta == 0)
                {
                    continue;
                }

                outflanked.AddRange(OutflankedInDir(pos, player, rDelta, cDelta));
            }
        }

        return outflanked;
    }
    
    private bool IsMoveLegal(Player player, Position pos, out List<Position> outflanked)
    {
        if (Board[pos.Row, pos.Col] != Player.None)
        {
            outflanked = null;
            return false;
        }

        outflanked = Outflanked(pos, player);
        return outflanked.Count > 0;
    }

    public Dictionary<Position, List<Position>> FindLegalMoves(Player player)
    {
        Dictionary<Position, List<Position>> legalMoves = new Dictionary<Position, List<Position>>();
        
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Position pos = new Position(r, c);
                
                if (IsMoveLegal(player, pos, out List<Position> outflanked))
                {
                    legalMoves[pos] = outflanked;
                }
            }
        }

        return legalMoves;
    }
}
