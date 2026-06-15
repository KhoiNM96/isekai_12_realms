using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.CloudSave;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using UnityEngine;

#if USE_FIREBASE
using Firebase.Firestore;
#endif

namespace Isekai12Realms.FirebaseIntegration
{
    public class FirestoreCloudSaveService : ICloudSaveService
    {
        private readonly ISaveService saveService;
#if USE_FIREBASE
        private FirebaseFirestore firestore;
#endif

        public FirestoreCloudSaveService(ISaveService save)
        {
            saveService = save;
#if USE_FIREBASE
            try
            {
                firestore = FirebaseFirestore.DefaultInstance;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Firestore] Unavailable: " + e.Message);
                firestore = null;
            }
#endif
        }

#if USE_FIREBASE
        public bool IsAvailable => firestore != null;
#else
        public bool IsAvailable => false;
#endif
        public CloudSaveStatus Status { get; private set; } = CloudSaveStatus.Unknown;

        public async Task<CloudSaveMeta> GetCloudMetaAsync(string uid)
        {
#if USE_FIREBASE
            DocumentSnapshot snapshot = await firestore.Document($"users/{uid}/saves/default").GetSnapshotAsync();
            if (!snapshot.Exists) return null;
            CloudSaveDocument doc = snapshot.ConvertTo<CloudSaveDocument>();
            return doc?.meta;
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        public async Task<CloudSaveDocument> DownloadSaveAsync(string uid)
        {
#if USE_FIREBASE
            DocumentSnapshot snapshot = await firestore.Document($"users/{uid}/saves/default").GetSnapshotAsync();
            return snapshot.Exists ? snapshot.ConvertTo<CloudSaveDocument>() : null;
#else
            await Task.CompletedTask;
            return null;
#endif
        }

        public async Task UploadSaveAsync(string uid, PlayerSaveData save)
        {
#if USE_FIREBASE
            Status = CloudSaveStatus.Syncing;
            string json = JsonUtility.ToJson(save, true);
            CloudSaveDocument doc = new CloudSaveDocument { meta = CloudSaveFactory.CreateMeta(uid, save), saveJson = json, checksum = SaveChecksumUtility.ComputeChecksum(json), purchaseRecords = save.purchaseRecords ?? new List<PurchaseRecord>(), uploadedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
            await firestore.Document($"users/{uid}/saves/default").SetAsync(doc);
            await firestore.Document($"users/{uid}/profile/main").SetAsync(doc.meta);
            await MergePurchaseRecordsAsync(uid, save.purchaseRecords);
            save.lastCloudUploadAt = doc.uploadedAt;
            save.firebaseUid = uid;
            saveService.SaveNow();
            Status = CloudSaveStatus.Synced;
#else
            await Task.CompletedTask;
#endif
        }

        public async Task MergePurchaseRecordsAsync(string uid, List<PurchaseRecord> localRecords)
        {
#if USE_FIREBASE
            foreach (PurchaseRecord record in localRecords ?? new List<PurchaseRecord>())
            {
                if (record == null || string.IsNullOrEmpty(record.transactionId)) continue;
                await firestore.Document($"users/{uid}/purchases/{record.transactionId}").SetAsync(record);
            }
#else
            await Task.CompletedTask;
#endif
        }
    }
}
