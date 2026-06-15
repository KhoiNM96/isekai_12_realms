using System.IO;
using System.Linq;
using Isekai12Realms.Build;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor.BuildTools
{
    public class BuildPipelineWindow : EditorWindow
    {
        private BuildValidationReport lastReport;
        private Vector2 scroll;

        [MenuItem("Tools/Isekai 12 Realms/Build/Build Window")]
        public static void Open()
        {
            GetWindow<BuildPipelineWindow>("Isekai Build");
        }

        private void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            BuildConfig config = AssetDatabase.LoadAssetAtPath<BuildConfig>(BuildValidator.BuildConfigPath);
            EditorGUILayout.LabelField("Current BuildConfig", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(config != null ? $"v{config.appVersion} ({config.bundleVersionCode}) env={config.environment}" : "Missing BuildConfig");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Android Settings Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Product", PlayerSettings.productName);
            EditorGUILayout.LabelField("Package", PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            EditorGUILayout.LabelField("Orientation", PlayerSettings.defaultInterfaceOrientation.ToString());
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scenes in Build", EditorStyles.boldLabel);
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) EditorGUILayout.LabelField((scene.enabled ? "[x] " : "[ ] ") + scene.path);
            EditorGUILayout.Space();
            if (GUILayout.Button("Apply Android Build Settings")) AndroidBuildSettingsApplier.ApplyAndroidBuildSettings();
            if (GUILayout.Button("Run Validation")) lastReport = BuildValidator.Validate(false);
            if (GUILayout.Button("Run Production Validation")) lastReport = ProductionBuildValidator.ValidateProductionBuild(true);
            if (lastReport != null) EditorGUILayout.HelpBox(lastReport.ToString(), lastReport.HasCriticalErrors ? MessageType.Error : MessageType.Info);
            EditorGUILayout.Space();
            if (GUILayout.Button("Build APK Development")) Build(false);
            if (GUILayout.Button("Build AAB Release Candidate")) Build(true);
            if (GUILayout.Button("Open Build Folder")) EditorUtility.RevealInFinder(OutputDir());
            EditorGUILayout.EndScrollView();
        }

        private static void Build(bool aab)
        {
            BuildValidationReport report = aab ? ProductionBuildValidator.ValidateProductionBuild(true) : BuildValidator.Validate(false);
            if (report.HasCriticalErrors)
            {
                Debug.LogError("[Build] Stopped by validation errors:\n" + report);
                return;
            }
            BuildConfig config = AndroidBuildSettingsApplier.EnsureBuildConfig();
            Directory.CreateDirectory(OutputDir());
            string extension = aab ? "aab" : "apk";
            string flavor = aab ? "rc" : "dev";
            string path = Path.Combine(OutputDir(), $"Isekai12Realms_{flavor}_v{config.appVersion}_{config.bundleVersionCode}.{extension}");
            EditorUserBuildSettings.buildAppBundle = aab;
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = path,
                target = BuildTarget.Android,
                options = aab ? BuildOptions.None : BuildOptions.Development
            };
            BuildPipeline.BuildPlayer(options);
        }

        private static string OutputDir() => "Builds/Android";
    }
}
