#if USE_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Isekai12Realms.Addressables
{
    public class AddressableAssetLoadService : IAssetLoadService
    {
        private readonly GameAssetManifest manifest;
        private readonly Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();
        private readonly Dictionary<string, AsyncOperationHandle> handles = new Dictionary<string, AsyncOperationHandle>();

        public bool IsAvailable => true;

        public AddressableAssetLoadService(GameAssetManifest manifest)
        {
            this.manifest = manifest;
            this.manifest?.RebuildLookup();
        }

        public async Task<Sprite> LoadSpriteAsync(string assetId)
        {
            Sprite sprite = await LoadAssetAsync<Sprite>(assetId);
            if (sprite != null) return sprite;
            return GetManifestSprite(assetId);
        }

        public async Task<T> LoadAssetAsync<T>(string assetId) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetId)) return GetFallback<T>(assetId);
            if (cache.TryGetValue(assetId, out UnityEngine.Object cached) && cached is T typedCached)
            {
                return typedCached;
            }

            try
            {
                AsyncOperationHandle<T> handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(assetId);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    cache[assetId] = handle.Result;
                    handles[assetId] = handle;
                    return handle.Result;
                }

                if (handle.IsValid()) UnityEngine.AddressableAssets.Addressables.Release(handle);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Assets] Addressable load failed for '{assetId}'. Using local fallback. {ex.Message}");
            }

            return GetFallback<T>(assetId);
        }

        public void ReleaseAsset(string assetId)
        {
            if (string.IsNullOrEmpty(assetId)) return;
            if (handles.TryGetValue(assetId, out AsyncOperationHandle handle) && handle.IsValid())
            {
                UnityEngine.AddressableAssets.Addressables.Release(handle);
            }
            handles.Remove(assetId);
            cache.Remove(assetId);
        }

        public bool HasCachedAsset(string assetId) => !string.IsNullOrEmpty(assetId) && (cache.ContainsKey(assetId) || (manifest != null && manifest.HasAsset(assetId)));

        public async Task PreloadAssetsAsync(List<string> assetIds)
        {
            if (assetIds == null) return;
            foreach (string assetId in assetIds)
            {
                await LoadAssetAsync<UnityEngine.Object>(assetId);
            }
        }

        public Task ClearOptionalCacheAsync()
        {
            List<string> keys = new List<string>(handles.Keys);
            foreach (string key in keys) ReleaseAsset(key);
            return Task.CompletedTask;
        }

        private T GetFallback<T>(string assetId) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(Sprite)) return GetManifestSprite(assetId) as T;
            return null;
        }

        private Sprite GetManifestSprite(string assetId)
        {
            if (manifest == null) return null;
            Sprite sprite = manifest.GetSprite(assetId);
            return sprite != null ? sprite : manifest.missingSprite;
        }
    }
}
#endif
