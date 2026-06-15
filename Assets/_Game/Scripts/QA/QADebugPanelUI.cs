using Isekai12Realms.Build;
using Isekai12Realms.CloudSave;
using Isekai12Realms.Core;
using Isekai12Realms.Diagnostics;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.QA
{
    public class QADebugPanelUI : MonoBehaviour
    {
        private BuildConfigService buildConfig;
        private GameObject root;
        private UIScreenManager screenManager;
        private ShopService shopService;
        private CloudSaveCoordinator cloudSaveCoordinator;
        private DiagnosticsService diagnosticsService;

        public void Initialize(Transform popupLayer, UIScreenManager ui, ShopService shop, CloudSaveCoordinator cloud, DiagnosticsService diagnostics, BuildConfigService config)
        {
            screenManager = ui;
            shopService = shop;
            cloudSaveCoordinator = cloud;
            diagnosticsService = diagnostics;
            buildConfig = config;
            if (!CanShowPanel()) return;
            BuildPanel(popupLayer);
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (root != null && keyboard != null && keyboard.f10Key.wasPressedThisFrame) root.SetActive(!root.activeSelf);
        }

        private bool CanShowPanel()
        {
            return IsEditorOrDevelopment && (buildConfig == null || buildConfig.EnableDebugPanel);
        }

        private bool CanUseCheats()
        {
            return IsEditorOrDevelopment && (buildConfig == null || buildConfig.AllowDebugCheats);
        }

        private bool CanUseMockPurchases()
        {
            return IsEditorOrDevelopment && (buildConfig == null || buildConfig.AllowMockPurchases);
        }

        private void BuildPanel(Transform popupLayer)
        {
            if (popupLayer == null) return;
            root = new GameObject("QADebugPanelUI", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(popupLayer, false);
            if (root.GetComponent<CanvasGroup>() == null) root.AddComponent<CanvasGroup>();
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-18f, 0f);
            rect.sizeDelta = new Vector2(420f, 1180f);
            root.GetComponent<Image>().color = new Color(0.04f, 0.06f, 0.1f, 0.94f);

            AddText(root.transform, "Title", "DEBUG QA PANEL", 30, new Vector2(0f, 540f), new Vector2(390f, 50f));
            float y = 480f;
            AddSection("Save", ref y);
            AddButton("DEBUG Print Save Info", ref y, PrintSaveInfo);
            AddButton("DEBUG Export Save JSON", ref y, ExportSaveJson);
            AddButton("DEBUG Delete Local Save", ref y, DeleteLocalSave);
            AddButton("DEBUG Add Gold", ref y, () => AddGold(500));
            AddButton("DEBUG Add Soul Gem", ref y, () => AddSoulGem(100));
            AddSection("Battle", ref y);
            AddButton("DEBUG Start Stage 1-1", ref y, () => screenManager?.ShowScreen(GameUIScreen.Battle));
            AddButton("DEBUG Win Current Battle", ref y, () => Debug.Log("[QA] Win Current Battle: use battle debug buttons in Battle screen."));
            AddButton("DEBUG Lose Current Battle", ref y, () => Debug.Log("[QA] Lose Current Battle: use battle debug buttons in Battle screen."));
            AddSection("Inventory", ref y);
            AddButton("DEBUG Add Test Equipment", ref y, () => Debug.Log("[QA] Add equipment through Settings debug or battle drops."));
            AddButton("DEBUG Add Materials", ref y, () => Debug.Log("[QA] Add materials through Settings debug."));
            AddSection("Quest", ref y);
            AddButton("DEBUG Complete Active Tutorial Quest", ref y, () => Debug.Log("[QA] Complete tutorial quest placeholder."));
            AddButton("DEBUG Reset Tutorial", ref y, ResetTutorial);
            AddSection("Shop", ref y);
            AddButton("DEBUG Reset Daily Shop", ref y, () => shopService?.ResetDailyShop(true));
            AddButton("DEBUG Simulate IAP", ref y, SimulateIap);
            AddSection("Cloud", ref y);
            AddButton("DEBUG Print Cloud Status", ref y, () => Debug.Log("[QA] Cloud status: " + (cloudSaveCoordinator != null ? cloudSaveCoordinator.GetStatus().ToString() : "Unavailable")));
            AddButton("DEBUG Force Conflict Test", ref y, () => cloudSaveCoordinator?.ForceConflictTest());
            AddButton("DEBUG Export Diagnostics", ref y, () => diagnosticsService?.ExportReport());
            root.SetActive(false);
        }

        private void AddSection(string title, ref float y)
        {
            AddText(root.transform, "Section_" + title, title, 24, new Vector2(0f, y), new Vector2(390f, 36f));
            y -= 42f;
        }

        private void AddButton(string label, ref float y, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(label.Replace(" ", "_"), typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(root.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 38f);
            rect.anchoredPosition = new Vector2(0f, y);
            buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.28f, 0.48f, 1f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
            TextMeshProUGUI text = AddText(buttonObject.transform, "Text", label, 18, Vector2.zero, new Vector2(340f, 34f));
            text.color = Color.white;
            y -= 42f;
        }

        private static TextMeshProUGUI AddText(Transform parent, string name, string value, int size, Vector2 pos, Vector2 rectSize)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = rectSize;
            rect.anchoredPosition = pos;
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.92f, 0.65f, 1f);
            return text;
        }

        private void PrintSaveInfo()
        {
            if (!ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService) || saveService.CurrentSave == null) return;
            Debug.Log($"[QA] Save player={saveService.CurrentSave.playerName} level={saveService.CurrentSave.level} gold={saveService.CurrentSave.gold} gems={saveService.CurrentSave.soulGem}");
        }

        private void ExportSaveJson()
        {
            if (ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService)) Debug.Log(saveService.ExportCurrentSaveJson());
        }

        private void DeleteLocalSave()
        {
            if (!CanUseCheats()) return;
            if (ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService)) saveService.DeleteSave();
        }

        private void AddGold(int amount)
        {
            if (!CanUseCheats() || !ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService) || saveService.CurrentSave == null) return;
            saveService.CurrentSave.gold += amount;
            saveService.SaveNow();
        }

        private void AddSoulGem(int amount)
        {
            if (!CanUseCheats() || !ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService) || saveService.CurrentSave == null) return;
            saveService.CurrentSave.soulGem += amount;
            saveService.SaveNow();
        }

        private void ResetTutorial()
        {
            if (!CanUseCheats() || !ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService) || saveService.CurrentSave == null) return;
            saveService.CurrentSave.completedTutorialStepIds.Clear();
            saveService.CurrentSave.activeTutorialId = string.Empty;
            saveService.CurrentSave.activeTutorialStepId = string.Empty;
            saveService.CurrentSave.tutorialEnabled = true;
            saveService.SaveNow();
        }

        private void SimulateIap()
        {
            if (!CanUseMockPurchases()) return;
            Debug.Log("[QA] Simulate IAP: open Shop > IAP and use DEBUG SIMULATE PURCHASE.");
        }

        private static bool IsEditorOrDevelopment
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }
    }
}
