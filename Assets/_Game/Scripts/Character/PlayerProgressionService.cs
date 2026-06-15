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
        public int maxHp;
        public int mana;
        public int atk;
        public int mag;
        public int def;
        public int spd;
        public int luck;
        public int foodBonus;
        public int manaGainBonus;
        public float dropRateBonus;
        public float expBonus;
        public float goldBonus;
        public float healBonus;
        public float critRate;
    }

    public class PlayerProgressionService : MonoBehaviour
    {
        private ISaveService saveService;
        private ToastService toastService;
        private EquipmentService equipmentService;

        public PlayerSaveData CurrentSave => saveService?.CurrentSave;
        public bool LastAddExpLeveledUp { get; private set; }
        public event Action Changed;

        public void Initialize(ISaveService save, ToastService toast)
        {
            saveService = save;
            toastService = toast;
            equipmentService = GetComponent<EquipmentService>();
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
            if (equipment == null) return;
            CurrentSave.inventory.equipments.Add(equipment);
            Toast($"Equipment gained: {equipment.displayName}");
            SaveAndNotify();
        }

        public bool Equip(string instanceId)
        {
            EquipmentService service = GetEquipmentService();
            if (service != null)
            {
                bool equipped = service.Equip(instanceId);
                Changed?.Invoke();
                return equipped;
            }
            return false;
        }

        public void Unequip(EquipmentSlot slot)
        {
            EquipmentService service = GetEquipmentService();
            if (service != null) service.Unequip(slot);
            Changed?.Invoke();
        }

        public PlayerStats CalculateTotalStats()
        {
            PlayerStats stats = new PlayerStats
            {
                maxHp = 100 + (CurrentSave.level - 1) * 18,
                mana = CurrentSave.maxMana,
                atk = 10 + (CurrentSave.level - 1) * 2,
                mag = 8 + (CurrentSave.level - 1) * 2,
                def = 5 + (CurrentSave.level - 1),
                spd = 5 + (CurrentSave.level - 1) / 2,
                luck = 1 + (CurrentSave.level - 1) / 5
            };

            EquipmentService service = GetEquipmentService();
            PlayerStatsData equipmentStats = service != null ? service.CalculateEquipmentStats() : null;
            if (equipmentStats != null)
            {
                stats.maxHp += equipmentStats.maxHp;
                stats.atk += equipmentStats.atk;
                stats.mag += equipmentStats.mag;
                stats.def += equipmentStats.def;
                stats.spd += equipmentStats.spd;
                stats.luck += equipmentStats.luck;
                stats.foodBonus += equipmentStats.foodBonus;
                stats.manaGainBonus += equipmentStats.manaGainBonus;
                stats.dropRateBonus += equipmentStats.dropRateBonus;
                stats.expBonus += equipmentStats.expBonus;
                stats.goldBonus += equipmentStats.goldBonus;
                stats.healBonus += equipmentStats.healBonus;
                stats.critRate += equipmentStats.critRate;
            }

            stats.hp = stats.maxHp;

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

        private EquipmentService GetEquipmentService()
        {
            if (equipmentService == null) equipmentService = GetComponent<EquipmentService>();
            return equipmentService;
        }
    }
}
