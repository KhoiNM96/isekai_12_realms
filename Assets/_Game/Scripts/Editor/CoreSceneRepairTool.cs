using System.IO;
using Isekai12Realms.Build;
using Isekai12Realms.Core;
using Isekai12Realms.Editor.BuildTools;
using Isekai12Realms.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Isekai12Realms.Editor
{
    public static class CoreSceneRepairTool
    {
        private const string GameScenePath = "Assets/_Game/Scenes/GameScene.unity";
        private const string BootScenePath = "Assets/_Game/Scenes/BootScene.unity";

        [MenuItem("Tools/Isekai 12 Realms/Repair Core Scenes")]
        public static void RepairCoreScenes()
        {
            EnsureSceneFolder();
            Scene bootScene = OpenOrCreateBootScene();
            EnsureBootLoader();
            EnsureBootLoadingSceneUi();
            EditorSceneManager.MarkSceneDirty(bootScene);
            EditorSceneManager.SaveScene(bootScene, BootScenePath);

            Scene scene = OpenOrCreateGameScene();

            EnsureMainCamera();
            GameSceneBootstrapper.EnsureEventSystem();
            GameSceneBootstrapper.EnsureRootCanvas();

            GameManager gameManager = EnsureGameManager();
            GameSceneBootstrapper bootstrapper = gameManager.GetComponent<GameSceneBootstrapper>();
            if (bootstrapper == null)
            {
                bootstrapper = gameManager.gameObject.AddComponent<GameSceneBootstrapper>();
            }

            if (NeedsUiRepair())
            {
                bootstrapper.RepairSceneUi();
            }
            EditorUtility.SetDirty(bootstrapper);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);
            AssetDatabase.Refresh();
            AndroidBuildSettingsApplier.EnsureScenesInBuild();

            Debug.Log($"[Game] Core scene repaired and saved: {GameScenePath}");
        }

        private static void EnsureSceneFolder()
        {
            string sceneDirectory = Path.GetDirectoryName(GameScenePath);
            if (!Directory.Exists(sceneDirectory))
            {
                Directory.CreateDirectory(sceneDirectory);
                AssetDatabase.Refresh();
            }
        }

        private static Scene OpenOrCreateGameScene()
        {
            if (File.Exists(GameScenePath))
            {
                return EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, GameScenePath);
            return scene;
        }

        private static Scene OpenOrCreateBootScene()
        {
            if (File.Exists(BootScenePath))
            {
                return EditorSceneManager.OpenScene(BootScenePath, OpenSceneMode.Single);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, BootScenePath);
            return scene;
        }

        private static void EnsureBootLoader()
        {
            Isekai12Realms.Core.BootLoader loader = null;
            GameObject[] sceneObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                GameObject go = sceneObjects[i];
                if (go == null || !go.scene.IsValid() || go.name != "BootLoader")
                {
                    continue;
                }

                Isekai12Realms.Core.BootLoader coreLoader = go.GetComponent<Isekai12Realms.Core.BootLoader>();
                if (coreLoader != null && loader == null)
                {
                    loader = coreLoader;
                    continue;
                }

                Object.DestroyImmediate(go);
            }

            if (loader == null)
            {
                GameObject boot = new GameObject("BootLoader");
                loader = boot.AddComponent<Isekai12Realms.Core.BootLoader>();
            }

            EditorUtility.SetDirty(loader);
        }

        private static void EnsureBootLoadingSceneUi()
        {
            BootLoadingSceneUI bootLoadingUi = UnityEngine.Object.FindObjectOfType<BootLoadingSceneUI>();
            if (bootLoadingUi == null)
            {
                GameObject go = new GameObject("BootLoadingUI");
                bootLoadingUi = go.AddComponent<BootLoadingSceneUI>();
            }

            bootLoadingUi.EnsureHierarchy();
            bootLoadingUi.Show();
            EditorUtility.SetDirty(bootLoadingUi);
        }

        private static void EnsureMainCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.11f, 0.28f, 1f);
            camera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static GameManager EnsureGameManager()
        {
            GameManager gameManager = UnityEngine.Object.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                return gameManager;
            }

            GameObject gameManagerObject = new GameObject("GameManager");
            return gameManagerObject.AddComponent<GameManager>();
        }

        private static bool NeedsUiRepair()
        {
            GameObject root = GameObject.Find("RootCanvas");
            if (root == null) return true;
            Transform safe = root.transform.Find("SafeAreaRoot");
            if (safe == null) return true;
            foreach (string layer in new[] { "BackgroundLayer", "MainLayer", "HudLayer", "NavigationLayer", "PopupLayer", "ToastLayer", "LoadingLayer" })
            {
                if (safe.Find(layer) == null) return true;
            }

            Transform popupLayer = safe.Find("PopupLayer");
            if (popupLayer == null) return true;

            Transform blocker = popupLayer.Find("ModalBlocker");
            if (blocker == null) return true;
            if (blocker.gameObject.activeSelf) return true;

            Transform settings = popupLayer.Find("SettingsPopup");
            if (settings == null) return true;
            if (settings.gameObject.activeSelf) return true;
            if (settings.Find("ModalPanel/ScrollView/Viewport/Content") == null) return true;

            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform child = popupLayer.GetChild(i);
                if (child == null || child.name == "ModalBlocker") continue;
                if (child.gameObject.activeSelf) return true;
            }

            return false;
        }
    }
}
