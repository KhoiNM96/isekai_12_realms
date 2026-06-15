using System;
using Isekai12Realms.Economy;

namespace Isekai12Realms.Shop
{
    [Serializable]
    public class ShopItemDefinition
    {
        public string id;
        public string displayName;
        public string description;
        public string iconAssetId;
        public ShopItemType itemType;
        public string itemId;
        public string equipmentId;
        public string cosmeticId;
        public int amount = 1;
        public CurrencyType priceCurrency;
        public int priceAmount;
        public int purchaseLimitPerDay;
        public int purchaseLimitLifetime;
        public bool enabled = true;
    }
}
