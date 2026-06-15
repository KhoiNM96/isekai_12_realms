using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isekai12Realms.Crafting;
using Isekai12Realms.ContentPacks;
using Isekai12Realms.Data;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Equipment;
using Isekai12Realms.Quests;
using Isekai12Realms.Realms;
using Isekai12Realms.Shop;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using Isekai12Realms.Tutorial;
using Isekai12Realms.UI;
using Isekai12Realms.Battle;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor.ContentTools
{
    public class IsekaiContentEditorWindow : EditorWindow
    {
        private const string Root = "Assets/_Game/ScriptableObjects";
        private const string RealmPath = Root + "/Realms";
        private const string StagePath = Root + "/Stages";
        private const string EnemyPath = Root + "/Enemies";
        private const string DropPath = Root + "/DropTables";
        private const string SkillPath = Root + "/Skills";
        private const string EquipmentPath = Root + "/Equipment";
        private const string ShopPath = Root + "/Shop";
        private const string EconomyPath = Root + "/Economy";
        private const string DatabasePath = Root + "/GameContentDatabase.asset";
        private const string ExportPath = "Assets/_Game/Export/content_export.json";

        private readonly string[] tabs = { "Realms", "Stages", "Enemies", "Drop Tables", "Skills", "Equipment", "Shops", "IAP Products", "Validate", "Export / Import" };
        private int tabIndex;
        private Vector2 scroll;
        private GameContentDatabase database;
        private RealmDefinition selectedRealm;
        private StageDefinition selectedStage;
        private EnemyDefinition selectedEnemy;
        private DropTableDefinition selectedDropTable;
        private SkillDefinition selectedSkill;
        private EquipmentDefinition selectedEquipment;
        private ShopDefinition selectedShop;
        private IAPProductDefinition selectedIapProduct;
        private string newRealmId = "realm_new";
        private string newStageId = "stage_new";
        private string newEnemyId = "enemy_new";
        private string newDropTableId = "drop_new";
        private string newSkillId = "skill_new";
        private string newEquipmentId = "equip_new";
        private string newShopId = "shop_new";
        private string newIapProductId = "gems_new";
        private string skillClassFilter = string.Empty;
        private EquipmentSlot equipmentSlotFilter;
        private EquipmentRarity equipmentRarityFilter;
        private ShopType shopTypeFilter;
        private bool useEquipmentSlotFilter;
        private bool useEquipmentRarityFilter;
        private bool useShopTypeFilter;
        private string stageRealmFilter = string.Empty;
        private string validationReport = "Run validation to see report.";

        [MenuItem("Tools/Isekai 12 Realms/Content Editor")]
        public static void Open()
        {
            GetWindow<IsekaiContentEditorWindow>("Isekai Content");
        }

        [MenuItem("Tools/Isekai 12 Realms/Rebuild Content Database")]
        public static void RebuildContentDatabaseMenu()
        {
            GameContentDatabase db = RebuildDatabase();
            Debug.Log($"[Content] Rebuilt database: {db.realms.Count} realms, {db.stages.Count} stages, {db.enemies.Count} enemies, {db.dropTables.Count} drop tables.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Create 12 Realm Skeleton Content")]
        public static void Create12RealmSkeletonContent()
        {
            EnsureFolders();
            bool overwrite = EditorUtility.DisplayDialog("Create 12 Realm Skeleton Content", "Update existing skeleton assets if found? This will not delete assets or rename IDs.", "Update Existing", "Only Missing");
            string[] names = { "meadow", "ember", "tide", "thunder", "rootwood", "crystal", "bazaar", "snow", "clock", "candy", "library", "eclipse" };
            string[] display = { "Meadow Gate", "Ember Village", "Tide Shrine", "Thunder Peak", "Rootwood Forest", "Crystal Mine", "Moon Bazaar", "Snow Lantern", "Clock Ruin", "Candy Citadel", "Sky Library", "Eclipse Throne" };

            for (int i = 0; i < 12; i++)
            {
                int number = i + 1;
                string realmId = $"realm_{number:00}_{names[i]}";
                RealmDefinition realm = LoadOrCreate<RealmDefinition>($"{RealmPath}/{realmId}.asset");
                if (overwrite || string.IsNullOrEmpty(realm.id))
                {
                    realm.id = realmId;
                    realm.displayName = display[i];
                    realm.description = $"Placeholder realm {display[i]}.";
                    realm.order = number;
                    realm.backgroundAssetId = string.Empty;
                    if (realm.stages == null) realm.stages = new List<StageDefinition>();
                    realm.stages.Clear();
                    EditorUtility.SetDirty(realm);
                }

                for (int s = 1; s <= 3; s++)
                {
                    bool boss = s == 3;
                    string enemyId = boss ? $"boss_{names[i]}_placeholder" : $"enemy_{names[i]}_{s:00}";
                    EnemyDefinition enemy = LoadOrCreate<EnemyDefinition>($"{EnemyPath}/{enemyId}.asset");
                    if (overwrite || string.IsNullOrEmpty(enemy.id))
                    {
                        enemy.id = enemyId;
                        enemy.displayName = boss ? $"{display[i]} Boss" : $"{display[i]} Enemy {s}";
                        enemy.level = number * 3 + s;
                        enemy.maxHp = boss ? 180 + number * 20 : 80 + number * 15 + s * 10;
                        enemy.attack = boss ? 14 + number : 8 + number + s;
                        enemy.defense = boss ? 3 + number / 3 : number / 3;
                        enemy.maxMana = 100;
                        enemy.spriteAssetId = enemyId;
                        enemy.difficulty = boss ? EnemyAIDifficulty.Boss : EnemyAIDifficulty.Normal;
                        EditorUtility.SetDirty(enemy);
                    }

                    string dropId = $"drop_stage_{number:00}_{s:00}" + (boss ? "_boss" : string.Empty);
                    DropTableDefinition drop = LoadOrCreate<DropTableDefinition>($"{DropPath}/{dropId}.asset");
                    if (overwrite || string.IsNullOrEmpty(drop.id))
                    {
                        drop.id = dropId;
                        drop.drops = new List<DropEntry> { new DropEntry { itemId = "item_potion_small", minAmount = 1, maxAmount = boss ? 2 : 1, chance = boss ? 0.5f : 0.25f } };
                        EditorUtility.SetDirty(drop);
                    }

                    string stageId = boss ? $"stage_{number:00}_03_boss" : $"stage_{number:00}_{s:00}";
                    StageDefinition stage = LoadOrCreate<StageDefinition>($"{StagePath}/{stageId}.asset");
                    if (overwrite || string.IsNullOrEmpty(stage.id))
                    {
                        stage.id = stageId;
                        stage.realmId = realmId;
                        stage.displayName = boss ? $"{display[i]} Trial" : $"{display[i]} Stage {s}";
                        stage.description = "Skeleton placeholder stage.";
                        stage.stageNumber = s;
                        stage.recommendedLevel = number * 3 + s;
                        stage.enemy = enemy;
                        stage.dropTable = drop;
                        stage.baseGoldReward = 25 + number * 10 + s * 5;
                        stage.baseExpReward = 45 + number * 18 + s * 10;
                        stage.isBossStage = boss;
                        stage.replayable = true;
                        stage.battleBackgroundAssetId = string.Empty;
                        if (stage.requiredCompletedStageIds == null) stage.requiredCompletedStageIds = new List<string>();
                        EditorUtility.SetDirty(stage);
                    }
                    if (realm.stages == null) realm.stages = new List<StageDefinition>();
                    if (!realm.stages.Contains(stage)) realm.stages.Add(stage);
                }
            }

            RebuildDatabase();
            AssetDatabase.SaveAssets();
            Debug.Log("[Content] 12 realm skeleton content created/updated.");
        }

        private void OnEnable()
        {
            RefreshDatabase();
        }

        private void OnGUI()
        {
            RefreshDatabase();
            tabIndex = GUILayout.Toolbar(tabIndex, tabs);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            switch (tabIndex)
            {
                case 0: DrawRealmsTab(); break;
                case 1: DrawStagesTab(); break;
                case 2: DrawEnemiesTab(); break;
                case 3: DrawDropTablesTab(); break;
                case 4: DrawSkillsTab(); break;
                case 5: DrawEquipmentTab(); break;
                case 6: DrawShopsTab(); break;
                case 7: DrawIapProductsTab(); break;
                case 8: DrawValidateTab(); break;
                case 9: DrawExportImportTab(); break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawRealmsTab()
        {
            EditorGUILayout.LabelField("Realm Editor", EditorStyles.boldLabel);
            DrawCreateRow("New Realm ID", ref newRealmId, CreateRealm);
            DrawAssetList(database.realms, selectedRealm, asset => selectedRealm = asset);
            if (selectedRealm == null) return;
            DrawRealmFields(selectedRealm);
            DrawValidationBox(ValidateRealm(selectedRealm));
            DrawSavePing(selectedRealm);
            if (GUILayout.Button("Sort Realms By Order"))
            {
                database.realms = database.realms.Where(r => r != null).OrderBy(r => r.order).ToList();
                SaveAsset(database);
            }
        }

        private void DrawStagesTab()
        {
            EditorGUILayout.LabelField("Stage Editor", EditorStyles.boldLabel);
            stageRealmFilter = EditorGUILayout.TextField("Filter realmId", stageRealmFilter);
            DrawCreateRow("New Stage ID", ref newStageId, CreateStage);
            List<StageDefinition> stages = string.IsNullOrEmpty(stageRealmFilter) ? database.stages : database.stages.Where(s => s != null && s.realmId.Contains(stageRealmFilter)).ToList();
            DrawAssetList(stages, selectedStage, asset => selectedStage = asset);
            if (selectedStage == null) return;
            if (GUILayout.Button("Duplicate Selected Stage")) DuplicateAsset(selectedStage, StagePath, copy =>
            {
                copy.id = UniqueId(selectedStage.id + "_copy", database.stages.Select(s => s != null ? s.id : string.Empty));
                copy.displayName = selectedStage.displayName + " Copy";
                selectedStage = copy;
                AddMissing(database.stages, copy);
                AddStageToRealm(copy);
            });
            DrawStageFields(selectedStage);
            DrawStagePreview(selectedStage);
            DrawValidationBox(ValidateStage(selectedStage));
            DrawSavePing(selectedStage);
            if (GUILayout.Button("Playtest Selected Stage")) PlaytestStage(selectedStage);
        }

        private void DrawEnemiesTab()
        {
            EditorGUILayout.LabelField("Enemy Editor", EditorStyles.boldLabel);
            DrawCreateRow("New Enemy ID", ref newEnemyId, CreateEnemy);
            DrawAssetList(database.enemies, selectedEnemy, asset => selectedEnemy = asset);
            if (selectedEnemy == null) return;
            if (GUILayout.Button("Duplicate Selected Enemy")) DuplicateAsset(selectedEnemy, EnemyPath, copy =>
            {
                copy.id = UniqueId(selectedEnemy.id + "_copy", database.enemies.Select(e => e != null ? e.id : string.Empty));
                copy.displayName = selectedEnemy.displayName + " Copy";
                selectedEnemy = copy;
                AddMissing(database.enemies, copy);
            });
            DrawEnemyFields(selectedEnemy);
            EditorGUILayout.HelpBox("Estimated Difficulty: " + EstimateDifficulty(selectedEnemy), MessageType.Info);
            DrawValidationBox(ValidateEnemy(selectedEnemy));
            DrawSavePing(selectedEnemy);
        }

        private void DrawDropTablesTab()
        {
            EditorGUILayout.LabelField("DropTable Editor", EditorStyles.boldLabel);
            DrawCreateRow("New Drop Table ID", ref newDropTableId, CreateDropTable);
            DrawAssetList(database.dropTables, selectedDropTable, asset => selectedDropTable = asset);
            if (selectedDropTable == null) return;
            if (GUILayout.Button("Duplicate Selected Drop Table")) DuplicateAsset(selectedDropTable, DropPath, copy =>
            {
                copy.id = UniqueId(selectedDropTable.id + "_copy", database.dropTables.Select(d => d != null ? d.id : string.Empty));
                selectedDropTable = copy;
                AddMissing(database.dropTables, copy);
            });
            DrawDropTableFields(selectedDropTable);
            DrawValidationBox(ValidateDropTable(selectedDropTable));
            DrawSavePing(selectedDropTable);
        }

        private void DrawSkillsTab()
        {
            EditorGUILayout.LabelField("Skill Editor", EditorStyles.boldLabel);
            skillClassFilter = EditorGUILayout.TextField("Filter classId", skillClassFilter);
            DrawCreateRow("New Skill ID", ref newSkillId, CreateSkill);
            List<SkillDefinition> skillSource = database.skills ?? new List<SkillDefinition>();
            List<SkillDefinition> skills = string.IsNullOrEmpty(skillClassFilter) ? skillSource : skillSource.Where(s => s != null && s.classId.Contains(skillClassFilter)).ToList();
            DrawAssetList(skills, selectedSkill, asset => selectedSkill = asset);
            if (selectedSkill == null) return;
            if (GUILayout.Button("Duplicate Selected Skill")) DuplicateAsset(selectedSkill, SkillPath, copy => { if (database.skills == null) database.skills = new List<SkillDefinition>(); copy.id = UniqueId(selectedSkill.id + "_copy", database.skills.Select(s => s != null ? s.id : string.Empty)); copy.displayName = selectedSkill.displayName + " Copy"; selectedSkill = copy; AddMissing(database.skills, copy); });
            DrawSkillFields(selectedSkill);
            DrawValidationBox(ValidateSkill(selectedSkill));
            DrawSavePing(selectedSkill);
        }

        private void DrawEquipmentTab()
        {
            EditorGUILayout.LabelField("Equipment Editor", EditorStyles.boldLabel);
            useEquipmentSlotFilter = EditorGUILayout.Toggle("Filter by slot", useEquipmentSlotFilter);
            if (useEquipmentSlotFilter) equipmentSlotFilter = (EquipmentSlot)EditorGUILayout.EnumPopup("Slot", equipmentSlotFilter);
            useEquipmentRarityFilter = EditorGUILayout.Toggle("Filter by rarity", useEquipmentRarityFilter);
            if (useEquipmentRarityFilter) equipmentRarityFilter = (EquipmentRarity)EditorGUILayout.EnumPopup("Rarity", equipmentRarityFilter);
            DrawCreateRow("New Equipment ID", ref newEquipmentId, CreateEquipment);
            List<EquipmentDefinition> source = database.equipmentDefinitions ?? new List<EquipmentDefinition>();
            List<EquipmentDefinition> equipment = source.Where(e => e != null && (!useEquipmentSlotFilter || e.slot == equipmentSlotFilter) && (!useEquipmentRarityFilter || e.rarity == equipmentRarityFilter)).ToList();
            DrawAssetList(equipment, selectedEquipment, asset => selectedEquipment = asset);
            if (selectedEquipment == null) return;
            if (GUILayout.Button("Duplicate Selected Equipment")) DuplicateAsset(selectedEquipment, EquipmentPath, copy => { if (database.equipmentDefinitions == null) database.equipmentDefinitions = new List<EquipmentDefinition>(); copy.id = UniqueId(selectedEquipment.id + "_copy", database.equipmentDefinitions.Select(e => e != null ? e.id : string.Empty)); copy.displayName = selectedEquipment.displayName + " Copy"; selectedEquipment = copy; AddMissing(database.equipmentDefinitions, copy); });
            DrawEquipmentFields(selectedEquipment);
            DrawValidationBox(ValidateEquipment(selectedEquipment));
            DrawSavePing(selectedEquipment);
        }

        private void DrawShopsTab()
        {
            EditorGUILayout.LabelField("Shop Editor", EditorStyles.boldLabel);
            useShopTypeFilter = EditorGUILayout.Toggle("Filter by ShopType", useShopTypeFilter);
            if (useShopTypeFilter) shopTypeFilter = (ShopType)EditorGUILayout.EnumPopup("ShopType", shopTypeFilter);
            DrawCreateRow("New Shop ID", ref newShopId, CreateShopAsset);
            List<ShopDefinition> source = database.shops ?? new List<ShopDefinition>();
            List<ShopDefinition> shops = useShopTypeFilter ? source.Where(s => s != null && s.shopType == shopTypeFilter).ToList() : source;
            DrawAssetList(shops, selectedShop, asset => selectedShop = asset);
            if (selectedShop == null) return;
            if (GUILayout.Button("Duplicate Selected Shop")) DuplicateAsset(selectedShop, ShopPath, copy => { if (database.shops == null) database.shops = new List<ShopDefinition>(); copy.id = UniqueId(selectedShop.id + "_copy", database.shops.Select(s => s != null ? s.id : string.Empty)); copy.displayName = selectedShop.displayName + " Copy"; selectedShop = copy; AddMissing(database.shops, copy); });
            DrawScriptableInspector(selectedShop);
            DrawValidationBox(ValidateShop(selectedShop));
            DrawSavePing(selectedShop);
        }

        private void DrawIapProductsTab()
        {
            EditorGUILayout.LabelField("IAP Product Editor", EditorStyles.boldLabel);
            DrawCreateRow("New Product ID", ref newIapProductId, CreateIapProductAsset);
            DrawAssetList(database.iapProducts ?? new List<IAPProductDefinition>(), selectedIapProduct, asset => selectedIapProduct = asset);
            if (selectedIapProduct == null) return;
            if (GUILayout.Button("Duplicate Selected Product")) DuplicateAsset(selectedIapProduct, EconomyPath, copy => { if (database.iapProducts == null) database.iapProducts = new List<IAPProductDefinition>(); copy.productId = UniqueId(selectedIapProduct.productId + "_copy", database.iapProducts.Select(p => p != null ? p.productId : string.Empty)); copy.displayName = selectedIapProduct.displayName + " Copy"; selectedIapProduct = copy; AddMissing(database.iapProducts, copy); });
            DrawScriptableInspector(selectedIapProduct);
            DrawValidationBox(ValidateIapProduct(selectedIapProduct));
            if (GUILayout.Button("Validate IAP Products")) validationReport = RunValidation(true);
            DrawSavePing(selectedIapProduct);
        }

        private void DrawValidateTab()
        {
            EditorGUILayout.LabelField("Content Validator", EditorStyles.boldLabel);
            if (GUILayout.Button("Run Validation")) validationReport = RunValidation(true);
            if (GUILayout.Button("Fix Safe Issues")) { FixSafeIssues(); validationReport = RunValidation(true); }
            EditorGUILayout.TextArea(validationReport, GUILayout.MinHeight(360));
        }

        private void DrawExportImportTab()
        {
            EditorGUILayout.LabelField("Export / Import JSON", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Path", ExportPath);
            if (GUILayout.Button("Export Content JSON")) ExportContent();
            if (GUILayout.Button("Import Content JSON"))
            {
                if (EditorUtility.DisplayDialog("Import Content", "Import content_export.json and update ScriptableObjects?", "Import", "Cancel")) ImportContent();
            }
        }

        private void DrawRealmFields(RealmDefinition realm)
        {
            realm.id = EditorGUILayout.TextField("id", realm.id);
            realm.displayName = EditorGUILayout.TextField("displayName", realm.displayName);
            realm.description = EditorGUILayout.TextField("description", realm.description);
            realm.order = EditorGUILayout.IntField("order", realm.order);
            realm.backgroundAssetId = EditorGUILayout.TextField("backgroundAssetId", realm.backgroundAssetId);
            EditorGUILayout.LabelField("Stages", EditorStyles.boldLabel);
            if (realm.stages == null) realm.stages = new List<StageDefinition>();
            DrawObjectList(realm.stages);
            StageDefinition addStage = (StageDefinition)EditorGUILayout.ObjectField("Add Stage", null, typeof(StageDefinition), false);
            if (addStage != null && !realm.stages.Contains(addStage)) realm.stages.Add(addStage);
        }

        private void DrawStageFields(StageDefinition stage)
        {
            stage.id = EditorGUILayout.TextField("id", stage.id);
            stage.realmId = EditorGUILayout.TextField("realmId", stage.realmId);
            stage.displayName = EditorGUILayout.TextField("displayName", stage.displayName);
            stage.description = EditorGUILayout.TextField("description", stage.description);
            stage.stageNumber = EditorGUILayout.IntField("stageNumber", stage.stageNumber);
            stage.recommendedLevel = EditorGUILayout.IntField("recommendedLevel", stage.recommendedLevel);
            stage.enemy = (EnemyDefinition)EditorGUILayout.ObjectField("enemy", stage.enemy, typeof(EnemyDefinition), false);
            stage.dropTable = (DropTableDefinition)EditorGUILayout.ObjectField("dropTable", stage.dropTable, typeof(DropTableDefinition), false);
            stage.baseGoldReward = EditorGUILayout.IntField("baseGoldReward", stage.baseGoldReward);
            stage.baseExpReward = EditorGUILayout.IntField("baseExpReward", stage.baseExpReward);
            if (stage.requiredCompletedStageIds == null) stage.requiredCompletedStageIds = new List<string>();
            DrawStringList("requiredCompletedStageIds", stage.requiredCompletedStageIds);
            stage.isBossStage = EditorGUILayout.Toggle("isBossStage", stage.isBossStage);
            stage.replayable = EditorGUILayout.Toggle("replayable", stage.replayable);
            stage.battleBackgroundAssetId = EditorGUILayout.TextField("battleBackgroundAssetId", stage.battleBackgroundAssetId);
        }

        private void DrawEnemyFields(EnemyDefinition enemy)
        {
            enemy.id = EditorGUILayout.TextField("id", enemy.id);
            enemy.displayName = EditorGUILayout.TextField("displayName", enemy.displayName);
            enemy.level = EditorGUILayout.IntField("level", enemy.level);
            enemy.maxHp = EditorGUILayout.IntField("maxHp", enemy.maxHp);
            enemy.attack = EditorGUILayout.IntField("attack", enemy.attack);
            enemy.defense = EditorGUILayout.IntField("defense", enemy.defense);
            enemy.maxMana = EditorGUILayout.IntField("maxMana", enemy.maxMana);
            enemy.spriteAssetId = EditorGUILayout.TextField("spriteAssetId", enemy.spriteAssetId);
            enemy.difficulty = (EnemyAIDifficulty)EditorGUILayout.EnumPopup("difficulty", enemy.difficulty);
        }

        private void DrawDropTableFields(DropTableDefinition table)
        {
            table.id = EditorGUILayout.TextField("id", table.id);
            if (table.drops == null) table.drops = new List<DropEntry>();
            for (int i = 0; i < table.drops.Count; i++)
            {
                DropEntry drop = table.drops[i];
                EditorGUILayout.BeginVertical("box");
                drop.isEquipment = EditorGUILayout.Toggle("isEquipment", drop.isEquipment);
                drop.itemId = EditorGUILayout.TextField("itemId", drop.itemId);
                drop.equipmentId = EditorGUILayout.TextField("equipmentId", drop.equipmentId);
                drop.minAmount = EditorGUILayout.IntField("minAmount", drop.minAmount);
                drop.maxAmount = EditorGUILayout.IntField("maxAmount", drop.maxAmount);
                drop.chance = EditorGUILayout.Slider("chance", drop.chance, 0f, 1f);
                if (GUILayout.Button("Remove Drop")) { table.drops.RemoveAt(i); i--; }
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Item Drop")) table.drops.Add(new DropEntry { itemId = "mat_slime_jelly", minAmount = 1, maxAmount = 1, chance = 1f });
            if (GUILayout.Button("Add Equipment Drop")) table.drops.Add(new DropEntry { isEquipment = true, equipmentId = "equip_weapon_wooden_sword", minAmount = 1, maxAmount = 1, chance = 0.1f });
            if (GUILayout.Button("Test Roll 10 Times")) TestRoll(table, 10);
            if (GUILayout.Button("Test Roll 100 Times")) TestRoll(table, 100);
        }

        private void DrawSkillFields(SkillDefinition skill)
        {
            skill.id = EditorGUILayout.TextField("id", skill.id);
            skill.displayName = EditorGUILayout.TextField("displayName", skill.displayName);
            skill.description = EditorGUILayout.TextField("description", skill.description);
            skill.classId = EditorGUILayout.TextField("classId", skill.classId);
            skill.iconAssetId = EditorGUILayout.TextField("iconAssetId", skill.iconAssetId);
            skill.slotType = (SkillSlotType)EditorGUILayout.EnumPopup("slotType", skill.slotType);
            skill.targetType = (SkillTargetType)EditorGUILayout.EnumPopup("targetType", skill.targetType);
            skill.activationType = (SkillActivationType)EditorGUILayout.EnumPopup("activationType", skill.activationType);
            skill.maxLevel = EditorGUILayout.IntField("maxLevel", skill.maxLevel);
            skill.baseManaCost = EditorGUILayout.IntField("baseManaCost", skill.baseManaCost);
            skill.baseCooldown = EditorGUILayout.IntField("baseCooldown", skill.baseCooldown);
            if (skill.levels == null) skill.levels = new List<SkillLevelData>();
            EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);
            for (int i = 0; i < skill.levels.Count; i++)
            {
                SkillLevelData level = skill.levels[i];
                EditorGUILayout.BeginVertical("box");
                level.level = EditorGUILayout.IntField("level", level.level);
                level.manaCost = EditorGUILayout.IntField("manaCost", level.manaCost);
                level.cooldown = EditorGUILayout.IntField("cooldown", level.cooldown);
                level.upgradeGoldCost = EditorGUILayout.IntField("upgradeGoldCost", level.upgradeGoldCost);
                level.requiredItemId = EditorGUILayout.TextField("requiredItemId", level.requiredItemId);
                level.requiredItemAmount = EditorGUILayout.IntField("requiredItemAmount", level.requiredItemAmount);
                level.descriptionOverride = EditorGUILayout.TextField("descriptionOverride", level.descriptionOverride);
                if (GUILayout.Button("Remove Level")) { skill.levels.RemoveAt(i); i--; }
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Level")) skill.levels.Add(new SkillLevelData { level = skill.levels.Count + 1 });
            if (skill.effects == null) skill.effects = new List<SkillEffectData>();
            EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
            for (int i = 0; i < skill.effects.Count; i++)
            {
                SkillEffectData effect = skill.effects[i];
                EditorGUILayout.BeginVertical("box");
                effect.effectType = (SkillEffectType)EditorGUILayout.EnumPopup("effectType", effect.effectType);
                effect.baseValue = EditorGUILayout.IntField("baseValue", effect.baseValue);
                effect.multiplier = EditorGUILayout.FloatField("multiplier", effect.multiplier);
                effect.tileCount = EditorGUILayout.IntField("tileCount", effect.tileCount);
                effect.areaSize = EditorGUILayout.IntField("areaSize", effect.areaSize);
                effect.fromTileType = (TileType)EditorGUILayout.EnumPopup("fromTileType", effect.fromTileType);
                effect.toTileType = (TileType)EditorGUILayout.EnumPopup("toTileType", effect.toTileType);
                effect.scalesWithLevel = EditorGUILayout.Toggle("scalesWithLevel", effect.scalesWithLevel);
                if (GUILayout.Button("Remove Effect")) { skill.effects.RemoveAt(i); i--; }
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Effect")) skill.effects.Add(new SkillEffectData());
        }

        private void DrawEquipmentFields(EquipmentDefinition equipment)
        {
            equipment.id = EditorGUILayout.TextField("id", equipment.id);
            equipment.displayName = EditorGUILayout.TextField("displayName", equipment.displayName);
            equipment.description = EditorGUILayout.TextField("description", equipment.description);
            equipment.iconAssetId = EditorGUILayout.TextField("iconAssetId", equipment.iconAssetId);
            equipment.slot = (EquipmentSlot)EditorGUILayout.EnumPopup("slot", equipment.slot);
            equipment.rarity = (EquipmentRarity)EditorGUILayout.EnumPopup("rarity", equipment.rarity);
            equipment.baseHp = EditorGUILayout.IntField("baseHp", equipment.baseHp);
            equipment.baseAtk = EditorGUILayout.IntField("baseAtk", equipment.baseAtk);
            equipment.baseMag = EditorGUILayout.IntField("baseMag", equipment.baseMag);
            equipment.baseDef = EditorGUILayout.IntField("baseDef", equipment.baseDef);
            equipment.baseSpd = EditorGUILayout.IntField("baseSpd", equipment.baseSpd);
            equipment.baseLuck = EditorGUILayout.IntField("baseLuck", equipment.baseLuck);
            equipment.maxLevel = EditorGUILayout.IntField("maxLevel", equipment.maxLevel);
            if (equipment.upgradeCosts == null) equipment.upgradeCosts = new List<EquipmentUpgradeCostData>();
            EditorGUILayout.LabelField("Upgrade Costs", EditorStyles.boldLabel);
            for (int i = 0; i < equipment.upgradeCosts.Count; i++)
            {
                EquipmentUpgradeCostData cost = equipment.upgradeCosts[i];
                EditorGUILayout.BeginVertical("box");
                cost.targetLevel = EditorGUILayout.IntField("targetLevel", cost.targetLevel);
                cost.goldCost = EditorGUILayout.IntField("goldCost", cost.goldCost);
                cost.materialItemId = EditorGUILayout.TextField("materialItemId", cost.materialItemId);
                cost.materialAmount = EditorGUILayout.IntField("materialAmount", cost.materialAmount);
                if (GUILayout.Button("Remove Cost")) { equipment.upgradeCosts.RemoveAt(i); i--; }
                EditorGUILayout.EndVertical();
            }
            if (GUILayout.Button("Add Cost")) equipment.upgradeCosts.Add(new EquipmentUpgradeCostData { targetLevel = equipment.upgradeCosts.Count + 2 });
        }

        private void DrawStagePreview(StageDefinition stage)
        {
            string realmName = database.GetRealmById(stage.realmId)?.displayName ?? "Missing Realm";
            string enemyName = stage.enemy != null ? stage.enemy.displayName : "Missing Enemy";
            int dropCount = stage.dropTable != null && stage.dropTable.drops != null ? stage.dropTable.drops.Count : 0;
            EditorGUILayout.HelpBox($"Stage: {stage.displayName}\nRealm: {realmName}\nEnemy: {enemyName}\nEnemy Lv/HP: {(stage.enemy != null ? stage.enemy.level.ToString() : "-")} / {(stage.enemy != null ? stage.enemy.maxHp.ToString() : "-")}\nGold: {stage.baseGoldReward}\nEXP: {stage.baseExpReward}\nDrops: {dropCount}\nRequires: {string.Join(", ", stage.requiredCompletedStageIds ?? new List<string>())}\n{(stage.isBossStage ? "Boss Stage" : "Normal Stage")} | {(stage.replayable ? "Replayable" : "No Replay")}", MessageType.Info);
        }

        private string EstimateDifficulty(EnemyDefinition enemy)
        {
            bool usedByBossStage = database.stages.Any(s => s != null && s.enemy == enemy && s.isBossStage);
            if (enemy.difficulty == EnemyAIDifficulty.Boss || usedByBossStage) return "Boss";
            if (enemy.maxHp <= 100 && enemy.attack <= 10) return "Easy";
            if (enemy.maxHp <= 180 && enemy.attack <= 18) return "Normal";
            if (enemy.maxHp <= 300 && enemy.attack <= 30) return "Hard";
            return "Boss";
        }

        private void PlaytestStage(StageDefinition stage)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Playtest Selected Stage", "Enter Play Mode first to playtest this stage.", "OK");
                return;
            }
            BattleUIController battle = Resources.FindObjectsOfTypeAll<BattleUIController>().FirstOrDefault();
            UIScreenManager ui = Resources.FindObjectsOfTypeAll<UIScreenManager>().FirstOrDefault();
            if (battle == null || ui == null) { Debug.LogWarning("[Content] Battle UI not found."); return; }
            battle.SetStage(stage);
            ui.ShowScreen(GameUIScreen.Battle);
        }

        private string RunValidation(bool log)
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            if (database == null) errors.Add("GameContentDatabase missing.");
            ValidateDatabase(errors, warnings);
            string report = $"Errors\n{(errors.Count == 0 ? "None" : string.Join("\n", errors))}\n\nWarnings\n{(warnings.Count == 0 ? "None" : string.Join("\n", warnings))}\n\nSummary\nRealms: {database?.realms.Count ?? 0}\nStages: {database?.stages.Count ?? 0}\nEnemies: {database?.enemies.Count ?? 0}\nDrop Tables: {database?.dropTables.Count ?? 0}\nSkills: {database?.skills.Count ?? 0}\nEquipment: {database?.equipmentDefinitions.Count ?? 0}";
            if (log)
            {
                if (errors.Count > 0) Debug.LogError("[Content Validation]\n" + report);
                else Debug.Log("[Content Validation]\n" + report);
            }
            return report;
        }

        private void ValidateDatabase(List<string> errors, List<string> warnings)
        {
            if (database == null) return;
            HashSet<string> realmIds = new HashSet<string>();
            HashSet<string> stageIds = new HashSet<string>();
            HashSet<string> enemyIds = new HashSet<string>();
            HashSet<string> dropIds = new HashSet<string>();
            foreach (RealmDefinition r in (database.realms ?? new List<RealmDefinition>()).Where(r => r != null)) { AddValidation(ValidateRealm(r), errors); if (!realmIds.Add(r.id)) errors.Add("Duplicate realm id: " + r.id); if (r.stages == null || r.stages.Count == 0) errors.Add("Realm has no stages: " + r.id); }
            foreach (StageDefinition s in (database.stages ?? new List<StageDefinition>()).Where(s => s != null)) { AddValidation(ValidateStage(s), errors); if (!stageIds.Add(s.id)) errors.Add("Duplicate stage id: " + s.id); if (!realmIds.Contains(s.realmId)) errors.Add("Stage realmId missing: " + s.id); }
            foreach (EnemyDefinition e in (database.enemies ?? new List<EnemyDefinition>()).Where(e => e != null)) { AddValidation(ValidateEnemy(e), errors); if (!enemyIds.Add(e.id)) errors.Add("Duplicate enemy id: " + e.id); }
            foreach (DropTableDefinition d in (database.dropTables ?? new List<DropTableDefinition>()).Where(d => d != null)) { AddValidation(ValidateDropTable(d), errors); if (!dropIds.Add(d.id)) errors.Add("Duplicate drop table id: " + d.id); }
            HashSet<string> skillIds = new HashSet<string>();
            foreach (SkillDefinition s in (database.skills ?? new List<SkillDefinition>()).Where(s => s != null)) { AddValidation(ValidateSkill(s), errors); if (!skillIds.Add(s.id)) errors.Add("Duplicate skill id: " + s.id); }
            HashSet<string> equipmentIds = new HashSet<string>();
            foreach (EquipmentDefinition e in (database.equipmentDefinitions ?? new List<EquipmentDefinition>()).Where(e => e != null)) { AddValidation(ValidateEquipment(e), errors); if (!equipmentIds.Add(e.id)) errors.Add("Duplicate equipment id: " + e.id); }
            Isekai12Realms.Editor.EconomyValidator.Validate(database, errors);
            foreach (string defaultSkill in new[] { "skill_flame_spark_slash", "skill_flame_shuffle_bell", "skill_flame_realm_burst", "skill_tide_aqua_heal", "skill_tide_bubble_guard", "skill_tide_moon_tide", "skill_storm_quick_jab", "skill_storm_static_step", "skill_storm_thunder_chain" }) if (!skillIds.Contains(defaultSkill)) errors.Add("Default skill missing: " + defaultSkill);
            foreach (StageDefinition s in (database.stages ?? new List<StageDefinition>()).Where(s => s != null && s.requiredCompletedStageIds != null)) foreach (string req in s.requiredCompletedStageIds) if (!stageIds.Contains(req)) errors.Add($"{s.id} requires missing stage {req}");
            foreach (DropTableDefinition d in (database.dropTables ?? new List<DropTableDefinition>()).Where(d => d != null)) foreach (DropEntry drop in d.drops ?? new List<DropEntry>()) if (drop.isEquipment && !equipmentIds.Contains(drop.equipmentId)) errors.Add($"Drop table {d.id} references missing equipment {drop.equipmentId}");
            if (!(database.stages ?? new List<StageDefinition>()).Any(s => s != null && (s.requiredCompletedStageIds == null || s.requiredCompletedStageIds.Count == 0))) errors.Add("World progression has no unlocked first stage.");
        }

        private void FixSafeIssues()
        {
            database.realms = DistinctById(database.realms.Where(x => x != null).OrderBy(x => x.order).ToList(), x => x.id);
            database.stages = DistinctById(database.stages.Where(x => x != null).OrderBy(x => x.realmId).ThenBy(x => x.stageNumber).ToList(), x => x.id);
            database.enemies = DistinctById(database.enemies.Where(x => x != null).ToList(), x => x.id);
            database.dropTables = DistinctById(database.dropTables.Where(x => x != null).ToList(), x => x.id);
            database.skills = DistinctById((database.skills ?? new List<SkillDefinition>()).Where(x => x != null).ToList(), x => x.id);
            database.shops = DistinctById((database.shops ?? new List<ShopDefinition>()).Where(x => x != null).ToList(), x => x.id);
            database.iapProducts = DistinctById((database.iapProducts ?? new List<IAPProductDefinition>()).Where(x => x != null).ToList(), x => x.productId);
            SaveAsset(database);
        }

        private void ExportContent()
        {
            ContentExportDto dto = new ContentExportDto
            {
                realms = database.realms.Where(x => x != null).Select(RealmDto.From).ToList(),
                stages = database.stages.Where(x => x != null).Select(StageDto.From).ToList(),
                enemies = database.enemies.Where(x => x != null).Select(EnemyDto.From).ToList(),
                dropTables = database.dropTables.Where(x => x != null).Select(DropTableDto.From).ToList()
            };
            Directory.CreateDirectory(Path.GetDirectoryName(ExportPath));
            File.WriteAllText(ExportPath, JsonUtility.ToJson(dto, true));
            AssetDatabase.Refresh();
            Debug.Log("[Content] Exported JSON: " + ExportPath);
        }

        private void ImportContent()
        {
            if (!File.Exists(ExportPath)) { Debug.LogError("[Content] Missing export file: " + ExportPath); return; }
            ContentExportDto dto = JsonUtility.FromJson<ContentExportDto>(File.ReadAllText(ExportPath));
            if (dto == null) { Debug.LogError("[Content] Could not parse export file: " + ExportPath); return; }
            Dictionary<string, EnemyDefinition> enemies = new Dictionary<string, EnemyDefinition>();
            Dictionary<string, DropTableDefinition> drops = new Dictionary<string, DropTableDefinition>();
            foreach (EnemyDto e in dto.enemies ?? new List<EnemyDto>()) { if (!ValidId(e.id)) { Debug.LogWarning("[Content] Skipped enemy with invalid id: " + e.id); continue; } EnemyDefinition asset = FindEnemyById(e.id) ?? LoadOrCreate<EnemyDefinition>($"{EnemyPath}/{e.id}.asset"); e.Apply(asset); enemies[e.id] = asset; SaveAsset(asset); }
            foreach (DropTableDto d in dto.dropTables ?? new List<DropTableDto>()) { if (!ValidId(d.id)) { Debug.LogWarning("[Content] Skipped drop table with invalid id: " + d.id); continue; } DropTableDefinition asset = FindDropTableById(d.id) ?? LoadOrCreate<DropTableDefinition>($"{DropPath}/{d.id}.asset"); d.Apply(asset); drops[d.id] = asset; SaveAsset(asset); }
            Dictionary<string, StageDefinition> stages = new Dictionary<string, StageDefinition>();
            foreach (StageDto s in dto.stages ?? new List<StageDto>()) { if (!ValidId(s.id)) { Debug.LogWarning("[Content] Skipped stage with invalid id: " + s.id); continue; } StageDefinition asset = FindStageById(s.id) ?? LoadOrCreate<StageDefinition>($"{StagePath}/{s.id}.asset"); s.Apply(asset, enemies, drops); stages[s.id] = asset; SaveAsset(asset); }
            foreach (RealmDto r in dto.realms ?? new List<RealmDto>()) { if (!ValidId(r.id)) { Debug.LogWarning("[Content] Skipped realm with invalid id: " + r.id); continue; } RealmDefinition asset = FindRealmById(r.id) ?? LoadOrCreate<RealmDefinition>($"{RealmPath}/{r.id}.asset"); r.Apply(asset, stages); SaveAsset(asset); }
            RebuildDatabase();
            Debug.Log("[Content] Imported JSON content.");
        }

        private void CreateRealm()
        {
            if (!CanCreate(newRealmId)) return;
            selectedRealm = FindRealmById(newRealmId);
            if (selectedRealm == null)
            {
                selectedRealm = CreateAsset<RealmDefinition>(RealmPath, newRealmId);
                selectedRealm.id = newRealmId;
                selectedRealm.order = Mathf.Max(1, database.realms.Count + 1);
                selectedRealm.displayName = newRealmId;
            }
            AddMissing(database.realms, selectedRealm);
            SaveAsset(selectedRealm);
            SaveAsset(database);
        }

        private void CreateStage()
        {
            if (!CanCreate(newStageId)) return;
            selectedStage = FindStageById(newStageId);
            if (selectedStage == null)
            {
                selectedStage = CreateAsset<StageDefinition>(StagePath, newStageId);
                selectedStage.id = newStageId;
                selectedStage.stageNumber = 1;
                selectedStage.recommendedLevel = 1;
                selectedStage.displayName = newStageId;
                selectedStage.realmId = stageRealmFilter;
            }
            AddMissing(database.stages, selectedStage);
            AddStageToRealm(selectedStage);
            SaveAsset(selectedStage);
            SaveAsset(database);
        }

        private void CreateEnemy()
        {
            if (!CanCreate(newEnemyId)) return;
            selectedEnemy = FindEnemyById(newEnemyId);
            if (selectedEnemy == null)
            {
                selectedEnemy = CreateAsset<EnemyDefinition>(EnemyPath, newEnemyId);
                selectedEnemy.id = newEnemyId;
                selectedEnemy.displayName = newEnemyId;
                selectedEnemy.level = 1;
                selectedEnemy.maxHp = 80;
                selectedEnemy.attack = 8;
                selectedEnemy.maxMana = 100;
            }
            AddMissing(database.enemies, selectedEnemy);
            SaveAsset(selectedEnemy);
            SaveAsset(database);
        }

        private void CreateDropTable()
        {
            if (!CanCreate(newDropTableId)) return;
            selectedDropTable = FindDropTableById(newDropTableId);
            if (selectedDropTable == null)
            {
                selectedDropTable = CreateAsset<DropTableDefinition>(DropPath, newDropTableId);
                selectedDropTable.id = newDropTableId;
            }
            AddMissing(database.dropTables, selectedDropTable);
            SaveAsset(selectedDropTable);
            SaveAsset(database);
        }

        private void CreateSkill()
        {
            if (!CanCreate(newSkillId)) return;
            selectedSkill = FindSkillById(newSkillId);
            if (selectedSkill == null)
            {
                selectedSkill = CreateAsset<SkillDefinition>(SkillPath, newSkillId);
                selectedSkill.id = newSkillId;
                selectedSkill.displayName = newSkillId;
                selectedSkill.classId = skillClassFilter;
                selectedSkill.maxLevel = 1;
            }
            AddMissing(database.skills, selectedSkill);
            SaveAsset(selectedSkill);
            SaveAsset(database);
        }

        private void CreateEquipment()
        {
            if (!CanCreate(newEquipmentId)) return;
            selectedEquipment = FindEquipmentById(newEquipmentId);
            if (selectedEquipment == null)
            {
                selectedEquipment = CreateAsset<EquipmentDefinition>(EquipmentPath, newEquipmentId);
                selectedEquipment.id = newEquipmentId;
                selectedEquipment.displayName = newEquipmentId;
                selectedEquipment.iconAssetId = newEquipmentId;
                selectedEquipment.maxLevel = 1;
            }
            AddMissing(database.equipmentDefinitions, selectedEquipment);
            SaveAsset(selectedEquipment);
            SaveAsset(database);
        }

        private void CreateShopAsset()
        {
            if (!CanCreate(newShopId)) return;
            selectedShop = FindShopById(newShopId);
            if (selectedShop == null)
            {
                selectedShop = CreateAsset<ShopDefinition>(ShopPath, newShopId);
                selectedShop.id = newShopId;
                selectedShop.displayName = newShopId;
                selectedShop.shopType = shopTypeFilter;
                selectedShop.items = new List<ShopItemDefinition>();
            }
            if (database.shops == null) database.shops = new List<ShopDefinition>();
            AddMissing(database.shops, selectedShop);
            SaveAsset(selectedShop);
            SaveAsset(database);
        }

        private void CreateIapProductAsset()
        {
            if (!CanCreate(newIapProductId)) return;
            selectedIapProduct = FindIapProductById(newIapProductId);
            if (selectedIapProduct == null)
            {
                selectedIapProduct = CreateAsset<IAPProductDefinition>(EconomyPath, newIapProductId);
                selectedIapProduct.productId = newIapProductId;
                selectedIapProduct.displayName = newIapProductId;
                selectedIapProduct.platformProductId = newIapProductId;
                selectedIapProduct.priceTextPlaceholder = "$0.99";
                selectedIapProduct.enabled = true;
            }
            if (database.iapProducts == null) database.iapProducts = new List<IAPProductDefinition>();
            AddMissing(database.iapProducts, selectedIapProduct);
            SaveAsset(selectedIapProduct);
            SaveAsset(database);
        }

        private void AddStageToRealm(StageDefinition stage) { RealmDefinition realm = database.GetRealmById(stage.realmId); if (realm == null) return; if (realm.stages == null) realm.stages = new List<StageDefinition>(); if (!realm.stages.Contains(stage)) { realm.stages.Add(stage); SaveAsset(realm); } }
        private void RefreshDatabase() { database = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath) ?? RebuildDatabase(); if (database.skills == null) database.skills = new List<SkillDefinition>(); if (database.equipmentDefinitions == null) database.equipmentDefinitions = new List<EquipmentDefinition>(); if (database.shops == null) database.shops = new List<ShopDefinition>(); if (database.iapProducts == null) database.iapProducts = new List<IAPProductDefinition>(); if (database.contentPacks == null) database.contentPacks = new List<ContentPackDefinition>(); }
        private static void EnsureFolders() { foreach (string p in new[] { Root, RealmPath, StagePath, EnemyPath, DropPath, SkillPath, EquipmentPath, ShopPath, EconomyPath }) if (!Directory.Exists(p)) Directory.CreateDirectory(p); }

        private static GameContentDatabase RebuildDatabase()
        {
            EnsureFolders();
            GameContentDatabase db = LoadOrCreate<GameContentDatabase>(DatabasePath);
            db.realms = FindAssets<RealmDefinition>().OrderBy(r => r.order).ToList();
            db.stages = FindAssets<StageDefinition>().OrderBy(s => s.realmId).ThenBy(s => s.stageNumber).ToList();
            db.enemies = FindAssets<EnemyDefinition>().ToList();
            db.dropTables = FindAssets<DropTableDefinition>().ToList();
            db.skills = FindAssets<SkillDefinition>().ToList();
            db.equipmentDefinitions = FindAssets<EquipmentDefinition>().ToList();
            db.craftingRecipes = FindAssets<CraftingRecipeDefinition>().ToList();
            db.quests = FindAssets<QuestDefinition>().ToList();
            db.tutorials = FindAssets<TutorialDefinition>().ToList();
            db.shops = FindAssets<ShopDefinition>().ToList();
            db.iapProducts = FindAssets<IAPProductDefinition>().ToList();
            db.contentPacks = FindAssets<ContentPackDefinition>().ToList();
            EditorUtility.SetDirty(db); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); return db;
        }

        private static List<T> FindAssets<T>() where T : UnityEngine.Object => AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).Where(x => x != null).ToList();
        private static RealmDefinition FindRealmById(string id) => FindAssets<RealmDefinition>().FirstOrDefault(r => r.id == id);
        private static StageDefinition FindStageById(string id) => FindAssets<StageDefinition>().FirstOrDefault(s => s.id == id);
        private static EnemyDefinition FindEnemyById(string id) => FindAssets<EnemyDefinition>().FirstOrDefault(e => e.id == id);
        private static DropTableDefinition FindDropTableById(string id) => FindAssets<DropTableDefinition>().FirstOrDefault(d => d.id == id);
        private static SkillDefinition FindSkillById(string id) => FindAssets<SkillDefinition>().FirstOrDefault(s => s.id == id);
        private static EquipmentDefinition FindEquipmentById(string id) => FindAssets<EquipmentDefinition>().FirstOrDefault(e => e.id == id);
        private static ShopDefinition FindShopById(string id) => FindAssets<ShopDefinition>().FirstOrDefault(s => s.id == id);
        private static IAPProductDefinition FindIapProductById(string id) => FindAssets<IAPProductDefinition>().FirstOrDefault(p => p.productId == id);
        private static T LoadOrCreate<T>(string path) where T : ScriptableObject { T asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) return asset; EnsureFolders(); asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset; }
        private static T CreateAsset<T>(string folder, string id) where T : ScriptableObject { EnsureFolders(); string path = $"{folder}/{id}.asset"; T asset = AssetDatabase.LoadAssetAtPath<T>(path); if (asset != null) return asset; asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset; }
        private static void SaveAsset(UnityEngine.Object asset) { if (asset == null) return; EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets(); }
        private static bool ValidId(string id) => !string.IsNullOrEmpty(id) && System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-z0-9_]+$");
        private static bool CanCreate(string id) { if (ValidId(id)) return true; EditorUtility.DisplayDialog("Invalid ID", "Use lowercase letters, numbers, and underscores only.", "OK"); return false; }
        private static void AddMissing<T>(List<T> list, T asset) where T : UnityEngine.Object { if (asset != null && !list.Contains(asset)) list.Add(asset); }
        private static string UniqueId(string baseId, IEnumerable<string> existingIds) { HashSet<string> existing = new HashSet<string>(existingIds.Where(id => !string.IsNullOrEmpty(id))); string id = baseId; int suffix = 2; while (existing.Contains(id)) id = baseId + "_" + suffix++; return id; }
        private static void AddValidation(List<string> src, List<string> dst) { foreach (string s in src) dst.Add(s); }
        private List<string> ValidateRealm(RealmDefinition r) { List<string> e = new List<string>(); if (!ValidId(r.id)) e.Add("Invalid realm id: " + r.id); if (string.IsNullOrEmpty(r.displayName)) e.Add("Realm displayName missing: " + r.id); if (r.order <= 0) e.Add("Realm order <= 0: " + r.id); if (database.realms.Count(x => x != null && x.id == r.id) > 1) e.Add("Duplicate realm id: " + r.id); return e; }
        private List<string> ValidateStage(StageDefinition s) { List<string> e = new List<string>(); if (!ValidId(s.id)) e.Add("Invalid stage id: " + s.id); if (string.IsNullOrEmpty(s.realmId)) e.Add("Stage realmId missing: " + s.id); if (s.enemy == null) e.Add("Stage enemy missing: " + s.id); if (s.dropTable == null) e.Add("Stage dropTable missing: " + s.id); if (s.recommendedLevel < 1) e.Add("Stage recommendedLevel < 1: " + s.id); if (s.baseGoldReward < 0) e.Add("Stage gold < 0: " + s.id); if (s.baseExpReward < 0) e.Add("Stage exp < 0: " + s.id); if (database.stages.Count(x => x != null && x.id == s.id) > 1) e.Add("Duplicate stage id: " + s.id); HashSet<string> stageIds = new HashSet<string>(database.stages.Where(x => x != null).Select(x => x.id)); foreach (string required in s.requiredCompletedStageIds ?? new List<string>()) if (!stageIds.Contains(required)) e.Add($"{s.id} requires missing stage {required}"); return e; }
        private List<string> ValidateEnemy(EnemyDefinition eDef) { List<string> e = new List<string>(); if (!ValidId(eDef.id)) e.Add("Invalid enemy id: " + eDef.id); if (string.IsNullOrEmpty(eDef.displayName)) e.Add("Enemy displayName missing: " + eDef.id); if (eDef.level < 1) e.Add("Enemy level < 1: " + eDef.id); if (eDef.maxHp <= 0) e.Add("Enemy maxHp <= 0: " + eDef.id); if (eDef.attack < 0) e.Add("Enemy attack < 0: " + eDef.id); if (eDef.defense < 0) e.Add("Enemy defense < 0: " + eDef.id); if (database.enemies.Count(x => x != null && x.id == eDef.id) > 1) e.Add("Duplicate enemy id: " + eDef.id); return e; }
        private List<string> ValidateDropTable(DropTableDefinition d) { List<string> e = new List<string>(); if (!ValidId(d.id)) e.Add("Invalid drop table id: " + d.id); foreach (DropEntry drop in d.drops ?? new List<DropEntry>()) { if (drop.chance < 0 || drop.chance > 1) e.Add("Invalid chance in " + d.id); if (drop.minAmount < 1) e.Add("minAmount < 1 in " + d.id); if (drop.maxAmount < drop.minAmount) e.Add("maxAmount < minAmount in " + d.id); if (drop.isEquipment && string.IsNullOrEmpty(drop.equipmentId)) e.Add("Equipment drop missing equipmentId in " + d.id); if (!drop.isEquipment && string.IsNullOrEmpty(drop.itemId)) e.Add("Item drop missing itemId in " + d.id); } if (database.dropTables.Count(x => x != null && x.id == d.id) > 1) e.Add("Duplicate drop table id: " + d.id); return e; }
        private List<string> ValidateSkill(SkillDefinition s) { List<string> e = new List<string>(); if (!ValidId(s.id)) e.Add("Invalid skill id: " + s.id); if (string.IsNullOrEmpty(s.displayName)) e.Add("Skill displayName missing: " + s.id); if (string.IsNullOrEmpty(s.classId)) e.Add("Skill classId missing: " + s.id); if (s.maxLevel < 1) e.Add("Skill maxLevel < 1: " + s.id); if (s.activationType == SkillActivationType.Active && s.baseManaCost < 0) e.Add("Skill mana cost < 0: " + s.id); foreach (SkillLevelData l in s.levels ?? new List<SkillLevelData>()) if (l.level < 1 || l.level > s.maxLevel) e.Add("Skill level out of range: " + s.id); foreach (SkillEffectData effect in s.effects ?? new List<SkillEffectData>()) if (!Enum.IsDefined(typeof(SkillEffectType), effect.effectType)) e.Add("Skill effectType invalid: " + s.id); if ((database.skills ?? new List<SkillDefinition>()).Count(x => x != null && x.id == s.id) > 1) e.Add("Duplicate skill id: " + s.id); return e; }
        private List<string> ValidateEquipment(EquipmentDefinition eDef) { List<string> e = new List<string>(); if (!ValidId(eDef.id)) e.Add("Invalid equipment id: " + eDef.id); if (string.IsNullOrEmpty(eDef.displayName)) e.Add("Equipment displayName missing: " + eDef.id); if (eDef.maxLevel < 1) e.Add("Equipment maxLevel < 1: " + eDef.id); if (!Enum.IsDefined(typeof(EquipmentSlot), eDef.slot)) e.Add("Equipment slot invalid: " + eDef.id); if (!Enum.IsDefined(typeof(EquipmentRarity), eDef.rarity)) e.Add("Equipment rarity invalid: " + eDef.id); foreach (EquipmentUpgradeCostData c in eDef.upgradeCosts ?? new List<EquipmentUpgradeCostData>()) if (c.targetLevel < 2 || c.targetLevel > eDef.maxLevel) e.Add("Equipment upgrade targetLevel out of range: " + eDef.id); if ((database.equipmentDefinitions ?? new List<EquipmentDefinition>()).Count(x => x != null && x.id == eDef.id) > 1) e.Add("Duplicate equipment id: " + eDef.id); return e; }
        private List<string> ValidateShop(ShopDefinition shop) { List<string> e = new List<string>(); GameContentDatabase temp = ScriptableObject.CreateInstance<GameContentDatabase>(); temp.shops = new List<ShopDefinition> { shop }; temp.equipmentDefinitions = database.equipmentDefinitions; temp.iapProducts = database.iapProducts; Isekai12Realms.Editor.EconomyValidator.Validate(temp, e); DestroyImmediate(temp); return e; }
        private List<string> ValidateIapProduct(IAPProductDefinition product) { List<string> e = new List<string>(); GameContentDatabase temp = ScriptableObject.CreateInstance<GameContentDatabase>(); temp.iapProducts = new List<IAPProductDefinition> { product }; Isekai12Realms.Editor.EconomyValidator.Validate(temp, e); DestroyImmediate(temp); return e; }
        private static List<T> DistinctById<T>(List<T> list, Func<T, string> id) { HashSet<string> seen = new HashSet<string>(); return list.Where(x => seen.Add(id(x))).ToList(); }
        private void TestRoll(DropTableDefinition table, int count) { Dictionary<string, int> rewards = new Dictionary<string, int>(); for (int i = 0; i < count; i++) foreach (DropEntry d in table.drops ?? new List<DropEntry>()) if (UnityEngine.Random.value <= d.chance) { string key = d.isEquipment ? d.equipmentId : d.itemId; int amount = d.isEquipment ? 1 : UnityEngine.Random.Range(d.minAmount, d.maxAmount + 1); rewards[key] = rewards.ContainsKey(key) ? rewards[key] + amount : amount; } Debug.Log($"[DropTest] {table.id} x{count}\n" + string.Join("\n", rewards.Select(kv => kv.Key + ": " + kv.Value))); }
        private void DrawCreateRow(string label, ref string id, Action create) { EditorGUILayout.BeginHorizontal(); id = EditorGUILayout.TextField(label, id); if (GUILayout.Button("Create", GUILayout.Width(90))) create(); EditorGUILayout.EndHorizontal(); }
        private void DrawSavePing(UnityEngine.Object asset) { EditorGUILayout.BeginHorizontal(); if (GUILayout.Button("Save Selected")) SaveAsset(asset); if (GUILayout.Button("Ping Asset")) EditorGUIUtility.PingObject(asset); EditorGUILayout.EndHorizontal(); }
        private void DrawValidationBox(List<string> errors) { if (errors.Count > 0) EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Error); }
        private void DrawObjectList<T>(List<T> list) where T : UnityEngine.Object { for (int i = list.Count - 1; i >= 0; i--) { EditorGUILayout.BeginHorizontal(); list[i] = (T)EditorGUILayout.ObjectField(list[i], typeof(T), false); if (GUILayout.Button("Remove", GUILayout.Width(80))) list.RemoveAt(i); EditorGUILayout.EndHorizontal(); } }
        private void DrawStringList(string label, List<string> list) { if (list == null) return; EditorGUILayout.LabelField(label, EditorStyles.boldLabel); for (int i = list.Count - 1; i >= 0; i--) { EditorGUILayout.BeginHorizontal(); list[i] = EditorGUILayout.TextField(list[i]); if (GUILayout.Button("Remove", GUILayout.Width(80))) list.RemoveAt(i); EditorGUILayout.EndHorizontal(); } if (GUILayout.Button("Add Requirement")) list.Add(string.Empty); }
        private void DrawAssetList<T>(List<T> assets, T selected, Action<T> select) where T : UnityEngine.Object { foreach (T asset in assets.Where(a => a != null)) { GUI.enabled = asset != selected; if (GUILayout.Button(asset.name)) select(asset); GUI.enabled = true; } }
        private void DrawScriptableInspector(UnityEngine.Object asset) { if (asset == null) return; UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(asset); if (editor != null) { editor.OnInspectorGUI(); DestroyImmediate(editor); } }
        private void DuplicateAsset<T>(T source, string folder, Action<T> onCreated) where T : ScriptableObject { string path = AssetDatabase.GetAssetPath(source); string newPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{source.name}_copy.asset"); if (!AssetDatabase.CopyAsset(path, newPath)) { Debug.LogError("[Content] Could not duplicate asset: " + path); return; } T copy = AssetDatabase.LoadAssetAtPath<T>(newPath); if (copy == null) { Debug.LogError("[Content] Could not load duplicated asset: " + newPath); return; } onCreated(copy); SaveAsset(copy); SaveAsset(database); }

        [Serializable] public class ContentExportDto { public List<RealmDto> realms; public List<StageDto> stages; public List<EnemyDto> enemies; public List<DropTableDto> dropTables; }
        [Serializable] public class RealmDto { public string id, displayName, description, backgroundAssetId; public int order; public List<string> stageIds; public static RealmDto From(RealmDefinition r) => new RealmDto { id = r.id, displayName = r.displayName, description = r.description, order = r.order, backgroundAssetId = r.backgroundAssetId, stageIds = (r.stages ?? new List<StageDefinition>()).Where(s => s != null).Select(s => s.id).ToList() }; public void Apply(RealmDefinition r, Dictionary<string, StageDefinition> stages) { r.id = id; r.displayName = displayName; r.description = description; r.order = order; r.backgroundAssetId = backgroundAssetId; r.stages = (stageIds ?? new List<string>()).Where(stages.ContainsKey).Select(x => stages[x]).ToList(); } }
        [Serializable] public class StageDto { public string id, realmId, displayName, description, enemyId, dropTableId, battleBackgroundAssetId; public int stageNumber, recommendedLevel, baseGoldReward, baseExpReward; public List<string> requiredCompletedStageIds; public bool isBossStage, replayable; public static StageDto From(StageDefinition s) => new StageDto { id = s.id, realmId = s.realmId, displayName = s.displayName, description = s.description, stageNumber = s.stageNumber, recommendedLevel = s.recommendedLevel, enemyId = s.enemy != null ? s.enemy.id : string.Empty, dropTableId = s.dropTable != null ? s.dropTable.id : string.Empty, baseGoldReward = s.baseGoldReward, baseExpReward = s.baseExpReward, requiredCompletedStageIds = s.requiredCompletedStageIds, isBossStage = s.isBossStage, replayable = s.replayable, battleBackgroundAssetId = s.battleBackgroundAssetId }; public void Apply(StageDefinition s, Dictionary<string, EnemyDefinition> enemies, Dictionary<string, DropTableDefinition> drops) { s.id = id; s.realmId = realmId; s.displayName = displayName; s.description = description; s.stageNumber = stageNumber; s.recommendedLevel = recommendedLevel; s.enemy = enemies.ContainsKey(enemyId) ? enemies[enemyId] : null; s.dropTable = drops.ContainsKey(dropTableId) ? drops[dropTableId] : null; s.baseGoldReward = baseGoldReward; s.baseExpReward = baseExpReward; s.requiredCompletedStageIds = requiredCompletedStageIds ?? new List<string>(); s.isBossStage = isBossStage; s.replayable = replayable; s.battleBackgroundAssetId = battleBackgroundAssetId; } }
        [Serializable] public class EnemyDto { public string id, displayName, spriteAssetId; public int level, maxHp, attack, defense, maxMana; public EnemyAIDifficulty difficulty; public static EnemyDto From(EnemyDefinition e) => new EnemyDto { id = e.id, displayName = e.displayName, level = e.level, maxHp = e.maxHp, attack = e.attack, defense = e.defense, maxMana = e.maxMana, spriteAssetId = e.spriteAssetId, difficulty = e.difficulty }; public void Apply(EnemyDefinition e) { e.id = id; e.displayName = displayName; e.level = level; e.maxHp = maxHp; e.attack = attack; e.defense = defense; e.maxMana = maxMana; e.spriteAssetId = spriteAssetId; e.difficulty = difficulty; } }
        [Serializable] public class DropTableDto { public string id; public List<DropEntryDto> drops; public static DropTableDto From(DropTableDefinition d) => new DropTableDto { id = d.id, drops = (d.drops ?? new List<DropEntry>()).Select(DropEntryDto.From).ToList() }; public void Apply(DropTableDefinition d) { d.id = id; d.drops = (drops ?? new List<DropEntryDto>()).Select(x => x.ToDropEntry()).ToList(); } }
        [Serializable] public class DropEntryDto { public string itemId, equipmentId; public int minAmount, maxAmount; public float chance; public bool isEquipment; public static DropEntryDto From(DropEntry d) => new DropEntryDto { itemId = d.itemId, equipmentId = d.equipmentId, minAmount = d.minAmount, maxAmount = d.maxAmount, chance = d.chance, isEquipment = d.isEquipment }; public DropEntry ToDropEntry() => new DropEntry { itemId = itemId, equipmentId = equipmentId, minAmount = minAmount, maxAmount = maxAmount, chance = chance, isEquipment = isEquipment }; }
    }
}
