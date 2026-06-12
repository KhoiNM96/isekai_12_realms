using System.Collections.Generic;
using Isekai12Realms.Data;

namespace Isekai12Realms.Board
{
    public class BoardResolveResult
    {
        public List<MatchGroup> allMatchGroups = new List<MatchGroup>();
        public int cascadeCount;
        public bool hasAnyMatch;
        public bool grantsExtraTurn;
        public int totalTilesCleared;
        public Dictionary<TileType, int> clearedTileCounts = new Dictionary<TileType, int>();
        public int maxMatchSize;

        public void AddGroups(List<MatchGroup> groups)
        {
            if (groups == null) return;
            foreach (MatchGroup group in groups)
            {
                if (group == null) continue;
                allMatchGroups.Add(group);
                hasAnyMatch = true;
                totalTilesCleared += group.count;
                if (group.count > maxMatchSize) maxMatchSize = group.count;
                if (group.grantsExtraTurn) grantsExtraTurn = true;
                if (!clearedTileCounts.ContainsKey(group.tileType))
                {
                    clearedTileCounts[group.tileType] = 0;
                }
                clearedTileCounts[group.tileType] += group.count;
            }
        }
    }
}
