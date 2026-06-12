using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Realms;
using UnityEngine;

namespace Isekai12Realms.Stages
{
    public class ContentDatabaseService : MonoBehaviour
    {
        [SerializeField] private GameContentDatabase database;
        public GameContentDatabase Database => database;

        public void SetDatabase(GameContentDatabase contentDatabase)
        {
            database = contentDatabase;
        }

        public void Initialize(GameContentDatabase contentDatabase = null)
        {
            database = contentDatabase != null ? contentDatabase : database;
#if UNITY_EDITOR
            if (database == null)
            {
                database = UnityEditor.AssetDatabase.LoadAssetAtPath<GameContentDatabase>("Assets/_Game/ScriptableObjects/GameContentDatabase.asset");
            }
#endif
            if (database == null)
            {
                Debug.LogWarning("[Content] GameContentDatabase missing. Using runtime prototype fallback.");
                database = CreateRuntimeFallback();
            }
        }

        public List<RealmDefinition> Realms => database != null ? database.realms : new List<RealmDefinition>();
        public RealmDefinition GetRealmById(string id) => database != null ? database.GetRealmById(id) : null;
        public StageDefinition GetStageById(string id) => database != null ? database.GetStageById(id) : null;
        public EnemyDefinition GetEnemyById(string id) => database != null ? database.GetEnemyById(id) : null;
        public List<StageDefinition> GetStagesForRealm(string realmId) => database != null ? database.GetStagesForRealm(realmId) : new List<StageDefinition>();

        private GameContentDatabase CreateRuntimeFallback()
        {
            GameContentDatabase db = ScriptableObject.CreateInstance<GameContentDatabase>();
            EnemyDefinition slime = ScriptableObject.CreateInstance<EnemyDefinition>();
            slime.id = "enemy_meadow_slime"; slime.displayName = "Meadow Slime"; slime.level = 1; slime.maxHp = 80; slime.attack = 8; slime.maxMana = 100; slime.difficulty = EnemyAIDifficulty.Easy;
            DropTableDefinition drop = ScriptableObject.CreateInstance<DropTableDefinition>();
            drop.id = "drop_stage_01_01";
            drop.drops.Add(new DropEntry { itemId = "mat_slime_jelly", minAmount = 1, maxAmount = 2, chance = 1f });
            drop.drops.Add(new DropEntry { itemId = "item_potion_small", minAmount = 1, maxAmount = 1, chance = 0.25f });
            drop.drops.Add(new DropEntry { equipmentId = "equip_weapon_wooden_sword", minAmount = 1, maxAmount = 1, chance = 0.15f, isEquipment = true });
            StageDefinition stage = ScriptableObject.CreateInstance<StageDefinition>();
            stage.id = "stage_01_01"; stage.realmId = "realm_01_meadow"; stage.displayName = "First Slime"; stage.stageNumber = 1; stage.recommendedLevel = 1; stage.baseGoldReward = 30; stage.baseExpReward = 50; stage.enemy = slime; stage.dropTable = drop; stage.replayable = true;
            RealmDefinition realm = ScriptableObject.CreateInstance<RealmDefinition>();
            realm.id = "realm_01_meadow"; realm.displayName = "Meadow Gate"; realm.description = "A peaceful floating meadow where the reborn hero begins the journey."; realm.order = 1; realm.stages.Add(stage);
            db.enemies.Add(slime); db.dropTables.Add(drop); db.stages.Add(stage); db.realms.Add(realm);
            return db;
        }
    }
}
