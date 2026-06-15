using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Data;
using Isekai12Realms.Purchases;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class IAPSetupEditor
    {
        private const string GuidePath = "docs/iap_setup.md";
        private const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";

        [MenuItem("Tools/Isekai 12 Realms/IAP/Create Setup Guide")]
        public static void CreateSetupGuide()
        {
            Directory.CreateDirectory("docs");
            File.WriteAllText(GuidePath, GuideText());
            AssetDatabase.Refresh();
            Debug.Log("[IAP] Setup guide refreshed: " + GuidePath);
        }

        [MenuItem("Tools/Isekai 12 Realms/IAP/Validate IAP Products")]
        public static void ValidateIapProducts()
        {
            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath);
            List<string> errors = new List<string>();
            EconomyValidator.Validate(db, errors);
            if (errors.Count == 0) Debug.Log("[IAP] Product validation passed.");
            else Debug.LogError("[IAP] Product validation failed:\n" + string.Join("\n", errors));
        }

        [MenuItem("Tools/Isekai 12 Realms/IAP/Print Purchase Ledger")]
        public static void PrintPurchaseLedger()
        {
            SaveService save = new SaveService();
            save.LoadOrCreateSave();
            List<PurchaseRecord> records = save.CurrentSave.purchaseRecords ?? new List<PurchaseRecord>();
            string text = "[IAP] Purchase Ledger (" + records.Count + ")";
            foreach (PurchaseRecord record in records)
            {
                if (record == null) continue;
                text += $"\n{record.transactionId} product={record.productId} total={record.totalGranted} granted={record.granted} cloudSynced={record.cloudSynced}";
            }
            Debug.Log(text);
        }

        [MenuItem("Tools/Isekai 12 Realms/IAP/Clear Debug Purchase Records")]
        public static void ClearDebugPurchaseRecords()
        {
            if (!EditorUtility.DisplayDialog("Clear Debug Purchase Records", "Clear records with debug transaction/source only? Real-looking transactions are kept.", "Clear Debug", "Cancel")) return;
            SaveService save = new SaveService();
            save.LoadOrCreateSave();
            PurchaseLedgerService ledger = new PurchaseLedgerService(save);
            ledger.ClearDebugRecords();
            Debug.Log("[IAP] Debug purchase records cleared.");
        }

        private static string GuideText()
        {
            return "# Unity IAP Setup Guide\n\nInstall Unity IAP, create consumable Soul Gem products (`gems_tiny`, `gems_small`, `gems_medium`, `gems_large`, `gems_mega`), use matching Google Play product IDs, define `USE_UNITY_IAP`, and test with license tester accounts. IAP only grants Soul Gem currency. The purchase ledger prevents duplicate grants by transactionId. See `docs/release/iap_server_validation_plan.md` for the production receipt-verification plan.\n";
        }
    }
}
