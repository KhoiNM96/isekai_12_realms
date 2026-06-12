using System;

namespace Isekai12Realms.Equipment
{
    [Serializable]
    public class EquipmentInstanceData
    {
        public string instanceId;
        public string equipmentId;
        public string displayName;
        public EquipmentSlot slot;
        public EquipmentRarity rarity;
        public int level;
        public int hpBonus;
        public int atkBonus;
        public int magBonus;
        public int defBonus;
        public int spdBonus;
        public bool locked;
    }
}
