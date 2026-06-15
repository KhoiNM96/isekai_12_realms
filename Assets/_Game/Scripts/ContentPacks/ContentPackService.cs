using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Addressables;
using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.ContentPacks
{
    public class ContentPackService
    {
        private readonly GameContentDatabase database;
        private readonly IAssetLoadService assetLoadService;
        private readonly Dictionary<string, ContentPackDownloadStatus> statuses = new Dictionary<string, ContentPackDownloadStatus>();

        public ContentPackService(GameContentDatabase database, IAssetLoadService assetLoadService)
        {
            this.database = database;
            this.assetLoadService = assetLoadService;
        }

        public List<ContentPackDefinition> GetAllPacks() => database != null && database.contentPacks != null ? database.contentPacks.FindAll(p => p != null) : new List<ContentPackDefinition>();
        public List<ContentPackDefinition> GetRequiredPacks() => GetAllPacks().FindAll(p => p.required);
        public List<ContentPackDefinition> GetDownloadablePacks() => GetAllPacks().FindAll(p => p.downloadable);

        public bool IsPackAvailable(string packId)
        {
            ContentPackDefinition pack = database != null ? database.GetContentPackById(packId) : null;
            if (pack == null) return true;
            if (pack.includedInBuild || pack.packType == ContentPackType.Core) return true;
            return GetPackDownloadStatus(packId) == ContentPackDownloadStatus.Downloaded;
        }

        public async Task<bool> DownloadPackAsync(string packId)
        {
            ContentPackDefinition pack = database != null ? database.GetContentPackById(packId) : null;
            if (pack == null) return true;
            if (pack.includedInBuild || pack.packType == ContentPackType.Core)
            {
                statuses[packId] = ContentPackDownloadStatus.AvailableLocal;
                return true;
            }
            if (assetLoadService == null || !assetLoadService.IsAvailable)
            {
                statuses[packId] = ContentPackDownloadStatus.AddressablesDisabled;
                return false;
            }

            statuses[packId] = ContentPackDownloadStatus.Downloading;
            try
            {
                await assetLoadService.PreloadAssetsAsync(pack.assetIds);
                statuses[packId] = ContentPackDownloadStatus.Downloaded;
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[ContentPacks] Download failed for '{packId}': {ex.Message}");
                statuses[packId] = ContentPackDownloadStatus.Failed;
                return false;
            }
        }

        public Task PreloadPackAsync(string packId)
        {
            ContentPackDefinition pack = database != null ? database.GetContentPackById(packId) : null;
            return pack != null && assetLoadService != null ? assetLoadService.PreloadAssetsAsync(pack.assetIds) : Task.CompletedTask;
        }

        public async Task ClearOptionalPacksAsync()
        {
            if (assetLoadService != null) await assetLoadService.ClearOptionalCacheAsync();
            statuses.Clear();
        }

        public ContentPackDownloadStatus GetPackDownloadStatus(string packId)
        {
            ContentPackDefinition pack = database != null ? database.GetContentPackById(packId) : null;
            if (pack == null) return ContentPackDownloadStatus.Unknown;
            if (pack.includedInBuild || pack.packType == ContentPackType.Core) return ContentPackDownloadStatus.AvailableLocal;
            if (statuses.TryGetValue(packId, out ContentPackDownloadStatus status)) return status;
            return assetLoadService != null && assetLoadService.IsAvailable ? ContentPackDownloadStatus.NotDownloaded : ContentPackDownloadStatus.AddressablesDisabled;
        }
    }
}
