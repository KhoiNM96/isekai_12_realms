using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Auth;
using Isekai12Realms.Data;
using Isekai12Realms.FirebaseIntegration;
using Isekai12Realms.Purchases;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.CloudSave
{
    public class CloudSaveCoordinator : MonoBehaviour
    {
        private ISaveService saveService;
        private ToastService toastService;
        private IAuthService authService;
        private ICloudSaveService cloudSaveService;
        private CloudSaveDocument pendingCloudDocument;
        private CloudSaveMeta pendingCloudMeta;
        private bool initialized;
        private bool autoSyncDisabledThisSession;
        private bool syncQueued;
        private float nextSyncAt;

        public event Action StatusChanged;
        public event Action<CloudSaveMeta, CloudSaveMeta> ConflictDetected;

        public CloudSaveStatus Status { get; private set; } = CloudSaveStatus.Unknown;
        public AuthUserData CurrentUser => authService?.CurrentUser;
        public bool IsCloudAvailable => cloudSaveService != null && cloudSaveService.IsAvailable;
        public bool CloudSyncEnabled => saveService?.CurrentSave == null || saveService.CurrentSave.cloudSyncEnabled;
        public bool IsGoogleSignInConfigured => authService is FirebaseIntegration.FirebaseAuthService firebaseAuth && firebaseAuth.IsGoogleSignInConfigured;

        public void Initialize(ISaveService save, ToastService toast)
        {
            saveService = save;
            toastService = toast;
#if USE_FIREBASE
            authService = new FirebaseAuthService(saveService, toastService);
            cloudSaveService = new FirestoreCloudSaveService(saveService);
#else
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            authService = new MockAuthService(saveService);
            cloudSaveService = new MockCloudSaveService();
#else
            authService = new LocalOnlyAuthService();
            cloudSaveService = new LocalOnlyCloudSaveService();
#endif
#endif
            _ = InitializeAsync();
        }

        private void Update()
        {
            if (syncQueued && Time.unscaledTime >= nextSyncAt)
            {
                syncQueued = false;
                _ = UploadLocalSaveAsync(false);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause) _ = UploadLocalSaveAsync(false);
        }

        public async Task InitializeAsync()
        {
            EnsureLocalCloudDefaults();
            initialized = true;
            if (!IsCloudAvailable)
            {
                SetStatus(CloudSaveStatus.LocalOnly);
                Debug.Log("[Cloud] Cloud save unavailable. Running local-only.");
                return;
            }

            if (!CloudSyncEnabled)
            {
                SetStatus(CloudSaveStatus.LocalOnly);
                return;
            }

            await SignInGuestAsync(false);
            await CompareLocalAndCloudAsync(true);
        }

        public async Task<AuthUserData> SignInGuestAsync(bool showToast = true)
        {
            EnsureLocalCloudDefaults();
            AuthUserData user = await authService.SignInAnonymousAsync();
            if (user == null || !IsCloudAvailable)
            {
                SetStatus(CloudSaveStatus.LocalOnly);
                if (showToast) toastService?.ShowToast("Cloud save is currently unavailable.");
                return user;
            }

            PlayerSaveData save = saveService.CurrentSave;
            save.firebaseUid = user.uid;
            save.authProvider = user.providerType.ToString();
            saveService.SaveNow();
            SetStatus(CloudSaveStatus.SignedIn);
            if (showToast) toastService?.ShowToast("Guest cloud account ready.");
            return user;
        }

        public async Task<AuthUserData> SignInGoogleAsync()
        {
            AuthUserData user = await authService.SignInGoogleAsync();
            if (user == null || user.providerType != AuthProviderType.Google) toastService?.ShowToast("Google Sign-In is not available.");
            return user;
        }

        public async Task SyncNowAsync()
        {
            if (!ReadyForCloudAction()) return;
            await CompareLocalAndCloudAsync(false);
            if (Status != CloudSaveStatus.Conflict) await UploadLocalSaveAsync(true);
        }

        public async Task UploadLocalSaveAsync(bool showToast = true)
        {
            if (!ReadyForCloudAction(showToast)) return;
            try
            {
                SetStatus(CloudSaveStatus.Syncing);
                await cloudSaveService.UploadSaveAsync(CurrentUser.uid, saveService.CurrentSave);
                await SyncPurchaseLedgerNowAsync();
                saveService.CurrentSave.lastCloudUploadAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                saveService.CurrentSave.lastSyncedDeviceId = saveService.CurrentSave.deviceId;
                saveService.SaveNow();
                SetStatus(CloudSaveStatus.Synced);
                if (showToast) toastService?.ShowToast("Cloud save uploaded.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Cloud] Upload failed: " + e.Message);
                SetStatus(CloudSaveStatus.Error);
                if (showToast) toastService?.ShowToast("Cloud sync failed. Please try again.");
            }
        }

        public async Task DownloadCloudSaveAsync()
        {
            if (!ReadyForCloudAction()) return;
            try
            {
                CloudSaveDocument doc = pendingCloudDocument ?? await cloudSaveService.DownloadSaveAsync(CurrentUser.uid);
                if (!ApplyCloudDocument(doc))
                {
                    toastService?.ShowToast("Cloud save is invalid. Local save kept.");
                    return;
                }
                pendingCloudDocument = null;
                pendingCloudMeta = null;
                SetStatus(CloudSaveStatus.Synced);
                toastService?.ShowToast("Cloud save downloaded.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Cloud] Download failed: " + e.Message);
                SetStatus(CloudSaveStatus.Error);
                toastService?.ShowToast("Cloud save download failed. Please try again.");
            }
        }

        public async Task ResolveConflictUseLocalAsync()
        {
            await UploadLocalSaveAsync(true);
            pendingCloudDocument = null;
            pendingCloudMeta = null;
        }

        public async Task ResolveConflictUseCloudAsync()
        {
            await DownloadCloudSaveAsync();
        }

        public void CancelConflictForSession()
        {
            autoSyncDisabledThisSession = true;
            SetStatus(CloudSaveStatus.SignedIn);
            toastService?.ShowToast("Cloud sync paused for this session.");
        }

        public CloudSaveStatus GetStatus() => Status;

        public void QueueCloudSync(float debounceSeconds = 10f)
        {
            if (!initialized || autoSyncDisabledThisSession || !ReadyForCloudAction(false)) return;
            syncQueued = true;
            nextSyncAt = Time.unscaledTime + Mathf.Max(1f, debounceSeconds);
        }

        public void QueuePurchaseLedgerSync()
        {
            QueueCloudSync(1f);
        }

        public async Task SyncPurchaseLedgerNowAsync()
        {
            if (!ReadyForCloudAction()) return;
            PurchaseLedgerService ledger = new PurchaseLedgerService(saveService);
            List<PurchaseRecord> unsynced = ledger.GetUnsyncedRecords();
            await cloudSaveService.MergePurchaseRecordsAsync(CurrentUser.uid, unsynced);
            foreach (PurchaseRecord record in unsynced)
            {
                if (record != null) ledger.MarkCloudSynced(record.transactionId);
            }
        }

        public void SetCloudSyncEnabled(bool enabled)
        {
            if (saveService?.CurrentSave == null) return;
            saveService.CurrentSave.cloudSyncEnabled = enabled;
            saveService.SaveNow();
            if (!enabled) SetStatus(CloudSaveStatus.LocalOnly);
        }

        public void ClearFirebaseUidFromLocalSave()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PlayerSaveData save = saveService?.CurrentSave;
            if (save == null) return;
            save.firebaseUid = string.Empty;
            save.authProvider = AuthProviderType.LocalOnly.ToString();
            saveService.SaveNow();
            toastService?.ShowToast("Firebase UID cleared locally.");
            StatusChanged?.Invoke();
#endif
        }

        public void ForceConflictTest()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            PlayerSaveData save = saveService?.CurrentSave;
            if (save == null) return;
            CloudSaveMeta local = CloudSaveFactory.CreateMeta(save.firebaseUid, save);
            CloudSaveMeta cloud = CloudSaveFactory.CreateMeta(save.firebaseUid, save);
            cloud.saveVersion += 2;
            cloud.updatedAt += 120;
            cloud.deviceId = "debug_cloud_device";
            pendingCloudMeta = cloud;
            SetStatus(CloudSaveStatus.Conflict);
            ConflictDetected?.Invoke(local, cloud);
#endif
        }

        private async Task CompareLocalAndCloudAsync(bool allowAutoUpload)
        {
            if (!ReadyForCloudAction(false)) return;
            CloudSaveMeta cloud = await cloudSaveService.GetCloudMetaAsync(CurrentUser.uid);
            PlayerSaveData save = saveService.CurrentSave;
            CloudSaveMeta local = CloudSaveFactory.CreateMeta(CurrentUser.uid, save);
            if (cloud == null)
            {
                if (allowAutoUpload) await UploadLocalSaveAsync(false);
                return;
            }
            if (HasMeaningfulConflict(local, cloud))
            {
                pendingCloudMeta = cloud;
                pendingCloudDocument = await cloudSaveService.DownloadSaveAsync(CurrentUser.uid);
                SetStatus(CloudSaveStatus.Conflict);
                ConflictDetected?.Invoke(local, cloud);
                return;
            }
            if (local.saveVersion > cloud.saveVersion && allowAutoUpload) await UploadLocalSaveAsync(false);
        }

        private bool HasMeaningfulConflict(CloudSaveMeta local, CloudSaveMeta cloud)
        {
            if (local == null || cloud == null) return false;
            if (local.saveVersion > cloud.saveVersion) return false;
            if (cloud.saveVersion > local.saveVersion) return true;
            return local.deviceId != cloud.deviceId && local.updatedAt != cloud.updatedAt && (local.level > 1 || cloud.level > 1 || local.gold > 0 || cloud.gold > 0);
        }

        private bool ApplyCloudDocument(CloudSaveDocument doc)
        {
            if (doc == null || string.IsNullOrEmpty(doc.saveJson)) return false;
            if (!SaveChecksumUtility.VerifyChecksum(doc.saveJson, doc.checksum)) return false;
            PlayerSaveData downloaded = JsonUtility.FromJson<PlayerSaveData>(doc.saveJson);
            if (downloaded == null || string.IsNullOrEmpty(downloaded.playerId)) return false;
            MergePurchaseRecords(downloaded, saveService.CurrentSave?.purchaseRecords, doc.purchaseRecords);
            downloaded.firebaseUid = CurrentUser?.uid ?? downloaded.firebaseUid;
            downloaded.authProvider = CurrentUser != null ? CurrentUser.providerType.ToString() : downloaded.authProvider;
            return saveService.ReplaceCurrentSave(downloaded);
        }

        private static void MergePurchaseRecords(PlayerSaveData target, List<PurchaseRecord> local, List<PurchaseRecord> cloud)
        {
            if (target.purchaseRecords == null) target.purchaseRecords = new List<PurchaseRecord>();
            HashSet<string> seen = new HashSet<string>();
            foreach (PurchaseRecord record in target.purchaseRecords) if (record != null && !string.IsNullOrEmpty(record.transactionId)) seen.Add(record.transactionId);
            foreach (PurchaseRecord record in local ?? new List<PurchaseRecord>()) AddRecord(target.purchaseRecords, seen, record);
            foreach (PurchaseRecord record in cloud ?? new List<PurchaseRecord>()) AddRecord(target.purchaseRecords, seen, record);
        }

        private static void AddRecord(List<PurchaseRecord> target, HashSet<string> seen, PurchaseRecord record)
        {
            if (record == null || string.IsNullOrEmpty(record.transactionId) || !seen.Add(record.transactionId)) return;
            target.Add(record);
        }

        private bool ReadyForCloudAction(bool showToast = true)
        {
            if (saveService?.CurrentSave == null) return false;
            if (!CloudSyncEnabled)
            {
                if (showToast) toastService?.ShowToast("Cloud sync is disabled.");
                return false;
            }
            if (!IsCloudAvailable || CurrentUser == null)
            {
                if (showToast) toastService?.ShowToast("Cloud save is currently unavailable.");
                SetStatus(CloudSaveStatus.LocalOnly);
                return false;
            }
            return true;
        }

        private void EnsureLocalCloudDefaults()
        {
            PlayerSaveData save = saveService?.CurrentSave;
            if (save == null) return;
            bool changed = false;
            if (string.IsNullOrEmpty(save.deviceId)) { save.deviceId = "device_" + Guid.NewGuid().ToString("N"); changed = true; }
            if (string.IsNullOrEmpty(save.authProvider)) { save.authProvider = AuthProviderType.LocalOnly.ToString(); changed = true; }
            if (string.IsNullOrEmpty(save.cloudSaveId)) { save.cloudSaveId = "default"; changed = true; }
            if (changed) saveService.SaveNow();
        }

        private void SetStatus(CloudSaveStatus status)
        {
            Status = status;
            StatusChanged?.Invoke();
        }

        private class LocalOnlyAuthService : IAuthService
        {
            public bool IsAvailable => false;
            public bool IsSignedIn => false;
            public AuthUserData CurrentUser => null;
            public Task<AuthUserData> SignInAnonymousAsync() => Task.FromResult<AuthUserData>(null);
            public Task<AuthUserData> SignInGoogleAsync() => Task.FromResult<AuthUserData>(null);
            public Task SignOutAsync() => Task.CompletedTask;
        }

        private class LocalOnlyCloudSaveService : ICloudSaveService
        {
            public bool IsAvailable => false;
            public CloudSaveStatus Status => CloudSaveStatus.LocalOnly;
            public Task<CloudSaveMeta> GetCloudMetaAsync(string uid) => Task.FromResult<CloudSaveMeta>(null);
            public Task<CloudSaveDocument> DownloadSaveAsync(string uid) => Task.FromResult<CloudSaveDocument>(null);
            public Task UploadSaveAsync(string uid, PlayerSaveData save) => Task.CompletedTask;
            public Task MergePurchaseRecordsAsync(string uid, List<PurchaseRecord> localRecords) => Task.CompletedTask;
        }
    }
}
