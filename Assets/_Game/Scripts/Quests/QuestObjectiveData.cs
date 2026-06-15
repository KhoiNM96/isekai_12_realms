using System;

namespace Isekai12Realms.Quests
{
    [Serializable]
    public class QuestObjectiveData
    {
        public QuestObjectiveType objectiveType;
        public string targetId;
        public int requiredAmount = 1;
        public string description;
    }
}
