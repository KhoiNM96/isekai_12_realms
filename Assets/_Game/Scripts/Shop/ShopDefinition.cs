using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Shop
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Shop Definition")]
    public class ShopDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public ShopType shopType;
        public List<ShopItemDefinition> items = new List<ShopItemDefinition>();
        public bool refreshDaily;
        public int refreshHourLocal = 4;
        public string iconAssetId;
    }
}
