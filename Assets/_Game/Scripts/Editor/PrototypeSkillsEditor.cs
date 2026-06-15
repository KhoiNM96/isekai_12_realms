using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Data;
using Isekai12Realms.Skills;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class PrototypeSkillsEditor
    {
        private const string SkillsPath = "Assets/_Game/ScriptableObjects/Skills";
        private const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";

        [MenuItem("Tools/Isekai 12 Realms/Create Prototype Skills")]
        public static void CreatePrototypeSkills()
        {
            if (!Directory.Exists(SkillsPath)) Directory.CreateDirectory(SkillsPath);
            Skill("skill_flame_spark_slash", "Spark Slash", "flame_squire", SkillSlotType.Skill1, SkillTargetType.Enemy, "skill_flame_spark_slash", 30, 1, Effect(SkillEffectType.Damage, 20, true));
            Skill("skill_flame_shuffle_bell", "Shuffle Bell", "flame_squire", SkillSlotType.Skill2, SkillTargetType.Board, "skill_flame_shuffle_bell", 20, 3, Effect(SkillEffectType.ShuffleBoard));
            Skill("skill_flame_realm_burst", "Realm Burst", "flame_squire", SkillSlotType.Ultimate, SkillTargetType.Enemy, "skill_flame_realm_burst", 100, 0, Effect(SkillEffectType.Damage, 50, true), Area(3));
            Skill("skill_tide_aqua_heal", "Aqua Heal", "tide_acolyte", SkillSlotType.Skill1, SkillTargetType.Player, "skill_tide_aqua_heal", 25, 1, Effect(SkillEffectType.Heal, 28, true));
            Skill("skill_tide_bubble_guard", "Bubble Guard", "tide_acolyte", SkillSlotType.Skill2, SkillTargetType.Player, "skill_tide_bubble_guard", 30, 2, Effect(SkillEffectType.Shield, 35, true));
            Skill("skill_tide_moon_tide", "Moon Tide", "tide_acolyte", SkillSlotType.Ultimate, SkillTargetType.Player, "skill_tide_moon_tide", 100, 0, Effect(SkillEffectType.Heal, 70, true), Effect(SkillEffectType.Shield, 40, true), Effect(SkillEffectType.CleanseDebuff));
            Skill("skill_storm_quick_jab", "Quick Jab", "storm_scout", SkillSlotType.Skill1, SkillTargetType.Enemy, "skill_storm_quick_jab", 25, 1, Effect(SkillEffectType.Damage, 16, true));
            Skill("skill_storm_static_step", "Static Step", "storm_scout", SkillSlotType.Skill2, SkillTargetType.RandomTiles, "skill_storm_static_step", 35, 3, RandomTiles(5));
            Skill("skill_storm_thunder_chain", "Thunder Chain", "storm_scout", SkillSlotType.Ultimate, SkillTargetType.Enemy, "skill_storm_thunder_chain", 100, 0, Effect(SkillEffectType.Damage, 35, true), RandomTiles(8), Effect(SkillEffectType.ExtraTurn));

            RebuildSkillsInDatabase();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Skills] Prototype skills created/updated.");
        }

        private static SkillDefinition Skill(string id, string name, string classId, SkillSlotType slot, SkillTargetType target, string icon, int mana, int cooldown, params SkillEffectData[] effects)
        {
            SkillDefinition skill = LoadOrCreate<SkillDefinition>($"{SkillsPath}/{id}.asset");
            skill.id = id;
            skill.displayName = name;
            skill.description = name + " prototype skill.";
            skill.classId = classId;
            skill.iconAssetId = icon;
            skill.slotType = slot;
            skill.targetType = target;
            skill.activationType = SkillActivationType.Active;
            skill.maxLevel = 5;
            skill.baseManaCost = mana;
            skill.baseCooldown = cooldown;
            skill.levels = new List<SkillLevelData>
            {
                new SkillLevelData { level = 1, manaCost = mana, cooldown = cooldown, upgradeGoldCost = 0 },
                new SkillLevelData { level = 2, manaCost = mana, cooldown = cooldown, upgradeGoldCost = 100, requiredItemId = "item_skill_scroll", requiredItemAmount = 1 },
                new SkillLevelData { level = 3, manaCost = mana, cooldown = cooldown, upgradeGoldCost = 250, requiredItemId = "item_skill_scroll", requiredItemAmount = 2 },
                new SkillLevelData { level = 4, manaCost = mana, cooldown = cooldown, upgradeGoldCost = 500, requiredItemId = "item_skill_scroll", requiredItemAmount = 3 },
                new SkillLevelData { level = 5, manaCost = mana, cooldown = cooldown, upgradeGoldCost = 900, requiredItemId = "item_skill_scroll", requiredItemAmount = 5 }
            };
            skill.effects = new List<SkillEffectData>(effects);
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static SkillEffectData Effect(SkillEffectType type, int value = 0, bool scales = false) => new SkillEffectData { effectType = type, baseValue = value, multiplier = 1f, scalesWithLevel = scales };
        private static SkillEffectData Area(int size) => new SkillEffectData { effectType = SkillEffectType.DestroyArea, areaSize = size };
        private static SkillEffectData RandomTiles(int count) => new SkillEffectData { effectType = SkillEffectType.DestroyRandomTiles, tileCount = count };

        private static void RebuildSkillsInDatabase()
        {
            GameContentDatabase db = LoadOrCreate<GameContentDatabase>(DatabasePath);
            db.skills = FindAssets<SkillDefinition>();
            EditorUtility.SetDirty(db);
        }

        private static List<T> FindAssets<T>() where T : Object
        {
            List<T> result = new List<T>();
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) result.Add(asset);
            }
            return result;
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
