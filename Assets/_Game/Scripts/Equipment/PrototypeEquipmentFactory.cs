using System;

namespace Isekai12Realms.Equipment
{
    public static class PrototypeEquipmentFactory
    {
        public static EquipmentInstanceData Create(string equipmentId)
        {
            EquipmentInstanceData equipment = new EquipmentInstanceData
            {
                instanceId = Guid.NewGuid().ToString(),
                equipmentId = equipmentId,
                level = 1,
                rarity = EquipmentRarity.Common
            };

            switch (equipmentId)
            {
                case "equip_weapon_wooden_sword":
                    equipment.displayName = "Wooden Sword";
                    equipment.slot = EquipmentSlot.Weapon;
                    equipment.atkBonus = 5;
                    break;
                case "equip_armor_traveler_coat":
                    equipment.displayName = "Traveler Coat";
                    equipment.slot = EquipmentSlot.Armor;
                    equipment.hpBonus = 20;
                    equipment.defBonus = 3;
                    break;
                case "equip_ring_lucky":
                    equipment.displayName = "Lucky Ring";
                    equipment.slot = EquipmentSlot.Ring;
                    equipment.rarity = EquipmentRarity.Uncommon;
                    equipment.spdBonus = 2;
                    break;
                default:
                    equipment.displayName = equipmentId;
                    equipment.slot = EquipmentSlot.Weapon;
                    break;
            }

            return equipment;
        }
    }
}
