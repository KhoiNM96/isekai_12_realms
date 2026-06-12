using System;
using UnityEngine;

namespace Isekai12Realms.Data
{
    [Serializable]
    public class GameAssetEntry
    {
        public string id;
        public string fileName;
        public string relativePath;
        public int width;
        public int height;
        public GameAssetCategory category;
        public bool transparent;
        public int priority = 1;
        public Sprite sprite;
    }
}
