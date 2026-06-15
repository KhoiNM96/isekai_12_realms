using System;

namespace Isekai12Realms.Shop
{
    [Serializable]
    public class PurchaseRecord
    {
        public string transactionId;
        public string productId;
        public string platformProductId;
        public string source;
        public int amount;
        public int bonusAmount;
        public int totalGranted;
        public string platform;
        public string receiptHash;
        public string appVersion;
        public string deviceId;
        public long purchasedAt;
        public long grantedAt;
        public bool granted;
        public bool cloudSynced;
    }
}
