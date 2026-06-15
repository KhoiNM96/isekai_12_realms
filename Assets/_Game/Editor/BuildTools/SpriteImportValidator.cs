using System.Collections.Generic;
using Isekai12Realms.Data;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class SpriteImportValidator
    {
        private const string GeneratedRoot = "Assets/_Game/Art/Generated";

        [MenuItem("Tools/Isekai 12 Realms/Assets/Validate Sprite Import Settings")]
        public static void ValidateSpriteImportSettings()
        {
            List<string> issues = FindIssues(false);
            if (issues.Count == 0) Debug.Log("[Assets] Sprite import validation passed.");
            else Debug.LogWarning("[Assets] Sprite import validation issues:\n" + string.Join("\n", issues));
        }

        [MenuItem("Tools/Isekai 12 Realms/Assets/Fix Generated Sprite Import Settings")]
        public static void FixGeneratedSpriteImportSettings()
        {
            List<string> issues = FindIssues(true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Assets] Sprite import settings fixed. Issues found before fix: " + issues.Count);
        }

        private static List<string> FindIssues(bool fix)
        {
            List<string> issues = new List<string>();
            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(BuildValidator.ManifestPath);
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { GeneratedRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;
                bool isUiOrIcon = path.Contains("/UI/") || path.Contains("/Icons/") || path.Contains("/Items/") || path.Contains("/Tokens/");
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite) { issues.Add(path + " texture type is not Sprite."); if (fix) { importer.textureType = TextureImporterType.Sprite; changed = true; } }
                if (isUiOrIcon && importer.mipmapEnabled) { issues.Add(path + " has mipmaps enabled."); if (fix) { importer.mipmapEnabled = false; changed = true; } }
                if (importer.maxTextureSize > 2048) { issues.Add(path + " max size is high: " + importer.maxTextureSize); if (fix) { importer.maxTextureSize = 2048; changed = true; } }
                if (isUiOrIcon && importer.textureCompression == TextureImporterCompression.Compressed) { issues.Add(path + " UI/icon compression may be too aggressive."); if (fix) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; } }
                string id = System.IO.Path.GetFileNameWithoutExtension(path);
                int sizeIndex = id.LastIndexOf('_');
                if (sizeIndex > 0) id = id.Substring(0, sizeIndex);
                GameAssetEntry entry = manifest != null ? manifest.GetEntry(id) : null;
                if (entry != null && entry.transparent && !importer.alphaIsTransparency) { issues.Add(path + " alpha transparency disabled."); if (fix) { importer.alphaIsTransparency = true; changed = true; } }
                if (changed) importer.SaveAndReimport();
            }
            return issues;
        }
    }
}
