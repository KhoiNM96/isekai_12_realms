using System;

namespace Isekai12Realms.Realms
{
    [Serializable]
    public class RealmProgressData
    {
        public string realmId;
        public int normalMonstersDefeated;
        public bool bossDefeated;
        public long firstEnteredAt;
        public long completedAt;
    }
}
