using Isekai12Realms.Data;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Skills
{
    public class SkillService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private ToastService toastService;

        public PlayerSaveData Save => saveService?.CurrentSave;

        public void Initialize(ISaveService save, ContentDatabaseService content, ToastService toast)
        {
            saveService = save;
            contentService = content;
            toastService = toast;
            EnsureSkillDefaults();
        }

        public PlayerSkillData GetPlayerSkill(string skillId) => Save?.skills != null ? Save.skills.Find(s => s.skillId == skillId) : null;
        public SkillDefinition GetSkillDefinition(string skillId) => contentService?.Database != null ? contentService.Database.GetSkillById(skillId) : null;

        public SkillDefinition GetEquippedSkill(SkillSlotType slot)
        {
            return GetSkillDefinition(GetEquippedSkillId(slot));
        }

        public string GetEquippedSkillId(SkillSlotType slot)
        {
            if (Save == null) return string.Empty;
            switch (slot)
            {
                case SkillSlotType.Skill1: return Save.equippedSkill1Id;
                case SkillSlotType.Skill2: return Save.equippedSkill2Id;
                case SkillSlotType.Ultimate: return Save.equippedUltimateId;
                default: return string.Empty;
            }
        }

        public bool EquipSkill(string skillId, SkillSlotType slot)
        {
            SkillDefinition def = GetSkillDefinition(skillId);
            PlayerSkillData skill = GetPlayerSkill(skillId);
            if (Save == null || def == null || skill == null || !skill.unlocked || def.slotType != slot || def.classId != Save.selectedClassId)
            {
                Toast("Skill cannot be equipped.");
                return false;
            }
            if (slot == SkillSlotType.Skill1) Save.equippedSkill1Id = skillId;
            if (slot == SkillSlotType.Skill2) Save.equippedSkill2Id = skillId;
            if (slot == SkillSlotType.Ultimate) Save.equippedUltimateId = skillId;
            SaveNow();
            Toast("Skill equipped: " + def.displayName);
            return true;
        }

        public bool CanUpgradeSkill(string skillId)
        {
            return GetUpgradeBlocker(skillId) == string.Empty;
        }

        public bool UpgradeSkill(string skillId)
        {
            string blocker = GetUpgradeBlocker(skillId);
            if (!string.IsNullOrEmpty(blocker))
            {
                Toast(blocker);
                return false;
            }

            PlayerSkillData playerSkill = GetPlayerSkill(skillId);
            SkillDefinition def = GetSkillDefinition(skillId);
            SkillLevelData next = def.GetLevelData(playerSkill.level + 1);
            Save.gold -= next.upgradeGoldCost;
            if (!string.IsNullOrEmpty(next.requiredItemId) && next.requiredItemAmount > 0)
            {
                ItemStackData stack = Save.inventory.items.Find(i => i.itemId == next.requiredItemId);
                stack.amount -= next.requiredItemAmount;
                if (stack.amount <= 0) Save.inventory.items.Remove(stack);
            }
            playerSkill.level++;
            SaveNow();
            Toast($"{def.displayName} upgraded to Lv. {playerSkill.level}");
            return true;
        }

        public int GetSkillLevel(string skillId) => GetPlayerSkill(skillId)?.level ?? 1;
        public int GetManaCost(string skillId) => GetLevelData(skillId)?.manaCost ?? GetSkillDefinition(skillId)?.baseManaCost ?? 0;
        public int GetCooldown(string skillId) => GetLevelData(skillId)?.cooldown ?? GetSkillDefinition(skillId)?.baseCooldown ?? 0;
        public void StartCooldown(string skillId) { PlayerSkillData skill = GetPlayerSkill(skillId); if (skill != null) { skill.cooldownRemaining = GetCooldown(skillId); SaveNow(); } }

        public void TickCooldownsAfterPlayerTurn()
        {
            if (Save == null) return;
            if (Save.skills == null) return;
            foreach (PlayerSkillData skill in Save.skills)
            {
                if (skill.cooldownRemaining > 0) skill.cooldownRemaining--;
            }
            SaveNow();
        }

        public bool IsSkillUsable(string skillId, Battle.BattleState state)
        {
            PlayerSkillData skill = GetPlayerSkill(skillId);
            SkillDefinition def = GetSkillDefinition(skillId);
            return skill != null && def != null && skill.unlocked && skill.cooldownRemaining <= 0 && state != null && state.currentTurnOwner == Data.BattleTurnOwner.Player && state.mana >= GetManaCost(skillId) && state.battleResult == Data.BattleResultType.None;
        }

        public void EnsureSkillDefaults()
        {
            if (Save == null) return;
            if (Save.skills == null) Save.skills = new System.Collections.Generic.List<PlayerSkillData>();
            if (Save.inventory == null) Save.inventory = new InventorySaveData();
            if (Save.inventory.items == null) Save.inventory.items = new System.Collections.Generic.List<ItemStackData>();
            AddDefault("skill_flame_spark_slash");
            AddDefault("skill_flame_shuffle_bell");
            AddDefault("skill_flame_realm_burst");
            if (string.IsNullOrEmpty(Save.equippedSkill1Id)) Save.equippedSkill1Id = "skill_flame_spark_slash";
            if (string.IsNullOrEmpty(Save.equippedSkill2Id)) Save.equippedSkill2Id = "skill_flame_shuffle_bell";
            if (string.IsNullOrEmpty(Save.equippedUltimateId)) Save.equippedUltimateId = "skill_flame_realm_burst";
        }

        public void SetDefaultClassSkills(string classId)
        {
            if (Save == null) return;
            Save.selectedClassId = string.IsNullOrEmpty(classId) ? "flame_squire" : classId;
            string s1 = "skill_flame_spark_slash";
            string s2 = "skill_flame_shuffle_bell";
            string ult = "skill_flame_realm_burst";
            if (Save.selectedClassId == "tide_acolyte")
            {
                s1 = "skill_tide_aqua_heal";
                s2 = "skill_tide_bubble_guard";
                ult = "skill_tide_moon_tide";
            }
            else if (Save.selectedClassId == "storm_scout")
            {
                s1 = "skill_storm_quick_jab";
                s2 = "skill_storm_static_step";
                ult = "skill_storm_thunder_chain";
            }
            AddDefault(s1);
            AddDefault(s2);
            AddDefault(ult);
            Save.equippedSkill1Id = s1;
            Save.equippedSkill2Id = s2;
            Save.equippedUltimateId = ult;
            SaveNow();
        }

        private string GetUpgradeBlocker(string skillId)
        {
            PlayerSkillData playerSkill = GetPlayerSkill(skillId);
            SkillDefinition def = GetSkillDefinition(skillId);
            if (Save == null || playerSkill == null || def == null) return "Skill not found.";
            if (def.classId != Save.selectedClassId) return "Class switching will be added later.";
            if (Save.inventory == null) Save.inventory = new InventorySaveData();
            if (Save.inventory.items == null) Save.inventory.items = new System.Collections.Generic.List<ItemStackData>();
            if (playerSkill.level >= def.maxLevel) return "Skill is already max level.";
            SkillLevelData next = def.GetLevelData(playerSkill.level + 1);
            if (next == null) return "Upgrade data missing.";
            if (Save.gold < next.upgradeGoldCost) return "Not enough gold.";
            if (!string.IsNullOrEmpty(next.requiredItemId) && next.requiredItemAmount > 0)
            {
                ItemStackData stack = Save.inventory.items.Find(i => i.itemId == next.requiredItemId);
                if (stack == null || stack.amount < next.requiredItemAmount) return "Not enough item.";
            }
            return string.Empty;
        }

        private SkillLevelData GetLevelData(string skillId)
        {
            return GetSkillDefinition(skillId)?.GetLevelData(GetSkillLevel(skillId));
        }

        private void AddDefault(string skillId)
        {
            if (Save.skills.Exists(s => s.skillId == skillId)) return;
            Save.skills.Add(new PlayerSkillData { skillId = skillId, level = 1, unlocked = true });
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
