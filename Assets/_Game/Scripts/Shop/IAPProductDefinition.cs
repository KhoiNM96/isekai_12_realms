using UnityEngine;

namespace Isekai12Realms.Shop
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/IAP Product Definition")]
    public class IAPProductDefinition : ScriptableObject
    {
        public string productId;
        public string displayName;
        public string description;
        public string platformProductId;
        public int soulGemAmount;
        public int bonusSoulGemAmount;
        public string priceTextPlaceholder;
        public bool enabled = true;
    }
}
