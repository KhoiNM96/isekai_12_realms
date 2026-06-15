using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Data;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Equipment;
using Isekai12Realms.Realms;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class PrototypeContentEditor
    {
        private const string RealmsPath = "Assets/_Game/ScriptableObjects/Realms";
        private const string StagesPath = "Assets/_Game/ScriptableObjects/Stages";
        private const string EnemiesPath = "Assets/_Game/ScriptableObjects/Enemies";
        private const string DropsPath = "Assets/_Game/ScriptableObjects/DropTables";
        private const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";

        [MenuItem("Tools/Isekai 12 Realms/Create Prototype Content")]
        public static void CreatePrototypeContent()
        {
            EnsureFolders();

            EnemyDefinition slime = Enemy("enemy_meadow_slime", "Meadow Slime", 1, 80, 8, 0, EnemyAIDifficulty.Easy);
            EnemyDefinition mushroom = Enemy("enemy_meadow_mushroom", "Meadow Mushroom", 2, 100, 10, 1, EnemyAIDifficulty.Easy);
            EnemyDefinition slimeKing = Enemy("boss_slime_king", "Slime King", 3, 160, 14, 2, EnemyAIDifficulty.Boss);
            EnemyDefinition piglet = Enemy("enemy_ember_piglet", "Ember Piglet", 4, 130, 13, 2, EnemyAIDifficulty.Normal);
            EnemyDefinition emberSprite = Enemy("enemy_ember_sprite", "Tiny Flame", 5, 150, 15, 2, EnemyAIDifficulty.Normal);
            EnemyDefinition cinderBoar = Enemy("boss_cinder_boar", "Cinder Boar", 6, 220, 20, 4, EnemyAIDifficulty.Boss);
            EnemyDefinition bubble = Enemy("enemy_tide_bubble", "Bubble Trouble", 7, 170, 17, 3, EnemyAIDifficulty.Normal);
            EnemyDefinition crab = Enemy("enemy_tide_crab", "Tide Crab", 8, 190, 19, 5, EnemyAIDifficulty.Normal);
            EnemyDefinition serpent = Enemy("boss_bubble_serpent", "Bubble Serpent", 9, 280, 24, 6, EnemyAIDifficulty.Boss);

            DropTableDefinition d0101 = Drop("drop_stage_01_01", new DropEntry { itemId = "mat_slime_jelly", minAmount = 1, maxAmount = 2, chance = 1f }, new DropEntry { itemId = "item_potion_small", minAmount = 1, maxAmount = 1, chance = 0.25f }, new DropEntry { equipmentId = "equip_weapon_wooden_sword", minAmount = 1, maxAmount = 1, chance = 0.15f, isEquipment = true });
            DropTableDefinition d0102 = Drop("drop_stage_01_02", new DropEntry { itemId = "mat_slime_jelly", minAmount = 2, maxAmount = 3, chance = 1f }, new DropEntry { itemId = "item_potion_small", minAmount = 1, maxAmount = 1, chance = 0.35f }, new DropEntry { equipmentId = "equip_armor_traveler_coat", minAmount = 1, maxAmount = 1, chance = 0.15f, isEquipment = true });
            DropTableDefinition d0103 = Drop("drop_stage_01_03", new DropEntry { itemId = "mat_slime_jelly", minAmount = 3, maxAmount = 5, chance = 1f }, new DropEntry { itemId = "item_skill_scroll", minAmount = 1, maxAmount = 1, chance = 0.25f }, new DropEntry { equipmentId = "equip_ring_lucky", minAmount = 1, maxAmount = 1, chance = 0.2f, isEquipment = true });
            DropTableDefinition d0201 = Drop("drop_stage_02_01", new DropEntry { itemId = "item_potion_small", minAmount = 1, maxAmount = 1, chance = 0.3f });
            DropTableDefinition d0202 = Drop("drop_stage_02_02", new DropEntry { itemId = "item_skill_scroll", minAmount = 1, maxAmount = 1, chance = 0.2f });
            DropTableDefinition d0203 = Drop("drop_stage_02_03", new DropEntry { equipmentId = "equip_armor_traveler_coat", minAmount = 1, maxAmount = 1, chance = 0.25f, isEquipment = true });
            DropTableDefinition d0301 = Drop("drop_stage_03_01", new DropEntry { itemId = "item_potion_small", minAmount = 1, maxAmount = 2, chance = 0.4f });
            DropTableDefinition d0302 = Drop("drop_stage_03_02", new DropEntry { itemId = "item_skill_scroll", minAmount = 1, maxAmount = 1, chance = 0.25f });
            DropTableDefinition d0303 = Drop("drop_stage_03_03", new DropEntry { equipmentId = "equip_ring_lucky", minAmount = 1, maxAmount = 1, chance = 0.3f, isEquipment = true });

            StageDefinition s0101 = Stage("stage_01_01", "realm_01_meadow", "First Slime", 1, 1, slime, d0101, 30, 50, false, new string[0]);
            StageDefinition s0102 = Stage("stage_01_02", "realm_01_meadow", "Mushroom Trouble", 2, 2, mushroom, d0102, 45, 70, false, "stage_01_01");
            StageDefinition s0103 = Stage("stage_01_03", "realm_01_meadow", "Slime King Trial", 3, 3, slimeKing, d0103, 100, 150, true, "stage_01_02");
            StageDefinition s0201 = Stage("stage_02_01", "realm_02_ember", "Ember Piglet", 1, 4, piglet, d0201, 60, 90, false, "stage_01_03");
            StageDefinition s0202 = Stage("stage_02_02", "realm_02_ember", "Tiny Flame", 2, 5, emberSprite, d0202, 75, 110, false, "stage_02_01");
            StageDefinition s0203 = Stage("stage_02_03", "realm_02_ember", "Cinder Boar Trial", 3, 6, cinderBoar, d0203, 140, 190, true, "stage_02_02");
            StageDefinition s0301 = Stage("stage_03_01", "realm_03_tide", "Bubble Trouble", 1, 7, bubble, d0301, 90, 130, false, "stage_02_03");
            StageDefinition s0302 = Stage("stage_03_02", "realm_03_tide", "Tide Crab", 2, 8, crab, d0302, 105, 150, false, "stage_03_01");
            StageDefinition s0303 = Stage("stage_03_03", "realm_03_tide", "Bubble Serpent Trial", 3, 9, serpent, d0303, 180, 250, true, "stage_03_02");

            RealmDefinition r01 = Realm("realm_01_meadow", "Meadow Gate", "A peaceful floating meadow where the reborn hero begins the journey.", 1, s0101, s0102, s0103);
            RealmDefinition r02 = Realm("realm_02_ember", "Ember Village", "A warm village surrounded by gentle flame spirits.", 2, s0201, s0202, s0203);
            RealmDefinition r03 = Realm("realm_03_tide", "Tide Shrine", "A calm shrine washed by healing moonlit water.", 3, s0301, s0302, s0303);

            GameContentDatabase db = LoadOrCreate<GameContentDatabase>(DatabasePath);
            db.realms = new List<RealmDefinition> { r01, r02, r03 };
            db.stages = new List<StageDefinition> { s0101, s0102, s0103, s0201, s0202, s0203, s0301, s0302, s0303 };
            db.enemies = new List<EnemyDefinition> { slime, mushroom, slimeKing, piglet, emberSprite, cinderBoar, bubble, crab, serpent };
            db.dropTables = new List<DropTableDefinition> { d0101, d0102, d0103, d0201, d0202, d0203, d0301, d0302, d0303 };

            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Content] Prototype content created/updated.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Validate Content")]
        public static void ValidateContent()
        {
            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath);
            if (db == null)
            {
                Debug.LogError("[Content] GameContentDatabase.asset missing. Run Create Prototype Content first.");
                return;
            }

            List<string> errors = new List<string>();
            HashSet<string> stageIds = new HashSet<string>();
            HashSet<string> realmIds = new HashSet<string>();
            foreach (RealmDefinition realm in db.realms ?? new List<RealmDefinition>())
            {
                if (realm == null || string.IsNullOrEmpty(realm.id)) errors.Add("Realm missing id.");
                else if (!realmIds.Add(realm.id)) errors.Add("Duplicate realm id: " + realm.id);
                if (realm != null && (realm.stages == null || realm.stages.Count == 0)) errors.Add("Realm has no stages: " + realm.id);
            }
            foreach (StageDefinition stage in db.stages ?? new List<StageDefinition>())
            {
                if (stage == null || string.IsNullOrEmpty(stage.id)) { errors.Add("Stage missing id."); continue; }
                if (!stageIds.Add(stage.id)) errors.Add("Duplicate stage id: " + stage.id);
                if (stage.enemy == null) errors.Add("Stage missing enemy: " + stage.id);
                if (stage.dropTable == null) errors.Add("Stage missing drop table: " + stage.id);
            }
            foreach (StageDefinition stage in db.stages ?? new List<StageDefinition>())
            {
                if (stage == null || stage.requiredCompletedStageIds == null) continue;
                foreach (string required in stage.requiredCompletedStageIds)
                {
                    if (!stageIds.Contains(required)) errors.Add($"Stage {stage.id} requires missing stage {required}");
                }
            }
            foreach (DropTableDefinition dropTable in db.dropTables ?? new List<DropTableDefinition>())
            {
                if (dropTable == null) continue;
                foreach (DropEntry drop in dropTable.drops)
                {
                    if (drop.isEquipment && string.IsNullOrEmpty(drop.equipmentId)) errors.Add("Drop entry missing equipmentId in " + dropTable.id);
                    if (!drop.isEquipment && string.IsNullOrEmpty(drop.itemId)) errors.Add("Drop entry missing itemId in " + dropTable.id);
                }
            }
            HashSet<string> skillIds = new HashSet<string>();
            HashSet<string> equipmentIds = new HashSet<string>();
            if (db.equipmentDefinitions != null)
            {
                foreach (EquipmentDefinition equipment in db.equipmentDefinitions)
                {
                    if (equipment == null || string.IsNullOrEmpty(equipment.id)) { errors.Add("Equipment missing id."); continue; }
                    if (!equipmentIds.Add(equipment.id)) errors.Add("Duplicate equipment id: " + equipment.id);
                    if (string.IsNullOrEmpty(equipment.displayName)) errors.Add("Equipment missing displayName: " + equipment.id);
                    if (equipment.maxLevel < 1) errors.Add("Equipment maxLevel < 1: " + equipment.id);
                    if (!System.Enum.IsDefined(typeof(EquipmentSlot), equipment.slot)) errors.Add("Equipment slot invalid: " + equipment.id);
                    if (!System.Enum.IsDefined(typeof(EquipmentRarity), equipment.rarity)) errors.Add("Equipment rarity invalid: " + equipment.id);
                    foreach (EquipmentUpgradeCostData cost in equipment.upgradeCosts ?? new List<EquipmentUpgradeCostData>())
                    {
                        if (cost.targetLevel < 2 || cost.targetLevel > equipment.maxLevel) errors.Add("Equipment upgrade targetLevel out of range: " + equipment.id);
                    }
                }
            }
            if (db.skills != null)
            {
                foreach (SkillDefinition skill in db.skills)
                {
                    if (skill == null || string.IsNullOrEmpty(skill.id)) { errors.Add("Skill missing id."); continue; }
                    if (!skillIds.Add(skill.id)) errors.Add("Duplicate skill id: " + skill.id);
                    if (string.IsNullOrEmpty(skill.displayName)) errors.Add("Skill missing displayName: " + skill.id);
                    if (string.IsNullOrEmpty(skill.classId)) errors.Add("Skill missing classId: " + skill.id);
                    if (skill.maxLevel < 1) errors.Add("Skill maxLevel < 1: " + skill.id);
                    if (skill.activationType == SkillActivationType.Active && skill.baseManaCost < 0) errors.Add("Skill mana cost < 0: " + skill.id);
                    foreach (SkillLevelData level in skill.levels ?? new List<SkillLevelData>())
                    {
                        if (level.level < 1 || level.level > skill.maxLevel) errors.Add("Skill level out of range: " + skill.id);
                    }
                    foreach (SkillEffectData effect in skill.effects ?? new List<SkillEffectData>())
                    {
                        if (!System.Enum.IsDefined(typeof(SkillEffectType), effect.effectType)) errors.Add("Skill effectType invalid: " + skill.id);
                    }
                }
            }
            foreach (string defaultSkill in new[] { "skill_flame_spark_slash", "skill_flame_shuffle_bell", "skill_flame_realm_burst", "skill_tide_aqua_heal", "skill_tide_bubble_guard", "skill_tide_moon_tide", "skill_storm_quick_jab", "skill_storm_static_step", "skill_storm_thunder_chain" })
            {
                if (!skillIds.Contains(defaultSkill)) errors.Add("Default skill missing: " + defaultSkill);
            }
            foreach (DropTableDefinition dropTable in db.dropTables ?? new List<DropTableDefinition>())
            {
                if (dropTable == null) continue;
                foreach (DropEntry drop in dropTable.drops ?? new List<DropEntry>())
                {
                    if (drop.isEquipment && !equipmentIds.Contains(drop.equipmentId)) errors.Add($"Drop table {dropTable.id} references missing equipment {drop.equipmentId}");
                }
            }
            EconomyValidator.Validate(db, errors);

            if (errors.Count == 0) Debug.Log("[Content] Validation passed.");
            else Debug.LogError("[Content] Validation failed:\n" + string.Join("\n", errors));
        }

        private static void EnsureFolders()
        {
            foreach (string path in new[] { RealmsPath, StagesPath, EnemiesPath, DropsPath })
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
        }

        private static EnemyDefinition Enemy(string id, string name, int level, int hp, int atk, int def, EnemyAIDifficulty difficulty)
        {
            EnemyDefinition enemy = LoadOrCreate<EnemyDefinition>($"{EnemiesPath}/{id}.asset");
            enemy.id = id; enemy.displayName = name; enemy.level = level; enemy.maxHp = hp; enemy.attack = atk; enemy.defense = def; enemy.maxMana = 100; enemy.spriteAssetId = id; enemy.difficulty = difficulty;
            EditorUtility.SetDirty(enemy);
            return enemy;
        }

        private static DropTableDefinition Drop(string id, params DropEntry[] entries)
        {
            DropTableDefinition table = LoadOrCreate<DropTableDefinition>($"{DropsPath}/{id}.asset");
            table.id = id; table.drops = new List<DropEntry>(entries);
            EditorUtility.SetDirty(table);
            return table;
        }

        private static StageDefinition Stage(string id, string realm, string name, int number, int level, EnemyDefinition enemy, DropTableDefinition drop, int gold, int exp, bool boss, params string[] required)
        {
            StageDefinition stage = LoadOrCreate<StageDefinition>($"{StagesPath}/{id}.asset");
            stage.id = id; stage.realmId = realm; stage.displayName = name; stage.description = name; stage.stageNumber = number; stage.recommendedLevel = level; stage.enemy = enemy; stage.dropTable = drop; stage.baseGoldReward = gold; stage.baseExpReward = exp; stage.requiredCompletedStageIds = new List<string>(required); stage.isBossStage = boss; stage.replayable = true; stage.battleBackgroundAssetId = "bg_battle_meadow";
            EditorUtility.SetDirty(stage);
            return stage;
        }

        private static RealmDefinition Realm(string id, string name, string description, int order, params StageDefinition[] stages)
        {
            RealmDefinition realm = LoadOrCreate<RealmDefinition>($"{RealmsPath}/{id}.asset");
            realm.id = id; realm.displayName = name; realm.description = description; realm.order = order; realm.backgroundAssetId = order == 1 ? "bg_world_map_scroll" : string.Empty; realm.stages = new List<StageDefinition>(stages);
            EditorUtility.SetDirty(realm);
            return realm;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
