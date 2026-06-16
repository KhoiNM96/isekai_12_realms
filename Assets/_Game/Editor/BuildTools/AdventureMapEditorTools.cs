using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isekai12Realms.Adventure;
using Isekai12Realms.Data;
using Isekai12Realms.Realms;
using Isekai12Realms.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Isekai12Realms.Editor.BuildTools
{
    public static class AdventureMapEditorTools
    {
        private const string GameScenePath = "Assets/_Game/Scenes/GameScene.unity";

        [MenuItem("Tools/Isekai 12 Realms/Adventure/Rebuild Realm Adventure Map UI")]
        public static void RebuildRealmAdventureMapUi()
        {
            if (!OpenGameScene())
            {
                return;
            }

            GameObject gameManagerObject = GameObject.Find("GameManager") ?? new GameObject("GameManager");
            GameSceneBootstrapper bootstrapper = gameManagerObject.GetComponent<GameSceneBootstrapper>() ?? gameManagerObject.AddComponent<GameSceneBootstrapper>();
            bootstrapper.RepairSceneUi();

            EditorUtility.SetDirty(gameManagerObject);
            EditorSceneManager.MarkSceneDirty(gameManagerObject.scene);
            EditorSceneManager.SaveScene(gameManagerObject.scene, GameScenePath);

            Debug.Log("[Adventure] Realm Adventure Map UI rebuilt.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Adventure/Create Prototype Realm Layouts")]
        public static void CreatePrototypeRealmLayouts()
        {
            GameContentDatabase database = LoadDatabase();
            if (database == null)
            {
                Debug.LogError("[Adventure] GameContentDatabase.asset missing.");
                return;
            }

            List<RealmDefinition> realms = LoadRealms();
            if (realms.Count == 0)
            {
                Debug.LogError("[Adventure] No realm assets found.");
                return;
            }

            foreach (RealmDefinition realm in realms)
            {
                if (realm == null)
                {
                    continue;
                }

                AdventurePlatformMapBuilder.RegeneratePrototypeLayout(realm, database);
                EditorUtility.SetDirty(realm);
            }

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Adventure] Prototype realm layouts created/updated.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Adventure/Test Build Current Realm Map")]
        public static void TestBuildCurrentRealmMap()
        {
            if (!OpenGameScene())
            {
                return;
            }

            GameObject gameManagerObject = GameObject.Find("GameManager");
            GameSceneBootstrapper bootstrapper = gameManagerObject != null ? gameManagerObject.GetComponent<GameSceneBootstrapper>() : null;
            RealmAdventureMapUIController adventureController = FindTransform("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI")?.GetComponent<RealmAdventureMapUIController>();

            if (bootstrapper == null || adventureController == null)
            {
                Debug.LogWarning("[Adventure] Test Build Current Realm Map preview not available in edit mode.");
                return;
            }

            adventureController.BuildOrRepair();
            Debug.Log("[Adventure] Test Build Current Realm Map completed.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Adventure/Validate Realm Layout Rules")]
        public static void ValidateRealmLayoutRules()
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            ValidateLayoutRules(errors, warnings);

            foreach (string warning in warnings)
            {
                Debug.LogWarning("[Adventure] " + warning);
            }

            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError("[Adventure] " + error);
                }

                Debug.LogError("[Adventure] Validate Realm Layout Rules failed.");
            }
            else
            {
                Debug.Log("[Adventure] Validate Realm Layout Rules passed.");
            }
        }

        [MenuItem("Tools/Isekai 12 Realms/Adventure/Validate Visible Realm Map")]
        public static void ValidateVisibleRealmMap()
        {
            if (!OpenGameScene())
            {
                return;
            }

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            ValidateSceneScreens(errors, warnings);
            ValidateLayoutData(errors, warnings);
            ValidateBottomNavigation(errors, warnings);

            foreach (string warning in warnings)
            {
                Debug.LogWarning("[Adventure] " + warning);
            }

            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError("[Adventure] " + error);
                }

                Debug.LogError("[Adventure] Validate Visible Realm Map failed.");
            }
            else
            {
                Debug.Log("[Adventure] Validate Visible Realm Map passed.");
            }
        }

        [MenuItem("Tools/Isekai 12 Realms/Adventure/Validate Realm Adventure Flow")]
        public static void ValidateRealmAdventureFlow()
        {
            if (!OpenGameScene())
            {
                return;
            }

            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            ValidateSceneScreens(errors, warnings);
            ValidateLayoutData(errors, warnings);
            ValidateScripts(errors, warnings);

            foreach (string warning in warnings)
            {
                Debug.LogWarning("[Adventure] " + warning);
            }

            if (errors.Count > 0)
            {
                foreach (string error in errors)
                {
                    Debug.LogError("[Adventure] " + error);
                }

                Debug.LogError("[Adventure] Validate Realm Adventure Flow failed.");
            }
            else
            {
                Debug.Log("[Adventure] Validate Realm Adventure Flow passed.");
            }
        }

        private static void ValidateSceneScreens(List<string> errors, List<string> warnings)
        {
            Transform realmMap = FindTransform("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI");
            if (realmMap == null)
            {
                errors.Add("RealmAdventureMapUI is missing.");
                return;
            }

            Transform worldMap = FindTransform("RootCanvas/SafeAreaRoot/MainLayer/WorldMapUI");
            if (worldMap == null)
            {
                errors.Add("WorldMapUI is missing.");
            }

            if (ContainsDescendantNamed(worldMap, "StageList") || ContainsDescendantNamed(worldMap, "StageDetail") || ContainsDescendantNamed(worldMap, "EnterBattle"))
            {
                errors.Add("WorldMapUI still contains legacy stage UI.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI/MapViewport/MapContent/PlayerRoot/Player")?.GetComponent<AdventurePlayerController>() == null)
            {
                errors.Add("RealmAdventureMapUI missing AdventurePlayerController.");
            }

            if (FindTransform("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI/MapViewport/MapContent/MonsterRoot") == null)
            {
                errors.Add("RealmAdventureMapUI missing MonsterRoot.");
            }

            GameObject title = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/TitleScreenUI");
            GameObject mainTown = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/MainTownUI");
            GameObject realmAdventure = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI");
            GameObject battle = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/BattleUI");
            if (title != null && !title.activeSelf) warnings.Add("TitleScreenUI should be active by default.");
            if (mainTown != null && mainTown.activeSelf) warnings.Add("MainTownUI should be inactive by default.");
            if (realmAdventure != null && realmAdventure.activeSelf) warnings.Add("RealmAdventureMapUI should be inactive by default.");
            if (battle != null && battle.activeSelf) warnings.Add("BattleUI should be inactive by default.");
        }

        private static void ValidateLayoutData(List<string> errors, List<string> warnings)
        {
            List<RealmDefinition> realms = LoadRealms();
            if (realms.Count < 12)
            {
                warnings.Add("Less than 12 realm assets were found.");
            }

            for (int i = 0; i < realms.Count; i++)
            {
                RealmDefinition realm = realms[i];
                if (realm == null)
                {
                    continue;
                }

                if (realm.platforms == null || realm.platforms.Count == 0)
                {
                    errors.Add($"Realm missing platform layout: {realm.id}");
                }

                if (i < 3)
                {
                    if (realm.normalMonsterSpawns == null || realm.normalMonsterSpawns.Count == 0)
                    {
                        errors.Add($"Realm 01-03 must have monster spawns: {realm.id}");
                    }
                }
            }
        }

        private static void ValidateLayoutRules(List<string> errors, List<string> warnings)
        {
            List<RealmDefinition> realms = LoadRealms();
            if (realms.Count == 0)
            {
                errors.Add("No realm assets found.");
                return;
            }

            for (int i = 0; i < realms.Count; i++)
            {
                RealmDefinition realm = realms[i];
                if (realm == null)
                {
                    continue;
                }

                RealmMapLayoutData layout = realm.mapLayout;
                if (layout == null)
                {
                    errors.Add($"Realm missing map layout data: {realm.id}");
                    continue;
                }

                float viewportWidth = layout.viewportWidth > 0f ? layout.viewportWidth : 960f;
                float totalWidth = layout.totalMapWidth > 0f ? layout.totalMapWidth : 0f;
                if (layout.tier1Segments == null || layout.tier1Segments.Count != 1)
                {
                    errors.Add($"Tier 1 must contain exactly one continuous segment: {realm.id}");
                }

                if (layout.tier2Segments == null || layout.tier2Segments.Count == 0)
                {
                    errors.Add($"Tier 2 is missing: {realm.id}");
                }

                if (layout.tier3Segments == null || layout.tier3Segments.Count == 0)
                {
                    errors.Add($"Tier 3 is missing: {realm.id}");
                }

                if (Mathf.Abs(totalWidth - viewportWidth * 6f) > viewportWidth * 0.15f)
                {
                    errors.Add($"Tier 1 width must be about 6 viewport widths: {realm.id}");
                }

                if (!HasSingleTier(layout.tier1Segments, 1, totalWidth, true))
                {
                    errors.Add($"Tier 1 is not continuous or has the wrong tier index: {realm.id}");
                }

                float tier2Width = GetCombinedWidth(layout.tier2Segments);
                float tier3Width = GetCombinedWidth(layout.tier3Segments);
                if (layout.tier2Segments != null && layout.tier2Segments.Count > 0 && Mathf.Abs(tier2Width - totalWidth * 0.1f) > totalWidth * 0.05f)
                {
                    errors.Add($"Tier 2 combined width must be about 10% of Tier 1: {realm.id}");
                }

                if (layout.tier3Segments != null && layout.tier3Segments.Count > 0 && Mathf.Abs(tier3Width - totalWidth * 0.1f) > totalWidth * 0.05f)
                {
                    errors.Add($"Tier 3 combined width must be about 10% of Tier 1: {realm.id}");
                }

                if (GetTierCount(layout) > 3)
                {
                    errors.Add($"Realm exceeds the maximum of 3 tiers: {realm.id}");
                }

                ValidateSegmentTierIds(realm, layout, errors);

                if (!HasReachableTierPair(layout.tier1Segments, layout.tier2Segments))
                {
                    errors.Add($"Tier 2 is not reachable from Tier 1: {realm.id}");
                }

                if (layout.tier3Segments != null && layout.tier3Segments.Count > 0 && !HasReachableTierPair(layout.tier2Segments, layout.tier3Segments))
                {
                    errors.Add($"Tier 3 is not reachable from Tier 2: {realm.id}");
                }

                if (layout.playerSpawnPosition == Vector2.zero)
                {
                    errors.Add($"Player spawn is missing: {realm.id}");
                }
                else if (!IsOnTier(layout.playerSpawnPosition, layout.tier1Segments, 1))
                {
                    errors.Add($"Player spawn must be on Tier 1: {realm.id}");
                }

                if (layout.monsterSpawns == null || layout.monsterSpawns.Count == 0)
                {
                    errors.Add($"At least one monster spawn is required: {realm.id}");
                }

                if (realm.bossEnemy != null && layout.bossSpawn != null)
                {
                    if (string.IsNullOrEmpty(layout.bossSpawn.enemyId))
                    {
                        errors.Add($"Boss spawn must be defined when a boss exists: {realm.id}");
                    }
                    else
                    {
                        ValidateSpawnAgainstTier(realm, layout.bossSpawn, layout, errors, true);
                    }
                }
                else
                {
                    warnings.Add($"Boss spawn is optional for realm without a boss: {realm.id}");
                }

                ValidateMonsterBounds(realm, layout, errors);
            }
        }

        private static bool HasSingleTier(List<PlatformSegmentData> segments, int tierIndex, float totalWidth, bool allowOneSegment)
        {
            if (segments == null || segments.Count == 0)
            {
                return false;
            }

            if (allowOneSegment && segments.Count != 1)
            {
                return false;
            }

            PlatformSegmentData segment = segments[0];
            return segment != null && segment.tierIndex == tierIndex && Mathf.Abs(segment.size.x - totalWidth) <= totalWidth * 0.05f;
        }

        private static float GetCombinedWidth(List<PlatformSegmentData> segments)
        {
            float total = 0f;
            if (segments == null)
            {
                return total;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                if (segments[i] != null)
                {
                    total += segments[i].size.x;
                }
            }

            return total;
        }

        private static int GetTierCount(RealmMapLayoutData layout)
        {
            int count = 0;
            if (layout.tier1Segments != null && layout.tier1Segments.Count > 0) count++;
            if (layout.tier2Segments != null && layout.tier2Segments.Count > 0) count++;
            if (layout.tier3Segments != null && layout.tier3Segments.Count > 0) count++;
            return count;
        }

        private static bool HasReachableTierPair(List<PlatformSegmentData> lower, List<PlatformSegmentData> upper)
        {
            if (lower == null || upper == null || lower.Count == 0 || upper.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < lower.Count; i++)
            {
                PlatformSegmentData lowerSegment = lower[i];
                if (lowerSegment == null)
                {
                    continue;
                }

                float lowerTop = lowerSegment.position.y + lowerSegment.size.y * 0.5f;
                for (int j = 0; j < upper.Count; j++)
                {
                    PlatformSegmentData upperSegment = upper[j];
                    if (upperSegment == null)
                    {
                        continue;
                    }

                    float upperBottom = upperSegment.position.y - upperSegment.size.y * 0.5f;
                    bool horizontalOverlap = Mathf.Abs(lowerSegment.position.x - upperSegment.position.x) <= (lowerSegment.size.x + upperSegment.size.x) * 0.5f;
                    bool jumpable = upperBottom - lowerTop <= 190f && upperBottom > lowerTop;
                    if (horizontalOverlap && jumpable)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void ValidateSegmentTierIds(RealmDefinition realm, RealmMapLayoutData layout, List<string> errors)
        {
            ValidateTierIdList(realm, layout.tier1Segments, 1, errors);
            ValidateTierIdList(realm, layout.tier2Segments, 2, errors);
            ValidateTierIdList(realm, layout.tier3Segments, 3, errors);
        }

        private static void ValidateTierIdList(RealmDefinition realm, List<PlatformSegmentData> segments, int tierIndex, List<string> errors)
        {
            if (segments == null)
            {
                return;
            }

            for (int i = 0; i < segments.Count; i++)
            {
                PlatformSegmentData segment = segments[i];
                if (segment != null && segment.tierIndex != tierIndex)
                {
                    errors.Add($"Tier index mismatch in {realm.id}: expected tier {tierIndex}");
                    return;
                }
            }
        }

        private static bool IsOnTier(Vector2 position, List<PlatformSegmentData> tierSegments, int tierIndex)
        {
            if (tierSegments == null)
            {
                return false;
            }

            for (int i = 0; i < tierSegments.Count; i++)
            {
                PlatformSegmentData segment = tierSegments[i];
                if (segment == null || segment.tierIndex != tierIndex)
                {
                    continue;
                }

                float left = segment.position.x - segment.size.x * 0.5f;
                float right = segment.position.x + segment.size.x * 0.5f;
                float top = segment.position.y + segment.size.y * 0.5f;
                if (position.x >= left && position.x <= right && Mathf.Abs(position.y - (top + 48f)) <= 96f)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidateMonsterBounds(RealmDefinition realm, RealmMapLayoutData layout, List<string> errors)
        {
            if (layout.monsterSpawns == null)
            {
                return;
            }

            for (int i = 0; i < layout.monsterSpawns.Count; i++)
            {
                MonsterSpawnData spawn = layout.monsterSpawns[i];
                if (spawn == null)
                {
                    continue;
                }

                if (spawn.tierIndex < 1 || spawn.tierIndex > 3)
                {
                    errors.Add($"Monster spawn tier index is invalid: {realm.id}");
                    continue;
                }
                ValidateSpawnAgainstTier(realm, spawn, layout, errors, false);
            }
        }

        private static void ValidateSpawnAgainstTier(RealmDefinition realm, MonsterSpawnData spawn, RealmMapLayoutData layout, List<string> errors, bool isBoss)
        {
            List<PlatformSegmentData> tierPlatforms = spawn.tierIndex == 2 ? layout.tier2Segments : spawn.tierIndex == 3 ? layout.tier3Segments : layout.tier1Segments;
            PlatformSegmentData platform = tierPlatforms != null && spawn.platformSegmentIndex >= 0 && spawn.platformSegmentIndex < tierPlatforms.Count
                ? tierPlatforms[spawn.platformSegmentIndex]
                : null;

            if (platform == null)
            {
                errors.Add((isBoss ? "Boss" : "Monster") + $" spawn is outside valid platform bounds: {realm.id}");
                return;
            }

            float left = platform.position.x - platform.size.x * 0.5f;
            float right = platform.position.x + platform.size.x * 0.5f;
            if (spawn.spawnPosition.x < left - 80f || spawn.spawnPosition.x > right + 80f)
            {
                errors.Add((isBoss ? "Boss" : "Monster") + $" spawn x is outside its platform bounds: {realm.id}");
            }
        }

        private static void ValidateScripts(List<string> errors, List<string> warnings)
        {
            if (!File.Exists(GameScenePath))
            {
                errors.Add("GameScene is missing.");
            }

            string worldMapSource = File.Exists("Assets/_Game/Scripts/UI/WorldMapUIController.cs")
                ? File.ReadAllText("Assets/_Game/Scripts/UI/WorldMapUIController.cs")
                : string.Empty;
            if (!worldMapSource.Contains("GameUIScreen.RealmAdventureMap"))
            {
                errors.Add("WorldMapUIController does not enter RealmAdventureMapUI.");
            }

            if (worldMapSource.Contains("BattleService") || worldMapSource.Contains("GameUIScreen.Battle"))
            {
                errors.Add("WorldMapUIController still references battle directly.");
            }

            if (!File.Exists("Assets/_Game/Scripts/Adventure/RealmAdventureMapUIController.cs") ||
                !File.Exists("Assets/_Game/Scripts/Adventure/AdventurePlayerController.cs") ||
                !File.Exists("Assets/_Game/Scripts/Adventure/AdventureMonsterController.cs"))
            {
                errors.Add("Required adventure map scripts are missing.");
            }
        }

        private static void ValidateBottomNavigation(List<string> errors, List<string> warnings)
        {
            GameObject bottomNavigation = FindGameObject("RootCanvas/SafeAreaRoot/NavigationLayer/BottomNavigation");
            GameObject realmAdventure = FindGameObject("RootCanvas/SafeAreaRoot/MainLayer/RealmAdventureMapUI");
            if (realmAdventure != null && realmAdventure.activeSelf && bottomNavigation != null && bottomNavigation.activeSelf)
            {
                errors.Add("BottomNavigation must be hidden during RealmAdventureMapUI.");
            }
        }

        private static bool OpenGameScene()
        {
            if (!File.Exists(GameScenePath))
            {
                Debug.LogError("[Adventure] GameScene not found: " + GameScenePath);
                return false;
            }

            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            return true;
        }

        private static GameContentDatabase LoadDatabase()
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<GameContentDatabase>("Assets/_Game/ScriptableObjects/GameContentDatabase.asset");
#else
            return null;
#endif
        }

        private static List<RealmDefinition> LoadRealms()
        {
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:RealmDefinition", new[] { "Assets/_Game/ScriptableObjects/Realms" });
            List<RealmDefinition> realms = new List<RealmDefinition>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RealmDefinition realm = AssetDatabase.LoadAssetAtPath<RealmDefinition>(path);
                if (realm != null)
                {
                    realms.Add(realm);
                }
            }

            realms.Sort((a, b) => a.order.CompareTo(b.order));
            return realms;
#else
            return new List<RealmDefinition>();
#endif
        }

        private static GameObject FindGameObject(string path)
        {
            Transform transform = FindTransform(path);
            return transform != null ? transform.gameObject : null;
        }

        private static Transform FindTransform(string path)
        {
            string[] parts = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return null;
            }

            Transform current = null;
            foreach (Transform candidate in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (candidate != null && candidate.parent == null && candidate.name == parts[0])
                {
                    current = candidate;
                    break;
                }
            }

            if (current == null)
            {
                return null;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                Transform next = null;
                for (int j = 0; j < current.childCount; j++)
                {
                    Transform child = current.GetChild(j);
                    if (child != null && child.name == parts[i])
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        private static bool ContainsDescendantNamed(Transform root, string childName)
        {
            if (root == null)
            {
                return false;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name == childName || ContainsDescendantNamed(child, childName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
