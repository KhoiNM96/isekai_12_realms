using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.Enemies;
using Isekai12Realms.Realms;
using Isekai12Realms.Stages;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Adventure
{
    public static class AdventurePlatformMapBuilder
    {
        private const float DefaultViewportWidth = 960f;
        private const float DefaultTotalMapWidth = 5760f;
        private const float DefaultTier1Y = -360f;
        private const float DefaultTier2Y = -220f;
        private const float DefaultTier3Y = -80f;
        private const float Tier1Height = 120f;
        private const float UpperTierHeight = 64f;

        public static void EnsurePrototypeLayout(RealmDefinition realm, ContentDatabaseService content)
        {
            EnsurePrototypeLayout(realm, content != null ? content.Database : null);
        }

        public static void EnsurePrototypeLayout(RealmDefinition realm, GameContentDatabase database)
        {
            if (realm == null)
            {
                return;
            }

            if (realm.mapLayout == null)
            {
                realm.mapLayout = new RealmMapLayoutData();
            }

            EnsureLayoutDefaults(realm.mapLayout);
            if (!HasLayout(realm.mapLayout))
            {
                RegeneratePrototypeLayout(realm, database);
            }

            SyncLegacyFields(realm);
        }

        public static void RegeneratePrototypeLayout(RealmDefinition realm, GameContentDatabase database)
        {
            if (realm == null)
            {
                return;
            }

            if (realm.mapLayout == null)
            {
                realm.mapLayout = new RealmMapLayoutData();
            }

            EnsureLayoutDefaults(realm.mapLayout);
            RealmMapLayoutData layout = realm.mapLayout;
            ClearLayout(layout);

            if (realm.id == "realm_01_meadow")
            {
                BuildHandcraftedLayout(layout, 3, 3, false);
            }
            else if (realm.id == "realm_02_ember")
            {
                BuildHandcraftedLayout(layout, 3, 3, true);
            }
            else if (realm.id == "realm_03_tide")
            {
                BuildHandcraftedLayout(layout, 3, 3, true);
            }
            else
            {
                BuildProceduralLayout(layout, realm.order);
            }

            BuildMonsterSpawns(layout, realm, database);
            BuildBossSpawn(layout, realm);
            SyncLegacyFields(realm);
        }

        public static void BuildPlatformVisuals(RectTransform platformRoot, IList<PlatformSegmentData> platforms)
        {
            if (platformRoot == null || platforms == null)
            {
                return;
            }

            ClearChildren(platformRoot);
            for (int i = 0; i < platforms.Count; i++)
            {
                PlatformSegmentData platform = platforms[i];
                if (platform == null)
                {
                    continue;
                }

                GameObject go = new GameObject($"Platform_{i:00}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(platformRoot, false);
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = platform.size;
                rect.anchoredPosition = platform.position;
                Image image = go.GetComponent<Image>();
                image.color = GetPlatformColor(platform);
                image.raycastTarget = false;
            }
        }

        public static List<MonsterSpawnData> BuildMonsterSpawns(RealmDefinition realm, ContentDatabaseService content)
        {
            List<MonsterSpawnData> spawns = new List<MonsterSpawnData>();
            if (realm == null)
            {
                return spawns;
            }

            RealmMapLayoutData layout = realm.mapLayout;
            if (layout != null && layout.monsterSpawns != null && layout.monsterSpawns.Count > 0)
            {
                spawns.AddRange(layout.monsterSpawns);
                return spawns;
            }

            if (realm.normalMonsterSpawns != null && realm.normalMonsterSpawns.Count > 0)
            {
                spawns.AddRange(realm.normalMonsterSpawns);
                return spawns;
            }

            GameContentDatabase database = content != null ? content.Database : null;
            List<EnemyDefinition> enemies = GetRealmEnemies(realm, database);
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyDefinition enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                spawns.Add(new MonsterSpawnData
                {
                    enemyId = enemy.id,
                    spawnPosition = new Vector2(0f, 0f),
                    patrolDistance = 180f,
                    isBoss = false,
                    initiallyHidden = false,
                    tierIndex = 1,
                    platformSegmentIndex = 0
                });
            }

            return spawns;
        }

        private static void BuildHandcraftedLayout(RealmMapLayoutData layout, int tier2Count, int tier3Count, bool moreVertical)
        {
            AddTier1(layout);

            float tier2YOffset = moreVertical ? 4f : 0f;
            float tier3YOffset = moreVertical ? -8f : 0f;
            float tier2Width = layout.totalMapWidth * 0.1f / tier2Count;
            float tier3Width = layout.totalMapWidth * 0.1f / tier3Count;

            List<float> tier2Centers = BuildDistributedCenters(layout.totalMapWidth, tier2Count, 920f, 540f, 1.0f);
            List<float> tier3Centers = BuildTier3Centers(tier2Centers, tier3Count);

            AddUpperTierSegments(layout.tier2Segments, tier2Centers, tier2Width, layout.tier2Y + tier2YOffset, 2, true);
            AddUpperTierSegments(layout.tier3Segments, tier3Centers, tier3Width, layout.tier3Y + tier3YOffset, 3, true);
            layout.playerSpawnPosition = new Vector2(180f, layout.tier1Y + Tier1Height * 0.5f + 48f);
        }

        private static void BuildProceduralLayout(RealmMapLayoutData layout, int realmOrder)
        {
            AddTier1(layout);

            int tier2Count = 3;
            int tier3Count = 3;
            float tier2Width = layout.totalMapWidth * 0.1f / tier2Count;
            float tier3Width = layout.totalMapWidth * 0.1f / tier3Count;
            float offset = Mathf.Clamp((realmOrder % 5) * 64f, 0f, 256f);

            List<float> tier2Centers = BuildDistributedCenters(layout.totalMapWidth, tier2Count, 840f + offset, 560f, 1.0f);
            List<float> tier3Centers = BuildTier3Centers(tier2Centers, tier3Count);

            AddUpperTierSegments(layout.tier2Segments, tier2Centers, tier2Width, layout.tier2Y, 2, true);
            AddUpperTierSegments(layout.tier3Segments, tier3Centers, tier3Width, layout.tier3Y, 3, true);
            layout.playerSpawnPosition = new Vector2(180f, layout.tier1Y + Tier1Height * 0.5f + 48f);
        }

        private static void AddTier1(RealmMapLayoutData layout)
        {
            layout.tier1Segments.Add(new PlatformSegmentData
            {
                position = new Vector2(0f, layout.tier1Y),
                size = new Vector2(layout.totalMapWidth, Tier1Height),
                oneWay = false,
                tierIndex = 1
            });
        }

        private static void AddUpperTierSegments(List<PlatformSegmentData> target, List<float> centers, float width, float y, int tierIndex, bool oneWay)
        {
            for (int i = 0; i < centers.Count; i++)
            {
                target.Add(new PlatformSegmentData
                {
                    position = new Vector2(centers[i] - width * 0.5f, y),
                    size = new Vector2(width, UpperTierHeight),
                    oneWay = oneWay,
                    tierIndex = tierIndex
                });
            }
        }

        private static List<float> BuildDistributedCenters(float totalWidth, int count, float leftMargin, float rightMargin, float stagger)
        {
            List<float> centers = new List<float>();
            if (count <= 0)
            {
                return centers;
            }

            float usable = Mathf.Max(0f, totalWidth - leftMargin - rightMargin);
            float step = usable / Mathf.Max(1, count);
            float start = leftMargin + step * 0.5f;
            for (int i = 0; i < count; i++)
            {
                float offset = (i % 2 == 0 ? -1f : 1f) * stagger * 24f;
                centers.Add(start + i * step + offset);
            }

            return centers;
        }

        private static List<float> BuildTier3Centers(List<float> tier2Centers, int count)
        {
            List<float> centers = new List<float>();
            if (tier2Centers == null || tier2Centers.Count == 0 || count <= 0)
            {
                return centers;
            }

            if (count >= tier2Centers.Count)
            {
                centers.AddRange(tier2Centers);
                return centers;
            }

            for (int i = 0; i < count; i++)
            {
                int index = Mathf.RoundToInt((float)i * Mathf.Max(0, tier2Centers.Count - 1) / Mathf.Max(1, count - 1));
                centers.Add(tier2Centers[Mathf.Clamp(index, 0, tier2Centers.Count - 1)]);
            }

            return centers;
        }

        private static void BuildMonsterSpawns(RealmMapLayoutData layout, RealmDefinition realm, GameContentDatabase database)
        {
            layout.monsterSpawns.Clear();

            List<EnemyDefinition> enemies = GetRealmEnemies(realm, database);
            if (enemies.Count == 0 && realm.bossEnemy != null)
            {
                enemies.Add(realm.bossEnemy);
            }

            List<PlatformSegmentData> tier1 = layout.tier1Segments;
            List<PlatformSegmentData> tier2 = layout.tier2Segments;
            List<PlatformSegmentData> tier3 = layout.tier3Segments;

            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyDefinition enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                int tierIndex = 1;
                if (enemies.Count >= 3)
                {
                    if (i == enemies.Count - 1)
                    {
                        tierIndex = 3;
                    }
                    else if (i % 3 == 1)
                    {
                        tierIndex = 2;
                    }
                }
                List<PlatformSegmentData> source = tierIndex == 1 ? tier1 : tierIndex == 2 ? tier2 : tier3;
                if (source == null || source.Count == 0)
                {
                    source = tier1;
                    tierIndex = 1;
                }

                int segmentIndex = i % source.Count;
                PlatformSegmentData platform = source[segmentIndex];
                Vector2 spawnPosition = GetSpawnPositionOnPlatform(platform, enemy, false, i);

                layout.monsterSpawns.Add(new MonsterSpawnData
                {
                    enemyId = enemy.id,
                    spawnPosition = spawnPosition,
                    patrolDistance = Mathf.Max(120f, platform.size.x * 0.35f),
                    isBoss = false,
                    initiallyHidden = false,
                    tierIndex = tierIndex,
                    platformSegmentIndex = segmentIndex
                });
            }
        }

        private static void BuildBossSpawn(RealmMapLayoutData layout, RealmDefinition realm)
        {
            if (layout.bossSpawn == null)
            {
                layout.bossSpawn = new MonsterSpawnData();
            }

            if (realm.bossEnemy == null)
            {
                layout.bossSpawn.enemyId = string.Empty;
                return;
            }

            List<PlatformSegmentData> bossTier = layout.tier2Segments.Count > 0 ? layout.tier2Segments : layout.tier1Segments;
            int tierIndex = layout.tier2Segments.Count > 0 ? 2 : 1;
            int segmentIndex = Mathf.Max(0, bossTier.Count - 1);
            PlatformSegmentData platform = bossTier[segmentIndex];

            layout.bossSpawn.enemyId = realm.bossEnemy.id;
            layout.bossSpawn.spawnPosition = GetSpawnPositionOnPlatform(platform, realm.bossEnemy, true, segmentIndex);
            layout.bossSpawn.patrolDistance = Mathf.Max(160f, platform.size.x * 0.42f);
            layout.bossSpawn.isBoss = true;
            layout.bossSpawn.initiallyHidden = true;
            layout.bossSpawn.tierIndex = tierIndex;
            layout.bossSpawn.platformSegmentIndex = segmentIndex;
        }

        private static Vector2 GetSpawnPositionOnPlatform(PlatformSegmentData platform, EnemyDefinition enemy, bool boss, int index)
        {
            if (platform == null)
            {
                return new Vector2(160f, DefaultTier1Y + Tier1Height * 0.5f + 48f);
            }

            float halfWidth = platform.size.x * 0.5f;
            float edgeMargin = Mathf.Min(halfWidth - 40f, boss ? 90f : Mathf.Clamp(halfWidth * 0.28f, 48f, 140f));
            float minX = platform.position.x + edgeMargin;
            float maxX = platform.position.x + platform.size.x - edgeMargin;
            float fraction = boss ? 0.88f : 0.12f + (index % 4) * 0.18f;
            float x = Mathf.Clamp(platform.position.x + platform.size.x * fraction, minX, maxX);
            float y = platform.position.y + platform.size.y * 0.5f + 52f;
            return new Vector2(x, y);
        }

        private static List<EnemyDefinition> GetRealmEnemies(RealmDefinition realm, GameContentDatabase database)
        {
            List<EnemyDefinition> enemies = new List<EnemyDefinition>();
            if (realm != null && realm.normalEnemies != null)
            {
                for (int i = 0; i < realm.normalEnemies.Count; i++)
                {
                    if (realm.normalEnemies[i] != null)
                    {
                        enemies.Add(realm.normalEnemies[i]);
                    }
                }
            }

            if (enemies.Count > 0)
            {
                return enemies;
            }

            List<StageDefinition> stages = database != null && realm != null ? database.GetStagesForRealm(realm.id) : new List<StageDefinition>();
            for (int i = 0; i < stages.Count; i++)
            {
                StageDefinition stage = stages[i];
                if (stage != null && stage.enemy != null && !stage.isBossStage)
                {
                    enemies.Add(stage.enemy);
                }
            }

            return enemies;
        }

        private static void EnsureLayoutDefaults(RealmMapLayoutData layout)
        {
            if (layout.viewportWidth <= 0f)
            {
                layout.viewportWidth = DefaultViewportWidth;
            }

            if (layout.totalMapWidth <= 0f)
            {
                layout.totalMapWidth = layout.viewportWidth * 6f;
            }

            if (layout.tier1Y == 0f && layout.tier2Y == 0f && layout.tier3Y == 0f)
            {
                layout.tier1Y = DefaultTier1Y;
                layout.tier2Y = DefaultTier2Y;
                layout.tier3Y = DefaultTier3Y;
            }

            if (layout.playerSpawnPosition == Vector2.zero)
            {
                layout.playerSpawnPosition = new Vector2(180f, layout.tier1Y + Tier1Height * 0.5f + 48f);
            }

            if (layout.tier1Segments == null) layout.tier1Segments = new List<PlatformSegmentData>();
            if (layout.tier2Segments == null) layout.tier2Segments = new List<PlatformSegmentData>();
            if (layout.tier3Segments == null) layout.tier3Segments = new List<PlatformSegmentData>();
            if (layout.monsterSpawns == null) layout.monsterSpawns = new List<MonsterSpawnData>();
        }

        private static bool HasLayout(RealmMapLayoutData layout)
        {
            return layout != null && layout.tier1Segments != null && layout.tier1Segments.Count > 0;
        }

        private static void ClearLayout(RealmMapLayoutData layout)
        {
            layout.tier1Segments.Clear();
            layout.tier2Segments.Clear();
            layout.tier3Segments.Clear();
            layout.monsterSpawns.Clear();
            layout.bossSpawn = new MonsterSpawnData();
        }

        private static void SyncLegacyFields(RealmDefinition realm)
        {
            if (realm == null || realm.mapLayout == null)
            {
                return;
            }

            RealmMapLayoutData layout = realm.mapLayout;
            realm.mapWidth = Mathf.RoundToInt(layout.totalMapWidth);
            realm.mapHeight = Mathf.RoundToInt(Mathf.Max(1400f, layout.viewportWidth * 1.5f));
            realm.playerSpawnPosition = layout.playerSpawnPosition;

            realm.platforms = new List<PlatformSegmentData>();
            realm.platforms.AddRange(layout.tier1Segments);
            realm.platforms.AddRange(layout.tier2Segments);
            realm.platforms.AddRange(layout.tier3Segments);

            realm.normalMonsterSpawns = new List<MonsterSpawnData>();
            realm.normalMonsterSpawns.AddRange(layout.monsterSpawns);
            realm.bossSpawn = layout.bossSpawn;
        }

        private static Color GetPlatformColor(PlatformSegmentData platform)
        {
            if (platform == null)
            {
                return new Color(0.45f, 0.35f, 0.28f, 0.96f);
            }

            if (platform.tierIndex <= 1)
            {
                return new Color(0.46f, 0.35f, 0.25f, 0.98f);
            }

            if (platform.tierIndex == 2)
            {
                return platform.oneWay ? new Color(0.57f, 0.78f, 0.92f, 0.78f) : new Color(0.5f, 0.74f, 0.88f, 0.78f);
            }

            return platform.oneWay ? new Color(0.72f, 0.85f, 0.98f, 0.8f) : new Color(0.68f, 0.8f, 0.95f, 0.8f);
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Object.Destroy(child);
                }
                else
                {
                    Object.DestroyImmediate(child);
                }
            }
        }
    }
}
