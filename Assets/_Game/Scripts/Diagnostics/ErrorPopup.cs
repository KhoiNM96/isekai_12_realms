using Isekai12Realms.Core;
using Isekai12Realms.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Diagnostics
{
    public class ErrorPopup : MonoBehaviour
    {
        private GameObject root;
        private TextMeshProUGUI bodyText;
        private string debugInfo;

        public void Initialize(Transform popupLayer)
        {
            if (popupLayer == null) return;
            root = new GameObject("ErrorPopup", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(popupLayer, false);
            if (root.GetComponent<CanvasGroup>() == null) root.AddComponent<CanvasGroup>();
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);

            GameObject panel = CreateChild(root.transform, "Panel", new Vector2(760f, 520f), Vector2.zero, new Color(1f, 0.95f, 0.84f, 0.98f));
            CreateText(panel.transform, "Title", "Something went wrong", 44, new Vector2(0f, 150f), new Vector2(650f, 80f));
            bodyText = CreateText(panel.transform, "Body", "Error", 28, new Vector2(0f, 45f), new Vector2(650f, 170f));
            CreateButton(panel.transform, "Button_OK", "OK", new Vector2(-150f, -160f), Hide);
            CreateButton(panel.transform, "Button_Copy", "Copy Debug Info", new Vector2(150f, -160f), CopyDebugInfo);
            root.SetActive(false);
        }

        public void Show(string message, string code, string debug = null)
        {
            debugInfo = debug;
            if (bodyText != null) bodyText.text = $"{message}\n\nCode: {code}";
            if (ServiceLocator.TryResolve<IPopupService>(out IPopupService popupService))
            {
                popupService.ShowPopup("ErrorPopup");
            }
            else if (root != null)
            {
                root.SetActive(true);
            }
        }

        public void Hide()
        {
            if (ServiceLocator.TryResolve<IPopupService>(out IPopupService popupService))
            {
                popupService.HidePopup("ErrorPopup");
            }
            else if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void CopyDebugInfo()
        {
            GUIUtility.systemCopyBuffer = debugInfo ?? (bodyText != null ? bodyText.text : string.Empty);
        }

        private static GameObject CreateChild(Transform parent, string name, Vector2 size, Vector2 pos, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, Vector2 pos, Vector2 rectSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = rectSize;
            rect.anchoredPosition = pos;
            TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = new Color(0.12f, 0.18f, 0.28f, 1f);
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            GameObject go = CreateChild(parent, name, new Vector2(260f, 82f), pos, new Color(0.18f, 0.76f, 0.82f, 1f));
            Button button = go.AddComponent<Button>();
            button.onClick.AddListener(action);
            TextMeshProUGUI label = CreateText(go.transform, "Text", text, 24, Vector2.zero, new Vector2(240f, 70f));
            label.color = Color.white;
        }
    }
}
