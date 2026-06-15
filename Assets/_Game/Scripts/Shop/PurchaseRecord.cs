using System;

namespace Isekai12Realms.Shop
{
    [Serializable]
    public class PurchaseRecord
    {
        public string transactionId;
        public string productId;
        public string source;
        public int amount;
        public long purchasedAt;
        public bool granted;
    }
}
