using System;

namespace Isekai12Realms.Skills
{
    [Serializable]
    public class SkillLevelData
    {
        public int level = 1;
        public int manaCost;
        public int cooldown;
        public int upgradeGoldCost;
        public string requiredItemId;
        public int requiredItemAmount;
        public string descriptionOverride;
    }
}
