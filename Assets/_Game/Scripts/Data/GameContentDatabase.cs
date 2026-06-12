using System.Collections.Generic;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Realms;
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

        public RealmDefinition GetRealmById(string id) => realms.Find(r => r != null && r.id == id);
        public StageDefinition GetStageById(string id) => stages.Find(s => s != null && s.id == id);
        public EnemyDefinition GetEnemyById(string id) => enemies.Find(e => e != null && e.id == id);

        public List<StageDefinition> GetStagesForRealm(string realmId)
        {
            List<StageDefinition> result = stages.FindAll(s => s != null && s.realmId == realmId);
            result.Sort((a, b) => a.stageNumber.CompareTo(b.stageNumber));
            return result;
        }

        public StageDefinition GetNextStage(string currentStageId)
        {
            StageDefinition current = GetStageById(currentStageId);
            if (current == null) return null;

            List<StageDefinition> ordered = new List<StageDefinition>(stages);
            ordered.Sort((a, b) => a.stageNumber == b.stageNumber ? string.CompareOrdinal(a.realmId, b.realmId) : a.stageNumber.CompareTo(b.stageNumber));
            int index = ordered.IndexOf(current);
            return index >= 0 && index + 1 < ordered.Count ? ordered[index + 1] : null;
        }
    }
}
