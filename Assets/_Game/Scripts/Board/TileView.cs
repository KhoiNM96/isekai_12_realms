using Isekai12Realms.Data;
using Isekai12Realms.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Board
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI debugText;
        [SerializeField] private Color selectedColor = Color.white;

        private Color baseColor;
        private Vector3 baseScale = Vector3.one;
        private Coroutine motionRoutine;
        private Coroutine visualRoutine;
        private BoardController owner;

        public TileData Data { get; private set; }
        public Vector2Int BoardPosition => Data != null ? Data.position : new Vector2Int(-1, -1);

        public void Initialize(BoardController controller, TileData data)
        {
            owner = controller;
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<TextMeshProUGUI>();
            }

            Button button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);
            Bind(data, controller);
        }

        public void Bind(TileData data)
        {
            Bind(data, owner);
        }

        public void Bind(TileData data, BoardController controller)
        {
            Data = data;
            owner = controller;
            transform.name = Data != null ? $"Tile_{Data.position.x}_{Data.position.y}" : "Tile_Empty";
            RefreshVisual();
            UpdateDebugLabel();
        }

        public void RefreshVisual()
        {
            if (background == null)
            {
                background = GetComponent<Image>();
            }

            if (label == null)
            {
                label = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (Data == null)
            {
                if (background != null)
                {
                    background.sprite = null;
                    background.color = Color.clear;
                }
                if (label != null) label.text = string.Empty;
                if (debugText != null) debugText.text = string.Empty;
                UpdateDebugLabel();
                return;
            }

            string assetId = GetAssetId(Data.type);
            Sprite tokenSprite = AssetSpriteBinder.HasAsset(assetId) ? AssetSpriteBinder.GetSprite(assetId) : null;
            baseColor = tokenSprite != null ? Color.white : GetColor(Data.type);
            background.sprite = tokenSprite;
            background.preserveAspect = tokenSprite != null;
            background.color = baseColor;
            if (label != null)
            {
                label.text = tokenSprite != null ? string.Empty : GetLabel(Data);
                label.color = Color.white;
            }
            UpdateDebugLabel();
        }

        public void SetBoardPosition(Vector2Int position)
        {
            if (Data != null)
            {
                Data.position = position;
            }
            UpdateDebugLabel();
        }

        public void SetSelected(bool selected)
        {
            background.color = selected ? Color.Lerp(baseColor, selectedColor, 0.55f) : baseColor;
            transform.localScale = selected ? baseScale * 1.08f : baseScale;
        }

        public void PlaySwapMove(Vector3 targetPosition, float duration)
        {
            StartMotion(MoveTo(targetPosition, duration));
        }

        public void PlayInvalidShake()
        {
            StartMotion(Shake(0.16f, 14f));
        }

        public void PlayMatchPop()
        {
            StartVisual(Pop(0.14f));
        }

        public void PlayDropMove(Vector3 targetPosition, float duration)
        {
            StartMotion(MoveTo(targetPosition, duration));
        }

        public void PlaySpawn()
        {
            StartVisual(Spawn(0.18f));
        }

        public void PlayHintPulse()
        {
            StartVisual(Pulse(0.4f));
        }

        public void RefreshPosition()
        {
            if (Data != null)
            {
                transform.name = $"Tile_{Data.position.x}_{Data.position.y}";
            }
            UpdateDebugLabel();
        }

        public void SetDebugVisible(bool visible)
        {
            EnsureDebugText();
            debugText.gameObject.SetActive(visible);
            UpdateDebugLabel();
        }

        public string CurrentSpriteName()
        {
            return background != null && background.sprite != null ? background.sprite.name : string.Empty;
        }

        private void OnClicked()
        {
            owner?.TrySelectTile(this);
        }

        private void EnsureDebugText()
        {
            if (debugText != null) return;
            Transform existing = transform.Find("DebugText");
            if (existing != null)
            {
                debugText = existing.GetComponent<TextMeshProUGUI>();
                if (debugText != null) return;
            }

            GameObject debugObject = new GameObject("DebugText", typeof(RectTransform));
            debugObject.transform.SetParent(transform, false);
            RectTransform rect = debugObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            debugText = debugObject.AddComponent<TextMeshProUGUI>();
            debugText.alignment = TextAlignmentOptions.Bottom;
            debugText.fontSize = 12;
            debugText.color = Color.black;
            debugText.raycastTarget = false;
            debugText.gameObject.SetActive(false);
        }

        private void UpdateDebugLabel()
        {
            if (debugText == null || Data == null) return;
            string type = Data.type.ToString();
            string shortType = string.IsNullOrEmpty(type) ? "?" : type.Substring(0, 1);
            debugText.text = $"{shortType} {Data.position.x},{Data.position.y} {Data.debugId}";
        }

        private void StartMotion(IEnumerator routine)
        {
            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
            }
            motionRoutine = StartCoroutine(routine);
        }

        private void StartVisual(IEnumerator routine)
        {
            if (visualRoutine != null)
            {
                StopCoroutine(visualRoutine);
            }
            visualRoutine = StartCoroutine(routine);
        }

        private IEnumerator MoveTo(Vector3 target, float duration)
        {
            Vector3 start = transform.localPosition;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t / duration));
                yield return null;
            }
            transform.localPosition = target;
        }

        private IEnumerator Shake(float duration, float strength)
        {
            Vector3 start = transform.localPosition;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.localPosition = start + Vector3.right * Mathf.Sin(t * 90f) * strength;
                yield return null;
            }
            transform.localPosition = start;
        }

        private IEnumerator Pop(float duration)
        {
            Color startColor = background.color;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float n = t / duration;
                transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.22f, n);
                Color c = startColor;
                c.a = 1f - n;
                background.color = c;
                yield return null;
            }
            transform.localScale = baseScale;
            background.color = startColor;
        }

        private IEnumerator Spawn(float duration)
        {
            Vector3 start = baseScale * 0.35f;
            Color startColor = baseColor;
            startColor.a = 0f;
            background.color = startColor;
            if (label != null)
            {
                Color labelColor = label.color;
                labelColor.a = 0f;
                label.color = labelColor;
            }
            transform.localScale = start;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(start, baseScale, Mathf.SmoothStep(0f, 1f, t / duration));
                Color c = baseColor;
                c.a = Mathf.Lerp(0f, 1f, t / duration);
                background.color = c;
                if (label != null)
                {
                    Color labelColor = label.color;
                    labelColor.a = Mathf.Lerp(0f, 1f, t / duration);
                    label.color = labelColor;
                }
                yield return null;
            }
            transform.localScale = baseScale;
            background.color = baseColor;
            if (label != null)
            {
                Color labelColor = label.color;
                labelColor.a = 1f;
                label.color = labelColor;
            }
        }

        private IEnumerator Pulse(float duration)
        {
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float n = Mathf.Sin((t / duration) * Mathf.PI);
                transform.localScale = Vector3.Lerp(baseScale, baseScale * 1.12f, n);
                yield return null;
            }
            transform.localScale = baseScale;
        }

        private static string GetLabel(TileData data)
        {
            if (data.specialType == SpecialTileType.RealmCrystal) return "C";
            if (data.specialType == SpecialTileType.BombRune) return "B";
            if (data.specialType == SpecialTileType.RowRune) return "R";
            if (data.specialType == SpecialTileType.ColumnRune) return "L";
            return data.type.ToString().Substring(0, 1);
        }

        private static Color GetColor(TileType type)
        {
            switch (type)
            {
                case TileType.Sword: return new Color(0.25f, 0.8f, 0.82f, 1f);
                case TileType.Heart: return new Color(1f, 0.28f, 0.38f, 1f);
                case TileType.Coin: return new Color(1f, 0.78f, 0.22f, 1f);
                case TileType.Food: return new Color(0.45f, 0.85f, 0.32f, 1f);
                case TileType.Book: return new Color(0.62f, 0.42f, 0.95f, 1f);
                case TileType.Mana: return new Color(0.32f, 0.52f, 1f, 1f);
                case TileType.Shield: return new Color(0.58f, 0.64f, 0.72f, 1f);
                case TileType.Star: return new Color(1f, 0.92f, 0.25f, 1f);
                default: return Color.gray;
            }
        }

        private static string GetAssetId(TileType type)
        {
            switch (type)
            {
                case TileType.Sword: return "icon_token_sword";
                case TileType.Heart: return "icon_token_heart";
                case TileType.Coin: return "icon_token_coin";
                case TileType.Food: return "icon_token_food";
                case TileType.Book: return "icon_token_book";
                case TileType.Mana: return "icon_token_mana";
                case TileType.Shield: return "icon_token_shield";
                case TileType.Star: return "icon_token_star";
                default: return string.Empty;
            }
        }
    }
}
