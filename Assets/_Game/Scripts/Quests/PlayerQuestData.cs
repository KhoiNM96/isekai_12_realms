using System;
using System.Collections.Generic;

namespace Isekai12Realms.Quests
{
    [Serializable]
    public class QuestObjectiveProgressData
    {
        public int objectiveIndex;
        public int currentAmount;
        public bool completed;
    }

    [Serializable]
    public class PlayerQuestData
    {
        public string questId;
        public QuestStatus status;
        public List<QuestObjectiveProgressData> objectiveProgress = new List<QuestObjectiveProgressData>();
        public long startedAt;
        public long completedAt;
        public long claimedAt;
    }
}
