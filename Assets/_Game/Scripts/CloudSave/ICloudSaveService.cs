using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Data;
using Isekai12Realms.Shop;

namespace Isekai12Realms.CloudSave
{
    public interface ICloudSaveService
    {
        bool IsAvailable { get; }
        CloudSaveStatus Status { get; }
        Task<CloudSaveMeta> GetCloudMetaAsync(string uid);
        Task<CloudSaveDocument> DownloadSaveAsync(string uid);
        Task UploadSaveAsync(string uid, PlayerSaveData save);
        Task MergePurchaseRecordsAsync(string uid, List<PurchaseRecord> localRecords);
    }
}
