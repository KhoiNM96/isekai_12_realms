using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Core
{
    [ExecuteAlways]
    public class BootLoadingSceneUI : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private Vector2 loadingTextPosition = new Vector2(0f, -250f);
        [SerializeField] private Vector2 loadingBarPosition = new Vector2(0f, -330f);
        [SerializeField] private Vector2 percentTextPosition = new Vector2(0f, 20f);
        [SerializeField] private Vector2 progressBarPosition = new Vector2(0f, 74f);
        [SerializeField] private Vector2 progressBarSize = new Vector2(688f, 22f);
        [SerializeField] private Vector2 progressBarBackgroundSize = new Vector2(700f, 34f);
        [SerializeField] private int progressBarCornerRadius = 4;
        [SerializeField] private string loadingLabel = "LOADING...";
        [SerializeField] private float loadingDuration = 5f;

        [Header("Refs")]
        [SerializeField] private RectTransform textRoot;
        [SerializeField] private RectTransform progressBarBackgroundRoot;
        [SerializeField] private RectTransform progressBarFillRoot;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI percentText;

        private RawImage backgroundImage;
        private Image progressBarBackgroundImage;
        private Image progressBarFillImage;
        private float currentProgress;
        private bool autoAnimating;
        private float autoStartTime;

        private void OnEnable()
        {
            EnsureHierarchy();
            SetProgress(currentProgress);
        }

        private void Update()
        {
            if (!Application.isPlaying || !autoAnimating)
            {
                return;
            }

            float duration = Mathf.Max(0.1f, loadingDuration);
            SetProgress(Mathf.Clamp01((Time.unscaledTime - autoStartTime) / duration));
        }

        private void OnValidate()
        {
            EnsureHierarchy();
            SetProgress(currentProgress);
        }

        public void EnsureHierarchy()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            backgroundImage = EnsureRawImage("Background", transform, "BootLoading/loading_background", true);
            backgroundImage.color = Color.white;

            loadingText = EnsureText("LoadingText", transform, loadingLabel, 56, new Color(1f, 0.92f, 0.72f, 1f));
            textRoot = loadingText.rectTransform;

            progressBarBackgroundRoot = EnsureRect("ProgressBarBackground", transform);
            progressBarBackgroundImage = EnsureImageComponent(progressBarBackgroundRoot.gameObject);
            progressBarBackgroundImage.sprite = CreateRoundedBarSprite(progressBarCornerRadius);
            progressBarBackgroundImage.type = Image.Type.Sliced;
            progressBarBackgroundImage.color = Color.white;

            progressBarFillRoot = EnsureRect("ProgressBarFill", progressBarBackgroundRoot);
            progressBarFillImage = EnsureImageComponent(progressBarFillRoot.gameObject);
            progressBarFillImage.sprite = CreateRoundedBarSprite(progressBarCornerRadius);
            progressBarFillImage.type = Image.Type.Sliced;
            progressBarFillImage.color = new Color(0.18f, 0.62f, 1f, 1f);

            percentText = EnsurePercentText();
        }

        public void SetProgress(float normalized)
        {
            currentProgress = Mathf.Clamp01(normalized);
            if (progressBarFillRoot != null)
            {
                progressBarFillRoot.sizeDelta = new Vector2(progressBarSize.x * currentProgress, progressBarSize.y);
            }

            if (percentText != null)
            {
                percentText.text = Mathf.RoundToInt(currentProgress * 100f) + "%";
            }
        }

        public void BeginLoading()
        {
            autoStartTime = Time.unscaledTime;
            autoAnimating = true;
            SetProgress(0f);
            Show();
        }

        public void FinishLoading()
        {
            autoAnimating = false;
            SetProgress(1f);
        }

        public float LoadingDuration => Mathf.Max(0.1f, loadingDuration);

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private static RawImage EnsureRawImage(string childName, Transform parent, string resourcePath, bool stretch)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject go = new GameObject(childName, typeof(RectTransform), typeof(RawImage));
                child = go.transform;
                child.SetParent(parent, false);
            }

            RectTransform rect = child.GetComponent<RectTransform>();
            if (stretch)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (!stretch && texture != null)
            {
                rect.sizeDelta = new Vector2(texture.width, texture.height);
            }

            RawImage image = child.GetComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform EnsureRect(string childName, Transform parent)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject go = new GameObject(childName, typeof(RectTransform));
                child = go.transform;
                child.SetParent(parent, false);
            }

            return child.GetComponent<RectTransform>();
        }

        private static Image EnsureImageComponent(GameObject go)
        {
            RawImage raw = go.GetComponent<RawImage>();
            if (raw != null)
            {
#if UNITY_EDITOR
                Object.DestroyImmediate(raw);
#else
                Object.Destroy(raw);
#endif
            }

            CanvasRenderer renderer = go.GetComponent<CanvasRenderer>();
            if (renderer == null)
            {
                renderer = go.AddComponent<CanvasRenderer>();
            }

            Image image = go.GetComponent<Image>();
            if (image == null)
            {
                image = go.AddComponent<Image>();
            }

            return image;
        }

        private TextMeshProUGUI EnsurePercentText()
        {
            Transform child = transform.Find("PercentText");
            if (child == null)
            {
                GameObject go = new GameObject("PercentText", typeof(RectTransform), typeof(TextMeshProUGUI));
                child = go.transform;
                child.SetParent(transform, false);
            }

            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                tmp = child.gameObject.AddComponent<TextMeshProUGUI>();
            }
            if (child.GetComponent<CanvasRenderer>() == null)
            {
                child.gameObject.AddComponent<CanvasRenderer>();
            }
            tmp.text = Mathf.RoundToInt(currentProgress * 100f) + "%";
            tmp.fontSize = 42;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private TextMeshProUGUI EnsureText(string childName, Transform parent, string text, int fontSize, Color color)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject go = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                child = go.transform;
                child.SetParent(parent, false);
            }

            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null)
            {
                tmp = child.gameObject.AddComponent<TextMeshProUGUI>();
            }
            if (child.GetComponent<CanvasRenderer>() == null)
            {
                child.gameObject.AddComponent<CanvasRenderer>();
            }
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            return tmp;
        }

        private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
        }

        private static Texture2D CreateWhiteTexture()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static Sprite CreateRoundedBarSprite(int radius)
        {
            int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color32 opaque = new Color32(255, 255, 255, 255);
            Color32 transparent = new Color32(255, 255, 255, 0);
            int r = Mathf.Clamp(radius, 1, size / 2);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = x >= r && x < size - r || y >= r && y < size - r;
                    if (!inside)
                    {
                        float dx = x < r ? (r - x - 0.5f) : (x - (size - r - 0.5f));
                        float dy = y < r ? (r - y - 0.5f) : (y - (size - r - 0.5f));
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);
                        inside = distance <= r;
                    }

                    texture.SetPixel(x, y, inside ? opaque : transparent);
                }
            }

            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        }
    }
}
