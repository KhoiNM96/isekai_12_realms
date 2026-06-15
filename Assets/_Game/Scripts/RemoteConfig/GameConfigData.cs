using UnityEngine;

namespace Isekai12Realms.RemoteConfig
{
    [CreateAssetMenu(fileName = "GameConfigData", menuName = "Isekai 12 Realms/Game Config Data")]
    public class GameConfigData : ScriptableObject
    {
        public bool cloudSyncEnabledDefault = true;
        public bool iapEnabled = true;
        public bool remoteContentEnabled = false;
        public bool tutorialEnabledDefault = true;
        public int dailyShopRefreshHour = 4;
        public int maxLevelCap = 60;
        public float goldRewardMultiplier = 1f;
        public float expRewardMultiplier = 1f;
        public string currentContentVersion = "0.1.0";
        public string minimumAppVersion = "0.1.0";
        public string maintenanceMessage = string.Empty;

        public static GameConfigData CreateDefaults()
        {
            return CreateInstance<GameConfigData>();
        }
    }
}
