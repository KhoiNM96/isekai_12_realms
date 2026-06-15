using System.IO;
using Isekai12Realms.Build;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class AndroidBuildSettingsApplier
    {
        [MenuItem("Tools/Isekai 12 Realms/Build/Apply Android Build Settings")]
        public static void ApplyAndroidBuildSettings()
        {
            BuildConfig config = EnsureBuildConfig();
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            PlayerSettings.productName = "Isekai 12 Realms";
            if (string.IsNullOrEmpty(PlayerSettings.companyName)) PlayerSettings.companyName = "Your Studio";
            if (string.IsNullOrEmpty(PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android)))
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.nmksinobi.isekai12realms");
                Debug.LogWarning("[Build] Android package name was empty. Suggested package applied: com.nmksinobi.isekai12realms");
            }
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
            PlayerSettings.bundleVersion = config.appVersion;
            PlayerSettings.Android.bundleVersionCode = Mathf.Max(1, config.bundleVersionCode);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.forceInternetPermission = true;
            EnsureScenesInBuild();
            AssetDatabase.SaveAssets();
            Debug.Log("[Build] Android build settings applied.");
        }

        public static BuildConfig EnsureBuildConfig()
        {
            string dir = Path.GetDirectoryName(BuildValidator.BuildConfigPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            BuildConfig config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildValidator.BuildConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BuildConfig>();
                AssetDatabase.CreateAsset(config, BuildValidator.BuildConfigPath);
                AssetDatabase.SaveAssets();
            }
            return config;
        }

        public static void EnsureScenesInBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BuildValidator.BootScenePath, true),
                new EditorBuildSettingsScene(BuildValidator.GameScenePath, true)
            };
        }
    }
}
