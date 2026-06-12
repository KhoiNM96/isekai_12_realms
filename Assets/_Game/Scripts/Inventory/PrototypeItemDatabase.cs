using System.Collections.Generic;

namespace Isekai12Realms.Inventory
{
    public enum PrototypeItemType
    {
        Consumable,
        Material
    }

    public class PrototypeItemDefinition
    {
        public string itemId;
        public string displayName;
        public PrototypeItemType type;
    }

    public static class PrototypeItemDatabase
    {
        private static readonly Dictionary<string, PrototypeItemDefinition> Items = new Dictionary<string, PrototypeItemDefinition>
        {
            { "item_potion_small", new PrototypeItemDefinition { itemId = "item_potion_small", displayName = "Small Potion", type = PrototypeItemType.Consumable } },
            { "mat_slime_jelly", new PrototypeItemDefinition { itemId = "mat_slime_jelly", displayName = "Slime Jelly", type = PrototypeItemType.Material } },
            { "item_skill_scroll", new PrototypeItemDefinition { itemId = "item_skill_scroll", displayName = "Skill Scroll", type = PrototypeItemType.Material } }
        };

        public static PrototypeItemDefinition Get(string itemId)
        {
            return Items.TryGetValue(itemId, out PrototypeItemDefinition item) ? item : new PrototypeItemDefinition { itemId = itemId, displayName = itemId, type = PrototypeItemType.Material };
        }
    }
}
