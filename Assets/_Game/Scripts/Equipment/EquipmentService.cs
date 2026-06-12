using System;
using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Equipment
{
    public class EquipmentService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private ToastService toastService;

        private PlayerSaveData Save => saveService?.CurrentSave;

        public void Initialize(ISaveService save, ContentDatabaseService content, ToastService toast)
        {
            saveService = save;
            contentService = content;
            toastService = toast;
            EnsureEquipmentDefaults();
        }

        public List<EquipmentInstanceData> GetAllEquipment()
        {
            EnsureEquipmentDefaults();
            return Save?.inventory?.equipments ?? new List<EquipmentInstanceData>();
        }

        public EquipmentInstanceData GetEquipmentByInstanceId(string instanceId)
        {
            return GetAllEquipment().Find(e => e != null && e.instanceId == instanceId);
        }

        public EquipmentInstanceData GetEquipped(EquipmentSlot slot)
        {
            string id = GetEquippedInstanceId(slot);
            return string.IsNullOrEmpty(id) ? null : GetEquipmentByInstanceId(id);
        }

        public bool Equip(string instanceId)
        {
            EquipmentInstanceData equipment = GetEquipmentByInstanceId(instanceId);
            if (equipment == null)
            {
                Toast("Equipment not found.");
                return false;
            }

            SetEquippedInstance(equipment.slot, equipment.instanceId);
            SaveNow();
            Toast("Equipment equipped: " + equipment.displayName);
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            SetEquippedInstance(slot, string.Empty);
            SaveNow();
            Toast("Equipment unequipped.");
            return true;
        }

        public void LockEquipment(string instanceId, bool locked)
        {
            EquipmentInstanceData equipment = GetEquipmentByInstanceId(instanceId);
            if (equipment == null) return;
            equipment.locked = locked;
            SaveNow();
            Toast(locked ? "Equipment locked." : "Equipment unlocked.");
        }

        public bool SellEquipment(string instanceId)
        {
            EquipmentInstanceData equipment = GetEquipmentByInstanceId(instanceId);
            if (equipment == null) return false;
            if (equipment.locked)
            {
                Toast("Locked equipment cannot be sold.");
                return false;
            }
            if (IsEquipped(instanceId))
            {
                Toast("Equipped equipment cannot be sold.");
                return false;
            }

            int value = GetSellValue(equipment);
            Save.inventory.equipments.Remove(equipment);
            Save.gold += value;
            SaveNow();
            Toast("Equipment sold");
            return true;
        }

        public bool CanUpgrade(string instanceId)
        {
            return string.IsNullOrEmpty(GetUpgradeBlocker(instanceId));
        }

        public bool UpgradeEquipment(string instanceId)
        {
            string blocker = GetUpgradeBlocker(instanceId);
            if (!string.IsNullOrEmpty(blocker))
            {
                Toast(blocker);
                return false;
            }

            EquipmentInstanceData equipment = GetEquipmentByInstanceId(instanceId);
            EquipmentDefinition definition = GetDefinition(equipment.equipmentId);
            EquipmentUpgradeCostData cost = GetUpgradeCost(definition, equipment.level + 1);
            Save.gold -= cost.goldCost;
            if (!string.IsNullOrEmpty(cost.materialItemId) && cost.materialAmount > 0)
            {
                ItemStackData stack = Save.inventory.items.Find(i => i.itemId == cost.materialItemId);
                stack.amount -= cost.materialAmount;
                if (stack.amount <= 0) Save.inventory.items.Remove(stack);
            }

            equipment.level += 1;
            ApplyStatsFromDefinition(equipment, definition);
            SaveNow();
            Toast("Equipment upgraded!");
            return true;
        }

        public PlayerStatsData CalculateEquipmentStats()
        {
            PlayerStatsData stats = new PlayerStatsData();
            foreach (EquipmentInstanceData equipment in GetAllEquipment())
            {
                if (equipment == null || !IsEquipped(equipment.instanceId)) continue;
                stats.maxHp += equipment.hpBonus;
                stats.atk += equipment.atkBonus;
                stats.mag += equipment.magBonus;
                stats.def += equipment.defBonus;
                stats.spd += equipment.spdBonus;
                stats.luck += equipment.luckBonus;
            }
            return stats;
        }

        public EquipmentInstanceData CreateEquipmentInstance(string equipmentId)
        {
            EquipmentDefinition definition = GetDefinition(equipmentId);
            EquipmentInstanceData equipment = new EquipmentInstanceData
            {
                instanceId = Guid.NewGuid().ToString(),
                equipmentId = equipmentId,
                level = 1,
                acquiredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            ApplyStatsFromDefinition(equipment, definition);
            return equipment;
        }

        public EquipmentDefinition GetDefinition(string equipmentId)
        {
            EquipmentDefinition definition = contentService?.Database?.GetEquipmentDefinitionById(equipmentId);
            if (definition != null) return definition;
            Debug.LogWarning("[Equipment] Missing definition: " + equipmentId);
            return CreateFallbackDefinition(equipmentId);
        }

        public int GetSellValue(EquipmentInstanceData equipment)
        {
            int level = Mathf.Max(1, equipment != null ? equipment.level : 1);
            switch (equipment != null ? equipment.rarity : EquipmentRarity.Common)
            {
                case EquipmentRarity.Uncommon: return 50 + level * 10;
                case EquipmentRarity.Rare: return 120 + level * 20;
                case EquipmentRarity.Epic: return 300 + level * 40;
                case EquipmentRarity.Legendary: return 800 + level * 80;
                default: return 20 + level * 5;
            }
        }

        public bool IsEquipped(string instanceId)
        {
            EquipmentLoadoutData loadout = Save?.equipment;
            return loadout != null && (loadout.weaponInstanceId == instanceId || loadout.armorInstanceId == instanceId || loadout.headInstanceId == instanceId || loadout.bootsInstanceId == instanceId || loadout.ringInstanceId == instanceId || loadout.charmInstanceId == instanceId);
        }

        public string GetEquippedInstanceId(EquipmentSlot slot)
        {
            EquipmentLoadoutData loadout = Save?.equipment;
            if (loadout == null) return string.Empty;
            switch (slot)
            {
                case EquipmentSlot.Weapon: return loadout.weaponInstanceId;
                case EquipmentSlot.Armor: return loadout.armorInstanceId;
                case EquipmentSlot.Head: return loadout.headInstanceId;
                case EquipmentSlot.Boots: return loadout.bootsInstanceId;
                case EquipmentSlot.Ring: return loadout.ringInstanceId;
                case EquipmentSlot.Charm: return loadout.charmInstanceId;
                default: return string.Empty;
            }
        }

        public EquipmentUpgradeCostData GetUpgradeCost(EquipmentDefinition definition, int targetLevel)
        {
            if (definition == null) return null;
            return definition.GetUpgradeCost(targetLevel);
        }

        public string BuildStatText(EquipmentInstanceData equipment)
        {
            if (equipment == null) return "No equipment selected.";
            return $"HP +{equipment.hpBonus}\nATK +{equipment.atkBonus}\nMAG +{equipment.magBonus}\nDEF +{equipment.defBonus}\nSPD +{equipment.spdBonus}\nLUCK +{equipment.luckBonus}";
        }

        public string BuildComparisonText(EquipmentInstanceData equipment)
        {
            if (equipment == null) return string.Empty;
            EquipmentInstanceData equipped = GetEquipped(equipment.slot);
            if (equipped == null || equipped.instanceId == equipment.instanceId) return "Compare: no equipped item in slot.";
            return "Compare vs equipped\n" +
                   Delta("HP", equipment.hpBonus - equipped.hpBonus) + "\n" +
                   Delta("ATK", equipment.atkBonus - equipped.atkBonus) + "\n" +
                   Delta("MAG", equipment.magBonus - equipped.magBonus) + "\n" +
                   Delta("DEF", equipment.defBonus - equipped.defBonus) + "\n" +
                   Delta("SPD", equipment.spdBonus - equipped.spdBonus) + "\n" +
                   Delta("LUCK", equipment.luckBonus - equipped.luckBonus);
        }

        private string GetUpgradeBlocker(string instanceId)
        {
            EquipmentInstanceData equipment = GetEquipmentByInstanceId(instanceId);
            if (equipment == null) return "Equipment not found.";
            EquipmentDefinition definition = GetDefinition(equipment.equipmentId);
            if (equipment.level >= definition.maxLevel) return "Equipment is already max level.";
            EquipmentUpgradeCostData cost = GetUpgradeCost(definition, equipment.level + 1);
            if (cost == null) return "Upgrade cost missing.";
            if (Save.gold < cost.goldCost) return "Not enough gold.";
            if (!string.IsNullOrEmpty(cost.materialItemId) && cost.materialAmount > 0)
            {
                ItemStackData stack = Save.inventory.items.Find(i => i.itemId == cost.materialItemId);
                if (stack == null || stack.amount < cost.materialAmount) return "Not enough material.";
            }
            return string.Empty;
        }

        private void ApplyStatsFromDefinition(EquipmentInstanceData equipment, EquipmentDefinition definition)
        {
            equipment.displayName = definition.displayName;
            equipment.slot = definition.slot;
            equipment.rarity = definition.rarity;
            equipment.hpBonus = Scale(definition.baseHp, equipment.level, 0.08f);
            equipment.atkBonus = Scale(definition.baseAtk, equipment.level, 0.10f);
            equipment.magBonus = Scale(definition.baseMag, equipment.level, 0.10f);
            equipment.defBonus = Scale(definition.baseDef, equipment.level, 0.10f);
            equipment.spdBonus = Scale(definition.baseSpd, equipment.level, 0.05f);
            equipment.luckBonus = Scale(definition.baseLuck, equipment.level, 0.05f);
            if (equipment.acquiredAt <= 0) equipment.acquiredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private static int Scale(int baseValue, int level, float perLevel)
        {
            if (baseValue <= 0) return 0;
            int upgrades = Mathf.Max(0, level - 1);
            int bonus = Mathf.CeilToInt(baseValue * perLevel * upgrades);
            bonus = Mathf.Max(bonus, upgrades / 2);
            return baseValue + bonus;
        }

        private void EnsureEquipmentDefaults()
        {
            if (Save == null) return;
            if (Save.inventory == null) Save.inventory = new InventorySaveData();
            if (Save.inventory.items == null) Save.inventory.items = new List<ItemStackData>();
            if (Save.inventory.equipments == null) Save.inventory.equipments = new List<EquipmentInstanceData>();
            if (Save.equipment == null) Save.equipment = new EquipmentLoadoutData();
            foreach (EquipmentInstanceData equipment in Save.inventory.equipments)
            {
                if (equipment == null) continue;
                if (string.IsNullOrEmpty(equipment.instanceId)) equipment.instanceId = Guid.NewGuid().ToString();
                if (equipment.level <= 0) equipment.level = 1;
                if (equipment.acquiredAt <= 0) equipment.acquiredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                EquipmentDefinition definition = GetDefinition(equipment.equipmentId);
                if (definition != null) ApplyStatsFromDefinition(equipment, definition);
            }
        }

        private void SetEquippedInstance(EquipmentSlot slot, string instanceId)
        {
            if (Save == null) return;
            if (Save.equipment == null) Save.equipment = new EquipmentLoadoutData();
            switch (slot)
            {
                case EquipmentSlot.Weapon: Save.equipment.weaponInstanceId = instanceId; break;
                case EquipmentSlot.Armor: Save.equipment.armorInstanceId = instanceId; break;
                case EquipmentSlot.Head: Save.equipment.headInstanceId = instanceId; break;
                case EquipmentSlot.Boots: Save.equipment.bootsInstanceId = instanceId; break;
                case EquipmentSlot.Ring: Save.equipment.ringInstanceId = instanceId; break;
                case EquipmentSlot.Charm: Save.equipment.charmInstanceId = instanceId; break;
            }
        }

        private static string Delta(string label, int value)
        {
            return value == 0 ? label + " 0" : label + " " + (value > 0 ? "+" : string.Empty) + value;
        }

        private static EquipmentDefinition CreateFallbackDefinition(string equipmentId)
        {
            EquipmentDefinition fallback = ScriptableObject.CreateInstance<EquipmentDefinition>();
            fallback.id = equipmentId;
            fallback.displayName = string.IsNullOrEmpty(equipmentId) ? "Unknown Equipment" : equipmentId;
            fallback.iconAssetId = equipmentId;
            fallback.slot = EquipmentSlot.Weapon;
            fallback.rarity = EquipmentRarity.Common;
            fallback.maxLevel = 1;
            return fallback;
        }

        private void SaveNow()
        {
            saveService?.SaveNow();
        }

        private void Toast(string message)
        {
            toastService?.ShowToast(message);
        }
    }
}
