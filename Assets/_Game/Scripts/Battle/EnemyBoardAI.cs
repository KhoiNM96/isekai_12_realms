using System;
using System.Collections.Generic;
using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.Battle
{
    public class EnemyBoardAI
    {
        private readonly System.Random random = new System.Random();

        public EnemyMoveChoice ChooseMove(TileData[,] board, BattleState state)
        {
            List<EnemyMoveChoice> moves = FindAllValidMoves(board, state);
            Debug.Log($"[EnemyAI] Valid moves found: {moves.Count}");
            if (moves.Count == 0) return null;

            moves.Sort((a, b) => b.score.CompareTo(a.score));
            int pickCount = 3;
            string difficulty = state != null && state.stage != null && state.stage.enemy != null ? state.stage.enemy.difficulty.ToString() : "Normal";
            if (difficulty.IndexOf("Easy", StringComparison.OrdinalIgnoreCase) >= 0) pickCount = 5;
            if (difficulty.IndexOf("Hard", StringComparison.OrdinalIgnoreCase) >= 0 || difficulty.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0) pickCount = 1;
            EnemyMoveChoice choice = moves[random.Next(Mathf.Min(pickCount, moves.Count))];
            Debug.Log($"[EnemyAI] Selected move from {choice.from} to {choice.to}, score={choice.score}");
            return choice;
        }

        public List<EnemyMoveChoice> FindAllValidMoves(TileData[,] board)
        {
            return FindAllValidMoves(board, null);
        }

        private List<EnemyMoveChoice> FindAllValidMoves(TileData[,] board, BattleState state)
        {
            List<EnemyMoveChoice> moves = new List<EnemyMoveChoice>();
            if (board == null) return moves;
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TryAddMove(board, state, moves, new Vector2Int(x, y), new Vector2Int(x + 1, y));
                    TryAddMove(board, state, moves, new Vector2Int(x, y), new Vector2Int(x, y + 1));
                }
            }
            return moves;
        }

        public int ScoreMove(EnemyMoveChoice move)
        {
            return move != null ? move.score : 0;
        }

        private void TryAddMove(TileData[,] board, BattleState state, List<EnemyMoveChoice> moves, Vector2Int from, Vector2Int to)
        {
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            if (to.x < 0 || to.y < 0 || to.x >= width || to.y >= height) return;
            if (board[from.x, from.y] == null || board[to.x, to.y] == null) return;

            TileType[,] types = CopyTypes(board);
            TileType temp = types[from.x, from.y];
            types[from.x, from.y] = types[to.x, to.y];
            types[to.x, to.y] = temp;

            List<SimMatch> matches = FindMatches(types);
            if (matches.Count == 0) return;

            EnemyMoveChoice move = new EnemyMoveChoice
            {
                from = from,
                to = to,
                createsMatch = true
            };
            foreach (SimMatch match in matches)
            {
                if (match.count > move.maxMatchSize) move.maxMatchSize = match.count;
                if (match.count >= 5) move.createsMatch5 = true;
                move.score += ScoreMatch(match, state);
            }
            if (move.createsMatch5) move.score += 1000;
            moves.Add(move);
        }

        private static int ScoreMatch(SimMatch match, BattleState state)
        {
            int score = match.count >= 4 ? 50 : 30;
            switch (match.type)
            {
                case TileType.Sword:
                    score += 300;
                    break;
                case TileType.Heart:
                    if (state != null && state.enemyMaxHp > 0 && state.enemyHp <= state.enemyMaxHp * 0.5f) score += 250;
                    break;
                case TileType.Mana:
                    score += 200;
                    break;
                case TileType.Shield:
                    score += 150;
                    break;
                case TileType.Star:
                    score += 100;
                    break;
            }
            return score;
        }

        private static TileType[,] CopyTypes(TileData[,] board)
        {
            int width = board.GetLength(0);
            int height = board.GetLength(1);
            TileType[,] copy = new TileType[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    copy[x, y] = board[x, y].type;
                }
            }
            return copy;
        }

        private static List<SimMatch> FindMatches(TileType[,] types)
        {
            List<SimMatch> matches = new List<SimMatch>();
            int width = types.GetLength(0);
            int height = types.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                int start = 0;
                for (int x = 1; x <= width; x++)
                {
                    if (x == width || types[x, y] != types[start, y])
                    {
                        int count = x - start;
                        if (count >= 3) matches.Add(new SimMatch(types[start, y], count));
                        start = x;
                    }
                }
            }
            for (int x = 0; x < width; x++)
            {
                int start = 0;
                for (int y = 1; y <= height; y++)
                {
                    if (y == height || types[x, y] != types[x, start])
                    {
                        int count = y - start;
                        if (count >= 3) matches.Add(new SimMatch(types[x, start], count));
                        start = y;
                    }
                }
            }
            return matches;
        }

        private struct SimMatch
        {
            public readonly TileType type;
            public readonly int count;

            public SimMatch(TileType type, int count)
            {
                this.type = type;
                this.count = count;
            }
        }
    }
}
