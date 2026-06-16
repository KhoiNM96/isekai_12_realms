using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Realms;

namespace Isekai12Realms.Battle
{
    public class BattleEncounterData
    {
        public string encounterId;
        public string realmId;
        public string enemyId;
        public string displayName;
        public EnemyDefinition enemy;
        public bool isBoss;
        public int baseGoldReward;
        public int baseExpReward;
        public DropTableDefinition dropTable;
        public string battleBackgroundAssetId;
        public RealmDefinition realm;
    }
}
