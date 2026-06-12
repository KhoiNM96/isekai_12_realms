using System;
using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Data
{
    public enum GameAssetCategory
    {
        Background,
        Character,
        Enemy,
        NPC,
        Token,
        Skill,
        Equipment,
        Item,
        UI,
        Tileset,
        VFX,
        Map,
        Loading,
        Currency,
        Misc
    }

    [CreateAssetMenu(fileName = "GameAssetManifest", menuName = "Isekai 12 Realms/Asset Manifest")]
    public class GameAssetManifest : ScriptableObject
    {
        public List<GameAssetEntry> entries = new List<GameAssetEntry>();
        public Sprite missingSprite;

        private Dictionary<string, GameAssetEntry> lookup;

        public Sprite GetSprite(string id)
        {
            GameAssetEntry entry = GetEntry(id);
            if (entry != null && entry.sprite != null)
            {
                return entry.sprite;
            }

            return missingSprite;
        }

        public bool HasAsset(string id)
        {
            GameAssetEntry entry = GetEntry(id);
            return entry != null && entry.sprite != null;
        }

        public GameAssetEntry GetEntry(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (lookup == null || lookup.Count != entries.Count)
            {
                RebuildLookup();
            }

            return lookup.TryGetValue(id, out GameAssetEntry entry) ? entry : null;
        }

        public void RebuildLookup()
        {
            lookup = new Dictionary<string, GameAssetEntry>();
            foreach (GameAssetEntry entry in entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.id) || lookup.ContainsKey(entry.id)) continue;
                lookup.Add(entry.id, entry);
            }
        }
    }
}
