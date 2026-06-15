using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isekai12Realms.Build;
using Isekai12Realms.Data;
using Isekai12Realms.Economy;
using Isekai12Realms.Editor.ContentTools;
using Isekai12Realms.Editor.AssetTools;
using Isekai12Realms.Editor;
using Isekai12Realms.Shop;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class ProductionBuildValidator
    {
        private const string ValidationReportPath = "docs/release/production_validation_report.md";
        private const string FirestoreRulesGuidePath = "docs/release/firestore_security_rules_production.md";
        private const string GoogleServicesPath = "Assets/google-services.json";

        private static readonly string[] DebugKeywords = { "DEBUG", "Mock", "Simulate", "Test", "Add Gold", "Add SoulGem", "Force", "Print Save Info", "Export Local Save", "Clear Firebase UID", "Win Current Battle", "Lose Current Battle", "Reset Daily Shop", "Clear Debug Purchase Records" };

        [MenuItem("Tools/Isekai 12 Realms/Production/Validate Production Build")]
        public static void ValidateFromMenu()
        {
            BuildValidationReport report = ValidateProductionBuild(true);
            if (report.HasCriticalErrors) Debug.LogError("[ProductionBuildValidator] " + report);
            else Debug.Log("[ProductionBuildValidator] " + report);
        }

        [MenuItem("Tools/Isekai 12 Realms/Production/Clean Debug UI From Scenes")]
        public static void CleanDebugUiFromScenes()
        {
            CleanDebugUiFromScene(BuildValidator.BootScenePath);
            CleanDebugUiFromScene(BuildValidator.GameScenePath);
            Debug.Log("[Production] Debug UI cleanup complete.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Production/Prepare Production Build")]
        public static void PrepareProductionBuild()
        {
            CleanDebugUiFromScenes();
            AndroidBuildSettingsApplier.ApplyAndroidBuildSettings();
            GameAssetPngGeneratorWindow.RebuildAssetManifest();
            IsekaiContentEditorWindow.RebuildContentDatabaseMenu();
            PrototypeContentEditor.ValidateContent();
            IAPSetupEditor.ValidateIapProducts();
            BuildValidationReport report = ValidateProductionBuild(true);
            Debug.Log(report.HasCriticalErrors ? "[Production] Prepare Production Build failed." : "[Production] Prepare Production Build complete.");
        }

        public static BuildValidationReport ValidateProductionBuild(bool writeReport)
        {
            BuildValidationReport report = new BuildValidationReport();

            BuildConfig config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildValidator.BuildConfigPath);
            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(BuildValidator.DatabasePath);
            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(BuildValidator.ManifestPath);

            CheckBuildConfig(report, config);
            CheckPlayerSettings(report);
            CheckAssets(report, db, manifest);
            CheckBuildSettings(report);
            CheckProductionDefines(report, config, db);
            CheckContent(report, db);
            CheckIapProducts(report, db);
            CheckSceneContent(report, BuildValidator.BootScenePath);
            CheckSceneContent(report, BuildValidator.GameScenePath);

            if (writeReport)
            {
                WriteReport(report);
            }

            return report;
        }

        private static void CheckBuildConfig(BuildValidationReport report, BuildConfig config)
        {
            if (config == null)
            {
                report.errors.Add("BuildConfig missing.");
                return;
            }

            if (!string.Equals(config.environment, "production", StringComparison.OrdinalIgnoreCase)) report.errors.Add("BuildConfig.environment must be 'production'.");
            if (config.developmentBuild) report.errors.Add("BuildConfig.developmentBuild must be false.");
            if (config.enableDebugPanel) report.errors.Add("BuildConfig.enableDebugPanel must be false.");
            if (config.enableMockIAP) report.errors.Add("BuildConfig.enableMockIAP must be false.");
            if (config.allowDebugCheatsInBuild) report.errors.Add("BuildConfig.allowDebugCheatsInBuild must be false.");
            if (config.allowMockPurchasesInBuild) report.errors.Add("BuildConfig.allowMockPurchasesInBuild must be false.");
            if (!config.enableCloudSave) report.errors.Add("BuildConfig.enableCloudSave must be true for production.");
            if (config.targetFrameRate != 60) report.warnings.Add("BuildConfig.targetFrameRate should be 60.");
            if (config.fallbackFrameRate != 30) report.warnings.Add("BuildConfig.fallbackFrameRate should be 30.");
        }

        private static void CheckPlayerSettings(BuildValidationReport report)
        {
            string packageName = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (string.IsNullOrWhiteSpace(packageName) || packageName == "com.yourstudio.isekai12realms") report.errors.Add("Android package name must be set to the final production value.");
            if (string.IsNullOrWhiteSpace(PlayerSettings.productName)) report.errors.Add("Product name is empty.");
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait) report.errors.Add("Orientation must be Portrait.");
#if UNITY_ANDROID
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0) report.errors.Add("ARM64 is not enabled.");
#endif
            if (PlayerSettings.Android.bundleVersionCode <= 0) report.errors.Add("Version code must be > 0.");
        }

        private static void CheckAssets(BuildValidationReport report, GameContentDatabase db, GameAssetManifest manifest)
        {
            if (!File.Exists(BuildValidator.BootScenePath)) report.errors.Add("BootScene missing: " + BuildValidator.BootScenePath);
            if (!File.Exists(BuildValidator.GameScenePath)) report.errors.Add("GameScene missing: " + BuildValidator.GameScenePath);
            if (db == null) report.errors.Add("GameContentDatabase missing.");
            if (manifest == null) report.errors.Add("GameAssetManifest missing.");
        }

        private static void CheckBuildSettings(BuildValidationReport report)
        {
            List<string> scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToList();
            if (!scenes.Contains(BuildValidator.BootScenePath)) report.errors.Add("BootScene is missing from Build Settings.");
            if (!scenes.Contains(BuildValidator.GameScenePath)) report.errors.Add("GameScene is missing from Build Settings.");
        }

        private static void CheckProductionDefines(BuildValidationReport report, BuildConfig config, GameContentDatabase db)
        {
            bool useFirebase = HasDefine("USE_FIREBASE");
            bool useIap = HasDefine("USE_UNITY_IAP");
            bool useAddressables = HasDefine("USE_ADDRESSABLES");
            bool useRemoteConfig = HasDefine("USE_FIREBASE_REMOTE_CONFIG");

            if (config != null && config.enableCloudSave && !useFirebase) report.errors.Add("USE_FIREBASE must be defined when cloud save is enabled.");
            if (db != null && db.iapProducts != null && db.iapProducts.Count > 0 && !useIap) report.errors.Add("USE_UNITY_IAP must be defined when IAP products exist.");
            if (useFirebase && !File.Exists(GoogleServicesPath)) report.errors.Add("google-services.json is missing from Assets/.");
            if (!File.Exists(FirestoreRulesGuidePath)) report.errors.Add("Firestore security rules guide is missing.");
            if (!useAddressables) report.warnings.Add("Addressables disabled.");
            if (!useRemoteConfig) report.warnings.Add("Remote Config disabled.");
            if (config != null && !config.enableAnalytics) report.warnings.Add("Analytics disabled.");
            report.warnings.Add("Crashlytics disabled.");
            if (config != null && !config.requireServerReceiptValidationForProduction) report.warnings.Add("Server receipt validation not configured.");
            if (config != null && !config.enableRemoteContent && !useAddressables) report.warnings.Add("Remote content is disabled.");
        }

        private static void CheckContent(BuildValidationReport report, GameContentDatabase db)
        {
            if (db == null) return;
            List<string> errors = new List<string>();
            EconomyValidator.Validate(db, errors);
            report.errors.AddRange(errors);
            if (db.iapProducts == null || db.iapProducts.Count == 0) report.errors.Add("IAP products missing or invalid.");

            ValidateRequiredIapProduct(report, db, "gems_tiny", 120, 0);
            ValidateRequiredIapProduct(report, db, "gems_small", 400, 40);
            ValidateRequiredIapProduct(report, db, "gems_medium", 750, 180);
            ValidateRequiredIapProduct(report, db, "gems_large", 1650, 500);
            ValidateRequiredIapProduct(report, db, "gems_mega", 3600, 1800);
        }

        private static void CheckIapProducts(BuildValidationReport report, GameContentDatabase db)
        {
            if (db == null || db.iapProducts == null || db.iapProducts.Count == 0)
            {
                report.errors.Add("IAP products missing or invalid.");
                return;
            }

            string[] required = { "gems_tiny", "gems_small", "gems_medium", "gems_large", "gems_mega" };
            foreach (string id in required)
            {
                if (db.GetIAPProductById(id) == null) report.errors.Add("Missing IAP product: " + id);
            }
        }

        private static void ValidateRequiredIapProduct(BuildValidationReport report, GameContentDatabase db, string productId, int amount, int bonus)
        {
            IAPProductDefinition product = db != null ? db.GetIAPProductById(productId) : null;
            if (product == null) return;
            if (!product.enabled) report.errors.Add("IAP product disabled: " + productId);
            if (product.productId != productId) report.errors.Add("IAP productId mismatch: " + productId);
            if (string.IsNullOrEmpty(product.platformProductId) || product.platformProductId != productId) report.errors.Add("IAP platformProductId mismatch: " + productId);
            if (product.soulGemAmount != amount) report.errors.Add("IAP Soul Gem amount mismatch: " + productId);
            if (product.bonusSoulGemAmount != bonus) report.errors.Add("IAP bonus amount mismatch: " + productId);
        }

        private static void CheckSceneContent(BuildValidationReport report, string scenePath)
        {
            if (!File.Exists(scenePath)) return;

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    CheckGameObjectRecursive(report, scenePath, root);
                }
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }
        }

        private static void CheckGameObjectRecursive(BuildValidationReport report, string scenePath, GameObject go)
        {
            if (go == null || !go.activeInHierarchy) return;
            if (MatchesDebugKeywords(go.name)) report.errors.Add($"{Path.GetFileName(scenePath)} active object contains debug keyword: {go.name}");

            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null && label.gameObject.activeInHierarchy && MatchesDebugKeywords(label.text))
                {
                    report.errors.Add($"{Path.GetFileName(scenePath)} button text contains debug keyword: {label.text}");
                }
            }

            TMP_Text visibleText = go.GetComponent<TMP_Text>();
            if (visibleText != null && visibleText.gameObject.activeInHierarchy && MatchesBannedText(visibleText.text))
            {
                report.errors.Add($"{Path.GetFileName(scenePath)} TMP text contains banned text: {visibleText.text}");
            }

            foreach (Transform child in go.transform)
            {
                CheckGameObjectRecursive(report, scenePath, child.gameObject);
            }
        }

        private static void DisableProductionOnlyRecursive(GameObject go)
        {
            if (go == null || !go.activeSelf) return;
            if (MatchesDebugKeywords(go.name) || MatchesBannedText(go.name) || MatchesDebugKeywords(go.tag) || HasBannedText(go))
            {
                go.SetActive(false);
                return;
            }

            foreach (Transform child in go.transform)
            {
                DisableProductionOnlyRecursive(child.gameObject);
            }
        }

        private static bool MatchesDebugKeywords(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string upper = value.ToUpperInvariant();
            return DebugKeywords.Any(keyword => upper.Contains(keyword.ToUpperInvariant()));
        }

        private static bool MatchesBannedText(string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string lower = value.ToLowerInvariant();
            return lower.Contains("placeholder") || lower.Contains("firebase unavailable") || lower.Contains("mock") || lower.Contains("simulate") || lower.Contains("addressables disabled") || lower.Contains("iap is not configured");
        }

        private static bool HasBannedText(GameObject go)
        {
            foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text == null || string.IsNullOrEmpty(text.text)) continue;
                if (MatchesDebugKeywords(text.text) || MatchesBannedText(text.text)) return true;
            }
            return false;
        }

        private static bool HasDefine(string define)
        {
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            string[] parts = symbols.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            return Array.Exists(parts, item => item.Trim() == define);
        }

        private static void CleanDebugUiFromScene(string scenePath)
        {
            if (!File.Exists(scenePath)) return;

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int disabled = 0;
                foreach (DebugOnlyObject debugOnly in Resources.FindObjectsOfTypeAll<DebugOnlyObject>())
                {
                    if (debugOnly == null || !debugOnly.disableInProduction) continue;
                    if (!debugOnly.gameObject.scene.IsValid()) continue;
                    debugOnly.gameObject.SetActive(false);
                    disabled++;
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    LogDebugLookingTextWarnings(root, scenePath, root.name);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene, scenePath);
                Debug.Log($"[Production] Cleaned {Path.GetFileName(scenePath)}. Disabled {disabled} debug-only objects.");
            }
            finally
            {
                if (setup != null && setup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }
        }

        private static void LogDebugLookingTextWarnings(GameObject go, string scenePath, string path)
        {
            if (go == null) return;

            foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(true))
            {
                if (text == null || string.IsNullOrEmpty(text.text)) continue;
                if (MatchesDebugKeywords(text.text) || MatchesBannedText(text.text))
                {
                    Debug.LogWarning($"Found debug-looking text: {Path.GetFileName(scenePath)} / {GetTransformPath(text.transform)}");
                }
            }
        }

        private static string GetTransformPath(Transform transform)
        {
            if (transform == null) return string.Empty;
            List<string> parts = new List<string>();
            while (transform != null)
            {
                parts.Add(transform.name);
                transform = transform.parent;
            }
            parts.Reverse();
            return string.Join("/", parts);
        }

        private static void WriteReport(BuildValidationReport report)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ValidationReportPath));
            string content = "# Production Validation Report\n\n" +
                             $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                             $"## Summary\n- Errors: {report.errors.Count}\n- Warnings: {report.warnings.Count}\n\n" +
                             "## Errors\n" + (report.errors.Count == 0 ? "- None\n" : string.Join("\n", report.errors.Select(e => "- " + e)) + "\n") +
                             "\n## Warnings\n" + (report.warnings.Count == 0 ? "- None\n" : string.Join("\n", report.warnings.Select(w => "- " + w)) + "\n");
            File.WriteAllText(ValidationReportPath, content);
            AssetDatabase.Refresh();
        }
    }
}
