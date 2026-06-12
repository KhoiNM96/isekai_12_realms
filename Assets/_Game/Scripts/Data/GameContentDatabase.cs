using System.Collections.Generic;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Equipment;
using Isekai12Realms.Realms;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Data
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Game Content Database")]
    public class GameContentDatabase : ScriptableObject
    {
        public List<RealmDefinition> realms = new List<RealmDefinition>();
        public List<StageDefinition> stages = new List<StageDefinition>();
        public List<EnemyDefinition> enemies = new List<EnemyDefinition>();
        public List<DropTableDefinition> dropTables = new List<DropTableDefinition>();
        public List<SkillDefinition> skills = new List<SkillDefinition>();
        public List<EquipmentDefinition> equipmentDefinitions = new List<EquipmentDefinition>();

        public RealmDefinition GetRealmById(string id) => realms != null ? realms.Find(r => r != null && r.id == id) : null;
        public StageDefinition GetStageById(string id) => stages != null ? stages.Find(s => s != null && s.id == id) : null;
        public EnemyDefinition GetEnemyById(string id) => enemies != null ? enemies.Find(e => e != null && e.id == id) : null;
        public SkillDefinition GetSkillById(string id) => skills != null ? skills.Find(s => s != null && s.id == id) : null;
        public List<SkillDefinition> GetSkillsByClass(string classId) => skills != null ? skills.FindAll(s => s != null && s.classId == classId) : new List<SkillDefinition>();
        public EquipmentDefinition GetEquipmentDefinitionById(string id) => equipmentDefinitions != null ? equipmentDefinitions.Find(e => e != null && e.id == id) : null;
        public List<EquipmentDefinition> GetEquipmentBySlot(EquipmentSlot slot) => equipmentDefinitions != null ? equipmentDefinitions.FindAll(e => e != null && e.slot == slot) : new List<EquipmentDefinition>();

        public List<SkillDefinition> GetDefaultSkillsForClass(string classId)
        {
            List<SkillDefinition> result = GetSkillsByClass(classId).FindAll(s => s.slotType == SkillSlotType.Skill1 || s.slotType == SkillSlotType.Skill2 || s.slotType == SkillSlotType.Ultimate);
            result.Sort((a, b) => a.slotType.CompareTo(b.slotType));
            return result;
        }

        public List<StageDefinition> GetStagesForRealm(string realmId)
        {
            List<StageDefinition> result = stages != null ? stages.FindAll(s => s != null && s.realmId == realmId) : new List<StageDefinition>();
            result.Sort((a, b) => a.stageNumber.CompareTo(b.stageNumber));
            return result;
        }

        public StageDefinition GetNextStage(string currentStageId)
        {
            StageDefinition current = GetStageById(currentStageId);
            if (current == null) return null;

            List<StageDefinition> ordered = stages != null ? new List<StageDefinition>(stages) : new List<StageDefinition>();
            ordered.Sort((a, b) => a.stageNumber == b.stageNumber ? string.CompareOrdinal(a.realmId, b.realmId) : a.stageNumber.CompareTo(b.stageNumber));
            int index = ordered.IndexOf(current);
            return index >= 0 && index + 1 < ordered.Count ? ordered[index + 1] : null;
        }
    }
}
