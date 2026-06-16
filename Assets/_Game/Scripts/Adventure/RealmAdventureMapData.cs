using System;
using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.Adventure
{
    [Serializable]
    public class PlatformSegmentData
    {
        public Vector2 position;
        public Vector2 size = new Vector2(640f, 80f);
        public bool oneWay;
        public int tierIndex = 1;
    }

    [Serializable]
    public class MonsterSpawnData
    {
        public string enemyId = string.Empty;
        public Vector2 spawnPosition;
        public float patrolDistance = 220f;
        public bool isBoss;
        public bool initiallyHidden;
        public int tierIndex = 1;
        public int platformSegmentIndex = -1;
    }

    [Serializable]
    public class RealmMapLayoutData
    {
        public float viewportWidth = 960f;
        public float totalMapWidth = 5760f;
        public float tier1Y = -360f;
        public float tier2Y = -220f;
        public float tier3Y = -80f;
        public List<PlatformSegmentData> tier1Segments = new List<PlatformSegmentData>();
        public List<PlatformSegmentData> tier2Segments = new List<PlatformSegmentData>();
        public List<PlatformSegmentData> tier3Segments = new List<PlatformSegmentData>();
        public Vector2 playerSpawnPosition = new Vector2(160f, -300f);
        public List<MonsterSpawnData> monsterSpawns = new List<MonsterSpawnData>();
        public MonsterSpawnData bossSpawn = new MonsterSpawnData();
    }

    [Serializable]
    public class AdventureMapRuntimeState
    {
        public string currentRealmId = string.Empty;
        public Vector2 playerPosition;
        public readonly List<string> defeatedEncounterIds = new List<string>();
        public readonly List<string> defeatedMonsterIds = new List<string>();
        public bool bossVisible;
        public bool bossDefeated;
        public bool encounterInProgress;
    }
}
