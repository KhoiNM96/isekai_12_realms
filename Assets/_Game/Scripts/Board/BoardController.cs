using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Isekai12Realms.Audio;
using Isekai12Realms.Battle;
using Isekai12Realms.Data;
using Isekai12Realms.VFX;
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
        private bool inputLocked;
        private bool turnInputLocked;
        private BattleAnimationSettings animationSettings;
        private AudioService audioService;
        private VFXService vfxService;
        private const int MaxCascadeLoops = 20;
        private const float CellSize = 84f;
        private const float CellSpacing = 8f;
        [SerializeField] private bool showTileDebugLabels;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsResolving { get; private set; }
        public bool InputLocked => inputLocked || turnInputLocked;
        public BoardResolveResult LastResolveResult { get; private set; }
        public event Action<List<MatchGroup>, bool, int> MatchesResolved;
        public event Action<BoardResolveResult, BattleTurnOwner> MoveResolved;
        public event Action<List<MatchGroup>, int> CascadeResolved;
        public event Action<string> BoardFeedback;

        public void Initialize(int width, int height)
        {
            ClearBoard();
            Width = width;
            Height = height;
            tiles = new TileData[Width, Height];
            tileViews = new TileView[Width, Height];
            animationSettings = animationSettings != null ? animationSettings : BattleAnimationSettings.CreateDefault();
            audioService = FindObjectOfType<AudioService>();
            vfxService = FindObjectOfType<VFXService>();
            EnsureGridLayout();

            int attempts = 0;
            do
            {
                GenerateBoardData();
                attempts++;
            } while ((FindMatches().Count > 0 || !HasValidMove()) && attempts < 50);

            if (FindMatches().Count > 0 || !HasValidMove())
            {
                GenerateBoardData();
            }

            CreateTileViews();
            if (FindMatches().Count > 0 || !HasValidMove())
            {
                ShuffleBoard();
            }
            ForceRebindAllViewsFromData();
            ValidateBoardState();
            Debug.Log("[Board] Generated board");
        }

        public void TrySelectTile(TileView tileView)
        {
            if (tileView == null || tileView.Data == null || IsResolving || InputLocked)
            {
                return;
            }

            if (selectedTile == null)
            {
                selectedTile = tileView;
                selectedTile.SetSelected(true);
                audioService?.PlaySfx("sfx_tile_select");
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

        public void SetInputLocked(bool locked)
        {
            turnInputLocked = locked;
        }

        public bool TrySwap(Vector2Int a, Vector2Int b)
        {
            if (!IsAdjacent(a, b) || !IsInside(a) || !IsInside(b))
            {
                audioService?.PlaySfx("sfx_tile_invalid");
                return false;
            }

            if (IsResolving || InputLocked) return false;
            StartCoroutine(ExecuteMoveRoutine(a, b, BattleTurnOwner.Player, true));
            return true;
        }

        public IEnumerator ExecuteMove(Vector2Int from, Vector2Int to, BattleTurnOwner owner)
        {
            yield return ExecuteMoveRoutine(from, to, owner, false);
        }

        [ContextMenu("Force Find Matches")]
        public void ForceFindMatches()
        {
            List<MatchGroup> groups = FindMatches();
            Debug.Log($"[Board] ForceFindMatches found {groups.Count} groups\n{DumpBoardTypes()}");
        }

        [ContextMenu("Force Resolve All Current Matches")]
        public void ForceResolveAllCurrentMatches()
        {
            if (IsResolving)
            {
                Debug.LogWarning("[Board] Cannot force resolve while board is already resolving.");
                return;
            }

            StartCoroutine(ResolveBoardUntilStable());
        }

        [ContextMenu("Toggle Tile Debug Labels")]
        public void ToggleTileDebugLabels()
        {
            showTileDebugLabels = !showTileDebugLabels;
            RefreshAllTileVisuals();
        }

        [ContextMenu("Repair Board View Sync")]
        public void ForceRebindAllViewsFromData()
        {
            SyncAllTileViewsFromData(true);
            RefreshAllTileVisuals();
            ValidateBoardState();
        }

        public TileView GetViewAt(Vector2Int pos)
        {
            return IsInside(pos) && tileViews != null ? tileViews[pos.x, pos.y] : null;
        }

        public TileData[,] GetBoardSnapshot()
        {
            if (tiles == null) return null;
            TileData[,] copy = new TileData[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null) continue;
                    TileData snapshot = new TileData(tile.type, tile.position)
                    {
                        specialType = tile.specialType,
                        locked = tile.locked,
                        freezeTurns = tile.freezeTurns,
                        debugId = tile.debugId
                    };
                    copy[x, y] = snapshot;
                }
            }
            return copy;
        }

        // Coordinate convention: x increases left to right, y increases bottom to top.
        public Vector3 GetCellLocalPosition(int x, int y)
        {
            float step = CellSize + CellSpacing;
            float totalWidth = Width * CellSize + Mathf.Max(0, Width - 1) * CellSpacing;
            float totalHeight = Height * CellSize + Mathf.Max(0, Height - 1) * CellSpacing;
            return new Vector3(-totalWidth * 0.5f + CellSize * 0.5f + x * step, -totalHeight * 0.5f + CellSize * 0.5f + y * step, 0f);
        }

        public Vector3 GetCellWorldOrLocalPosition(int x, int y)
        {
            return transform.TransformPoint(GetCellLocalPosition(x, y));
        }

        public List<MatchGroup> FindMatches()
        {
            List<LineMatch> lines = new List<LineMatch>();
            if (tiles == null)
            {
                return new List<MatchGroup>();
            }

            for (int y = 0; y < Height; y++)
            {
                int runStart = 0;
                int runCount = 0;
                TileType runType = TileType.Sword;
                for (int x = 0; x < Width; x++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null)
                    {
                        AddLineIfMatch(lines, runType, runStart, y, runCount, true);
                        runCount = 0;
                        continue;
                    }

                    if (runCount == 0 || tile.type != runType)
                    {
                        AddLineIfMatch(lines, runType, runStart, y, runCount, true);
                        runStart = x;
                        runType = tile.type;
                        runCount = 1;
                    }
                    else
                    {
                        runCount++;
                    }
                }
                AddLineIfMatch(lines, runType, runStart, y, runCount, true);
            }

            for (int x = 0; x < Width; x++)
            {
                int runStart = 0;
                int runCount = 0;
                TileType runType = TileType.Sword;
                for (int y = 0; y < Height; y++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null)
                    {
                        AddLineIfMatch(lines, runType, x, runStart, runCount, false);
                        runCount = 0;
                        continue;
                    }

                    if (runCount == 0 || tile.type != runType)
                    {
                        AddLineIfMatch(lines, runType, x, runStart, runCount, false);
                        runStart = y;
                        runType = tile.type;
                        runCount = 1;
                    }
                    else
                    {
                        runCount++;
                    }
                }
                AddLineIfMatch(lines, runType, x, runStart, runCount, false);
            }

            List<MatchGroup> groups = MergeLines(lines);
            return groups;
        }

        public void ResolveMatches()
        {
            ResolveMatches(true);
        }

        public void ResolveMatches(bool notifyMoveResolved)
        {
            if (IsResolving)
            {
                return;
            }

            StartCoroutine(ResolveBoardUntilStable(BattleTurnOwner.Player, notifyMoveResolved));
        }

        private IEnumerator TrySwapRoutine(Vector2Int a, Vector2Int b)
        {
            yield return ExecuteMoveRoutine(a, b, BattleTurnOwner.Player, true);
        }

        private IEnumerator ExecuteMoveRoutine(Vector2Int a, Vector2Int b, BattleTurnOwner owner, bool notifyMoveResolved)
        {
            Debug.Log($"[Board] ExecuteMove owner={owner}, from={a}, to={b}");
            LastResolveResult = new BoardResolveResult();
            if (IsResolving || !IsAdjacent(a, b) || !IsInside(a) || !IsInside(b) || tiles == null || tileViews == null)
            {
                audioService?.PlaySfx("sfx_tile_invalid");
                yield break;
            }

            IsResolving = true;
            inputLocked = true;
            TileView viewA = tileViews[a.x, a.y];
            TileView viewB = tileViews[b.x, b.y];
            if (viewA == null || viewB == null)
            {
                IsResolving = false;
                inputLocked = false;
                yield break;
            }

            Vector3 posA = viewA.transform.localPosition;
            Vector3 posB = viewB.transform.localPosition;
            audioService?.PlaySfx("sfx_tile_swap");
            viewA.PlaySwapMove(posB, animationSettings.tileSwapDuration);
            viewB.PlaySwapMove(posA, animationSettings.tileSwapDuration);
            yield return new WaitForSeconds(animationSettings.tileSwapDuration);

            SwapDataOnly(a, b);
            List<MatchGroup> matches = FindMatches();
            if (matches.Count == 0)
            {
                SwapDataOnly(a, b);
                viewA.PlayInvalidShake();
                viewB.PlayInvalidShake();
                viewA.PlaySwapMove(posA, animationSettings.invalidSwapReturnDuration);
                viewB.PlaySwapMove(posB, animationSettings.invalidSwapReturnDuration);
                audioService?.PlaySfx("sfx_tile_invalid");
                yield return new WaitForSeconds(animationSettings.invalidSwapReturnDuration);
                SyncAllTileViewsFromData(true);
                ValidateBoardState();
                IsResolving = false;
                inputLocked = false;
                Debug.Log("[Board] Input unlocked");
                yield break;
            }

            SyncAllTileViewsFromData(true);
            ValidateBoardState();
            yield return ResolveBoardUntilStable(owner, notifyMoveResolved);
        }

        public IEnumerator ResolveBoardUntilStable()
        {
            yield return ResolveBoardUntilStable(BattleTurnOwner.Player, true);
        }

        public IEnumerator ResolveBoardUntilStable(BattleTurnOwner owner, bool notifyMoveResolved)
        {
            Debug.Log("[Board] Resolve started");
            IsResolving = true;
            inputLocked = true;
            comboCount = 0;
            BoardResolveResult result = new BoardResolveResult();

            while (comboCount < MaxCascadeLoops)
            {
                List<MatchGroup> groups = FindMatches();
                if (groups.Count == 0)
                {
                    break;
                }

                comboCount++;
                Debug.Log($"[Board] Cascade pass {comboCount}, matches: {groups.Count}");
                CascadeResolved?.Invoke(groups, comboCount);
                audioService?.PlaySfx(comboCount > 1 ? "sfx_combo" : "sfx_match");
                foreach (MatchGroup group in groups)
                {
                    if (group.grantsExtraTurn)
                    {
                        BoardFeedback?.Invoke("Extra Turn!");
                    }
                    foreach (Vector2Int position in group.positions)
                    {
                        if (!IsInside(position) || tileViews[position.x, position.y] == null) continue;
                        TileView view = tileViews[position.x, position.y];
                        view.PlayMatchPop();
                        vfxService?.PlayTileMatchVfx(view.transform.position, group.tileType);
                    }
                }

                result.AddGroups(groups);
                result.cascadeCount = comboCount;
                if (comboCount >= 2) BoardFeedback?.Invoke("Combo x" + comboCount);
                yield return new WaitForSeconds(animationSettings.tilePopDuration);
                RemoveMatchedTiles(groups);
                Debug.Log("[Board] Dropping tiles");
                DropAndRefill(true);
                Debug.Log("[Board] Refilled tiles");
                ValidateBoardState(false);
                yield return new WaitForSeconds(animationSettings.tileDropDuration + animationSettings.cascadeDelay);
                SyncAllTileViewsFromData(true);
                ValidateBoardState();
            }

            if (comboCount >= MaxCascadeLoops && FindMatches().Count > 0)
            {
                Debug.LogError("[Board] Cascade loop exceeded safety limit");
                ShuffleBoard();
                yield return new WaitForSeconds(animationSettings.tileDropDuration);
            }

            LastResolveResult = result;
            if (result.hasAnyMatch)
            {
                MatchesResolved?.Invoke(result.allMatchGroups, result.grantsExtraTurn, result.cascadeCount);
            }

            if (!HasValidMove())
            {
                Debug.Log("[Board] No valid moves, shuffle");
                ShuffleBoard();
                yield return new WaitForSeconds(animationSettings.tileDropDuration);
                yield return EnsureBoardStableAfterShuffle();
            }

            Debug.Log($"[Board] Board stable, cascade count: {comboCount}");
            ValidateBoardState();
            IsResolving = false;
            inputLocked = false;
            Debug.Log("[Board] Input unlocked");
            if (result.hasAnyMatch && notifyMoveResolved)
            {
                MoveResolved?.Invoke(result, owner);
            }
        }

        public void DropAndRefill()
        {
            DropAndRefill(true);
        }

        private void DropAndRefill(bool animate)
        {
            List<DropVisual> drops = new List<DropVisual>();
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
                        if (writeY != y)
                        {
                            drops.Add(new DropVisual(new Vector2Int(x, writeY), y - writeY, false));
                        }
                        data.position = new Vector2Int(x, writeY);
                        writeY++;
                    }
                }

                while (writeY < Height)
                {
                    tiles[x, writeY] = new TileData(RandomTileType(), new Vector2Int(x, writeY));
                    drops.Add(new DropVisual(new Vector2Int(x, writeY), Height - writeY + 1, true));
                    writeY++;
                }
            }

            SyncAllTileViewsFromData(true);

            if (!animate)
            {
                ValidateBoardState();
                return;
            }

            float cellStep = CellSize + CellSpacing;
            foreach (DropVisual drop in drops)
            {
                TileView view = tileViews[drop.position.x, drop.position.y];
                if (view == null)
                {
                    continue;
                }

                Vector3 targetPosition = GetCellLocalPosition(drop.position.x, drop.position.y);
                Vector3 startPosition = targetPosition + Vector3.up * cellStep * Mathf.Max(1, drop.distance);
                view.transform.localPosition = startPosition;
                view.PlayDropMove(targetPosition, animationSettings.tileDropDuration);
                if (drop.isNewTile)
                {
                    view.PlaySpawn();
                }
            }
            ValidateBoardState(false);
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

            SyncAllTileViewsFromData(true);
            ValidateBoardState();
            BoardFeedback?.Invoke("Board Shuffled");
        }

        public void ClearBoard()
        {
            selectedTile = null;
            IsResolving = false;
            inputLocked = false;
            turnInputLocked = false;
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

            DropAndRefill(false);
            ResolveMatches(false);
        }

        public void DestroyRandomTiles(int count)
        {
            if (tiles == null || count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int guard = 0;
                while (guard < 64)
                {
                    int x = random.Next(Width);
                    int y = random.Next(Height);
                    if (tiles[x, y] != null)
                    {
                        tiles[x, y] = null;
                        break;
                    }
                    guard++;
                }
            }

            DropAndRefill(false);
            ResolveMatches(false);
        }

        public void PulseAllTiles()
        {
            if (tileViews == null) return;
            foreach (TileView view in tileViews)
            {
                view?.PlayHintPulse();
            }
        }

        public void RefreshAllTileVisuals()
        {
            if (tileViews == null) return;
            foreach (TileView view in tileViews)
            {
                if (view == null) continue;
                view.RefreshVisual();
                view.SetDebugVisible(showTileDebugLabels);
            }
        }

        public string DumpBoardTypes()
        {
            return DumpBoardDataTypes();
        }

        public string DumpBoardDataTypes()
        {
            if (tiles == null) return "<null board>";
            StringBuilder builder = new StringBuilder();
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    builder.Append(tiles[x, y] != null ? tiles[x, y].type.ToString()[0] : '.');
                    if (x < Width - 1) builder.Append(' ');
                }
                if (y > 0) builder.AppendLine();
            }
            return builder.ToString();
        }

        public string DumpBoardViewTypes()
        {
            if (tileViews == null) return "<null views>";
            StringBuilder builder = new StringBuilder();
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    TileView view = tileViews[x, y];
                    builder.Append(view != null && view.Data != null ? view.Data.type.ToString()[0] : '.');
                    if (x < Width - 1) builder.Append(' ');
                }
                if (y > 0) builder.AppendLine();
            }
            return builder.ToString();
        }

        public bool ValidateBoardState()
        {
            return ValidateBoardState(true);
        }

        private bool ValidateBoardState(bool validateVisualPosition)
        {
            if (tiles == null)
            {
                Debug.LogError("[Board] Validate failed: tiles array is null.");
                return false;
            }

            bool valid = true;

            if (tiles.GetLength(0) != Width || tiles.GetLength(1) != Height)
            {
                Debug.LogError($"[Board] Validate failed: tiles dimensions {tiles.GetLength(0)}x{tiles.GetLength(1)} do not match {Width}x{Height}.");
                return false;
            }

            if (tileViews != null && (tileViews.GetLength(0) != Width || tileViews.GetLength(1) != Height))
            {
                Debug.LogError($"[BoardValidation] tileViews dimensions mismatch: {tileViews.GetLength(0)}x{tileViews.GetLength(1)} expected {Width}x{Height}.");
                valid = false;
            }

            HashSet<TileData> seen = new HashSet<TileData>();
            HashSet<TileView> seenViews = new HashSet<TileView>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TileData tile = tiles[x, y];
                    if (tile == null)
                    {
                        Debug.LogError($"[Board] Validate failed: null tile at {x},{y}.\n{DumpBoardTypes()}");
                        valid = false;
                        continue;
                    }

                    Vector2Int expected = new Vector2Int(x, y);
                    if (tile.position != expected)
                    {
                        Debug.LogError($"[Board] Validate failed: tile at {x},{y} has position {tile.position}.");
                        valid = false;
                    }

                    if (!seen.Add(tile))
                    {
                        Debug.LogError($"[Board] Validate failed: duplicate TileData reference at {x},{y}.");
                        valid = false;
                    }

                    if (tileViews != null)
                    {
                        TileView view = tileViews[x, y];
                        if (view == null)
                        {
                            Debug.LogError($"[Board] Validate failed: null TileView at {x},{y}.");
                            valid = false;
                        }
                        else if (view.Data != tile)
                        {
                            Debug.LogError($"[BoardValidation] Mismatch at {x},{y}: data type = {tile.type}, view data type = {(view.Data != null ? view.Data.type.ToString() : "null")}, data pos = {tile.position}, view pos = {view.BoardPosition}, visual pos = {view.transform.localPosition}, sprite = {view.CurrentSpriteName()}");
                            valid = false;
                        }
                        else if (view.BoardPosition != expected)
                        {
                            Debug.LogError($"[BoardValidation] View position mismatch at {x},{y}: view pos = {view.BoardPosition}, expected = {expected}.");
                            valid = false;
                        }
                        else if (!seenViews.Add(view))
                        {
                            Debug.LogError($"[BoardValidation] Duplicate TileView reference at {x},{y}.");
                            valid = false;
                        }
                        else if (validateVisualPosition && Vector3.Distance(view.transform.localPosition, GetCellLocalPosition(x, y)) > 0.5f)
                        {
                            Debug.LogError($"[BoardValidation] Visual position mismatch at {x},{y}: data type = {tile.type}, view data type = {view.Data.type}, data pos = {tile.position}, view pos = {view.BoardPosition}, visual pos = {view.transform.localPosition}, expected visual pos = {GetCellLocalPosition(x, y)}, sprite = {view.CurrentSpriteName()}\nData:\n{DumpBoardDataTypes()}\nViews:\n{DumpBoardViewTypes()}");
                            valid = false;
                        }
                    }
                }
            }

            return valid;
        }

        private void EnsureGridLayout()
        {
            gridLayout = GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = gameObject.AddComponent<GridLayoutGroup>();
            }
            gridLayout.enabled = false;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = Width;
            gridLayout.cellSize = new Vector2(CellSize, CellSize);
            gridLayout.spacing = new Vector2(CellSpacing, CellSpacing);
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
                    tileViews[x, y] = CreateTileView(x, y);
                }
            }
        }

        private TileView CreateTileView(int x, int y)
        {
            GameObject tileObject = new GameObject($"Tile_{x}_{y}", typeof(RectTransform), typeof(Image), typeof(Button));
            tileObject.transform.SetParent(transform, false);
            RectTransform tileRect = tileObject.GetComponent<RectTransform>();
            tileRect.sizeDelta = new Vector2(CellSize, CellSize);
            tileRect.localPosition = GetCellLocalPosition(x, y);
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
            view.SetDebugVisible(showTileDebugLabels);
            return view;
        }

        private void SyncAllTileViewsFromData(bool snapVisualPositions)
        {
            if (tiles == null) return;
            if (tileViews == null || tileViews.GetLength(0) != Width || tileViews.GetLength(1) != Height)
            {
                tileViews = new TileView[Width, Height];
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TileData data = tiles[x, y];
                    TileView view = tileViews[x, y];
                    if (data == null)
                    {
                        if (view != null) view.gameObject.SetActive(false);
                        continue;
                    }

                    data.position = new Vector2Int(x, y);
                    if (view == null)
                    {
                        view = CreateTileView(x, y);
                        tileViews[x, y] = view;
                    }

                    view.gameObject.SetActive(true);
                    view.Bind(data, this);
                    view.RefreshVisual();
                    view.SetDebugVisible(showTileDebugLabels);
                    if (snapVisualPositions)
                    {
                        view.transform.localPosition = GetCellLocalPosition(x, y);
                    }
                    view.RefreshPosition();
                }
            }
        }

        private IEnumerator EnsureBoardStableAfterShuffle()
        {
            int guard = 0;
            while ((FindMatches().Count > 0 || !HasValidMove()) && guard < 10)
            {
                ShuffleBoard();
                guard++;
                yield return new WaitForSeconds(animationSettings.tileDropDuration);
            }

            if (FindMatches().Count > 0)
            {
                Debug.LogError("[Board] Shuffle could not produce a stable board.");
            }
            ValidateBoardState();
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
                group.grantsExtraTurn = group.count >= 5;
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

        private void AddLineIfMatch(List<LineMatch> lines, TileType type, int startX, int startY, int count, bool horizontal)
        {
            if (count >= 3)
            {
                lines.Add(CreateLine(type, startX, startY, count, horizontal));
            }
        }

        private bool SwapCreatesMatch(Vector2Int a, Vector2Int b)
        {
            SwapDataOnly(a, b);
            bool result = FindMatches().Count > 0;
            SwapDataOnly(a, b);
            return result;
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

        private struct DropVisual
        {
            public readonly Vector2Int position;
            public readonly int distance;
            public readonly bool isNewTile;

            public DropVisual(Vector2Int position, int distance, bool isNewTile)
            {
                this.position = position;
                this.distance = distance;
                this.isNewTile = isNewTile;
            }
        }
    }
}
