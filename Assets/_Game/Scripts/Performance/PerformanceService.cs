using Isekai12Realms.Build;
using UnityEngine;

namespace Isekai12Realms.Performance
{
    public class PerformanceService
    {
        private readonly BuildConfigService buildConfigService;

        public bool ReduceEffects { get; private set; }
        public int TargetFrameRate { get; private set; }

        public PerformanceService(BuildConfigService config)
        {
            buildConfigService = config;
            DetectDeviceProfile();
        }

        public void Apply()
        {
            Application.targetFrameRate = TargetFrameRate;
            QualitySettings.vSyncCount = 0;
            Debug.Log($"[Performance] targetFrameRate={TargetFrameRate} reduceEffects={ReduceEffects}");
        }

        private void DetectDeviceProfile()
        {
            int memory = SystemInfo.systemMemorySize;
            bool lowEnd = memory > 0 && memory <= 2048;
            ReduceEffects = lowEnd;
            TargetFrameRate = lowEnd ? buildConfigService.FallbackFrameRate : buildConfigService.TargetFrameRate;
        }
    }
}
