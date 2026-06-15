using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Tutorial
{
    public class TutorialOverlayUI : MonoBehaviour
    {
        private GameObject root;
        private RectTransform highlight;
        private TextMeshProUGUI messageText;
        private Button nextButton;
        private Button skipButton;
        private Action onNext;
        private Action onSkip;

        public void Initialize(RectTransform parent)
        {
            if (root != null)
            {
                root.transform.SetParent(parent, false);
                root.transform.SetAsLastSibling();
                return;
            }

            root = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image dim = root.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            dim.raycastTarget = true;

            GameObject highlightObj = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlightObj.transform.SetParent(root.transform, false);
            highlight = highlightObj.GetComponent<RectTransform>();
            Image highlightImage = highlightObj.GetComponent<Image>();
            highlightImage.color = new Color(1f, 0.86f, 0.28f, 0.35f);
            highlightImage.raycastTarget = false;

            GameObject panel = new GameObject("MessagePanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 120f);
            panelRect.sizeDelta = new Vector2(860f, 330f);
            panel.GetComponent<Image>().color = new Color(1f, 0.95f, 0.84f, 0.96f);

            messageText = AddText(panel.transform, "Message", new Vector2(0f, 105f), new Vector2(780f, 150f), 34, new Color(0.12f, 0.18f, 0.28f, 1f));
            nextButton = AddButton(panel.transform, "Button_Next", "Next", new Vector2(-140f, 45f));
            skipButton = AddButton(panel.transform, "Button_Skip", "Skip", new Vector2(140f, 45f));
            nextButton.onClick.AddListener(() => { Hide(); onNext?.Invoke(); });
            skipButton.onClick.AddListener(() => { Hide(); onSkip?.Invoke(); });
            root.SetActive(false);
        }

        public void Show(TutorialDefinition tutorial, TutorialStepData step, Action next, Action skip)
        {
            if (root == null || step == null) return;
            onNext = next;
            onSkip = skip;
            messageText.text = step.message;
            skipButton.gameObject.SetActive(tutorial != null && tutorial.skippable);
            highlight.gameObject.SetActive(step.highlightType != TutorialHighlightType.None);
            if (highlight.gameObject.activeSelf)
            {
                highlight.anchorMin = highlight.anchorMax = new Vector2(0.5f, 0.5f);
                highlight.anchoredPosition = Vector2.zero;
                highlight.sizeDelta = new Vector2(420f, 140f);
                if (!string.IsNullOrEmpty(step.targetUiElementName)) Debug.LogWarning("[Tutorial] Target highlight fallback used for: " + step.targetUiElementName);
            }
            root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null) root.SetActive(false);
        }

        private static TextMeshProUGUI AddText(Transform parent, string name, Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.color = color;
            return text;
        }

        private static Button AddButton(Transform parent, string name, string label, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(220f, 78f);
            go.GetComponent<Image>().color = new Color(0.18f, 0.76f, 0.82f, 1f);
            AddText(go.transform, "Text", Vector2.zero, rect.sizeDelta, 30, Color.white).text = label;
            return go.GetComponent<Button>();
        }
    }
}
