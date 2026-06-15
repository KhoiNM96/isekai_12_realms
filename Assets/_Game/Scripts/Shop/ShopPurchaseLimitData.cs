using System;

namespace Isekai12Realms.Shop
{
    [Serializable]
    public class ShopPurchaseLimitData
    {
        public string shopItemId;
        public int dailyCount;
        public int lifetimeCount;
        public string dateKey;
    }
}
