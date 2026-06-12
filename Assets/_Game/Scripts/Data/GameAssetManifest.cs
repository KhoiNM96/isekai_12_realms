using System;
using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Data
{
    public enum GameAssetCategory
    {
        Backgrounds,
        Characters,
        NPCs,
        Enemies,
        Tokens,
        Skills,
        Equipment,
        Items,
        UI,
        Tilesets,
        VFX,
        Maps,
        Icons,
        Loading,
        Meta
    }

    [Serializable]
    public class GameAssetEntry
    {
        public string id;
        public string fileName;
        public string relativePath;
        public Vector2Int size;
        public Sprite sprite;
        public GameAssetCategory category;
        public bool transparent;
        public int priority = 1;
    }

    [CreateAssetMenu(fileName = "GameAssetManifest", menuName = "Isekai 12 Realms/Asset Manifest")]
    public class GameAssetManifest : ScriptableObject
    {
        public List<GameAssetEntry> entries = new List<GameAssetEntry>();

        public Sprite GetSprite(string assetId)
        {
            var entry = entries.Find(e => e.id == assetId);
            if (entry != null && entry.sprite != null)
            {
                return entry.sprite;
            }
            
            // Fallback
            var fallbackEntry = entries.Find(e => e.id == "missing_sprite");
            if (fallbackEntry != null)
            {
                return fallbackEntry.sprite;
            }

            return null;
        }
    }
}
