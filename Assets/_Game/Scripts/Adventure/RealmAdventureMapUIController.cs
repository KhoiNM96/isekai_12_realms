using System;
using System.Collections.Generic;
using Isekai12Realms.Character;
using Isekai12Realms.Battle;
using Isekai12Realms.Data;
using Isekai12Realms.Enemies;
using Isekai12Realms.Realms;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Adventure
{
    public class RealmAdventureMapUIController : MonoBehaviour
    {
        private UIScreenManager screenManager;
        private AdventureMapService adventureService;
        private ContentDatabaseService contentService;
        private RealmProgressionService realmProgressionService;
        private PlayerProgressionService progressionService;

        private RectTransform root;
        private RectTransform headerRoot;
        private RectTransform mapViewport;
        private RectTransform mapContentRoot;
        private RectTransform backgroundRoot;
        private RectTransform platformRoot;
        private RectTransform monsterRoot;
        private RectTransform playerRoot;
        private RectTransform fxRoot;
        private RectTransform controlsRoot;
        private TextMeshProUGUI realmNameText;
        private TextMeshProUGUI realmRankText;
        private TextMeshProUGUI realmProgressText;
        private TextMeshProUGUI footerHintText;
        private Button backButton;
        private Button leftButton;
        private Button rightButton;
        private Button jumpButton;
        private Button downButton;

        private readonly List<AdventureMonsterController> monsterControllers = new List<AdventureMonsterController>();
        private readonly AdventureMapRuntimeState runtimeState = new AdventureMapRuntimeState();
        private RealmDefinition currentRealm;
        private AdventurePlayerController playerController;
        private bool built;

        public void Initialize(UIScreenManager ui, AdventureMapService service, ContentDatabaseService content, RealmProgressionService realmProgression, PlayerProgressionService progression)
        {
            screenManager = ui;
            adventureService = service;
            contentService = content;
            realmProgressionService = realmProgression;
            progressionService = progression;
            BuildOrRepair();
            LoadCurrentRealm();
        }

        public void BuildOrRepair()
        {
            root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            ClearChildren(root);
            BuildHeader(root);
            BuildMapViewport(root);
            BuildControls(root);
            BuildFooter(root);
            built = true;
        }

        public void RefreshMap()
        {
            if (!built)
            {
                BuildOrRepair();
            }

            LoadCurrentRealm();
            UpdateHeaderText();
            UpdateProgressText();
            UpdateBossVisibility();
        }

        public void ShowRealm(RealmDefinition realm)
        {
            if (realm == null)
            {
                SetEmptyState();
                return;
            }

            currentRealm = realm;
            runtimeState.currentRealmId = realm.id;
            Debug.Log("[Adventure] BuildMap started");
            AdventurePlatformMapBuilder.EnsurePrototypeLayout(currentRealm, contentService);
            BuildRealmMap(currentRealm);
            Debug.Log($"[Adventure] Platform count: {GetPlatformsForRealm(currentRealm).Count}");
            Debug.Log("[Adventure] BuildMap completed");
        }

        public void SetMovementEnabled(bool enabled)
        {
            if (playerController != null)
            {
                playerController.SetMovementEnabled(enabled);
            }
        }

        public void OnEncounterVictory(BattleEncounterData encounter)
        {
            if (encounter == null || currentRealm == null)
            {
                return;
            }

            runtimeState.encounterInProgress = false;
            if (!runtimeState.defeatedEncounterIds.Contains(encounter.encounterId))
            {
                runtimeState.defeatedEncounterIds.Add(encounter.encounterId);
            }

            if (!runtimeState.defeatedMonsterIds.Contains(encounter.enemyId))
            {
                runtimeState.defeatedMonsterIds.Add(encounter.enemyId);
            }

            AdventureMonsterController monster = FindMonster(encounter.enemyId);
            if (monster != null)
            {
                monster.MarkDefeated();
            }

            if (encounter.isBoss)
            {
                runtimeState.bossDefeated = true;
                realmProgressionService?.MarkBossDefeated(currentRealm.id);
                realmProgressionService?.MarkRealmCompleted(currentRealm.id);
            }

            UpdateProgressText();
        }

        public void OnEncounterDefeat(BattleEncounterData encounter)
        {
            runtimeState.encounterInProgress = false;
            if (encounter != null)
            {
                AdventureMonsterController monster = FindMonster(encounter.enemyId);
                if (monster != null)
                {
                    monster.ResetEncounterLock();
                }
            }
            if (playerController != null && currentRealm != null)
            {
                playerController.SetSpawnPosition(currentRealm.playerSpawnPosition);
            }

            if (screenManager != null && screenManager.ToastService != null)
            {
                screenManager.ToastService.ShowToast("You retreated to the realm entrance.");
            }
        }

        public void StartEncounter(AdventureMonsterController monster)
        {
            if (monster == null || currentRealm == null || adventureService == null || screenManager == null)
            {
                return;
            }

            if (runtimeState.encounterInProgress)
            {
                return;
            }

            runtimeState.encounterInProgress = true;
            Battle.BattleEncounterData encounter = adventureService.CreateEncounterForMonster(currentRealm, monster);
            if (encounter == null)
            {
                runtimeState.encounterInProgress = false;
                return;
            }

            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }

            adventureService.BeginEncounter(encounter);
        }

        public void RevealBossIfNeeded()
        {
            UpdateBossVisibility();
        }

        private void LoadCurrentRealm()
        {
            RealmDefinition realm = adventureService != null ? adventureService.GetCurrentRealm() : null;
            if (realm == null)
            {
                currentRealm = null;
                SetEmptyState();
                return;
            }

            if (currentRealm != null && currentRealm.id == realm.id && monsterControllers.Count > 0)
            {
                return;
            }

            currentRealm = realm;
            runtimeState.currentRealmId = realm.id;
            AdventurePlatformMapBuilder.EnsurePrototypeLayout(currentRealm, contentService);
            BuildRealmMap(currentRealm);
        }

        private void BuildRealmMap(RealmDefinition realm)
        {
            if (realm == null)
            {
                return;
            }

            runtimeState.defeatedEncounterIds.Clear();
            runtimeState.defeatedMonsterIds.Clear();
            runtimeState.bossVisible = false;
            runtimeState.bossDefeated = realmProgressionService != null && realmProgressionService.IsRealmCompleted(realm.id);
            runtimeState.encounterInProgress = false;

            if (realmNameText != null)
            {
                realmNameText.text = realm.displayName;
            }

            if (realmRankText != null)
            {
                realmRankText.text = $"Rank: {realm.rank}";
            }

            AdventurePlatformMapBuilder.BuildPlatformVisuals(platformRoot, GetPlatformsForRealm(realm));
            ClearChildren(monsterRoot);
            monsterControllers.Clear();

            if (playerController != null)
            {
                Destroy(playerController.gameObject);
            }

            GameObject playerObject = new GameObject("Player", typeof(RectTransform), typeof(Image), typeof(AdventurePlayerController));
            playerObject.transform.SetParent(playerRoot, false);
            RectTransform playerRect = playerObject.GetComponent<RectTransform>();
            playerRect.anchorMin = playerRect.anchorMax = new Vector2(0f, 0.5f);
            playerRect.pivot = new Vector2(0.5f, 0.5f);
            playerRect.sizeDelta = new Vector2(96f, 96f);
            playerRect.anchoredPosition = GetPlayerSpawn(realm);
            Image playerImage = playerObject.GetComponent<Image>();
            playerImage.raycastTarget = false;
            SetSprite(playerImage, ClassIdleSpriteAssetId(progressionService != null && progressionService.CurrentSave != null ? progressionService.CurrentSave.selectedClassId : string.Empty));
            playerController = playerObject.GetComponent<AdventurePlayerController>();
            playerController.Initialize(mapContentRoot, mapViewport);
            playerController.SetPlatforms(GetPlatformsForRealm(realm));
            playerController.SetSpawnPosition(GetPlayerSpawn(realm));
            playerController.SetMovementEnabled(true);
            Debug.Log("[Adventure] Player spawned");

            SpawnNormalMonsters(realm);
            SpawnBossIfReady(realm);
            UpdateProgressText();
            UpdateHeaderText();
        }

        private void SetEmptyState()
        {
            runtimeState.currentRealmId = string.Empty;
            runtimeState.bossVisible = false;
            runtimeState.bossDefeated = false;

            if (realmNameText != null)
            {
                realmNameText.text = "Adventure Map";
            }

            if (realmRankText != null)
            {
                realmRankText.text = "Enter a realm from the world map.";
            }

            if (realmProgressText != null)
            {
                realmProgressText.text = string.Empty;
            }

            if (footerHintText != null)
            {
                footerHintText.text = string.Empty;
            }

            ClearChildren(mapContentRoot);
            ClearChildren(platformRoot);
            ClearChildren(monsterRoot);
            ClearChildren(playerRoot);
            playerController = null;
            monsterControllers.Clear();
        }

        private void SpawnNormalMonsters(RealmDefinition realm)
        {
            List<MonsterSpawnData> spawns = AdventurePlatformMapBuilder.BuildMonsterSpawns(realm, contentService);
            if ((spawns == null || spawns.Count == 0) && realm != null)
            {
                spawns = BuildFallbackMonsterSpawns(realm);
            }

            if (realm != null && realm.id == "realm_01_meadow" && spawns.Count < 4)
            {
                List<MonsterSpawnData> fallback = BuildFallbackMonsterSpawns(realm);
                for (int i = spawns.Count; i < 4 && i < fallback.Count; i++)
                {
                    spawns.Add(fallback[i]);
                }
            }

            for (int i = 0; i < spawns.Count; i++)
            {
                MonsterSpawnData spawn = spawns[i];
                EnemyDefinition enemy = GetEnemyById(spawn.enemyId);
                if (enemy == null)
                {
                    enemy = CreateFallbackEnemy(spawn.enemyId, spawn.tierIndex, false, i);
                }

                PlatformSegmentData ground = FindPlatformForSpawn(realm, spawn);
                Debug.Log($"[Adventure] Monster spawned: {spawn.enemyId}");
                CreateMonster(enemy, false, spawn, ground);
            }
        }

        private void SpawnBossIfReady(RealmDefinition realm)
        {
            if (realm == null || realm.bossEnemy == null || realm.bossSpawn == null)
            {
                return;
            }

            bool visible = ShouldShowBoss(realm);
            runtimeState.bossVisible = visible;
            if (!visible)
            {
                return;
            }

            MonsterSpawnData bossSpawn = realm.bossSpawn;
            bossSpawn.enemyId = string.IsNullOrEmpty(bossSpawn.enemyId) ? realm.bossEnemy.id : bossSpawn.enemyId;
            bossSpawn.isBoss = true;
            bossSpawn.initiallyHidden = false;
            PlatformSegmentData ground = FindPlatformForSpawn(realm, bossSpawn);
            CreateMonster(realm.bossEnemy, true, bossSpawn, ground);
        }

        private void UpdateBossVisibility()
        {
            if (currentRealm == null || currentRealm.bossEnemy == null)
            {
                return;
            }

            bool shouldShow = ShouldShowBoss(currentRealm);
            if (shouldShow && !runtimeState.bossVisible && screenManager != null && screenManager.ToastService != null)
            {
                screenManager.ToastService.ShowToast("A powerful enemy has appeared!");
            }

            runtimeState.bossVisible = shouldShow;
            for (int i = 0; i < monsterControllers.Count; i++)
            {
                AdventureMonsterController monster = monsterControllers[i];
                if (monster != null && monster.IsBoss)
                {
                    monster.SetVisible(shouldShow && !runtimeState.bossDefeated);
                }
            }
        }

        private void UpdateHeaderText()
        {
            if (currentRealm == null)
            {
                return;
            }

            if (realmNameText != null)
            {
                realmNameText.text = currentRealm.displayName;
            }

            if (realmRankText != null)
            {
                realmRankText.text = $"Rank: {currentRealm.rank}";
            }
        }

        private void UpdateProgressText()
        {
            if (realmProgressText == null || currentRealm == null)
            {
                return;
            }

            RealmProgressData progress = realmProgressionService != null ? realmProgressionService.GetCurrentRealmProgress(currentRealm.id) : null;
            int defeated = progress != null ? progress.normalMonstersDefeated : 0;
            realmProgressText.text = $"Progress: {defeated}/3 normals{(progress != null && progress.bossDefeated ? " | Boss cleared" : string.Empty)}";
        }

        private bool ShouldShowBoss(RealmDefinition realm)
        {
            RealmProgressData progress = realmProgressionService != null ? realmProgressionService.GetCurrentRealmProgress(realm.id) : null;
            return progress != null && progress.normalMonstersDefeated >= 3 && !progress.bossDefeated;
        }

        private EnemyDefinition GetEnemyById(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                return null;
            }

            if (contentService != null && contentService.Database != null)
            {
                return contentService.Database.GetEnemyById(enemyId);
            }

            return null;
        }

        private AdventureMonsterController FindMonster(string enemyId)
        {
            for (int i = 0; i < monsterControllers.Count; i++)
            {
                AdventureMonsterController monster = monsterControllers[i];
                if (monster != null && monster.EnemyDefinition != null && string.Equals(monster.EnemyDefinition.id, enemyId, StringComparison.OrdinalIgnoreCase))
                {
                    return monster;
                }
            }

            return null;
        }

        private void CreateMonster(EnemyDefinition enemy, bool boss, MonsterSpawnData spawn, PlatformSegmentData platform)
        {
            if (enemy == null || spawn == null)
            {
                return;
            }

            string monsterName = boss ? $"Boss_{enemy.id}" : enemy.id;
            GameObject monsterObject = new GameObject(monsterName, typeof(RectTransform), typeof(Image), typeof(AdventureMonsterController), typeof(AdventureEncounterTrigger));
            monsterObject.transform.SetParent(monsterRoot, false);
            RectTransform rect = monsterObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = boss ? new Vector2(128f, 128f) : new Vector2(106f, 106f);
            rect.anchoredPosition = spawn.spawnPosition;

            Image image = monsterObject.GetComponent<Image>();
            image.raycastTarget = false;
            SetSprite(image, enemy.spriteAssetId);

            TextMeshProUGUI label = CreateText(monsterObject.transform, "Label", enemy.displayName, boss ? 18 : 16, Color.white, new Vector2(0f, -88f), new Vector2(220f, 42f));
            label.alignment = TextAlignmentOptions.Center;

            AdventureMonsterController controller = monsterObject.GetComponent<AdventureMonsterController>();
            controller.Initialize(enemy, boss, adventureService, playerController, platform, spawn);
            AdventureEncounterTrigger trigger = monsterObject.GetComponent<AdventureEncounterTrigger>();
            trigger.Initialize(controller, playerController, boss ? 150f : 104f);
            monsterControllers.Add(controller);
        }

        private List<MonsterSpawnData> BuildFallbackMonsterSpawns(RealmDefinition realm)
        {
            List<MonsterSpawnData> spawns = new List<MonsterSpawnData>();
            if (realm == null)
            {
                return spawns;
            }

            List<PlatformSegmentData> tier1 = realm.mapLayout != null ? realm.mapLayout.tier1Segments : null;
            List<PlatformSegmentData> tier2 = realm.mapLayout != null ? realm.mapLayout.tier2Segments : null;
            List<PlatformSegmentData> tier3 = realm.mapLayout != null ? realm.mapLayout.tier3Segments : null;

            AddFallbackSpawnFromPlatform(spawns, tier1, 1, 0, "enemy_meadow_slime");
            AddFallbackSpawnFromPlatform(spawns, tier1, 1, 0, "enemy_meadow_mushroom");
            AddFallbackSpawnFromPlatform(spawns, tier2, 2, 0, "enemy_meadow_leaf_bug");
            AddFallbackSpawnFromPlatform(spawns, tier3, 3, 0, "enemy_meadow_shaman");
            return spawns;
        }

        private void AddFallbackSpawnFromPlatform(List<MonsterSpawnData> spawns, List<PlatformSegmentData> platforms, int tierIndex, int platformIndex, string enemyId)
        {
            if (platforms == null || platforms.Count == 0)
            {
                return;
            }

            PlatformSegmentData platform = platforms[Mathf.Clamp(platformIndex, 0, platforms.Count - 1)];
            if (platform == null)
            {
                return;
            }

            spawns.Add(new MonsterSpawnData
            {
                enemyId = enemyId,
                spawnPosition = new Vector2(platform.position.x + 120f, platform.position.y + platform.size.y * 0.5f + 52f),
                patrolDistance = Mathf.Max(120f, platform.size.x * 0.35f),
                isBoss = false,
                initiallyHidden = false,
                tierIndex = tierIndex,
                platformSegmentIndex = platformIndex
            });
        }

        private static EnemyDefinition CreateFallbackEnemy(string enemyId, int tierIndex, bool boss, int seed)
        {
            EnemyDefinition enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            string label = !string.IsNullOrEmpty(enemyId)
                ? enemyId
                : tierIndex == 2 ? "Mushroom" : tierIndex == 3 ? "Bug" : "Slime";
            enemy.id = string.IsNullOrEmpty(enemyId) ? $"fallback_{label.ToLowerInvariant()}_{seed}" : enemyId;
            enemy.displayName = boss ? "Boss" : (tierIndex == 2 ? "Mushroom" : tierIndex == 3 ? "Bug" : "Slime");
            enemy.level = Mathf.Max(1, tierIndex);
            enemy.maxHp = boss ? 220 : (tierIndex == 3 ? 120 : tierIndex == 2 ? 100 : 80);
            enemy.attack = boss ? 20 : (tierIndex == 3 ? 14 : tierIndex == 2 ? 12 : 8);
            enemy.defense = boss ? 8 : 2;
            enemy.maxMana = 100;
            enemy.spriteAssetId = tierIndex == 3 ? "enemy_meadow_leaf_bug" : tierIndex == 2 ? "enemy_meadow_mushroom" : "enemy_meadow_slime";
            enemy.difficulty = boss ? EnemyAIDifficulty.Boss : (tierIndex == 3 ? EnemyAIDifficulty.Hard : EnemyAIDifficulty.Easy);
            return enemy;
        }

        private void BuildHeader(RectTransform parent)
        {
            headerRoot = CreateRect(parent, "Header");
            SetTopRect(headerRoot, 150f);
            EnsureImage(headerRoot.gameObject, new Color(0.08f, 0.09f, 0.13f, 0.94f));

            backButton = CreateButton(headerRoot, "Button_BackToWorldMap", "Back", new Color(0.22f, 0.33f, 0.47f, 1f), new Vector2(-420f, -75f), new Vector2(220f, 70f), () =>
            {
                adventureService?.ExitRealm();
                screenManager?.ShowScreen(GameUIScreen.WorldMap);
            });

            realmNameText = CreateText(headerRoot, "RealmName_Text", "Realm", 40, Color.white, new Vector2(0f, -64f), new Vector2(560f, 54f));
            realmRankText = CreateText(headerRoot, "RealmRank_Text", "Rank: -", 24, new Color(0.93f, 0.95f, 0.97f, 1f), new Vector2(0f, -110f), new Vector2(420f, 34f));
            realmProgressText = CreateText(headerRoot, "RealmProgress_Text", "Progress: 0/3", 22, new Color(0.93f, 0.95f, 0.97f, 1f), new Vector2(260f, -110f), new Vector2(420f, 34f));
        }

        private void BuildMapViewport(RectTransform parent)
        {
            mapViewport = CreateRect(parent, "MapViewport");
            SetRect(mapViewport, new Vector2(0f, 0f), new Vector2(960f, 1460f));
            Image clipImage = EnsureImage(mapViewport.gameObject, new Color(1f, 1f, 1f, 0.01f));
            clipImage.raycastTarget = false;
            Mask mask = mapViewport.gameObject.GetComponent<Mask>() ?? mapViewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            float totalMapWidth = 5760f;
            if (currentRealm != null && currentRealm.mapLayout != null && currentRealm.mapLayout.totalMapWidth > 0f)
            {
                totalMapWidth = currentRealm.mapLayout.totalMapWidth;
            }

            mapContentRoot = CreateRect(mapViewport, "MapContent");
            mapContentRoot.anchorMin = mapContentRoot.anchorMax = new Vector2(0f, 0.5f);
            mapContentRoot.pivot = new Vector2(0f, 0.5f);
            mapContentRoot.sizeDelta = new Vector2(totalMapWidth, 1460f);
            mapContentRoot.anchoredPosition = Vector2.zero;

            backgroundRoot = CreateRect(mapContentRoot, "Background");
            backgroundRoot.anchorMin = backgroundRoot.anchorMax = new Vector2(0f, 0.5f);
            backgroundRoot.pivot = new Vector2(0f, 0.5f);
            backgroundRoot.sizeDelta = new Vector2(totalMapWidth, 1460f);
            backgroundRoot.anchoredPosition = Vector2.zero;
            EnsureImage(backgroundRoot.gameObject, new Color(0.08f, 0.14f, 0.18f, 0.92f)).raycastTarget = false;

            platformRoot = CreateRect(mapContentRoot, "PlatformRoot");
            platformRoot.anchorMin = platformRoot.anchorMax = new Vector2(0f, 0.5f);
            platformRoot.pivot = new Vector2(0f, 0.5f);
            platformRoot.sizeDelta = new Vector2(totalMapWidth, 1460f);
            platformRoot.anchoredPosition = Vector2.zero;
            monsterRoot = CreateRect(mapContentRoot, "MonsterRoot");
            monsterRoot.anchorMin = monsterRoot.anchorMax = new Vector2(0f, 0.5f);
            monsterRoot.pivot = new Vector2(0f, 0.5f);
            monsterRoot.sizeDelta = new Vector2(totalMapWidth, 1460f);
            monsterRoot.anchoredPosition = Vector2.zero;
            playerRoot = CreateRect(mapContentRoot, "PlayerRoot");
            playerRoot.anchorMin = playerRoot.anchorMax = new Vector2(0f, 0.5f);
            playerRoot.pivot = new Vector2(0f, 0.5f);
            playerRoot.sizeDelta = new Vector2(totalMapWidth, 1460f);
            playerRoot.anchoredPosition = Vector2.zero;
            fxRoot = CreateRect(mapContentRoot, "FXRoot");
            fxRoot.anchorMin = fxRoot.anchorMax = new Vector2(0f, 0.5f);
            fxRoot.pivot = new Vector2(0f, 0.5f);
            fxRoot.sizeDelta = new Vector2(totalMapWidth, 1460f);
            fxRoot.anchoredPosition = Vector2.zero;
        }

        private void BuildControls(RectTransform parent)
        {
            controlsRoot = CreateRect(parent, "MobileControls");
            SetBottomRect(controlsRoot, 290f, 380f);
            EnsureImage(controlsRoot.gameObject, new Color(0.05f, 0.06f, 0.08f, 0.56f));
            GridLayoutGroup grid = controlsRoot.gameObject.GetComponent<GridLayoutGroup>() ?? controlsRoot.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(104f, 82f);
            grid.spacing = new Vector2(16f, 16f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            leftButton = CreateHoldButton(controlsRoot, "Button_Left", "Left", () => playerController?.SetMoveInput(-1f), () => playerController?.SetMoveInput(0f), null);
            rightButton = CreateHoldButton(controlsRoot, "Button_Right", "Right", () => playerController?.SetMoveInput(1f), () => playerController?.SetMoveInput(0f), null);
            jumpButton = CreateHoldButton(controlsRoot, "Button_Jump", "Jump", null, null, () => playerController?.RequestJump());
            downButton = CreateHoldButton(controlsRoot, "Button_Down", "Down", () => playerController?.RequestDropThrough(), null, null);
        }

        private void BuildFooter(RectTransform parent)
        {
            footerHintText = CreateText(parent, "FooterHint_Text", "Move, jump, and touch monsters to battle.", 18, new Color(1f, 1f, 1f, 0.82f), new Vector2(0f, 16f), new Vector2(800f, 26f));
        }

        private List<PlatformSegmentData> GetPlatformsForRealm(RealmDefinition realm)
        {
            if (realm == null)
            {
                return new List<PlatformSegmentData>();
            }

            if (realm.platforms != null && realm.platforms.Count > 0)
            {
                return realm.platforms;
            }

            List<PlatformSegmentData> combined = new List<PlatformSegmentData>();
            if (realm.mapLayout != null)
            {
                if (realm.mapLayout.tier1Segments != null) combined.AddRange(realm.mapLayout.tier1Segments);
                if (realm.mapLayout.tier2Segments != null) combined.AddRange(realm.mapLayout.tier2Segments);
                if (realm.mapLayout.tier3Segments != null) combined.AddRange(realm.mapLayout.tier3Segments);
            }

            return combined;
        }

        private Vector2 GetPlayerSpawn(RealmDefinition realm)
        {
            if (realm != null && realm.mapLayout != null && realm.mapLayout.playerSpawnPosition != Vector2.zero)
            {
                return realm.mapLayout.playerSpawnPosition;
            }

            return realm != null ? realm.playerSpawnPosition : Vector2.zero;
        }

        private PlatformSegmentData FindPlatformForSpawn(RealmDefinition realm, MonsterSpawnData spawn)
        {
            List<PlatformSegmentData> platforms = GetTierPlatforms(realm, spawn != null ? spawn.tierIndex : 1);
            if (platforms.Count == 0)
            {
                platforms = GetPlatformsForRealm(realm);
                if (platforms.Count == 0)
                {
                    return null;
                }
            }

            if (spawn != null && spawn.platformSegmentIndex >= 0 && spawn.platformSegmentIndex < platforms.Count)
            {
                return platforms[spawn.platformSegmentIndex];
            }

            return platforms[0];
        }

        private List<PlatformSegmentData> GetTierPlatforms(RealmDefinition realm, int tierIndex)
        {
            List<PlatformSegmentData> platforms = new List<PlatformSegmentData>();
            if (realm == null)
            {
                return platforms;
            }

            if (realm.mapLayout != null)
            {
                if (tierIndex == 1 && realm.mapLayout.tier1Segments != null) return realm.mapLayout.tier1Segments;
                if (tierIndex == 2 && realm.mapLayout.tier2Segments != null) return realm.mapLayout.tier2Segments;
                if (tierIndex == 3 && realm.mapLayout.tier3Segments != null) return realm.mapLayout.tier3Segments;
            }

            if (realm.platforms != null)
            {
                for (int i = 0; i < realm.platforms.Count; i++)
                {
                    PlatformSegmentData platform = realm.platforms[i];
                    if (platform != null && platform.tierIndex == tierIndex)
                    {
                        platforms.Add(platform);
                    }
                }
            }

            return platforms;
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void SetRect(RectTransform rect, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static void SetTopRect(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0.03f, 1f);
            rect.anchorMax = new Vector2(0.97f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void SetBottomRect(RectTransform rect, float bottomInset, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(380f, height);
            rect.anchoredPosition = new Vector2(0f, bottomInset);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static Image EnsureImage(GameObject target, Color color)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }

            image.color = color;
            image.type = Image.Type.Simple;
            return image;
        }

        private static Button CreateButton(Transform parent, string name, string text, Color color, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = CreateRect(parent, name);
            SetRect(rect, pos, size);
            Image image = EnsureImage(rect.gameObject, color);
            image.raycastTarget = true;
            Button button = rect.gameObject.GetComponent<Button>() ?? rect.gameObject.AddComponent<Button>();
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }

            TextMeshProUGUI label = CreateText(rect, "Text", text, Mathf.RoundToInt(size.y * 0.35f), Color.white, Vector2.zero, size);
            label.margin = new Vector4(12f, 0f, 12f, 0f);
            return button;
        }

        private static Button CreateHoldButton(Transform parent, string name, string text, Action pressed, Action released, Action clicked)
        {
            Button button = CreateButton(parent, name, text, new Color(0.22f, 0.35f, 0.5f, 1f), Vector2.zero, new Vector2(104f, 82f), null);
            AdventureHoldButton hold = button.gameObject.GetComponent<AdventureHoldButton>() ?? button.gameObject.AddComponent<AdventureHoldButton>();
            hold.Pressed = pressed;
            hold.Released = released;
            hold.Clicked = clicked;
            return button;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, Color color, Vector2 pos, Vector2 rectSize)
        {
            RectTransform rect = CreateRect(parent, name);
            SetRect(rect, pos, rectSize);
            TextMeshProUGUI label = rect.gameObject.GetComponent<TextMeshProUGUI>() ?? rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static Sprite GetSpriteFromAsset(string assetId)
        {
            return string.IsNullOrEmpty(assetId) ? null : AssetSpriteBinder.GetSprite(assetId);
        }

        private static void SetSprite(Image image, string assetId)
        {
            if (image == null)
            {
                return;
            }

            Sprite sprite = GetSpriteFromAsset(assetId);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
                image.preserveAspect = true;
            }
            else if (!string.IsNullOrEmpty(assetId))
            {
                image.color = new Color(0.55f, 0.82f, 0.95f, 1f);
            }
        }

        private static string ClassIdleSpriteAssetId(string classId)
        {
            if (classId == "tide_acolyte") return "char_hero_tide_idle";
            if (classId == "storm_scout") return "char_hero_storm_idle";
            return "char_hero_flame_idle";
        }
    }
}
