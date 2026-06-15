using System;
using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Shop
{
    public class IAPPlaceholderService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private ToastService toastService;

        public event Action Changed;

        private PlayerSaveData Save => saveService?.CurrentSave;
        private GameContentDatabase Database => contentService?.Database;

        public void Initialize(ISaveService save, ContentDatabaseService content, ToastService toast)
        {
            saveService = save;
            contentService = content;
            toastService = toast;
            EnsureSaveState();
        }

        public List<IAPProductDefinition> GetProducts()
        {
            return Database?.iapProducts != null ? Database.iapProducts.FindAll(p => p != null && p.enabled) : new List<IAPProductDefinition>();
        }

        public bool SimulatePurchase(string productId)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return GrantSoulGemFromProduct(productId, "debug_" + now + "_" + productId);
#else
            toastService?.ShowToast("Coming Soon");
            return false;
#endif
        }

        public bool GrantSoulGemFromProduct(string productId, string transactionId)
        {
            EnsureSaveState();
            IAPProductDefinition product = Database?.GetIAPProductById(productId);
            if (Save == null || product == null || !product.enabled)
            {
                toastService?.ShowToast("IAP product unavailable.");
                return false;
            }
            if (Save.purchaseRecords.Exists(r => r != null && r.transactionId == transactionId && r.granted))
            {
                toastService?.ShowToast("Purchase already granted.");
                return false;
            }

            int total = Mathf.Max(0, product.soulGemAmount) + Mathf.Max(0, product.bonusSoulGemAmount);
            Save.soulGem += total;
            Save.purchaseRecords.Add(new PurchaseRecord { transactionId = transactionId, productId = product.productId, source = "debug_iap_placeholder", amount = total, purchasedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(), granted = true });
            saveService.SaveNow();
            toastService?.ShowToast("Soul Gems +" + total);
            Changed?.Invoke();
            return true;
        }

        public bool CanSimulatePurchases()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#else
            return false;
#endif
        }

        public void ClearPurchaseRecords()
        {
            EnsureSaveState();
            if (Save == null) return;
            Save.purchaseRecords.Clear();
            saveService.SaveNow();
            toastService?.ShowToast("Purchase records cleared.");
            Changed?.Invoke();
        }

        private void EnsureSaveState()
        {
            if (Save == null) return;
            if (Save.purchaseRecords == null) Save.purchaseRecords = new List<PurchaseRecord>();
        }
    }
}
