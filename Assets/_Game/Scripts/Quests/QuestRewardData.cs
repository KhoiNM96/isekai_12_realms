using System;

namespace Isekai12Realms.Quests
{
    [Serializable]
    public class QuestRewardData
    {
        public QuestRewardType rewardType;
        public string itemId;
        public string equipmentId;
        public int amount = 1;
    }
}
