using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Skills
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Skill Definition")]
    public class SkillDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public string classId;
        public string iconAssetId;
        public SkillSlotType slotType;
        public SkillTargetType targetType;
        public SkillActivationType activationType;
        public int maxLevel = 1;
        public int baseManaCost;
        public int baseCooldown;
        public List<SkillLevelData> levels = new List<SkillLevelData>();
        public List<SkillEffectData> effects = new List<SkillEffectData>();

        public SkillLevelData GetLevelData(int level)
        {
            return levels != null ? levels.Find(l => l.level == level) : null;
        }
    }
}
