using System.Collections.Generic;
using Isekai12Realms.Equipment;
using UnityEngine;

namespace Isekai12Realms.Crafting
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Crafting Recipe")]
    public class CraftingRecipeDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string resultEquipmentId;
        public EquipmentSlot resultSlot;
        public int goldCost;
        public List<CraftingRecipeItemCost> itemCosts = new List<CraftingRecipeItemCost>();
    }

    [System.Serializable]
    public class CraftingRecipeItemCost
    {
        public string itemId;
        public int amount;
    }
}
