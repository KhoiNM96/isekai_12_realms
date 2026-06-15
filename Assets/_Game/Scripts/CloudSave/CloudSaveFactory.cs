using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.CloudSave
{
    public static class CloudSaveFactory
    {
        public static CloudSaveMeta CreateMeta(string uid, PlayerSaveData save)
        {
            if (save == null) return null;
            return new CloudSaveMeta
            {
                uid = uid,
                playerName = save.playerName,
                level = save.level,
                exp = save.exp,
                gold = save.gold,
                soulGem = save.soulGem,
                currentRealmId = save.currentRealmId,
                currentStageId = save.currentStageId,
                saveVersion = save.saveVersion,
                updatedAt = save.updatedAt,
                totalPlaySeconds = save.totalPlaySeconds,
                deviceId = save.deviceId,
                appVersion = Application.version
            };
        }
    }
}
