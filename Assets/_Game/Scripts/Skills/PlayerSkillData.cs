using System;

namespace Isekai12Realms.Skills
{
    [Serializable]
    public class PlayerSkillData
    {
        public string skillId;
        public int level = 1;
        public bool unlocked = true;
        public int cooldownRemaining;
    }
}
