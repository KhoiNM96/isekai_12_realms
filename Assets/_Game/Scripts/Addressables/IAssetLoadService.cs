using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Isekai12Realms.Addressables
{
    public interface IAssetLoadService
    {
        bool IsAvailable { get; }
        Task<Sprite> LoadSpriteAsync(string assetId);
        Task<T> LoadAssetAsync<T>(string assetId) where T : UnityEngine.Object;
        void ReleaseAsset(string assetId);
        bool HasCachedAsset(string assetId);
        Task PreloadAssetsAsync(List<string> assetIds);
        Task ClearOptionalCacheAsync();
    }
}
