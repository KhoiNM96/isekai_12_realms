using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Build;
using Isekai12Realms.CloudSave;
using Isekai12Realms.Core;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Diagnostics
{
    public class DiagnosticsService : MonoBehaviour
    {
        private const int MaxLogs = 200;
        private readonly Queue<string> recentLogs = new Queue<string>();
        private BuildConfigService buildConfig;
        private ContentDatabaseService contentService;
        private CloudSaveCoordinator cloudSaveCoordinator;

        public string LastReportPath { get; private set; }

        public void Initialize(BuildConfigService config, ContentDatabaseService content, CloudSaveCoordinator cloudSave)
        {
            buildConfig = config;
            contentService = content;
            cloudSaveCoordinator = cloudSave;
        }

        private void OnEnable()
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        public void LogEvent(string message)
        {
            Debug.Log("[Diagnostics] " + message);
        }

        public string ExportReport()
        {
            string path = Path.Combine(Application.persistentDataPath, "diagnostics_report.txt");
            File.WriteAllText(path, BuildReport());
            LastReportPath = path;
            Debug.Log("[Diagnostics] Report exported: " + path);
            return path;
        }

        public string BuildReport()
        {
            PlayerSaveData save = ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService) ? saveService.CurrentSave : null;
            GameContentDatabase db = contentService != null ? contentService.Database : null;
            string report = "Isekai 12 Realms Diagnostics\n";
            report += $"Version: {buildConfig?.AppVersion ?? "0.1.0"} ({buildConfig?.BundleVersionCode ?? 1}) env={buildConfig?.Environment ?? "development"}\n";
            report += $"Device: {SystemInfo.deviceModel}\nOS: {SystemInfo.operatingSystem}\nScreen: {Screen.width}x{Screen.height}\nMemoryMB: {SystemInfo.systemMemorySize}\n";
            report += save != null ? $"Save: player={Short(save.playerId)} level={save.level} gold={save.gold} gems={save.soulGem} stages={save.completedStageIds?.Count ?? 0}\n" : "Save: unavailable\n";
            report += $"Cloud: {(cloudSaveCoordinator != null ? cloudSaveCoordinator.GetStatus().ToString() : "Unavailable")}\n";
            report += db != null ? $"Content: realms={db.realms?.Count ?? 0} stages={db.stages?.Count ?? 0} enemies={db.enemies?.Count ?? 0} skills={db.skills?.Count ?? 0} equipment={db.equipmentDefinitions?.Count ?? 0} shops={db.shops?.Count ?? 0}\n" : "Content: unavailable\n";
            report += "\nRecent Logs:\n" + string.Join("\n", recentLogs.ToArray());
            return report;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            string line = $"{System.DateTime.UtcNow:HH:mm:ss} [{type}] {condition}";
            recentLogs.Enqueue(line);
            while (recentLogs.Count > MaxLogs) recentLogs.Dequeue();
        }

        private static string Short(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value.Substring(0, Mathf.Min(8, value.Length));
        }
    }
}
