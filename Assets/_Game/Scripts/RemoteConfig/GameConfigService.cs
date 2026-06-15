using System.Threading.Tasks;
using UnityEngine;

namespace Isekai12Realms.RemoteConfig
{
    public class GameConfigService
    {
        private readonly GameConfigData defaults;
        private readonly IRemoteConfigService remoteConfig;

        public bool CloudSyncEnabledDefault { get; private set; }
        public bool IapEnabled { get; private set; }
        public bool RemoteContentEnabled { get; private set; }
        public bool TutorialEnabledDefault { get; private set; }
        public int DailyShopRefreshHour { get; private set; }
        public int MaxLevelCap { get; private set; }
        public float GoldRewardMultiplier { get; private set; }
        public float ExpRewardMultiplier { get; private set; }
        public string CurrentContentVersion { get; private set; }
        public string MinimumAppVersion { get; private set; }
        public string MaintenanceMessage { get; private set; }

        public GameConfigService(GameConfigData defaults, IRemoteConfigService remoteConfig)
        {
            this.defaults = defaults != null ? defaults : GameConfigData.CreateDefaults();
            this.remoteConfig = remoteConfig ?? new MockRemoteConfigService();
            ApplyDefaults();
        }

        public async Task InitializeAsync()
        {
            ApplyDefaults();
            try
            {
                await remoteConfig.InitializeAsync();
                await remoteConfig.FetchAndActivateAsync();
                if (remoteConfig.IsAvailable) ApplyRemoteOverrides();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[Config] Remote config unavailable. Using local defaults. " + ex.Message);
                ApplyDefaults();
            }
        }

        private void ApplyDefaults()
        {
            CloudSyncEnabledDefault = defaults.cloudSyncEnabledDefault;
            IapEnabled = defaults.iapEnabled;
            RemoteContentEnabled = defaults.remoteContentEnabled;
            TutorialEnabledDefault = defaults.tutorialEnabledDefault;
            DailyShopRefreshHour = Mathf.Clamp(defaults.dailyShopRefreshHour, 0, 23);
            MaxLevelCap = Mathf.Max(1, defaults.maxLevelCap);
            GoldRewardMultiplier = Mathf.Max(0f, defaults.goldRewardMultiplier);
            ExpRewardMultiplier = Mathf.Max(0f, defaults.expRewardMultiplier);
            CurrentContentVersion = string.IsNullOrEmpty(defaults.currentContentVersion) ? "0.1.0" : defaults.currentContentVersion;
            MinimumAppVersion = string.IsNullOrEmpty(defaults.minimumAppVersion) ? "0.1.0" : defaults.minimumAppVersion;
            MaintenanceMessage = defaults.maintenanceMessage ?? string.Empty;
        }

        private void ApplyRemoteOverrides()
        {
            IapEnabled = remoteConfig.GetBool("iap_enabled", IapEnabled);
            RemoteContentEnabled = remoteConfig.GetBool("remote_content_enabled", RemoteContentEnabled);
            DailyShopRefreshHour = Mathf.Clamp(remoteConfig.GetInt("daily_shop_refresh_hour", DailyShopRefreshHour), 0, 23);
            MaxLevelCap = Mathf.Max(1, remoteConfig.GetInt("max_level_cap", MaxLevelCap));
            GoldRewardMultiplier = Mathf.Max(0f, remoteConfig.GetFloat("gold_reward_multiplier", GoldRewardMultiplier));
            ExpRewardMultiplier = Mathf.Max(0f, remoteConfig.GetFloat("exp_reward_multiplier", ExpRewardMultiplier));
            CurrentContentVersion = remoteConfig.GetString("current_content_version", CurrentContentVersion);
            MinimumAppVersion = remoteConfig.GetString("minimum_app_version", MinimumAppVersion);
            MaintenanceMessage = remoteConfig.GetString("maintenance_message", MaintenanceMessage);
        }
    }
}
