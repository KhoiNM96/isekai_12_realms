using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Data;
using Isekai12Realms.Purchases;
using Isekai12Realms.Shop;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Isekai12Realms.IAP
{
    public class MockIAPService : IIAPService
    {
        private readonly ContentDatabaseService contentService;
        private readonly CurrencyGrantService currencyGrantService;
        private readonly PurchaseLedgerService ledgerService;
        private readonly ToastService toastService;

        public MockIAPService(ContentDatabaseService content, CurrencyGrantService grants, PurchaseLedgerService ledger, ToastService toast)
        {
            contentService = content;
            currencyGrantService = grants;
            ledgerService = ledger;
            toastService = toast;
        }

        public bool IsAvailable
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return true;
#else
                return false;
#endif
            }
        }
        public bool IsInitialized { get; private set; }

        public event Action OnInitialized;
        public event Action<string> OnPurchaseStarted;
        public event Action<PurchaseRecord> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;
        public event Action OnRestoreCompleted;
        public event Action<string> OnIAPStatusChanged;

        public Task InitializeAsync()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
            OnIAPStatusChanged?.Invoke("Unity IAP is not configured yet.");
            return Task.CompletedTask;
        }

        public List<IAPProductViewData> GetProducts()
        {
            List<IAPProductViewData> result = new List<IAPProductViewData>();
            foreach (IAPProductDefinition product in contentService?.Database?.iapProducts ?? new List<IAPProductDefinition>())
            {
                if (product == null) continue;
                if (string.IsNullOrEmpty(product.platformProductId)) product.platformProductId = product.productId;
                int amount = Mathf.Max(0, product.soulGemAmount);
                int bonus = Mathf.Max(0, product.bonusSoulGemAmount);
                result.Add(new IAPProductViewData { productId = product.productId, displayName = product.displayName, description = product.description, localizedPriceText = product.priceTextPlaceholder, soulGemAmount = amount, bonusSoulGemAmount = bonus, totalSoulGemAmount = amount + bonus, enabled = product.enabled, available = IsAvailable && product.enabled });
            }
            return result;
        }

        public void Purchase(string productId)
        {
            IAPProductDefinition product = contentService?.Database?.GetIAPProductById(productId);
            if (product == null || !product.enabled)
            {
                OnPurchaseFailed?.Invoke(productId, "Product unavailable.");
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            OnPurchaseStarted?.Invoke(productId);
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string transactionId = "debug_iap_" + productId + "_" + now;
            bool granted = currencyGrantService.GrantSoulGemFromPurchase(product.productId, string.IsNullOrEmpty(product.platformProductId) ? product.productId : product.platformProductId, product.soulGemAmount, product.bonusSoulGemAmount, transactionId, "debug_iap", Application.platform.ToString(), string.Empty, out PurchaseRecord record);
            if (granted) OnPurchaseSucceeded?.Invoke(record);
            else OnPurchaseFailed?.Invoke(productId, "Purchase already processed.");
#else
            toastService?.ShowToast("Unity IAP is not configured.");
            OnPurchaseFailed?.Invoke(productId, "Unity IAP is not configured.");
#endif
        }

        public void RestorePurchases()
        {
            toastService?.ShowToast("Restore is not available in mock mode.");
            OnRestoreCompleted?.Invoke();
        }

        public bool HasGrantedTransaction(string transactionId) => ledgerService.HasGrantedTransaction(transactionId);
    }
}
#endif
