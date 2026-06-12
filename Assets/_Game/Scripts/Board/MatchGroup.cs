using System.Collections.Generic;
using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.Board
{
    public class MatchGroup
    {
        public TileType tileType;
        public List<Vector2Int> positions = new List<Vector2Int>();
        public bool createsSpecial;
        public SpecialTileType specialCreated;
        public int count;
    }
}
