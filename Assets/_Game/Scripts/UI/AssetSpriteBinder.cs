using Isekai12Realms.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.UI
{
    [RequireComponent(typeof(Image))]
    public class AssetSpriteBinder : MonoBehaviour
    {
        private const string ManifestPath = "Assets/_Game/ScriptableObjects/AssetManifest/GameAssetManifest.asset";

        public string assetId;
        public Image targetImage;

        public static GameAssetManifest SharedManifest { get; private set; }

        public static void SetManifest(GameAssetManifest manifest)
        {
            SharedManifest = manifest;
            SharedManifest?.RebuildLookup();
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
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }

            if (targetImage == null) return;
            Sprite sprite = GetSprite(assetId);
            if (sprite == null) return;
            targetImage.sprite = sprite;
            targetImage.preserveAspect = ShouldPreserveAspect(assetId);
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
