using Isekai12Realms.Core;
using Isekai12Realms.Battle;
using Isekai12Realms.UI;
using Isekai12Realms.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.QA
{
    public class AndroidBackButtonService : MonoBehaviour
    {
        private UIScreenManager screenManager;
        private Transform popupLayer;
        private GameObject quitPopup;
        private GameObject battlePausePopup;
        private IPopupService popupService;

        public void Initialize(UIScreenManager ui, Transform popupRoot)
        {
            screenManager = ui;
            popupLayer = popupRoot;
            ServiceLocator.TryResolve<IPopupService>(out popupService);
            CreatePopups();
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) HandleBack();
        }

        public void HandleBack()
        {
            if (popupService != null && popupService.HasOpenPopup())
            {
                popupService.CloseTop();
                return;
            }

            if (screenManager == null) return;
            if (screenManager.CurrentScreen == GameUIScreen.Battle)
            {
                ShowBattlePause();
                return;
            }
            if (screenManager.CurrentScreen != GameUIScreen.MainTown && screenManager.CurrentScreen != GameUIScreen.Title)
            {
                screenManager.ShowScreen(GameUIScreen.MainTown);
                return;
            }
            if (screenManager.CurrentScreen == GameUIScreen.MainTown)
            {
                ShowQuitConfirm();
                return;
            }
#if !UNITY_EDITOR
            Application.Quit();
#endif
        }

        private void ShowQuitConfirm()
        {
            if (popupService != null)
            {
                popupService.ShowPopup("QuitConfirmPopup");
                return;
            }

            if (quitPopup != null) quitPopup.SetActive(true);
        }

        private void ShowBattlePause()
        {
            if (popupService != null)
            {
                popupService.ShowPopup("BattlePausePopup");
                return;
            }

            if (battlePausePopup != null) battlePausePopup.SetActive(true);
        }

        private void CreatePopups()
        {
            if (popupLayer == null) return;
            quitPopup = CreatePopup("QuitConfirmPopup", "Quit Game?", "Do you want to close the game?", "Quit", () =>
            {
#if UNITY_EDITOR
                quitPopup.SetActive(false);
#else
                Application.Quit();
#endif
            });
            battlePausePopup = CreatePopup("BattlePausePopup", "Leave battle?", "Rewards are only granted after victory.", "Leave Battle", () =>
            {
                BattleUIController battle = FindObjectOfType<BattleUIController>();
                if (battle != null)
                {
                    battle.BackToWorldMap();
                }
                else
                {
                    screenManager?.ShowScreen(GameUIScreen.RealmAdventureMap);
                }
            });
        }

        private GameObject CreatePopup(string name, string title, string body, string confirmLabel, UnityEngine.Events.UnityAction confirm)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(popupLayer, false);
            if (root.GetComponent<CanvasGroup>() == null) root.AddComponent<CanvasGroup>();
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.62f);
            GameObject panel = Child(root.transform, "Panel", new Vector2(760f, 520f), Vector2.zero, new Color(1f, 0.95f, 0.84f, 0.98f));
            Text(panel.transform, "Title", title, 48, new Vector2(0f, 150f), new Vector2(650f, 80f));
            Text(panel.transform, "Body", body, 30, new Vector2(0f, 50f), new Vector2(650f, 120f));
            Button(panel.transform, "Button_Resume", "Resume", new Vector2(-220f, -155f), () => root.SetActive(false));
            Button(panel.transform, "Button_Confirm", confirmLabel, new Vector2(0f, -155f), confirm);
            Button(panel.transform, "Button_Settings", "Settings", new Vector2(220f, -155f), () => screenManager?.OpenSettings());
            root.SetActive(false);
            return root;
        }

        private static GameObject Child(Transform parent, string name, Vector2 size, Vector2 pos, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.sizeDelta = size; rect.anchoredPosition = pos;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private static void Button(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            GameObject go = Child(parent, name, new Vector2(200f, 82f), pos, new Color(0.18f, 0.76f, 0.82f, 1f));
            go.AddComponent<Button>().onClick.AddListener(action);
            TextMeshProUGUI text = Text(go.transform, "Text", label, 24, Vector2.zero, new Vector2(190f, 70f));
            text.color = Color.white;
        }

        private static TextMeshProUGUI Text(Transform parent, string name, string value, int size, Vector2 pos, Vector2 rectSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f); rect.sizeDelta = rectSize; rect.anchoredPosition = pos;
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value; text.fontSize = size; text.alignment = TextAlignmentOptions.Center; text.color = new Color(0.12f, 0.18f, 0.28f, 1f);
            return text;
        }
    }
}
