using UnityEngine;

namespace Isekai12Realms.Data
{
    [System.Serializable]
    public class TileData
    {
        private static int nextDebugId = 1;

        public TileType type;
        public SpecialTileType specialType;
        public Vector2Int position;
        public bool locked;
        public int freezeTurns;
        public string debugId;

        public TileData(TileType type, Vector2Int position)
        {
            this.type = type;
            this.position = position;
            specialType = SpecialTileType.None;
            locked = false;
            freezeTurns = 0;
            debugId = "tile_" + nextDebugId.ToString("0000");
            nextDebugId++;
        }
    }
}
