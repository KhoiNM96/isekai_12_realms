using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Shop;

namespace Isekai12Realms.IAP
{
    public interface IIAPService
    {
        bool IsAvailable { get; }
        bool IsInitialized { get; }
        Task InitializeAsync();
        List<IAPProductViewData> GetProducts();
        void Purchase(string productId);
        void RestorePurchases();
        bool HasGrantedTransaction(string transactionId);

        event Action OnInitialized;
        event Action<string> OnPurchaseStarted;
        event Action<PurchaseRecord> OnPurchaseSucceeded;
        event Action<string, string> OnPurchaseFailed;
        event Action OnRestoreCompleted;
        event Action<string> OnIAPStatusChanged;
    }
}
