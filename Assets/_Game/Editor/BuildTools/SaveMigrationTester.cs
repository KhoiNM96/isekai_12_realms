using Isekai12Realms.Services;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class SaveMigrationTester
    {
        [MenuItem("Tools/Isekai 12 Realms/QA/Test Save Migration")]
        public static void TestSaveMigration()
        {
            SaveService service = new SaveService();
            string savePath = service.SaveFilePath;
            string backupPath = savePath + ".migration_test_backup";
            try
            {
                if (File.Exists(savePath)) File.Copy(savePath, backupPath, true);
                string oldJson = "{\"schemaVersion\":1,\"playerId\":\"old_save_test\",\"playerName\":\"Old Hero\",\"level\":1,\"gold\":10}";
                File.WriteAllText(savePath, oldJson);
                service.LoadOrCreateSave();
                var save = service.CurrentSave;
                bool pass = save != null && save.skills != null && save.skills.Count > 0 && save.quests != null && save.equipment != null && save.purchaseRecords != null && save.shopPurchaseLimits != null && !string.IsNullOrEmpty(save.authProvider) && !string.IsNullOrEmpty(save.deviceId);
                Debug.Log(pass ? "[QA] Save migration test passed." : "[QA] Save migration test failed: missing migrated fields.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[QA] Save migration test threw exception: " + ex.Message);
            }
            finally
            {
                if (File.Exists(backupPath)) File.Copy(backupPath, savePath, true);
                else if (File.Exists(savePath)) File.Delete(savePath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
            }
        }
    }
}
