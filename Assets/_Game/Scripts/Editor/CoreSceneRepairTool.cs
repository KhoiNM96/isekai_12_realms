using System.IO;
using Isekai12Realms.Core;
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

        [MenuItem("Tools/Isekai 12 Realms/Repair Core Scenes")]
        public static void RepairCoreScenes()
        {
            EnsureSceneFolder();
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

            bootstrapper.RepairSceneUi();
            EditorUtility.SetDirty(bootstrapper);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, GameScenePath);
            AssetDatabase.Refresh();

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
    }
}
