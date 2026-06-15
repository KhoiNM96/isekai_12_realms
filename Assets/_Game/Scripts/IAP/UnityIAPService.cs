using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Purchases;
using Isekai12Realms.Shop;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

#if USE_UNITY_IAP
using UnityEngine.Purchasing;
#endif

namespace Isekai12Realms.IAP
{
#if USE_UNITY_IAP
    public class UnityIAPService : IIAPService, IStoreListener
#else
    public class UnityIAPService : IIAPService
#endif
    {
        private readonly ContentDatabaseService contentService;
        private readonly CurrencyGrantService currencyGrantService;
        private readonly PurchaseLedgerService ledgerService;
        private readonly IReceiptValidatorService receiptValidator;
        private readonly ToastService toastService;
#if USE_UNITY_IAP
        private IStoreController controller;
        private IExtensionProvider extensions;
#endif

        public UnityIAPService(ContentDatabaseService content, CurrencyGrantService grants, PurchaseLedgerService ledger, IReceiptValidatorService validator, ToastService toast)
        {
            contentService = content;
            currencyGrantService = grants;
            ledgerService = ledger;
            receiptValidator = validator;
            toastService = toast;
        }

#if USE_UNITY_IAP
        public bool IsAvailable => controller != null;
#else
        public bool IsAvailable => false;
#endif
        public bool IsInitialized { get; private set; }

        public event Action OnInitialized;
        public event Action<string> OnPurchaseStarted;
        public event Action<PurchaseRecord> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;
        public event Action OnRestoreCompleted;
        public event Action<string> OnIAPStatusChanged;

        public Task InitializeAsync()
        {
#if USE_UNITY_IAP
            try
            {
                ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
                bool hasProducts = false;
                foreach (IAPProductDefinition product in contentService?.Database?.iapProducts ?? new List<IAPProductDefinition>())
                {
                    if (product == null || !product.enabled) continue;
                    string platformId = string.IsNullOrEmpty(product.platformProductId) ? product.productId : product.platformProductId;
                    builder.AddProduct(platformId, ProductType.Consumable);
                    hasProducts = true;
                }
                if (hasProducts)
                {
                    UnityPurchasing.Initialize(this, builder);
                }
                else
                {
                    OnIAPStatusChanged?.Invoke("Store is currently unavailable. Please try again later.");
                }
            }
            catch (Exception e)
            {
                OnIAPStatusChanged?.Invoke("Store is currently unavailable. Please try again later.");
                Debug.LogWarning("[IAP] Initialize failed: " + e.Message);
            }
#else
            OnIAPStatusChanged?.Invoke("Store is currently unavailable. Please try again later.");
#endif
            return Task.CompletedTask;
        }

        public List<IAPProductViewData> GetProducts()
        {
            List<IAPProductViewData> result = new List<IAPProductViewData>();
            foreach (IAPProductDefinition product in contentService?.Database?.iapProducts ?? new List<IAPProductDefinition>())
            {
                if (product == null) continue;
                string platformId = string.IsNullOrEmpty(product.platformProductId) ? product.productId : product.platformProductId;
                string price = product.priceTextPlaceholder;
#if USE_UNITY_IAP
                Product storeProduct = controller != null ? controller.products.WithID(platformId) : null;
                if (storeProduct != null && storeProduct.metadata != null) price = storeProduct.metadata.localizedPriceString;
#endif
                int amount = Mathf.Max(0, product.soulGemAmount);
                int bonus = Mathf.Max(0, product.bonusSoulGemAmount);
                result.Add(new IAPProductViewData { productId = product.productId, displayName = product.displayName, description = product.description, localizedPriceText = price, soulGemAmount = amount, bonusSoulGemAmount = bonus, totalSoulGemAmount = amount + bonus, enabled = product.enabled, available = IsAvailable && product.enabled });
            }
            return result;
        }

        public void Purchase(string productId)
        {
#if USE_UNITY_IAP
            IAPProductDefinition product = contentService?.Database?.GetIAPProductById(productId);
            string platformId = product != null && !string.IsNullOrEmpty(product.platformProductId) ? product.platformProductId : productId;
            Product storeProduct = controller?.products.WithID(platformId);
            if (storeProduct == null || !storeProduct.availableToPurchase)
            {
                toastService?.ShowToast("Store is currently unavailable. Please try again later.");
                OnPurchaseFailed?.Invoke(productId, "Store is currently unavailable.");
                return;
            }
            OnPurchaseStarted?.Invoke(productId);
            controller.InitiatePurchase(storeProduct);
#else
            toastService?.ShowToast("Store is currently unavailable. Please try again later.");
            OnPurchaseFailed?.Invoke(productId, "Store is currently unavailable.");
#endif
        }

        public void RestorePurchases()
        {
#if USE_UNITY_IAP
            if (extensions != null)
            {
                extensions.GetExtension<IAppleExtensions>()?.RestoreTransactions(_ => { OnRestoreCompleted?.Invoke(); });
            }
            else
            {
                OnRestoreCompleted?.Invoke();
            }
            toastService?.ShowToast("Purchase records refreshed.");
#else
            toastService?.ShowToast("Purchase records refreshed.");
            OnRestoreCompleted?.Invoke();
#endif
        }

        public bool HasGrantedTransaction(string transactionId) => ledgerService.HasGrantedTransaction(transactionId);

#if USE_UNITY_IAP
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.controller = controller;
            this.extensions = extensions;
            IsInitialized = true;
            OnInitialized?.Invoke();
            OnIAPStatusChanged?.Invoke("Unity IAP initialized.");
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            OnIAPStatusChanged?.Invoke("Unity IAP initialization failed: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            OnIAPStatusChanged?.Invoke("Unity IAP initialization failed: " + message);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Product storeProduct = args.purchasedProduct;
            string platformId = storeProduct.definition.id;
            IAPProductDefinition product = FindByPlatformId(platformId);
            if (product == null) return PurchaseProcessingResult.Complete;
            string transactionId = storeProduct.transactionID;
            string receipt = storeProduct.receipt;
            string receiptHash = ReceiptHashUtility.HashReceipt(receipt);
            if (string.IsNullOrEmpty(transactionId)) transactionId = platformId + "_" + receiptHash;
            if (ledgerService.HasGrantedTransaction(transactionId)) return PurchaseProcessingResult.Complete;
            ReceiptValidationResult validation = receiptValidator.ValidateAsync(product.productId, transactionId, receipt).GetAwaiter().GetResult();
            if (!validation.valid)
            {
                OnPurchaseFailed?.Invoke(product.productId, validation.message);
                return PurchaseProcessingResult.Pending;
            }
            bool granted = currencyGrantService.GrantSoulGemFromPurchase(product.productId, platformId, product.soulGemAmount, product.bonusSoulGemAmount, transactionId, "unity_iap", Application.platform.ToString(), receiptHash, out PurchaseRecord record);
            if (granted) OnPurchaseSucceeded?.Invoke(record);
            return granted || ledgerService.HasGrantedTransaction(transactionId) ? PurchaseProcessingResult.Complete : PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            OnPurchaseFailed?.Invoke(product?.definition?.id ?? string.Empty, failureReason.ToString());
        }

        private IAPProductDefinition FindByPlatformId(string platformId)
        {
            foreach (IAPProductDefinition product in contentService?.Database?.iapProducts ?? new List<IAPProductDefinition>())
            {
                if (product == null) continue;
                string id = string.IsNullOrEmpty(product.platformProductId) ? product.productId : product.platformProductId;
                if (id == platformId) return product;
            }
            return null;
        }
#endif
    }
}
