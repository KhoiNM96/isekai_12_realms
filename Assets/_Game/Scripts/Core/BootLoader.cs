using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Isekai12Realms.Core
{
    public class BootLoader : MonoBehaviour
    {
        private const float MinimumLoadSeconds = 5f;

        [Header("Fallback Overlay Layout")]
        [SerializeField] private Vector2 loadingTextPosition = new Vector2(0f, -250f);
        [SerializeField] private Vector2 loadingBarPosition = new Vector2(0f, -330f);
        [SerializeField] private Vector2 loadingGlowPosition = new Vector2(0f, -470f);
        [SerializeField] private Vector2 percentTextPosition = new Vector2(0f, 20f);
        [SerializeField] private Vector2 progressBarPosition = new Vector2(0f, 74f);
        [SerializeField] private Vector2 progressBarSize = new Vector2(688f, 22f);
        [SerializeField] private Vector2 progressBarBackgroundSize = new Vector2(700f, 34f);

        private GameObject overlayRoot;
        private RawImage fillImage;
        private TextMeshProUGUI percentText;
        private Coroutine loadRoutine;
        private RectTransform fillRect;
        private BootLoadingSceneUI sceneUi;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void BeginLoad(string sceneName)
        {
            if (loadRoutine != null)
            {
                return;
            }

            sceneUi = FindSceneUi();
            if (sceneUi != null)
            {
                sceneUi.BeginLoading();
            }
            else
            {
                CreateOverlay();
            }

            loadRoutine = StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            yield return null;

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loadOperation == null)
            {
                Debug.LogError("[Boot] Could not start loading scene: " + sceneName);
                yield break;
            }

            loadOperation.allowSceneActivation = false;

            float loadingDuration = sceneUi != null ? sceneUi.LoadingDuration : MinimumLoadSeconds;

            float elapsed = 0f;
            while (!loadOperation.isDone)
            {
                elapsed += Time.unscaledDeltaTime;

                float timeProgress = Mathf.Clamp01(elapsed / loadingDuration);
                float shownProgress = timeProgress;

                UpdateProgress(shownProgress);

                if (loadOperation.progress >= 0.9f && elapsed >= loadingDuration)
                {
                    UpdateProgress(1f);
                    loadOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            UpdateProgress(1f);
            sceneUi?.FinishLoading();
            yield return null;

            if (overlayRoot != null)
            {
                Destroy(overlayRoot);
            }

            Destroy(this);
        }

        private void CreateOverlay()
        {
            if (overlayRoot != null)
            {
                return;
            }

            overlayRoot = new GameObject("BootLoadingOverlay");
            overlayRoot.transform.SetParent(transform, false);

            Canvas canvas = overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            overlayRoot.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            overlayRoot.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080f, 1920f);
            overlayRoot.AddComponent<GraphicRaycaster>();

            RawImage background = CreateLayer("Background", overlayRoot.transform, LoadTexture("BootLoading/loading_background"), Vector2.zero, true);
            background.color = Color.white;

            RawImage textLayer = CreateLayer("LoadingText", overlayRoot.transform, LoadTexture("BootLoading/loading_text"), loadingTextPosition, false);
            textLayer.color = Color.white;

            RawImage barLayer = CreateLayer("LoadingBar", overlayRoot.transform, LoadTexture("BootLoading/loading_bar"), loadingBarPosition, false);
            barLayer.color = Color.white;

            RawImage iconGlowLayer = CreateLayer("IconGlow", overlayRoot.transform, LoadTexture("BootLoading/loading_icon_glow"), loadingGlowPosition, false);
            iconGlowLayer.color = Color.white;

            GameObject panel = CreateUiObject("Panel", overlayRoot.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900f, 520f);
            panelRect.anchoredPosition = new Vector2(0f, -260f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0f);

            GameObject barBackground = CreateUiObject("BarBackground", panel.transform);
            RectTransform barBackgroundRect = barBackground.AddComponent<RectTransform>();
            barBackgroundRect.anchorMin = barBackgroundRect.anchorMax = new Vector2(0.5f, 0f);
            barBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
            barBackgroundRect.sizeDelta = progressBarBackgroundSize;
            barBackgroundRect.anchoredPosition = progressBarPosition;
            Image barBgImage = barBackground.AddComponent<Image>();
            barBgImage.color = Color.white;

            GameObject fill = CreateUiObject("BarFill", barBackground.transform);
            fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(0f, 0.5f);
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.sizeDelta = new Vector2(0f, progressBarSize.y);
            fillRect.anchoredPosition = new Vector2(6f, 0f);
            fillImage = fill.AddComponent<RawImage>();
            fillImage.texture = CreateWhiteTexture();
            fillImage.color = new Color(0.18f, 0.62f, 1f, 1f);

            GameObject percent = CreateTextObject("Percent", panel.transform, "0%", 42, Color.white);
            RectTransform percentRect = percent.GetComponent<RectTransform>();
            percentRect.anchorMin = percentRect.anchorMax = new Vector2(0.5f, 0f);
            percentRect.pivot = new Vector2(0.5f, 0.5f);
            percentRect.sizeDelta = new Vector2(220f, 56f);
            percentRect.anchoredPosition = percentTextPosition;

            percentText = percent.GetComponent<TextMeshProUGUI>();
            UpdateProgress(0f);
        }

        private void UpdateProgress(float progress)
        {
            float normalized = Mathf.Clamp01(progress);
            if (sceneUi != null)
            {
                sceneUi.SetProgress(normalized);
                return;
            }

            if (fillImage != null)
            {
                fillRect.sizeDelta = new Vector2(progressBarSize.x * normalized, progressBarSize.y);
            }

            if (percentText != null)
            {
                percentText.text = Mathf.RoundToInt(normalized * 100f) + "%";
            }
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static RawImage CreateLayer(string name, Transform parent, Texture2D texture, Vector2 anchoredPosition, bool stretch)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            if (stretch)
            {
                Stretch(rect);
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                if (texture != null)
                {
                    rect.sizeDelta = new Vector2(texture.width, texture.height);
                }
                rect.anchoredPosition = anchoredPosition;
            }
            RawImage raw = go.AddComponent<RawImage>();
            raw.texture = texture;
            raw.color = texture != null ? Color.white : new Color(0f, 0f, 0f, 0f);
            raw.raycastTarget = false;
            return raw;
        }

        private static Texture2D LoadTexture(string resourcePath)
        {
            return Resources.Load<Texture2D>(resourcePath);
        }

        private static Texture2D CreateWhiteTexture()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private static BootLoadingSceneUI FindSceneUi()
        {
            BootLoadingSceneUI[] all = Resources.FindObjectsOfTypeAll<BootLoadingSceneUI>();
            for (int i = 0; i < all.Length; i++)
            {
                BootLoadingSceneUI ui = all[i];
                if (ui != null && ui.gameObject.scene.IsValid())
                {
                    return ui;
                }
            }

            return null;
        }

        private static GameObject CreateTextObject(string name, Transform parent, string text, int fontSize, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            return go;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
