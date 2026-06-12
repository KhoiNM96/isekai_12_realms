using System.Collections.Generic;

namespace Isekai12Realms.Skills
{
    public class SkillResolveResult
    {
        public int damageDealt;
        public int healingDone;
        public int shieldGained;
        public int manaGained;
        public int tilesDestroyed;
        public bool boardShuffled;
        public bool extraTurnGranted;
        public List<string> messages = new List<string>();
    }
}
