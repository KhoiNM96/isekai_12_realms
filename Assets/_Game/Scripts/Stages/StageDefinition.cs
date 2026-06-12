using System.Collections.Generic;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using UnityEngine;

namespace Isekai12Realms.Stages
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Stage Definition")]
    public class StageDefinition : ScriptableObject
    {
        public string id;
        public string realmId;
        public string displayName;
        public string description;
        public int stageNumber;
        public int recommendedLevel;
        public EnemyDefinition enemy;
        public DropTableDefinition dropTable;
        public int baseGoldReward;
        public int baseExpReward;
        public List<string> requiredCompletedStageIds = new List<string>();
        public bool isBossStage;
        public bool replayable = true;
        public string battleBackgroundAssetId;
    }
}
