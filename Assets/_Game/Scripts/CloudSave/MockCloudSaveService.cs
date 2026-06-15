using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Data;
using Isekai12Realms.Shop;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
namespace Isekai12Realms.CloudSave
{
    public class MockCloudSaveService : ICloudSaveService
    {
        public bool IsAvailable => false;
        public CloudSaveStatus Status { get; private set; } = CloudSaveStatus.LocalOnly;

        public Task<CloudSaveMeta> GetCloudMetaAsync(string uid)
        {
            Debug.Log("[Cloud] Firebase unavailable. Running local-only.");
            return Task.FromResult<CloudSaveMeta>(null);
        }

        public Task<CloudSaveDocument> DownloadSaveAsync(string uid)
        {
            Debug.Log("[Cloud] Firebase unavailable. Running local-only.");
            return Task.FromResult<CloudSaveDocument>(null);
        }

        public Task UploadSaveAsync(string uid, PlayerSaveData save)
        {
            Debug.Log("[Cloud] Firebase unavailable. Running local-only.");
            return Task.CompletedTask;
        }

        public Task MergePurchaseRecordsAsync(string uid, List<PurchaseRecord> localRecords)
        {
            Debug.Log("[Cloud] Firebase unavailable. Running local-only.");
            return Task.CompletedTask;
        }
    }
}
#endif
