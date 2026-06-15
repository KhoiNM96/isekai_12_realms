using Isekai12Realms.Data;
using Isekai12Realms.Addressables;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.UI
{
    [RequireComponent(typeof(Image))]
    public class AssetSpriteBinder : MonoBehaviour
    {
        private const string ManifestPath = "Assets/_Game/ScriptableObjects/GameAssetManifest.asset";

        public string assetId;
        public Image targetImage;
        public bool preserveAspect = true;
        public bool useNativeSize;

        public static GameAssetManifest SharedManifest { get; private set; }
        public static IAssetLoadService AssetLoadService { get; private set; }
        private int loadVersion;

        public static void SetManifest(GameAssetManifest manifest)
        {
            SharedManifest = manifest;
            SharedManifest?.RebuildLookup();
        }

        public static void SetAssetLoadService(IAssetLoadService service)
        {
            AssetLoadService = service;
        }

        public static Sprite GetSprite(string id)
        {
            GameAssetManifest manifest = GetManifest();
            return manifest != null ? manifest.GetSprite(id) : null;
        }

        public static bool HasAsset(string id)
        {
            GameAssetManifest manifest = GetManifest();
            return manifest != null && manifest.HasAsset(id);
        }

        public void Awake()
        {
            Apply();
        }

        public void Start()
        {
            Apply();
        }

        public void Apply()
        {
            _ = ApplyAsync();
        }

        public async System.Threading.Tasks.Task ApplyAsync()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage == null) return;
            int version = ++loadVersion;
            Sprite fallback = GetSprite(assetId);
            if (fallback != null)
            {
                ApplySprite(fallback);
            }

            Sprite sprite = fallback;
            if (AssetLoadService != null)
            {
                sprite = await AssetLoadService.LoadSpriteAsync(assetId);
            }

            if (version != loadVersion || this == null || !isActiveAndEnabled || targetImage == null || sprite == null) return;
            ApplySprite(sprite);
        }

        private void ApplySprite(Sprite sprite)
        {
            targetImage.sprite = sprite;
            targetImage.preserveAspect = preserveAspect || ShouldPreserveAspect(assetId);
            if (useNativeSize) targetImage.SetNativeSize();
        }

        private void OnDisable()
        {
            loadVersion++;
        }

        private static bool ShouldPreserveAspect(string id)
        {
            return !string.IsNullOrEmpty(id) &&
                   (id.StartsWith("char_") ||
                    id.StartsWith("enemy_") ||
                    id.StartsWith("boss_") ||
                    id.StartsWith("icon_") ||
                    id.StartsWith("currency_") ||
                    id.StartsWith("logo_"));
        }

        private static GameAssetManifest GetManifest()
        {
            if (SharedManifest != null) return SharedManifest;

#if UNITY_EDITOR
            SharedManifest = UnityEditor.AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath);
            SharedManifest?.RebuildLookup();
#endif
            return SharedManifest;
        }
    }
}
