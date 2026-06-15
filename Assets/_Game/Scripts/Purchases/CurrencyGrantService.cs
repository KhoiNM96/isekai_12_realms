using System;
using Isekai12Realms.Character;
using Isekai12Realms.CloudSave;
using Isekai12Realms.Purchases;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Purchases
{
    public class CurrencyGrantService
    {
        private readonly ISaveService saveService;
        private readonly PlayerProgressionService progressionService;
        private readonly PurchaseLedgerService ledgerService;
        private readonly CloudSaveCoordinator cloudSaveCoordinator;
        private readonly ToastService toastService;

        public CurrencyGrantService(ISaveService save, PlayerProgressionService progression, PurchaseLedgerService ledger, CloudSaveCoordinator cloud, ToastService toast)
        {
            saveService = save;
            progressionService = progression;
            ledgerService = ledger;
            cloudSaveCoordinator = cloud;
            toastService = toast;
        }

        public bool GrantSoulGemFromPurchase(string productId, string platformProductId, int amount, int bonusAmount, string transactionId, string source, string platform, string receiptHash, out PurchaseRecord record)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            record = new PurchaseRecord
            {
                transactionId = string.IsNullOrEmpty(transactionId) ? productId + "_" + now + "_" + (string.IsNullOrEmpty(receiptHash) ? "no_receipt" : receiptHash) : transactionId,
                productId = productId,
                platformProductId = string.IsNullOrEmpty(platformProductId) ? productId : platformProductId,
                source = source,
                amount = Mathf.Max(0, amount),
                bonusAmount = Mathf.Max(0, bonusAmount),
                totalGranted = Mathf.Max(0, amount) + Mathf.Max(0, bonusAmount),
                platform = platform,
                receiptHash = receiptHash,
                appVersion = Application.version,
                deviceId = saveService?.CurrentSave != null ? saveService.CurrentSave.deviceId : string.Empty,
                purchasedAt = now,
                grantedAt = now,
                granted = true,
                cloudSynced = false
            };

            if (ledgerService.HasGrantedTransaction(record.transactionId))
            {
                toastService?.ShowToast("Purchase already processed.");
                return false;
            }

            if (record.totalGranted <= 0)
            {
                toastService?.ShowToast("Purchase product has no Soul Gems.");
                return false;
            }

            progressionService?.AddSoulGem(record.totalGranted);
            bool added = ledgerService.AddPurchaseRecord(record);
            if (!added)
            {
                toastService?.ShowToast("Purchase already processed.");
                return false;
            }

            saveService.SaveNow();
            cloudSaveCoordinator?.QueuePurchaseLedgerSync();
            toastService?.ShowToast("Soul Gems +" + record.totalGranted);
            return true;
        }
    }
}
