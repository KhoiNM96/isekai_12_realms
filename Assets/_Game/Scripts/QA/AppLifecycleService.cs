using Isekai12Realms.CloudSave;
using Isekai12Realms.Core;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.QA
{
    public class AppLifecycleService : MonoBehaviour
    {
        private CloudSaveCoordinator cloudSaveCoordinator;
        private ShopService shopService;
        private UIScreenManager screenManager;

        public void Initialize(CloudSaveCoordinator cloud, ShopService shop, UIScreenManager ui)
        {
            cloudSaveCoordinator = cloud;
            shopService = shop;
            screenManager = ui;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveLocal();
                cloudSaveCoordinator?.QueueCloudSync(0f);
                AudioListener.pause = true;
                return;
            }

            AudioListener.pause = false;
            shopService?.CheckDailyRefresh();
            screenManager?.ToastService?.ShowToast("Welcome back.");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveLocal();
        }

        private static void SaveLocal()
        {
            if (ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService)) saveService.SaveNow();
        }
    }
}
