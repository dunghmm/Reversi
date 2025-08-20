using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class BotLogic
{
    static int prunedTimes;

    static int nodesVisited;

    const int SplitAt = 3000;

    public Position BestMove;

    public const int w = GameState.Rows;

    int[][] weightSet = new int[][] {
                    new int[] {8, -85, -40, 10, 210, 520},
                    new int[] {8, -85, -40, 10, 210, 520},
                    new int[] {33, -50, -15, 4, 416, 2153},
                    new int[] {46, -50, -1, 3, 612, 4141},
                    new int[] {51, -50, 62, 3, 595, 3184},
                    new int[] {33, -5,  66, 2, 384, 2777},
                    new int[] {44, 50, 163, 0, 443, 2568},
                    new int[] {13, 50, 66, 0, 121, 986},
                    new int[] {4, 50, 31, 0, 27, 192},
                    new int[] {8, 500, 77, 0, 36, 299}};

    int[] timingSet = new int[] { 0, 55, 56, 57, 58, 59, 60, 61, 62, 63 };

    public int Eval(Player player, GameState gameState)
    {
        Dictionary<Position, List<Position>> legalMoves = gameState.FindLegalMoves(player);

        int score = 0;
        if (gameState.GameOver)
        {
            if (gameState.Winner == player)
            {
                return Int32.MaxValue;
            }
            else if (gameState.Winner == player.Opponent())
            {
                return Int32.MinValue;
            }
            else
            {
                return 0;
            }
        }

        int mobility = GetMobility(gameState, player);
        //int frontier = GetFrontier(gameState.Board, player);
        int pieces = GetPieces(gameState.DiscCount, player);
        int placement = GetPlacement(gameState.Board, player);
        //int stability = GetStability(gameState.Board, player);
        int cornerGrab = GetCornerGrab(legalMoves, player);

        int[][] weightSetForDiscCount = RealtimeEvaluator(weightSet, timingSet);
        int[] weights = weightSetForDiscCount[gameState.DiscCount[Player.Black] + gameState.DiscCount[Player.White]];

        if (weights[0] != 0)
        {
            score += weights[0] * mobility;
        }
        //if (weights[1] != 0)
        //{
        //    score += weights[1] * frontier;
        //}
        if (weights[2] != 0)
        {
            score += weights[2] * pieces;
        }
        if (weights[3] != 0)
        {
            score += weights[3] * placement;
        }
        //if (weights[4] != 0)
        //{
        //    score += weights[4] * stability;
        //}
        if (weights[5] != 0)
        {
            score += weights[5] * cornerGrab;
        }

        //Debug.Log(weights[0] + " times mobility " + mobility);
        //Debug.Log(weights[1] + " times frontier " + frontier);
        //Debug.Log(weights[2] + " times pieces " + pieces);
        //Debug.Log(weights[3] + " times placement " + placement);
        //Debug.Log(weights[4] + " times stability " + stability);
        //Debug.Log(weights[5] + " times cornerGrab " + cornerGrab);

        return score;
    }

    private int[][] RealtimeEvaluator(int[][] weightSet, int[] timingSet)
    {
        int[][] weightSetForDiscCount = new int[65][];

        for (int dc = 0; dc <= 64; dc++)
        {
            int w = 0;
            for (int i = 0; i < timingSet.Length; i++)
            {
                if (dc <= timingSet[i])
                {
                    w = i;
                    break;
                }
            }
            weightSetForDiscCount[dc] = weightSet[w];
        }
        return weightSetForDiscCount;
    }

    private int GetCornerGrab(Dictionary<Position, List<Position>> legalMoves, Player player)
    {
        if (legalMoves.ContainsKey(new Position(0, 0)) || legalMoves.ContainsKey(new Position(0, 7))
            || legalMoves.ContainsKey(new Position(7, 0)) || legalMoves.ContainsKey(new Position(7, 7)))
        {
            return 100;
        }
        return 0;
    }

    private int[,] SQUARE_SCORE = {
            {5000 , -10 , 8  ,  6 ,  6 , 8  , -10 ,  5000},
            {-10 , -25 ,  -4, -4 , -4 , -4 , -25 , -10 },
            {8   ,  -4 ,   6,   4,   4,   6,  -4 ,  8  },
            {6   ,  -4 ,   4,   0,   0,   4,  -4 ,  6  },
            {6   ,  -4 ,   4,   0,   0,   4,  -4 ,  6  },
            {8   ,  -4 ,   6,   4,   4,   6,  -4 ,  8  },
            {-10 , -25 ,  -4, -4 , -4 , -4 , -25 , -10 },
            {5000 , -10 , 8  ,  6 ,  6 , 8  , -10 ,  5000}};

    private int GetPlacement(Player[,] Board, Player player)
    {
        int myW = 0;
        int opW = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (Board[i,j] == player) myW += SQUARE_SCORE[i,j];
                if (Board[i,j] == player.Opponent()) opW += SQUARE_SCORE[i,j];
            }
        }

        return myW - opW;
    }

    private int GetPieces(Dictionary<Player, int> discCount, Player player)
    {
        int mySC = discCount[player];
        int opSC = discCount[player.Opponent()];

        return 100 * (mySC - opSC) / (mySC + opSC + 1);
    }

    private int GetMobility(GameState gameState, Player player)
    {
        int myMoveCount = gameState.FindLegalMoves(player).Count;
        int opMoveCount = gameState.FindLegalMoves(player.Opponent()).Count;

        return 100 * (myMoveCount - opMoveCount) / (myMoveCount + opMoveCount + 1);
    }

    private int GetFrontier(Player[,] Board, Player player)
    {
        int myF = GetPlayerFrontierCount(Board, player);
        int opF = GetPlayerFrontierCount(Board, player.Opponent());

        return 100 * (myF - opF) / (myF + opF + 1);
    }

    private int GetPlayerFrontierCount(Player[,] Board, Player player)
    {
        int playerFrontier = 0;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (Board[i,j] == Player.None && IsFrontierOf(i, j, Board, player))
                {
                        playerFrontier++;
                }
            }
        }
        return playerFrontier;
    }

    private bool IsFrontierOf(int i, int j, Player[,] Board, Player player)
    {
        for (int ii = -1; ii <= 1; ii++)
        {
            for (int jj = -1; jj <= 1; jj++)
            {
                if (ii == 0 && jj == 0)
                {
                    continue;
                }

                if (IsInsideBoard(i + ii, j + jj) && Board[i + ii, j + jj] == player)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private int GetStableDiscCount(Player[,] Board, Player player)
    {
        List<Position> stableDiscs = new List<Position>();
        bool finished = false;

        while (!finished)
        {
            finished = true;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Board[i, j] == player && IsStable(stableDiscs, new Position(i, j)) && !stableDiscs.Contains(new Position(i, j)))
                    {
                        stableDiscs.Add(new Position(i, j));
                        finished = false;
                    }
                }
            }
        }

        return stableDiscs.Count;
    }

    private int GetStability(Player[,] Board, Player player)
    {
        int myS = GetStableDiscCount(Board, player);
        int opS = GetStableDiscCount(Board, player.Opponent());
        return 100 * (myS - opS) / (myS + opS + 1);
    }

    private bool IsStable(List<Position> stableDiscs, Position pos)
    {
        return IsStableInDir(stableDiscs, pos, 1, 0) && IsStableInDir(stableDiscs, pos, 0, 1)
            && IsStableInDir(stableDiscs, pos, 1, -1) && IsStableInDir(stableDiscs, pos, 1, 1);
    }

    private bool IsStableInDir(List<Position> stableDiscs, Position pos, int i, int j)
    {
        return !IsInsideBoard(pos.Row + i, pos.Col + j) || !IsInsideBoard(pos.Row - i, pos.Col - j) ||
                stableDiscs.Contains(new Position(pos.Row + i, pos.Col + j)) || stableDiscs.Contains(new Position(pos.Row - i, pos.Col - j));
    }

    private bool IsInsideBoard(int r, int c)
    {
        return r >= 0 && r < 8 && c >= 0 && c < 8;
    }

    public int MinMaxAB(Player player, int alpha, int beta, int depth, GameState gameState)
    {
        nodesVisited++;
        bool max = player == gameState.CurrentPlayer;

        if (depth == 0 || gameState.GameOver)
        {
            return Eval(player, gameState);
        }

        List<Position> legalMoves = new List<Position>(gameState.LegalMoves.Keys);

        int nodeValue = max ? Int32.MinValue : Int32.MaxValue;

        foreach (Position pos in legalMoves)
        {
            gameState.MakeMove(pos, out MoveInfo moveInfo);
            int subValue = MinMaxAB(player, alpha, beta, depth - 1, gameState);
            gameState.UndoMove();
            gameState.GameOver = false;

            nodeValue = MinMax(subValue, nodeValue, max);

            if (AlphaBeta(alpha, beta, nodeValue, max))
            {
                prunedTimes++;
                break;
            }

            if (max)
            {
                alpha = Math.Max(alpha, nodeValue);
            }
            else
            {
                beta = Math.Min(beta, nodeValue);
            }
        }
        return nodeValue;
    }

    private bool AlphaBeta(int alpha, int beta, int nodeValue, bool max)
    {
        return (nodeValue > beta && max) || (nodeValue < alpha && !max);
    }

    private int MinMax(int subValue, int nodeValue, bool max)
    {
        return (max) ? Math.Max(subValue, nodeValue) : Math.Min(subValue, nodeValue);
    }

    public IEnumerator Solve(GameState gameState, Player player, int depth)
    {
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        int bestScore = Int32.MinValue;
        BestMove = new Position(-1, -1);

        Dictionary<Position, List<Position>> legalMoves = gameState.FindLegalMoves(player);

        if (gameState.CurrentPlayer != player)
        {
            yield break;
        }

        foreach (Position pos in legalMoves.Keys)
        {
            gameState.MakeMove(pos, out MoveInfo moveInfo);
            int childScore = MinMaxAB(player, Int32.MinValue, Int32.MaxValue, depth - 1, gameState);
            gameState.UndoMove();
            gameState.GameOver = false;
            Debug.Log("Position " + pos.Row + "and" + pos.Col + " has score " + childScore);

            if (childScore >= bestScore)
            {
                bestScore = childScore;
                BestMove = pos;
            }

            //yield return null;
        }
        Debug.Log("Nodes visited: " + nodesVisited);
        Debug.Log("Pruned times: " + prunedTimes);
        nodesVisited = 0;
        prunedTimes = 0;
        gameState.RedoHistory.Clear();

        watch.Stop();

        Debug.Log($"Execution Time: {watch.ElapsedMilliseconds} ms");
    }
}
