using System;

namespace Isekai12Realms.CloudSave
{
    [Serializable]
    public class CloudSaveMeta
    {
        public string uid;
        public string playerName;
        public int level;
        public long exp;
        public int gold;
        public int soulGem;
        public string currentRealmId;
        public string currentStageId;
        public long saveVersion;
        public long updatedAt;
        public long totalPlaySeconds;
        public string deviceId;
        public string appVersion;
    }
}
