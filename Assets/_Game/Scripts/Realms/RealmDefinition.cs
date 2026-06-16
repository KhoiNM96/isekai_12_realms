using System.Collections.Generic;
using Isekai12Realms.Adventure;
using Isekai12Realms.Stages;
using Isekai12Realms.Enemies;
using UnityEngine;

namespace Isekai12Realms.Realms
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Realm Definition")]
    public class RealmDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public int order;
        public int requiredPlayerLevel = 1;
        public string requiredCompletedRealmId = string.Empty;
        public RealmRank rank = RealmRank.Beginner;
        public string mapBackgroundAssetId;
        public string mapNodeAssetId;
        public string battleBackgroundAssetId;
        public List<EnemyDefinition> normalEnemies = new List<EnemyDefinition>();
        public EnemyDefinition bossEnemy;
        public int monsterSpawnCount = 3;
        public bool unlockedByDefault;
        public string backgroundAssetId;
        public List<StageDefinition> stages = new List<StageDefinition>();
        public int mapWidth = 2400;
        public int mapHeight = 1400;
        public Vector2 playerSpawnPosition = new Vector2(-720f, -360f);
        public List<PlatformSegmentData> platforms = new List<PlatformSegmentData>();
        public List<MonsterSpawnData> normalMonsterSpawns = new List<MonsterSpawnData>();
        public MonsterSpawnData bossSpawn = new MonsterSpawnData();
        public RealmMapLayoutData mapLayout = new RealmMapLayoutData();
    }
}
