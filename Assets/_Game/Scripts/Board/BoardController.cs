using System;
using System.Collections.Generic;
using Isekai12Realms.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Board
{
    public class BoardController : MonoBehaviour
    {
        private readonly System.Random random = new System.Random();
        private TileData[,] tiles;
        private TileView[,] tileViews;
        private TileView selectedTile;
        private GridLayoutGroup gridLayout;
        private int comboCount;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsResolving { get; private set; }
        public event Action<List<MatchGroup>, bool, int> MatchesResolved;

        public void Initialize(int width, int height)
        {
            ClearBoard();
            Width = width;
            Height = height;
            tiles = new TileData[Width, Height];
            tileViews = new TileView[Width, Height];
            EnsureGridLayout();

            int attempts = 0;
            do
            {
                GenerateBoardData();
                attempts++;
            } while (!HasValidMove() && attempts < 50);

            if (!HasValidMove())
            {
                ShuffleBoard();
            }

            CreateTileViews();
            Debug.Log("[Board] Generated board");
        }

        public void TrySelectTile(TileView tileView)
        {
            if (tileView == null || tileView.Data == null || IsResolving)
            {
                return;
            }

            if (selectedTile == null)
            {
                selectedTile = tileView;
                selectedTile.SetSelected(true);
                return;
            }

            if (selectedTile == tileView)
            {
                selectedTile.SetSelected(false);
                selectedTile = null;
                return;
            }

            Vector2Int a = selectedTile.Data.position;
            Vector2Int b = tileView.Data.position;
            selectedTile.SetSelected(false);
            selectedTile = null;
            TrySwap(a, b);
        }

        public bool TrySwap(Vector2Int a, Vector2Int b)
        {
            if (!IsAdjacent(a, b) || !IsInside(a) || !IsInside(b))
            {
                return false;
            }

            Debug.Log("[Board] Swap");
            SwapTiles(a, b);
            List<MatchGroup> matches = FindMatches();
            if (matches.Count == 0)
            {
                SwapTiles(a, b);
                return false;
            }

            ResolveMatches();
            return true;
        }

        public List<MatchGroup> FindMatches()
        {
            List<LineMatch> lines = new List<LineMatch>();

            for (int y = 0; y < Height; y++)
            {
                int start = 0;
                for (int x = 1; x <= Width; x++)
                {
                    if (x == Width || tiles[x, y] == null || tiles[start, y] == null || tiles[x, y].type != tiles[start, y].type)
                    {
                        int count = x - start;
                        if (count >= 3 && tiles[start, y] != null)
                        {
                            lines.Add(CreateLine(tiles[start, y].type, start, y, count, true));
                        }
                        start = x;
                    }
                }
            }

            for (int x = 0; x < Width; x++)
            {
                int start = 0;
                for (int y = 1; y <= Height; y++)
                {
                    if (y == Height || tiles[x, y] == null || tiles[x, start] == null || tiles[x, y].type != tiles[x, start].type)
                    {
                        int count = y - start;
                        if (count >= 3 && tiles[x, start] != null)
                        {
                            lines.Add(CreateLine(tiles[x, start].type, x, start, count, false));
                        }
                        start = y;
                    }
                }
            }

            List<MatchGroup> groups = MergeLines(lines);
            if (groups.Count > 0)
            {
                Debug.Log("[Board] Match found");
            }
            return groups;
        }

        public void ResolveMatches()
        {
            if (IsResolving)
            {
                return;
            }

            IsResolving = true;
            comboCount = 0;
            bool anyExtraTurn = false;
            List<MatchGroup> allGroups = new List<MatchGroup>();

            while (true)
            {
                List<MatchGroup> groups = FindMatches();
                if (groups.Count == 0)
                {
                    break;
                }

                comboCount++;
                foreach (MatchGroup group in groups)
                {
                    if (group.createsSpecial)
                    {
                        anyExtraTurn = true;
                    }
                }

                allGroups.AddRange(groups);
                RemoveMatchedTiles(groups);
                DropAndRefill();
            }

            if (allGroups.Count > 0)
            {
                MatchesResolved?.Invoke(allGroups, anyExtraTurn, comboCount);
            }

            if (!HasValidMove())
            {
                ShuffleBoard();
            }

            IsResolving = false;
        }

        public void DropAndRefill()
        {
            for (int x = 0; x < Width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < Height; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        TileData data = tiles[x, y];
                        tiles[x, y] = null;
                        tiles[x, writeY] = data;
                        data.position = new Vector2Int(x, writeY);
                        writeY++;
                    }
                }

                while (writeY < Height)
                {
                    tiles[x, writeY] = new TileData(RandomTileType(), new Vector2Int(x, writeY));
                    writeY++;
                }
            }

            RefreshViews();
        }

        public bool HasValidMove()
        {
            if (tiles == null)
            {
                return false;
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector2Int a = new Vector2Int(x, y);
                    Vector2Int right = new Vector2Int(x + 1, y);
                    Vector2Int up = new Vector2Int(x, y + 1);
                    if (IsInside(right) && SwapCreatesMatch(a, right)) return true;
                    if (IsInside(up) && SwapCreatesMatch(a, up)) return true;
                }
            }
            return false;
        }

        public void ShuffleBoard()
        {
            int guard = 0;
            do
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        tiles[x, y] = new TileData(RandomTileType(), new Vector2Int(x, y));
                    }
                }
                guard++;
            } while ((FindMatches().Count > 0 || !HasValidMove()) && guard < 100);

            RefreshViews();
        }

        public void ClearBoard()
        {
            selectedTile = null;
            IsResolving = false;
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            tiles = null;
            tileViews = null;
        }

        public void DestroyRandomArea(int areaSize)
        {
            if (tiles == null)
            {
                return;
            }

            int cx = random.Next(Width);
            int cy = random.Next(Height);
            int radius = areaSize / 2;
            for (int x = cx - radius; x <= cx + radius; x++)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    if (IsInside(new Vector2Int(x, y)))
                    {
                        tiles[x, y] = null;
                    }
                }
            }

            DropAndRefill();
            ResolveMatches();
        }

        private void EnsureGridLayout()
        {
            gridLayout = GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = gameObject.AddComponent<GridLayoutGroup>();
            }
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = Width;
            gridLayout.cellSize = new Vector2(84f, 84f);
            gridLayout.spacing = new Vector2(8f, 8f);
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
        }

        private void GenerateBoardData()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TileType type;
                    int guard = 0;
                    do
                    {
                        type = RandomTileType();
                        guard++;
                    } while (WouldCreateInitialMatch(x, y, type) && guard < 50);

                    tiles[x, y] = new TileData(type, new Vector2Int(x, y));
                }
            }
        }

        private void CreateTileViews()
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    GameObject tileObject = new GameObject($"Tile_{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
                    tileObject.transform.SetParent(transform, false);
                    GameObject labelObject = new GameObject("Label", typeof(RectTransform));
                    labelObject.transform.SetParent(tileObject.transform, false);
                    RectTransform labelRect = labelObject.GetComponent<RectTransform>();
                    labelRect.anchorMin = Vector2.zero;
                    labelRect.anchorMax = Vector2.one;
                    labelRect.offsetMin = Vector2.zero;
                    labelRect.offsetMax = Vector2.zero;
                    TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
                    label.alignment = TextAlignmentOptions.Center;
                    label.fontSize = 26;

                    TileView view = tileObject.AddComponent<TileView>();
                    view.Initialize(this, tiles[x, y]);
                    tileViews[x, y] = view;
                }
            }
        }

        private void RefreshViews()
        {
            if (tileViews == null)
            {
                return;
            }

            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    TileView view = tileViews[x, y];
                    if (view == null)
                    {
                        continue;
                    }
                    view.transform.SetSiblingIndex((Height - 1 - y) * Width + x);
                    view.Bind(tiles[x, y]);
                    view.RefreshPosition();
                }
            }
        }

        private void RemoveMatchedTiles(List<MatchGroup> groups)
        {
            HashSet<Vector2Int> removed = new HashSet<Vector2Int>();
            foreach (MatchGroup group in groups)
            {
                Vector2Int specialPosition = group.createsSpecial && group.positions.Count > 0 ? group.positions[0] : new Vector2Int(-1, -1);
                foreach (Vector2Int position in group.positions)
                {
                    if (!IsInside(position) || removed.Contains(position)) continue;
                    if (position == specialPosition)
                    {
                        tiles[position.x, position.y].specialType = group.specialCreated;
                        continue;
                    }
                    tiles[position.x, position.y] = null;
                    removed.Add(position);
                }
            }
        }

        private List<MatchGroup> MergeLines(List<LineMatch> lines)
        {
            List<MatchGroup> groups = new List<MatchGroup>();
            bool[] used = new bool[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                if (used[i]) continue;
                MatchGroup group = new MatchGroup { tileType = lines[i].type };
                AddUnique(group.positions, lines[i].positions);
                used[i] = true;

                for (int j = i + 1; j < lines.Count; j++)
                {
                    if (used[j] || lines[j].type != group.tileType) continue;
                    if (Intersects(group.positions, lines[j].positions))
                    {
                        AddUnique(group.positions, lines[j].positions);
                        used[j] = true;
                    }
                }

                group.count = group.positions.Count;
                group.createsSpecial = group.count >= 4;
                group.specialCreated = GetSpecialType(group, lines[i]);
                groups.Add(group);
            }
            return groups;
        }

        private SpecialTileType GetSpecialType(MatchGroup group, LineMatch firstLine)
        {
            if (group.count > firstLine.positions.Count) return SpecialTileType.BombRune;
            if (group.count >= 5) return SpecialTileType.RealmCrystal;
            if (group.count == 4) return firstLine.horizontal ? SpecialTileType.RowRune : SpecialTileType.ColumnRune;
            return SpecialTileType.None;
        }

        private LineMatch CreateLine(TileType type, int startX, int startY, int count, bool horizontal)
        {
            LineMatch line = new LineMatch { type = type, horizontal = horizontal };
            for (int i = 0; i < count; i++)
            {
                line.positions.Add(horizontal ? new Vector2Int(startX + i, startY) : new Vector2Int(startX, startY + i));
            }
            return line;
        }

        private bool SwapCreatesMatch(Vector2Int a, Vector2Int b)
        {
            SwapDataOnly(a, b);
            bool result = FindMatches().Count > 0;
            SwapDataOnly(a, b);
            return result;
        }

        private void SwapTiles(Vector2Int a, Vector2Int b)
        {
            SwapDataOnly(a, b);
            RefreshViews();
        }

        private void SwapDataOnly(Vector2Int a, Vector2Int b)
        {
            TileData temp = tiles[a.x, a.y];
            tiles[a.x, a.y] = tiles[b.x, b.y];
            tiles[b.x, b.y] = temp;
            tiles[a.x, a.y].position = a;
            tiles[b.x, b.y].position = b;
        }

        private bool WouldCreateInitialMatch(int x, int y, TileType type)
        {
            bool horizontal = x >= 2 && tiles[x - 1, y] != null && tiles[x - 2, y] != null && tiles[x - 1, y].type == type && tiles[x - 2, y].type == type;
            bool vertical = y >= 2 && tiles[x, y - 1] != null && tiles[x, y - 2] != null && tiles[x, y - 1].type == type && tiles[x, y - 2].type == type;
            return horizontal || vertical;
        }

        private TileType RandomTileType()
        {
            return (TileType)random.Next(Enum.GetValues(typeof(TileType)).Length);
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }

        private bool IsInside(Vector2Int p)
        {
            return p.x >= 0 && p.y >= 0 && p.x < Width && p.y < Height;
        }

        private static void AddUnique(List<Vector2Int> target, List<Vector2Int> source)
        {
            foreach (Vector2Int position in source)
            {
                if (!target.Contains(position)) target.Add(position);
            }
        }

        private static bool Intersects(List<Vector2Int> a, List<Vector2Int> b)
        {
            foreach (Vector2Int position in a)
            {
                if (b.Contains(position)) return true;
            }
            return false;
        }

        private class LineMatch
        {
            public TileType type;
            public bool horizontal;
            public List<Vector2Int> positions = new List<Vector2Int>();
        }
    }
}
