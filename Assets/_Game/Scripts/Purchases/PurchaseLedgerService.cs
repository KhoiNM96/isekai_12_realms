using System;
using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using UnityEngine;

namespace Isekai12Realms.Purchases
{
    public class PurchaseLedgerService
    {
        private readonly ISaveService saveService;
        private PlayerSaveData Save => saveService?.CurrentSave;

        public PurchaseLedgerService(ISaveService save)
        {
            saveService = save;
            EnsureState();
        }

        public bool HasGrantedTransaction(string transactionId)
        {
            EnsureState();
            if (Save == null || string.IsNullOrEmpty(transactionId)) return false;
            return Save.purchaseRecords.Exists(r => r != null && r.transactionId == transactionId && r.granted);
        }

        public bool AddPurchaseRecord(PurchaseRecord record)
        {
            EnsureState();
            if (Save == null || record == null) return false;
            if (string.IsNullOrEmpty(record.transactionId))
            {
                Debug.LogWarning("[IAP] Missing transactionId. Creating fallback transaction key.");
                record.transactionId = CreateFallbackTransactionId(record);
            }
            if (HasGrantedTransaction(record.transactionId)) return false;
            Save.purchaseRecords.Add(record);
            saveService.SaveNow();
            return true;
        }

        public void MarkCloudSynced(string transactionId)
        {
            EnsureState();
            PurchaseRecord record = Save?.purchaseRecords?.Find(r => r != null && r.transactionId == transactionId);
            if (record == null) return;
            record.cloudSynced = true;
            saveService.SaveNow();
        }

        public List<PurchaseRecord> GetAllRecords()
        {
            EnsureState();
            return Save?.purchaseRecords ?? new List<PurchaseRecord>();
        }

        public List<PurchaseRecord> GetUnsyncedRecords()
        {
            EnsureState();
            return GetAllRecords().FindAll(r => r != null && !r.cloudSynced);
        }

        public void ClearDebugRecords()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            EnsureState();
            if (Save == null) return;
            Save.purchaseRecords.RemoveAll(r => r != null && ((r.transactionId ?? string.Empty).StartsWith("debug_iap_", StringComparison.Ordinal) || (r.source ?? string.Empty).StartsWith("debug", StringComparison.Ordinal)));
            saveService.SaveNow();
#endif
        }

        private void EnsureState()
        {
            if (Save == null) return;
            if (Save.purchaseRecords == null) Save.purchaseRecords = new List<PurchaseRecord>();
            foreach (PurchaseRecord record in Save.purchaseRecords)
            {
                if (record == null) continue;
                if (record.totalGranted <= 0) record.totalGranted = record.amount + record.bonusAmount;
                if (record.totalGranted <= 0) record.totalGranted = record.amount;
                if (record.grantedAt <= 0 && record.granted) record.grantedAt = record.purchasedAt;
                if (string.IsNullOrEmpty(record.platformProductId)) record.platformProductId = record.productId;
            }
        }

        private static string CreateFallbackTransactionId(PurchaseRecord record)
        {
            return record.productId + "_" + record.purchasedAt + "_" + (string.IsNullOrEmpty(record.receiptHash) ? "no_receipt" : record.receiptHash);
        }
    }
}
