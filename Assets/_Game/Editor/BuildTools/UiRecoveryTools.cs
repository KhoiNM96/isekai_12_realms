using System;
using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Adventure;
using Isekai12Realms.Build;
using Isekai12Realms.Character;
using Isekai12Realms.Core;
using Isekai12Realms.Realms;
using Isekai12Realms.UI;
using Isekai12Realms.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class UiRecoveryTools
    {
        private const string GameScenePath = "Assets/_Game/Scenes/GameScene.unity";

        private static readonly string[] KnownDebugOnlyNames =
        {
            "DEBUG_Export_Diagnostics",
            "DEBUG_Add_Gold",
            "DEBUG_Add_Soul_Gem",
            "DEBUG_Delete_Local_Save",
            "DEBUG_Complete_Active_Tutorial_Quest",
            "DEBUG_Export_Save_JSON",
            "DEBUG_Print_Cloud_Status",
            "DEBUG_Print_Save_Info",
            "DEBUG_Force_Conflict_Test",
            "DEBUG_Reset_Daily_Shop",
            "DEBUG_Reset_Tutorial",
            "DEBUG_Clear_Debug_Purchases",
            "DEBUG_Clear_Firebase_UID",
            "DEBUG_Start_Stage_1-1",
            "DEBUG_Simulate_IAP",
            "DEBUG_Add_Test_Equipment",
            "DEBUG_Add_Materials",
            "DEBUG_Win_Current_Battle",
            "DEBUG_Lose_Current_Battle",
            "Button_WinTest",
            "Button_LoseTest",
            "BattleDebugPanel",
            "QADebugPanelUI",
            "Button_StartBattle",
            "Button_EnterBattle",
            "DebugStageSelector"
        };

        [MenuItem("Tools/Isekai 12 Realms/UI/Emergency Restore Visible UI")]
        public static void EmergencyRestoreVisibleUi()
        {
            FixScreenOverlap();
        }

        [MenuItem("Tools/Isekai 12 Realms/UI/Fix Screen Overlap")]
        public static void FixScreenOverlap()
        {
            RebuildCleanWorldMapUi();
        }

        [MenuItem("Tools/Isekai 12 Realms/UI/Rebuild Clean World Map UI")]
        public static void RebuildCleanWorldMapUi()
        {
            if (!OpenGameScene())
            {
                return;
            }

            GameObject gameManagerObject = GameObject.Find("GameManager");
            if (gameManagerObject == null)
            {
                gameManagerObject = new GameObject("GameManager");
            }

            GameManager gameManager = gameManagerObject.GetComponent<GameManager>();
            if (gameManager == null)
            {
                gameManager = gameManagerObject.AddComponent<GameManager>();
            }

            GameSceneBootstrapper bootstrapper = gameManagerObject.GetComponent<GameSceneBootstrapper>();
            if (bootstrapper == null)
            {
                bootstrapper = gameManagerObject.AddComponent<GameSceneBootstrapper>();
            }

            bootstrapper.RepairSceneUi();
            TagKnownDebugOnlyObjects();
            DisableDebugOnlyObjects();
            RestoreTitleOnlyUi();
            RebuildWorldMapAndAdventureUi();
            DeactivatePopupRoots();
            DeactivateModalBlocker();

            EditorUtility.SetDirty(gameManagerObject);
            EditorSceneManager.MarkSceneDirty(gameManagerObject.scene);
            EditorSceneManager.SaveScene(gameManagerObject.scene, GameScenePath);
            AssetDatabase.Refresh();

            Debug.Log("[UI] Clean World Map UI rebuilt.");
        }

        [MenuItem("Tools/Isekai 12 Realms/UI/Validate Screen Overlap")]
        public static void ValidateScreenOverlap()
        {
            if (!OpenGameScene())
            {
                return;
            }

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            ValidateDefaultScreenState(errors, warnings);
            ValidateWorldMapLayout(errors, warnings);
            ValidateAdventureMapLayout(errors, warnings);
            ValidatePopupLayer(errors, warnings);

            foreach (string warning in warnings)
            {
                Debug.LogWarning("[UI] " + warning);
            }

            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError("[UI] " + error);
                }

                Debug.LogError("[UI] Validate Screen Overlap failed.");
            }
            else
            {
                Debug.Log("[UI] Validate Screen Overlap passed.");
            }
        }

        [MenuItem("Tools/Isekai 12 Realms/UI/Print Core UI Status")]
        public static void PrintCoreUiStatus()
        {
            if (!OpenGameScene())
            {
                return;
            }

            GameObject rootCanvas = GameObject.Find("RootCanvas");
            Transform safeAreaRoot = FindTransform("RootCanvas/SafeAreaRoot");
            Transform popupLayer = FindTransform("RootCanvas/SafeAreaRoot/PopupLayer");

            Debug.Log("[UI] RootCanvas active? " + BoolState(rootCanvas));
            Debug.Log("[UI] SafeAreaRoot active? " + BoolState(safeAreaRoot != null ? safeAreaRoot.gameObject : null));
            Debug.Log("[UI] MainLayer active? " + BoolState(FindGameObject("RootCanvas/SafeAreaRoot/MainLayer")));
            Debug.Log("[UI] TitleScreenUI exists/active? " + BoolState(FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/TitleScreenUI")));
            Debug.Log("[UI] MainTownUI exists/active? " + BoolState(FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/MainTownUI")));
            Debug.Log("[UI] GameManager active? " + BoolState(GameObject.Find("GameManager")));
            Debug.Log("[UI] UIScreenManager exists? " + (Resources.FindObjectsOfTypeAll<UIScreenManager>().Length > 0));
            Debug.Log("[UI] ProductionModeGuard exists? " + (Resources.FindObjectsOfTypeAll<ProductionModeGuard>().Length > 0));

            if (popupLayer != null)
            {
                for (int i = 0; i < popupLayer.childCount; i++)
                {
                    Transform child = popupLayer.GetChild(i);
                    if (child == null) continue;
                    Debug.Log("[UI] Popup child: " + child.name + " active=" + child.gameObject.activeSelf);
                }
            }
        }

        private static bool OpenGameScene()
        {
            if (!File.Exists(GameScenePath))
            {
                Debug.LogError("[UI] GameScene not found: " + GameScenePath);
                return false;
            }

            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            return true;
        }

        private static void RestoreTitleOnlyUi()
        {
            SetActive("RootCanvas", true);
            SetActive("RootCanvas/SafeAreaRoot", true);
            SetActive("RootCanvas/SafeAreaRoot/BackgroundLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/HudLayer", false);
            SetActive("RootCanvas/SafeAreaRoot/NavigationLayer", false);
            SetActive("RootCanvas/SafeAreaRoot/PopupLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/ToastLayer", false);
            SetActive("RootCanvas/SafeAreaRoot/LoadingLayer", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/TitleScreenUI", true);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/MainTownUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/AdventureMapUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/BattleUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/HeroUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/SkillsUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/EquipmentUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/InventoryUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/QuestUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/ShopUI", false);
            SetActive("RootCanvas/SafeAreaRoot/NavigationLayer/BottomNavigation", false);
            SetActive("GameManager", true);
            SetActive("EventSystem", true);
        }

        private static void RebuildWorldMapAndAdventureUi()
        {
            GameObject gameManagerObject = GameObject.Find("GameManager");
            UIScreenManager ui = gameManagerObject != null ? gameManagerObject.GetComponent<UIScreenManager>() : null;
            AdventureMapService adventureService = gameManagerObject != null ? gameManagerObject.GetComponent<AdventureMapService>() : null;
            ContentDatabaseService contentService = gameManagerObject != null ? gameManagerObject.GetComponent<ContentDatabaseService>() : null;
            RealmProgressionService realmProgressionService = gameManagerObject != null ? gameManagerObject.GetComponent<RealmProgressionService>() : null;
            PlayerProgressionService progressionService = gameManagerObject != null ? gameManagerObject.GetComponent<PlayerProgressionService>() : null;

            GameObject worldMap = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI");
            WorldMapUIController worldMapController = worldMap != null ? worldMap.GetComponent<WorldMapUIController>() : null;
            if (worldMapController == null && worldMap != null)
            {
                worldMapController = worldMap.AddComponent<WorldMapUIController>();
            }
            if (worldMapController != null)
            {
                worldMapController.Initialize(ui, adventureService, contentService, realmProgressionService, progressionService);
                worldMapController.RefreshView();
            }

            GameObject adventureMap = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI");
            RealmAdventureMapUIController adventureController = adventureMap != null ? adventureMap.GetComponent<RealmAdventureMapUIController>() : null;
            if (adventureController == null && adventureMap != null)
            {
                adventureController = adventureMap.AddComponent<RealmAdventureMapUIController>();
            }
            if (adventureController != null)
            {
                adventureController.Initialize(ui, adventureService, contentService, realmProgressionService, progressionService);
                adventureController.RefreshMap();
            }
        }

        private static void ValidateDefaultScreenState(List<string> errors, List<string> warnings)
        {
            GameObject title = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/TitleScreenUI");
            GameObject mainTown = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/MainTownUI");
            GameObject worldMap = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI");
            GameObject adventureMap = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI");
            GameObject battle = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/BattleUI");
            GameObject hero = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/HeroUI");
            GameObject skills = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/SkillsUI");
            GameObject equipment = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/EquipmentUI");
            GameObject inventory = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/InventoryUI");
            GameObject quest = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/QuestUI");
            GameObject shop = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/ShopUI");

            if (title == null || !title.activeSelf)
            {
                errors.Add("TitleScreenUI must be active by default.");
            }

            if (mainTown != null && mainTown.activeSelf)
            {
                errors.Add("MainTownUI must be inactive by default.");
            }

            if (worldMap != null && worldMap.activeSelf)
            {
                errors.Add("WorldMapUI must be inactive by default.");
            }

            if (adventureMap != null && adventureMap.activeSelf)
            {
                errors.Add("RealmAdventureMapUI must be inactive by default.");
            }

            if (battle != null && battle.activeSelf) errors.Add("BattleUI must be inactive by default.");
            if (hero != null && hero.activeSelf) errors.Add("HeroUI must be inactive by default.");
            if (skills != null && skills.activeSelf) errors.Add("SkillsUI must be inactive by default.");
            if (equipment != null && equipment.activeSelf) errors.Add("EquipmentUI must be inactive by default.");
            if (inventory != null && inventory.activeSelf) errors.Add("InventoryUI must be inactive by default.");
            if (quest != null && quest.activeSelf) errors.Add("QuestUI must be inactive by default.");
            if (shop != null && shop.activeSelf) errors.Add("ShopUI must be inactive by default.");

            int activeCount = 0;
            foreach (GameObject screen in new[] { title, mainTown, worldMap, adventureMap, battle, hero, skills, equipment, inventory, quest, shop })
            {
                if (screen != null && screen.activeSelf)
                {
                    activeCount++;
                }
            }

            if (activeCount > 1)
            {
                warnings.Add("More than one main screen is active in the scene.");
            }
        }

        private static void ValidateWorldMapLayout(List<string> errors, List<string> warnings)
        {
            Transform worldMap = FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI");
            if (worldMap == null)
            {
                errors.Add("WorldMapUI is missing.");
                return;
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/RealmScrollView") == null)
            {
                errors.Add("WorldMapUI missing RealmScrollView.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/RealmDetailPanel") == null)
            {
                errors.Add("WorldMapUI missing RealmDetailPanel.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/Header/Button_Back") == null)
            {
                warnings.Add("WorldMapUI missing header back button.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/Header/Button_Settings") == null)
            {
                warnings.Add("WorldMapUI missing header settings button.");
            }

            string[] forbidden = { "StageList", "StageListPanel", "StageNodes", "StageDetail", "StageDetailCard", "SelectStage", "StagePath", "OldWorldMap", "DebugStageSelector", "BattleTest", "EnterBattle", "WinTest", "LoseTest", "Placeholder_Text" };
            for (int i = 0; i < forbidden.Length; i++)
            {
                if (ContainsDescendantNamed(worldMap, forbidden[i]))
                {
                    errors.Add("WorldMapUI still contains legacy object: " + forbidden[i]);
                }
            }

            string[] forbiddenTexts = { "Stage nodes and realm paths will appear here", "Select a stage", "Available: First Slime" };
            foreach (TextMeshProUGUI label in Resources.FindObjectsOfTypeAll<TextMeshProUGUI>())
            {
                if (label == null || !label.gameObject.scene.IsValid())
                {
                    continue;
                }

                for (int i = 0; i < forbiddenTexts.Length; i++)
                {
                    if (string.Equals(label.text, forbiddenTexts[i], StringComparison.Ordinal))
                    {
                        errors.Add("Legacy WorldMap text still present: " + forbiddenTexts[i]);
                    }
                }
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/RealmDetailPanel/Button_EnterRealm") == null)
            {
                errors.Add("WorldMapUI missing Button_EnterRealm.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/RealmScrollView/Viewport/Content/RealmCard_01") == null)
            {
                warnings.Add("WorldMapUI missing realm cards.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI/RealmScrollView/Viewport/Content/RealmCard_12") == null)
            {
                warnings.Add("WorldMapUI is missing RealmCard_12.");
            }
        }

        private static void ValidateAdventureMapLayout(List<string> errors, List<string> warnings)
        {
            Transform adventureMap = FindTransform("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI");
            if (adventureMap == null)
            {
                errors.Add("RealmAdventureMapUI is missing.");
                return;
            }

            string[] required =
            {
                "Background",
                "Header",
                "Header/Button_BackToWorldMap",
                "Header/RealmName_Text",
                "Header/RealmProgress_Text",
                "MapViewport",
                "MapViewport/MapContent",
                "MapViewport/MapContent/Background",
                "MapViewport/MapContent/PlatformRoot",
                "MapViewport/MapContent/MonsterRoot",
                "MapViewport/MapContent/PlayerRoot",
                "MapViewport/MapContent/FXRoot",
                "MobileControls",
                "MobileControls/Button_Left",
                "MobileControls/Button_Right",
                "MobileControls/Button_Jump",
                "MobileControls/Button_Down",
                "FooterHint_Text"
            };

            for (int i = 0; i < required.Length; i++)
            {
                if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI/" + required[i]) == null)
                {
                    warnings.Add("RealmAdventureMapUI missing expected child: " + required[i]);
                }
            }
        }

        private static void ValidatePopupLayer(List<string> errors, List<string> warnings)
        {
            Transform popupLayer = FindTransform("RootCanvas/SafeAreaRoot/PopupLayer");
            if (popupLayer == null)
            {
                errors.Add("PopupLayer is missing.");
                return;
            }

            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform child = popupLayer.GetChild(i);
                if (child != null && child.gameObject.activeSelf)
                {
                    warnings.Add("Popup active by default: " + child.name);
                }
            }
        }

        private static bool ContainsDescendantNamed(Transform root, string childName)
        {
            if (root == null)
            {
                return false;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child != null)
                {
                    if (child.name == childName)
                    {
                        return true;
                    }

                    if (ContainsDescendantNamed(child, childName))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void DeactivatePopupRoots()
        {
            Transform popupLayer = FindTransform("RootCanvas/SafeAreaRoot/PopupLayer");
            if (popupLayer == null)
            {
                return;
            }

            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform child = popupLayer.GetChild(i);
                if (child == null) continue;
                child.gameObject.SetActive(false);
            }
        }

        private static void DeactivateModalBlocker()
        {
            Transform blocker = FindTransform("RootCanvas/SafeAreaRoot/PopupLayer/ModalBlocker");
            if (blocker != null)
            {
                blocker.gameObject.SetActive(false);
            }
        }

        private static void TagKnownDebugOnlyObjects()
        {
            foreach (string objectName in KnownDebugOnlyNames)
            {
                foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (go != null && go.scene.IsValid() && go.name == objectName && go.GetComponent<DebugOnlyObject>() == null)
                    {
                        go.AddComponent<DebugOnlyObject>();
                    }
                }
            }
        }

        private static void DisableDebugOnlyObjects()
        {
            foreach (DebugOnlyObject debugOnly in Resources.FindObjectsOfTypeAll<DebugOnlyObject>())
            {
                if (debugOnly != null && debugOnly.gameObject.scene.IsValid() && debugOnly.disableInProduction)
                {
                    debugOnly.gameObject.SetActive(false);
                }
            }
        }

        private static GameObject FindGameObject(string path)
        {
            Transform transform = FindTransform(path);
            return transform != null ? transform.gameObject : null;
        }

        private static Transform FindTransform(string path)
        {
            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            Transform current = null;
            foreach (Transform candidate in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (candidate != null && candidate.parent == null && candidate.name == parts[0])
                {
                    current = candidate;
                    break;
                }
            }

            if (current == null)
            {
                return null;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                Transform next = null;
                for (int j = 0; j < current.childCount; j++)
                {
                    Transform child = current.GetChild(j);
                    if (child != null && child.name == parts[i])
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        private static void SetActive(string path, bool active)
        {
            GameObject go = FindGameObject(path);
            if (go != null)
            {
                go.SetActive(active);
            }
        }

        private static string BoolState(GameObject go)
        {
            return go != null ? go.activeSelf.ToString() : "missing";
        }

        private static string BoolState(Transform transform)
        {
            return transform != null ? transform.gameObject.activeSelf.ToString() : "missing";
        }
    }
}
