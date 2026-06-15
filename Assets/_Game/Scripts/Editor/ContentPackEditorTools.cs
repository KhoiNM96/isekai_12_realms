using System.Collections.Generic;
using System.IO;
using System.Linq;
using Isekai12Realms.ContentPacks;
using Isekai12Realms.Data;
using Isekai12Realms.RemoteConfig;
using Isekai12Realms.Realms;
using Isekai12Realms.Stages;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class ContentPackEditorTools
    {
        private const string PackPath = "Assets/_Game/ScriptableObjects/ContentPacks";
        private const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";
        private const string ManifestPath = "Assets/_Game/ScriptableObjects/GameAssetManifest.asset";
        private const string ConfigPath = "Assets/_Game/ScriptableObjects/GameConfigData.asset";

        [MenuItem("Tools/Isekai 12 Realms/Addressables/Create Setup Guide")]
        public static void CreateAddressablesSetupGuide()
        {
            Directory.CreateDirectory("docs");
            File.WriteAllText("docs/addressables_setup.md", AddressablesGuide());
            AssetDatabase.Refresh();
            Debug.Log("[Docs] Addressables setup guide created.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Remote Config/Create Setup Guide")]
        public static void CreateRemoteConfigSetupGuide()
        {
            Directory.CreateDirectory("docs");
            File.WriteAllText("docs/remote_config_setup.md", RemoteConfigGuide());
            AssetDatabase.Refresh();
            Debug.Log("[Docs] Remote Config setup guide created.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Content Packs/Create Prototype Content Packs")]
        public static void CreatePrototypeContentPacks()
        {
            Directory.CreateDirectory(PackPath);
            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath);
            List<string> priorityAssets = manifest != null && manifest.entries != null
                ? manifest.entries.FindAll(e => e != null && e.priority <= 1).Select(e => e.id).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList()
                : new List<string> { "bg_title_sky_realm", "bg_town_meadow", "bg_world_map_scroll", "bg_battle_meadow", "char_hero_flame_idle", "enemy_meadow_slime", "currency_gold", "currency_soul_gem", "icon_token_sword", "icon_token_heart", "icon_token_coin" };

            ContentPackDefinition core = Pack("content_pack_core", "Core Content", "Required local MVP content.", ContentPackType.Core, true, true, false);
            core.assetIds = priorityAssets;
            core.realmIds = new List<string> { "realm_01_meadow" };

            ContentPackDefinition ember = Pack("content_pack_realm_02_ember", "Ember Realm Pack", "Realm 02 local MVP content, downloadable later.", ContentPackType.Realm, false, true, true);
            ember.realmIds = new List<string> { "realm_02_ember" };

            ContentPackDefinition tide = Pack("content_pack_realm_03_tide", "Tide Realm Pack", "Realm 03 local MVP content, downloadable later.", ContentPackType.Realm, false, true, true);
            tide.realmIds = new List<string> { "realm_03_tide" };

            ContentPackDefinition cosmetic = Pack("content_pack_cosmetic_meadow", "Meadow Cosmetic Pack", "Optional meadow cosmetics for future remote delivery.", ContentPackType.Cosmetic, false, false, true);
            cosmetic.cosmeticIds = new List<string> { "cosmetic_board_skin_meadow", "cosmetic_hero_aura_cyan" };
            cosmetic.estimatedSizeBytes = 2L * 1024L * 1024L;

            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath);
            if (db != null)
            {
                db.contentPacks = FindAssets<ContentPackDefinition>();
                EditorUtility.SetDirty(db);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ContentPacks] Prototype content packs created.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Create Prototype Content Packs")]
        public static void CreatePrototypeContentPacksRootMenu()
        {
            CreatePrototypeContentPacks();
        }

        [MenuItem("Tools/Isekai 12 Realms/Content Packs/Validate Content Packs")]
        public static void ValidateContentPacks()
        {
            GameContentDatabase db = AssetDatabase.LoadAssetAtPath<GameContentDatabase>(DatabasePath);
            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath);
            List<string> errors = new List<string>();
            List<ContentPackDefinition> packs = FindAssets<ContentPackDefinition>();
            HashSet<string> ids = new HashSet<string>();
            HashSet<string> realmIds = new HashSet<string>((db?.realms ?? new List<RealmDefinition>()).Where(r => r != null).Select(r => r.id));
            HashSet<string> stageIds = new HashSet<string>((db?.stages ?? new List<StageDefinition>()).Where(s => s != null).Select(s => s.id));

            foreach (ContentPackDefinition pack in packs)
            {
                if (pack == null) continue;
                if (string.IsNullOrEmpty(pack.id)) errors.Add("Pack missing id: " + pack.name);
                else if (!ids.Add(pack.id)) errors.Add("Duplicate pack id: " + pack.id);
                if (string.IsNullOrEmpty(pack.displayName)) errors.Add("Pack missing displayName: " + pack.id);
                if (pack.required && !pack.includedInBuild) errors.Add("Required pack must be includedInBuild: " + pack.id);
                if (pack.downloadable && pack.required && !pack.includedInBuild) errors.Add("Downloadable required pack must be includedInBuild: " + pack.id);
                if (pack.estimatedSizeBytes < 0) errors.Add("estimatedSizeBytes < 0: " + pack.id);
                foreach (string assetId in pack.assetIds ?? new List<string>())
                {
                    if (manifest != null && !manifest.HasAsset(assetId)) Debug.LogWarning("[ContentPacks] Asset id not in manifest, assumed future Addressable: " + assetId);
                }
                foreach (string realmId in pack.realmIds ?? new List<string>()) if (!realmIds.Contains(realmId)) errors.Add($"Pack {pack.id} references missing realm {realmId}");
                foreach (string stageId in pack.stageIds ?? new List<string>()) if (!stageIds.Contains(stageId)) errors.Add($"Pack {pack.id} references missing stage {stageId}");
            }
            if (!packs.Exists(p => p != null && p.packType == ContentPackType.Core && p.required)) errors.Add("Required core pack missing.");
            if (errors.Count == 0) Debug.Log("[ContentPacks] Validation passed.");
            else Debug.LogError("[ContentPacks] Validation failed:\n" + string.Join("\n", errors));
        }

        [MenuItem("Tools/Isekai 12 Realms/Remote Config/Validate Defaults")]
        public static void ValidateRemoteConfigDefaults()
        {
            GameConfigData config = AssetDatabase.LoadAssetAtPath<GameConfigData>(ConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<GameConfigData>();
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
            }
            List<string> errors = new List<string>();
            if (config.dailyShopRefreshHour < 0 || config.dailyShopRefreshHour > 23) errors.Add("dailyShopRefreshHour must be between 0 and 23.");
            if (config.maxLevelCap < 1) errors.Add("maxLevelCap must be >= 1.");
            if (config.goldRewardMultiplier < 0f) errors.Add("goldRewardMultiplier must be >= 0.");
            if (config.expRewardMultiplier < 0f) errors.Add("expRewardMultiplier must be >= 0.");
            if (string.IsNullOrEmpty(config.currentContentVersion)) errors.Add("currentContentVersion must not be empty.");
            if (errors.Count == 0) Debug.Log("[RemoteConfig] Default validation passed.");
            else Debug.LogError("[RemoteConfig] Default validation failed:\n" + string.Join("\n", errors));
        }

        private static ContentPackDefinition Pack(string id, string name, string description, ContentPackType type, bool required, bool included, bool downloadable)
        {
            ContentPackDefinition pack = AssetDatabase.LoadAssetAtPath<ContentPackDefinition>($"{PackPath}/{id}.asset");
            if (pack == null)
            {
                pack = ScriptableObject.CreateInstance<ContentPackDefinition>();
                AssetDatabase.CreateAsset(pack, $"{PackPath}/{id}.asset");
            }
            pack.id = id;
            pack.displayName = name;
            pack.description = description;
            pack.packType = type;
            pack.version = 1;
            pack.required = required;
            pack.includedInBuild = included;
            pack.downloadable = downloadable;
            pack.estimatedSizeBytes = included ? 0 : pack.estimatedSizeBytes;
            EditorUtility.SetDirty(pack);
            return pack;
        }

        private static List<T> FindAssets<T>() where T : Object
        {
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}").Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g))).Where(x => x != null).ToList();
        }

        private static string AddressablesGuide() => @"# Addressables Setup

Addressables are optional. The project compiles and plays without the package while `USE_ADDRESSABLES` is not defined.

## Steps
- Install the Unity Addressables package from Package Manager.
- Add `USE_ADDRESSABLES` to Player Settings Scripting Define Symbols only after the package imports cleanly.
- Create local and remote Addressables groups under `Assets/_Game/Addressables/Local/` and `Assets/_Game/Addressables/Remote/`.
- Mark asset addresses using the `assetId` from `GameAssetManifest` and content data.
- Build Addressables content before testing Addressables loads.
- Configure a remote load path later for optional realm, cosmetic, audio, or event packs.

## Fallback Rules
- Core content must always be included locally.
- Do not put core assets only in a remote group.
- Missing Addressables fall back to `GameAssetManifest.GetSprite(assetId)`.
- If remote catalogs fail or the player is offline, gameplay continues with local placeholder assets.
";

        private static string RemoteConfigGuide() => @"# Remote Config Setup

Firebase Remote Config is optional. The game uses local defaults while `USE_FIREBASE_REMOTE_CONFIG` is not defined.

## Steps
- Install Firebase Remote Config only after Firebase base packages are configured.
- Add `USE_FIREBASE_REMOTE_CONFIG` to Player Settings Scripting Define Symbols after the package imports cleanly.
- Keep game startup non-blocking. Remote config fetch failures must use local defaults.

## Recommended Keys
- `iap_enabled`
- `remote_content_enabled`
- `daily_shop_refresh_hour`
- `max_level_cap`
- `gold_reward_multiplier`
- `exp_reward_multiplier`
- `current_content_version`
- `minimum_app_version`
- `maintenance_message`
";
    }
}
