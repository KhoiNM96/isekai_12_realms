using System;
using System.Collections.Generic;
using Isekai12Realms.Core;
using Isekai12Realms.Services;
using TMPro;
using UnityEngine;

namespace Isekai12Realms.UI
{
    public enum GameUIScreen
    {
        Title,
        CharacterCreation,
        MainTown,
        WorldMap,
        RealmAdventureMap,
        AdventureMap,
        Battle,
        Hero,
        Skills,
        Equipment,
        Inventory,
        Quest,
        Shop,
        Settings
    }

    public class UIScreenManager : MonoBehaviour
    {
        private readonly Dictionary<GameUIScreen, GameObject> screenRoots = new Dictionary<GameUIScreen, GameObject>();
        private GameObject navigationRoot;
        private GameObject settingsPopup;
        private GameObject battleResultPopup;
        private TextMeshProUGUI battleResultTitle;
        private TextMeshProUGUI battleResultRewards;
        private GameObject victoryButtons;
        private GameObject defeatButtons;

        public GameUIScreen CurrentScreen { get; private set; } = GameUIScreen.Title;
        public ToastService ToastService { get; private set; }
        public LoadingOverlayUI LoadingOverlay { get; private set; }

        public event Action<GameUIScreen, GameUIScreen> ScreenChanged;

        public void RegisterScreen(GameUIScreen screen, GameObject root)
        {
            if (root == null)
            {
                return;
            }

            screenRoots[screen] = root;
        }

        public void RegisterNavigationRoot(GameObject root)
        {
            navigationRoot = root;
        }

        public void RegisterServices(ToastService toastService, LoadingOverlayUI loadingOverlay)
        {
            ToastService = toastService;
            LoadingOverlay = loadingOverlay;
        }

        public void RegisterSettingsPopup(GameObject popup)
        {
            settingsPopup = popup;
        }

        public void RegisterBattleResultPopup(
            GameObject popup,
            TextMeshProUGUI title,
            TextMeshProUGUI rewards,
            GameObject victoryButtonRoot,
            GameObject defeatButtonRoot)
        {
            battleResultPopup = popup;
            battleResultTitle = title;
            battleResultRewards = rewards;
            victoryButtons = victoryButtonRoot;
            defeatButtons = defeatButtonRoot;
        }

        public void ShowScreen(GameUIScreen screen)
        {
            GameUIScreen previous = CurrentScreen;

            DeactivateAllScreens();

            if (screenRoots.TryGetValue(screen, out GameObject activeRoot) && activeRoot != null)
            {
                activeRoot.SetActive(true);
            }

            if (navigationRoot != null)
            {
                navigationRoot.SetActive(IsBottomNavigationVisible(screen));
            }

            EnsureSingleActiveScreen(screen);
            foreach (KeyValuePair<GameUIScreen, GameObject> entry in screenRoots)
            {
                if (entry.Value != null && entry.Key == screen)
                {
                    entry.Value.SetActive(true);
                }
            }

            CurrentScreen = screen;
            Debug.Log($"[UI] ShowScreen {screen}");
            ScreenChanged?.Invoke(previous, screen);
        }

        private void DeactivateAllScreens()
        {
            foreach (KeyValuePair<GameUIScreen, GameObject> entry in screenRoots)
            {
                if (entry.Value != null)
                {
                    entry.Value.SetActive(false);
                }
            }
        }

        private void EnsureSingleActiveScreen(GameUIScreen current)
        {
            List<GameObject> activeScreens = new List<GameObject>();
            foreach (KeyValuePair<GameUIScreen, GameObject> entry in screenRoots)
            {
                if (entry.Value != null && entry.Value.activeSelf)
                {
                    activeScreens.Add(entry.Value);
                }
            }

            if (activeScreens.Count <= 1)
            {
                return;
            }

            Debug.LogWarning($"[UI] Multiple main screens were active while showing {current}. Auto-correcting.");
            foreach (KeyValuePair<GameUIScreen, GameObject> entry in screenRoots)
            {
                if (entry.Value != null)
                {
                    entry.Value.SetActive(entry.Key == current);
                }
            }
        }

        private static bool IsBottomNavigationVisible(GameUIScreen screen)
        {
            return screen == GameUIScreen.MainTown ||
                   screen == GameUIScreen.Hero ||
                   screen == GameUIScreen.Inventory ||
                   screen == GameUIScreen.Quest ||
                   screen == GameUIScreen.Shop;
        }

        public void OpenSettings()
        {
            if (ServiceLocator.TryResolve<IPopupService>(out IPopupService popupService))
            {
                popupService.ShowPopup("SettingsPopup");
                Debug.Log("[UI] Settings popup opened");
                return;
            }

            if (settingsPopup != null)
            {
                settingsPopup.SetActive(true);
                Debug.Log("[UI] Settings popup opened");
            }
        }

        public void CloseSettings()
        {
            if (ServiceLocator.TryResolve<IPopupService>(out IPopupService popupService))
            {
                popupService.HidePopup("SettingsPopup");
                return;
            }

            if (settingsPopup != null)
            {
                settingsPopup.SetActive(false);
            }
        }

        public void OpenBattleResult(bool victory)
        {
            OpenBattleResult(victory, 50, 30);
        }

        public void OpenBattleResult(bool victory, int expGained, int goldGained)
        {
            OpenBattleResult(victory, expGained, goldGained, string.Empty, false);
        }

        public void OpenBattleResult(bool victory, int expGained, int goldGained, string dropsText, bool leveledUp)
        {
            if (battleResultPopup == null)
            {
                return;
            }

            if (ServiceLocator.TryResolve<IPopupService>(out IPopupService popupService))
            {
                popupService.ShowPopup("BattleResultPopup");
            }
            else
            {
                battleResultPopup.SetActive(true);
            }

            if (battleResultTitle != null)
            {
                battleResultTitle.text = victory ? "Victory!" : "Try Again";
            }

            if (battleResultRewards != null)
            {
                battleResultRewards.text = victory
                    ? $"EXP +{expGained}\nGold +{goldGained}\n{dropsText}\n{(leveledUp ? "Level Up!" : string.Empty)}"
                    : "The slime pushed you back. Try again or upgrade.";
            }

            if (victoryButtons != null)
            {
                victoryButtons.SetActive(victory);
            }

            if (defeatButtons != null)
            {
                defeatButtons.SetActive(!victory);
            }

            Debug.Log(victory ? "[UI] BattleResult Victory shown" : "[UI] BattleResult Defeat shown");
        }

        public void CloseBattleResult()
        {
            if (ServiceLocator.TryResolve<IPopupService>(out IPopupService popupService))
            {
                popupService.HidePopup("BattleResultPopup");
                return;
            }

            if (battleResultPopup != null)
            {
                battleResultPopup.SetActive(false);
            }
        }

        public void ShowDisabledToast()
        {
            if (ToastService != null)
            {
                ToastService.ShowToast("This option is not available.");
            }
        }
    }
}
