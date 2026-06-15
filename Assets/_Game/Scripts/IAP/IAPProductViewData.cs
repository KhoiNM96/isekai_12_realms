using System;

namespace Isekai12Realms.IAP
{
    [Serializable]
    public class IAPProductViewData
    {
        public string productId;
        public string displayName;
        public string description;
        public string localizedPriceText;
        public int soulGemAmount;
        public int bonusSoulGemAmount;
        public int totalSoulGemAmount;
        public bool enabled;
        public bool available;
    }
}
