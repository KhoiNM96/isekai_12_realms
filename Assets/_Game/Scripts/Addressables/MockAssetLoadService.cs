using System.Collections.Generic;
using System.Threading.Tasks;
using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.Addressables
{
    public class MockAssetLoadService : IAssetLoadService
    {
        private static bool logged;
        private readonly GameAssetManifest manifest;

        public bool IsAvailable => false;

        public MockAssetLoadService(GameAssetManifest manifest)
        {
            this.manifest = manifest;
            this.manifest?.RebuildLookup();
            if (!logged)
            {
                Debug.Log("[Assets] Addressables disabled. Using AssetManifest local loading.");
                logged = true;
            }
        }

        public Task<Sprite> LoadSpriteAsync(string assetId)
        {
            return Task.FromResult(GetManifestSprite(assetId));
        }

        public Task<T> LoadAssetAsync<T>(string assetId) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(Sprite))
            {
                return Task.FromResult(GetManifestSprite(assetId) as T);
            }

            return Task.FromResult<T>(null);
        }

        public void ReleaseAsset(string assetId) { }
        public bool HasCachedAsset(string assetId) => manifest != null && manifest.HasAsset(assetId);
        public Task PreloadAssetsAsync(List<string> assetIds) => Task.CompletedTask;
        public Task ClearOptionalCacheAsync() => Task.CompletedTask;

        private Sprite GetManifestSprite(string assetId)
        {
            if (manifest == null) return null;
            Sprite sprite = manifest.GetSprite(assetId);
            return sprite != null ? sprite : manifest.missingSprite;
        }
    }
}
