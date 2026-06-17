using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Isekai12Realms.Core
{
    public class BootLoader : MonoBehaviour
    {
        private const float MinimumLoadSeconds = 2.5f;

        private GameObject overlayRoot;
        private Image fillImage;
        private TextMeshProUGUI percentText;
        private Coroutine loadRoutine;

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

            CreateOverlay();
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

            float elapsed = 0f;
            while (!loadOperation.isDone)
            {
                elapsed += Time.unscaledDeltaTime;

                float sceneProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                float timeProgress = Mathf.Clamp01(elapsed / MinimumLoadSeconds);
                float shownProgress = Mathf.Clamp01(Mathf.Max(sceneProgress, timeProgress));

                UpdateProgress(shownProgress);

                if (sceneProgress >= 1f && elapsed >= MinimumLoadSeconds)
                {
                    UpdateProgress(1f);
                    loadOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            UpdateProgress(1f);
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

            GameObject background = CreateUiObject("Background", overlayRoot.transform);
            RectTransform bgRect = background.AddComponent<RectTransform>();
            Stretch(bgRect);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.08f, 0.16f, 1f);

            GameObject panel = CreateUiObject("Panel", overlayRoot.transform);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(880f, 360f);
            panelRect.anchoredPosition = new Vector2(0f, -40f);
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.12f, 0.22f, 0.92f);

            GameObject title = CreateTextObject("Title", panel.transform, "Loading...", 54, new Color(1f, 0.92f, 0.72f, 1f));
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(700f, 80f);
            titleRect.anchoredPosition = new Vector2(0f, -42f);

            GameObject subtitle = CreateTextObject("Subtitle", panel.transform, "Preparing the world", 28, new Color(0.92f, 0.96f, 1f, 0.9f));
            RectTransform subtitleRect = subtitle.GetComponent<RectTransform>();
            subtitleRect.anchorMin = subtitleRect.anchorMax = new Vector2(0.5f, 1f);
            subtitleRect.pivot = new Vector2(0.5f, 1f);
            subtitleRect.sizeDelta = new Vector2(700f, 50f);
            subtitleRect.anchoredPosition = new Vector2(0f, -108f);

            GameObject barBackground = CreateUiObject("BarBackground", panel.transform);
            RectTransform barBackgroundRect = barBackground.AddComponent<RectTransform>();
            barBackgroundRect.anchorMin = barBackgroundRect.anchorMax = new Vector2(0.5f, 0f);
            barBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
            barBackgroundRect.sizeDelta = new Vector2(680f, 34f);
            barBackgroundRect.anchoredPosition = new Vector2(0f, 78f);
            Image barBgImage = barBackground.AddComponent<Image>();
            barBgImage.color = new Color(0f, 0f, 0f, 0.45f);

            GameObject fill = CreateUiObject("BarFill", barBackground.transform);
            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);
            fillImage = fill.AddComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = 0;
            fillImage.fillAmount = 0f;
            fillImage.color = new Color(0.25f, 0.82f, 0.9f, 1f);

            GameObject percent = CreateTextObject("Percent", panel.transform, "0%", 42, Color.white);
            RectTransform percentRect = percent.GetComponent<RectTransform>();
            percentRect.anchorMin = percentRect.anchorMax = new Vector2(0.5f, 0f);
            percentRect.pivot = new Vector2(0.5f, 0.5f);
            percentRect.sizeDelta = new Vector2(220f, 56f);
            percentRect.anchoredPosition = new Vector2(0f, 18f);

            percentText = percent.GetComponent<TextMeshProUGUI>();
            UpdateProgress(0f);
        }

        private void UpdateProgress(float progress)
        {
            float normalized = Mathf.Clamp01(progress);
            if (fillImage != null)
            {
                fillImage.fillAmount = normalized;
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
