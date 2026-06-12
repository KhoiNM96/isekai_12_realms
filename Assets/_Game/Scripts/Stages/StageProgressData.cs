using System;

namespace Isekai12Realms.Stages
{
    [Serializable]
    public class StageProgressData
    {
        public string stageId;
        public int clearCount;
        public long firstClearedAt;
        public long lastClearedAt;
    }
}
