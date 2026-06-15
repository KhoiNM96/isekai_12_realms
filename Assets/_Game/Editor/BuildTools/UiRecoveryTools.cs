using System;
using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Build;
using Isekai12Realms.Core;
using Isekai12Realms.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

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
            "QADebugPanelUI"
        };

        [MenuItem("Tools/Isekai 12 Realms/UI/Emergency Restore Visible UI")]
        public static void EmergencyRestoreVisibleUi()
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
            RestoreCoreUiRoots();
            DeactivatePopupRoots();

            EditorUtility.SetDirty(gameManagerObject);
            EditorSceneManager.MarkSceneDirty(gameManagerObject.scene);
            EditorSceneManager.SaveScene(gameManagerObject.scene, GameScenePath);
            AssetDatabase.Refresh();

            Debug.Log("[UI] Emergency restore complete.");
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

        private static void RestoreCoreUiRoots()
        {
            SetActive("RootCanvas", true);
            SetActive("RootCanvas/SafeAreaRoot", true);
            SetActive("RootCanvas/SafeAreaRoot/BackgroundLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/HudLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/NavigationLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/PopupLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/ToastLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/LoadingLayer", true);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/TitleScreenUI", true);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/MainTownUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/AdventureUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/BattleUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/HeroUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/SkillsUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/EquipmentUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/InventoryUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/QuestUI", false);
            SetActive("RootCanvas/SafeAreaRoot/MainLayer/ShopUI", false);
            SetActive("GameManager", true);
            SetActive("EventSystem", true);
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
