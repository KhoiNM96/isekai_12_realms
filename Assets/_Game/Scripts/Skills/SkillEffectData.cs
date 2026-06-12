using System;
using Isekai12Realms.Data;

namespace Isekai12Realms.Skills
{
    [Serializable]
    public class SkillEffectData
    {
        public SkillEffectType effectType;
        public int baseValue;
        public float multiplier = 1f;
        public int tileCount;
        public int areaSize;
        public TileType fromTileType;
        public TileType toTileType;
        public bool scalesWithLevel;
    }
}
