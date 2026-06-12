using UnityEngine;

namespace Isekai12Realms.Data
{
    [System.Serializable]
    public class TileData
    {
        public TileType type;
        public SpecialTileType specialType;
        public Vector2Int position;
        public bool locked;
        public int freezeTurns;

        public TileData(TileType type, Vector2Int position)
        {
            this.type = type;
            this.position = position;
            specialType = SpecialTileType.None;
            locked = false;
            freezeTurns = 0;
        }
    }
}
