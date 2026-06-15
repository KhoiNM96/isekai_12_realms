using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isekai12Realms.Build;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using Isekai12Realms.Realms;
using Isekai12Realms.Shop;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Isekai12Realms.Editor.BuildTools
{
    public class BuildValidationReport
    {
        public readonly List<string> errors = new List<string>();
        public readonly List<string> warnings = new List<string>();
        public bool HasCriticalErrors => errors.Count > 0;
        public string Summary => $"Errors: {errors.Count}  Warnings: {warnings.Count}";
        public override string ToString() => Summary + "\n\nErrors:\n" + string.Join("\n", errors) + "\n\nWarnings:\n" + string.Join("\n", warnings);
    }

    public static class BuildValidator
    {
        public const string BootScenePath = "Assets/_Game/Scenes/BootScene.unity";
        public const string GameScenePath = "Assets/_Game/Scenes/GameScene.unity";
        public const string BuildConfigPath = "Assets/_Game/ScriptableObjects/Economy/BuildConfig.asset";
        public const string ManifestPath = "Assets/_Game/ScriptableObjects/AssetManifest/GameAssetManifest.asset";
        public const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";

        [MenuItem("Tools/Isekai 12 Realms/Build/Run Build Validator")]
        public static void RunFromMenu()
        {
            BuildValidationReport report = Validate(false);
            if (report.HasCriticalErrors) Debug.LogError("[BuildValidator] " + report);
            else Debug.Log("[BuildValidator] " + report);
        }

        public static BuildValidationReport Validate(bool releaseBuild)
        {
            BuildValidationReport report = new BuildValidationReport();
            CheckAssets(report);
            CheckBuildSettings(report, releaseBuild);
            CheckContent(report);
            CheckGameScene(report);
            return report;
        }

        private static void CheckAssets(BuildValidationReport report)
        {
            if (!File.Exists(BootScenePath)) report.errors.Add("BootScene missing: " + BootScenePath);
            if (!File.Exists(GameScenePath)) report.errors.Add("GameScene missing: " + GameScenePath);
            if (AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath) == null) report.errors.Add("GameAssetManifest missing: " + ManifestPath);
            if (AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath) == null) report.errors.Add("GameContentDatabase missing: " + DatabasePath);
            if (AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildConfigPath) == null) report.errors.Add("BuildConfig missing: " + BuildConfigPath);
        }

        private static void CheckBuildSettings(BuildValidationReport report, bool releaseBuild)
        {
            List<string> scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
            if (!scenes.Contains(BootScenePath)) report.errors.Add("BootScene is not enabled in Build Settings.");
            if (!scenes.Contains(GameScenePath)) report.errors.Add("GameScene is not enabled in Build Settings.");
            if (string.IsNullOrEmpty(PlayerSettings.productName)) report.errors.Add("Product name is empty.");
            if (string.IsNullOrEmpty(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android))) report.errors.Add("Android package name is empty. Suggested: com.yourstudio.isekai12realms");
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait) report.errors.Add("Default orientation must be Portrait.");
#if UNITY_ANDROID
            if (releaseBuild && (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0) report.errors.Add("ARM64 must be enabled for release.");
#endif
            BuildConfig config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildConfigPath);
            if (config != null && config.bundleVersionCode <= 0) report.errors.Add("BuildConfig version code must be > 0.");
        }

        private static void CheckContent(BuildValidationReport report)
        {
            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath);
            if (db == null) return;
            CheckDuplicates(report, "realm", db.realms, r => r != null ? r.id : string.Empty);
            CheckDuplicates(report, "stage", db.stages, s => s != null ? s.id : string.Empty);
            CheckDuplicates(report, "skill", db.skills, s => s != null ? s.id : string.Empty);
            CheckDuplicates(report, "equipment", db.equipmentDefinitions, e => e != null ? e.id : string.Empty);
            CheckDuplicates(report, "shop", db.shops, s => s != null ? s.id : string.Empty);
            if (db.realms == null || db.realms.Count == 0) report.errors.Add("Prototype content missing: no realms.");
            if (db.stages == null || db.stages.Count == 0) report.errors.Add("Prototype content missing: no stages.");
            if (db.skills == null || db.skills.Count == 0) report.errors.Add("Prototype skills missing.");
            if (db.equipmentDefinitions == null || db.equipmentDefinitions.Count == 0) report.errors.Add("Prototype equipment missing.");
            if (db.shops == null || db.shops.Count == 0) report.errors.Add("Prototype shop missing.");
        }

        private static void CheckDuplicates<T>(BuildValidationReport report, string label, List<T> items, System.Func<T, string> idGetter)
        {
            if (items == null) return;
            HashSet<string> ids = new HashSet<string>();
            foreach (T item in items)
            {
                string id = idGetter(item);
                if (string.IsNullOrEmpty(id)) continue;
                if (!ids.Add(id)) report.errors.Add("Duplicate " + label + " id: " + id);
            }
        }

        private static void CheckGameScene(BuildValidationReport report)
        {
            if (!File.Exists(GameScenePath)) return;
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
                if (GameObject.Find("RootCanvas") == null) report.errors.Add("GameScene missing RootCanvas.");
                if (Object.FindObjectOfType<Isekai12Realms.Core.GameManager>() == null) report.errors.Add("GameScene missing GameManager.");
                if (Object.FindObjectOfType<EventSystem>() == null) report.errors.Add("GameScene missing EventSystem.");
                if (!scene.IsValid()) report.errors.Add("GameScene could not be opened.");
            }
            finally
            {
                if (setup != null && setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
            }
        }
    }
}
