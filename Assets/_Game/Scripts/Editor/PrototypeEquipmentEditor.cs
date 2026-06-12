using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class PrototypeEquipmentEditor
    {
        private const string EquipmentPath = "Assets/_Game/ScriptableObjects/Equipment";
        private const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";

        [MenuItem("Tools/Isekai 12 Realms/Create Prototype Equipment")]
        public static void CreatePrototypeEquipment()
        {
            if (!Directory.Exists(EquipmentPath)) Directory.CreateDirectory(EquipmentPath);
            Equipment("equip_weapon_wooden_sword", "Wooden Sword", EquipmentSlot.Weapon, EquipmentRarity.Common, 0, 5, 0, 0, 0, 0, 5);
            Equipment("equip_weapon_flame_sword", "Flame Sword", EquipmentSlot.Weapon, EquipmentRarity.Rare, 0, 14, 4, 0, 0, 0, 12);
            Equipment("equip_weapon_tide_wand", "Tide Wand", EquipmentSlot.Weapon, EquipmentRarity.Rare, 0, 0, 16, 0, 0, 0, 12);
            Equipment("equip_weapon_storm_dagger", "Storm Dagger", EquipmentSlot.Weapon, EquipmentRarity.Rare, 0, 10, 0, 0, 5, 0, 12);
            Equipment("equip_armor_traveler_coat", "Traveler Coat", EquipmentSlot.Armor, EquipmentRarity.Common, 20, 0, 0, 3, 0, 0, 5);
            Equipment("equip_armor_leaf_vest", "Leaf Vest", EquipmentSlot.Armor, EquipmentRarity.Uncommon, 35, 0, 0, 5, 0, 0, 8);
            Equipment("equip_armor_crystal_mail", "Crystal Mail", EquipmentSlot.Armor, EquipmentRarity.Rare, 60, 0, 0, 10, 0, 0, 12);
            Equipment("equip_head_leaf_hood", "Leaf Hood", EquipmentSlot.Head, EquipmentRarity.Common, 10, 0, 2, 0, 0, 0, 5);
            Equipment("equip_boots_traveler", "Traveler Boots", EquipmentSlot.Boots, EquipmentRarity.Common, 0, 0, 0, 0, 2, 0, 5);
            Equipment("equip_ring_lucky", "Lucky Ring", EquipmentSlot.Ring, EquipmentRarity.Uncommon, 0, 0, 0, 0, 1, 3, 8);
            Equipment("equip_charm_realm", "Realm Charm", EquipmentSlot.Charm, EquipmentRarity.Rare, 25, 0, 5, 0, 0, 2, 12);
            RebuildEquipmentInDatabase();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Equipment] Prototype equipment created/updated.");
        }

        private static void Equipment(string id, string name, EquipmentSlot slot, EquipmentRarity rarity, int hp, int atk, int mag, int def, int spd, int luck, int maxLevel)
        {
            EquipmentDefinition equipment = LoadOrCreate<EquipmentDefinition>($"{EquipmentPath}/{id}.asset");
            equipment.id = id;
            equipment.displayName = name;
            equipment.description = name + " prototype equipment.";
            equipment.iconAssetId = id;
            equipment.slot = slot;
            equipment.rarity = rarity;
            equipment.baseHp = hp;
            equipment.baseAtk = atk;
            equipment.baseMag = mag;
            equipment.baseDef = def;
            equipment.baseSpd = spd;
            equipment.baseLuck = luck;
            equipment.maxLevel = maxLevel;
            equipment.upgradeCosts = BuildCosts(rarity, maxLevel);
            EditorUtility.SetDirty(equipment);
        }

        private static List<EquipmentUpgradeCostData> BuildCosts(EquipmentRarity rarity, int maxLevel)
        {
            List<EquipmentUpgradeCostData> costs = new List<EquipmentUpgradeCostData>();
            for (int level = 2; level <= maxLevel; level++)
            {
                int gold;
                int material;
                if (rarity == EquipmentRarity.Common)
                {
                    int[] golds = { 0, 0, 50, 100, 160, 250 };
                    int[] mats = { 0, 0, 1, 2, 3, 5 };
                    gold = golds[Mathf.Min(level, 5)];
                    material = mats[Mathf.Min(level, 5)];
                }
                else if (rarity == EquipmentRarity.Uncommon)
                {
                    gold = level == 2 ? 100 : level == 3 ? 180 : 280 + (level - 4) * 120;
                    material = level == 2 ? 2 : level == 3 ? 3 : 4 + (level - 4);
                }
                else
                {
                    gold = level == 2 ? 180 : level == 3 ? 300 : 480 + (level - 4) * 180;
                    material = level == 2 ? 3 : level == 3 ? 5 : 7 + (level - 4) * 2;
                }
                costs.Add(new EquipmentUpgradeCostData { targetLevel = level, goldCost = gold, materialItemId = "mat_slime_jelly", materialAmount = material });
            }
            return costs;
        }

        private static void RebuildEquipmentInDatabase()
        {
            GameContentDatabase db = LoadOrCreate<GameContentDatabase>(DatabasePath);
            db.equipmentDefinitions = FindAssets<EquipmentDefinition>();
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
