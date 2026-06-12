using System;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Character
{
    public class PlayerStats
    {
        public int hp;
        public int mana;
        public int atk;
        public int mag;
        public int def;
        public int spd;
    }

    public class PlayerProgressionService : MonoBehaviour
    {
        private ISaveService saveService;
        private ToastService toastService;

        public PlayerSaveData CurrentSave => saveService?.CurrentSave;
        public bool LastAddExpLeveledUp { get; private set; }
        public event Action Changed;

        public void Initialize(ISaveService save, ToastService toast)
        {
            saveService = save;
            toastService = toast;
            saveService.LoadOrCreateSave();
            Changed?.Invoke();
        }

        public PlayerSaveData LoadOrCreate()
        {
            saveService.LoadOrCreateSave();
            Changed?.Invoke();
            return CurrentSave;
        }

        public PlayerSaveData CreateNewSave(string selectedClassId = "flame_squire", string playerName = "Guest Hero")
        {
            PlayerSaveData save = saveService.CreateNewSave();
            save.selectedClassId = selectedClassId;
            save.playerName = playerName;
            SaveAndNotify();
            return save;
        }

        public void DeleteSave()
        {
            saveService.DeleteSave();
            Changed?.Invoke();
        }

        public void AddGold(int amount)
        {
            CurrentSave.gold += amount;
            Toast($"Gold +{amount}");
            SaveAndNotify();
        }

        public bool AddExp(int amount)
        {
            LastAddExpLeveledUp = false;
            CurrentSave.exp += amount;
            while (CurrentSave.exp >= GetExpRequired(CurrentSave.level))
            {
                CurrentSave.exp -= GetExpRequired(CurrentSave.level);
                CurrentSave.level += 1;
                CurrentSave.maxHp += 18;
                CurrentSave.maxMana += 2;
                CurrentSave.hp = CurrentSave.maxHp;
                CurrentSave.mana = CurrentSave.maxMana;
                LastAddExpLeveledUp = true;
                Toast("Level Up!");
            }

            SaveAndNotify();
            return LastAddExpLeveledUp;
        }

        public void AddSoulGem(int amount)
        {
            CurrentSave.soulGem += amount;
            SaveAndNotify();
        }

        public void AddItem(string itemId, int amount)
        {
            ItemStackData stack = CurrentSave.inventory.items.Find(i => i.itemId == itemId);
            if (stack == null)
            {
                stack = new ItemStackData { itemId = itemId, amount = 0 };
                CurrentSave.inventory.items.Add(stack);
            }

            stack.amount += amount;
            Toast($"Item gained: {PrototypeItemDatabase.Get(itemId).displayName} x{amount}");
            SaveAndNotify();
        }

        public void AddEquipment(EquipmentInstanceData equipment)
        {
            CurrentSave.inventory.equipments.Add(equipment);
            Toast($"Equipment gained: {equipment.displayName}");
            SaveAndNotify();
        }

        public bool Equip(string instanceId)
        {
            EquipmentInstanceData equipment = CurrentSave.inventory.equipments.Find(e => e.instanceId == instanceId);
            if (equipment == null)
            {
                Toast("Not enough item");
                return false;
            }

            SetEquippedInstance(equipment.slot, equipment.instanceId);
            Toast($"Equipment equipped: {equipment.displayName}");
            SaveAndNotify();
            return true;
        }

        public void Unequip(EquipmentSlot slot)
        {
            SetEquippedInstance(slot, string.Empty);
            SaveAndNotify();
        }

        public PlayerStats CalculateTotalStats()
        {
            PlayerStats stats = new PlayerStats
            {
                hp = CurrentSave.maxHp,
                mana = CurrentSave.maxMana,
                atk = 10,
                mag = 8,
                def = 5,
                spd = 5
            };

            foreach (EquipmentInstanceData equipment in CurrentSave.inventory.equipments)
            {
                if (!IsEquipped(equipment.instanceId)) continue;
                stats.hp += equipment.hpBonus;
                stats.atk += equipment.atkBonus;
                stats.mag += equipment.magBonus;
                stats.def += equipment.defBonus;
                stats.spd += equipment.spdBonus;
            }

            return stats;
        }

        public int GetExpRequired(int level)
        {
            return Mathf.FloorToInt(50f * Mathf.Pow(level, 1.45f));
        }

        public void MarkStageCompleted(string stageId)
        {
            if (!CurrentSave.completedStageIds.Contains(stageId))
            {
                CurrentSave.completedStageIds.Add(stageId);
            }
            SaveAndNotify();
        }

        private void SaveAndNotify()
        {
            saveService.SaveNow();
            Toast("Save completed");
            Changed?.Invoke();
        }

        private void Toast(string message)
        {
            toastService?.ShowToast(message);
        }

        private bool IsEquipped(string instanceId)
        {
            EquipmentLoadoutData loadout = CurrentSave.equipment;
            return loadout.weaponInstanceId == instanceId || loadout.armorInstanceId == instanceId || loadout.headInstanceId == instanceId || loadout.bootsInstanceId == instanceId || loadout.ringInstanceId == instanceId || loadout.charmInstanceId == instanceId;
        }

        private void SetEquippedInstance(EquipmentSlot slot, string instanceId)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: CurrentSave.equipment.weaponInstanceId = instanceId; break;
                case EquipmentSlot.Armor: CurrentSave.equipment.armorInstanceId = instanceId; break;
                case EquipmentSlot.Head: CurrentSave.equipment.headInstanceId = instanceId; break;
                case EquipmentSlot.Boots: CurrentSave.equipment.bootsInstanceId = instanceId; break;
                case EquipmentSlot.Ring: CurrentSave.equipment.ringInstanceId = instanceId; break;
                case EquipmentSlot.Charm: CurrentSave.equipment.charmInstanceId = instanceId; break;
            }
        }
    }
}
