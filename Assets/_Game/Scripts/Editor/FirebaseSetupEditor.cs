using System.IO;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class FirebaseSetupEditor
    {
        private const string GuidePath = "docs/firebase_setup.md";

        [MenuItem("Tools/Isekai 12 Realms/Firebase/Create Setup Checklist")]
        public static void CreateSetupChecklist()
        {
            Directory.CreateDirectory("docs");
            File.WriteAllText(GuidePath, GuideText());
            AssetDatabase.Refresh();
            Debug.Log("[Firebase] Setup checklist refreshed. Required: Firebase Auth, Firestore, google-services.json, USE_FIREBASE define, Anonymous Auth enabled.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Cloud Save/Print Local Save Info")]
        public static void PrintLocalSaveInfo()
        {
            SaveService saveService = new SaveService();
            saveService.LoadOrCreateSave();
            PlayerSaveData save = saveService.CurrentSave;
            Debug.Log($"[Cloud] Local Save Info\nSave path: {saveService.SaveFilePath}\nBackup path: {saveService.BackupFilePath}\nPlayer: {save.playerName}\nLevel: {save.level}\nSaveVersion: {save.saveVersion}\nUpdatedAt: {save.updatedAt}\nFirebase UID: {save.firebaseUid}\nCloud Sync: {save.cloudSyncEnabled}");
        }

        private static string GuideText()
        {
            return "# Firebase Setup Guide\n\n" +
                   "Firebase is optional for editor/local development, but production builds should use the real Firebase setup.\n\n" +
                   "## Required Firebase Modules\n- Authentication\n- Cloud Firestore\n\n" +
                   "## Steps\n1. Enable Anonymous Auth.\n2. Enable Google Sign-In later.\n3. Add Android package name.\n4. Download google-services.json into Assets/.\n5. Add USE_FIREBASE scripting define.\n\n" +
                   "## Firestore Paths\n- /users/{uid}/profile/main\n- /users/{uid}/saves/default\n- /users/{uid}/purchases/{transactionId}\n\n" +
                   "## Production Rules\nSee `docs/release/firestore_security_rules_production.md`.\n";
        }
    }
}
