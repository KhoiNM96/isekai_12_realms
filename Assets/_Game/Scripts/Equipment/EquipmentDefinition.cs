using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Equipment
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Equipment Definition")]
    public class EquipmentDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public string iconAssetId;
        public EquipmentSlot slot;
        public EquipmentRarity rarity;
        public int baseHp;
        public int baseAtk;
        public int baseMag;
        public int baseDef;
        public int baseSpd;
        public int baseLuck;
        public int maxLevel = 1;
        public List<EquipmentUpgradeCostData> upgradeCosts = new List<EquipmentUpgradeCostData>();

        public EquipmentUpgradeCostData GetUpgradeCost(int targetLevel)
        {
            return upgradeCosts != null ? upgradeCosts.Find(c => c != null && c.targetLevel == targetLevel) : null;
        }
    }
}
