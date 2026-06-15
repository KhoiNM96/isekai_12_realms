using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Quests
{
    [CreateAssetMenu(menuName = "Isekai 12 Realms/Quest Definition")]
    public class QuestDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public QuestType questType;
        public List<QuestObjectiveData> objectives = new List<QuestObjectiveData>();
        public List<QuestRewardData> rewards = new List<QuestRewardData>();
        public List<string> requiredQuestIds = new List<string>();
        public bool autoStart;
        public bool autoClaim;
        public int order;
        public string iconAssetId;
    }
}
