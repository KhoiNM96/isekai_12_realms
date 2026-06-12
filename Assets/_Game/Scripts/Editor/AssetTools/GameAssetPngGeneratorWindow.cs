using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Isekai12Realms.Data;
using Isekai12Realms.UI;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor.AssetTools
{
    public class GameAssetPngGeneratorWindow : EditorWindow
    {
        private const string GeneratedRoot = "Assets/_Game/Art/Generated";
        private const string ManifestPath = "Assets/_Game/ScriptableObjects/AssetManifest/GameAssetManifest.asset";
        private const string MetaPath = GeneratedRoot + "/Meta";

        private static readonly string[] RequiredFolders =
        {
            "Backgrounds", "Characters", "Enemies", "NPCs", "Tokens", "Skills", "Equipment", "Items", "UI", "Tilesets", "VFX", "Maps", "Loading", "Meta"
        };

        private static readonly List<AssetSpec> Priority1Assets = new List<AssetSpec>
        {
            new AssetSpec("bg_title_sky_realm", "bg_title_sky_realm_1080x1920.png", "Backgrounds", 1080, 1920, GameAssetCategory.Background, false, "Title sky realm background"),
            new AssetSpec("bg_town_meadow", "bg_town_meadow_1080x1920.png", "Backgrounds", 1080, 1920, GameAssetCategory.Background, false, "Main town meadow background"),
            new AssetSpec("bg_world_map_scroll", "bg_world_map_scroll_1080x1920.png", "Backgrounds", 1080, 1920, GameAssetCategory.Background, false, "World map parchment background"),
            new AssetSpec("bg_battle_meadow", "bg_battle_meadow_1080x960.png", "Backgrounds", 1080, 960, GameAssetCategory.Background, false, "Battle meadow background"),

            new AssetSpec("char_hero_flame_idle", "char_hero_flame_idle_512x512.png", "Characters", 512, 512, GameAssetCategory.Character, true, "Flame hero idle placeholder"),
            new AssetSpec("char_hero_flame_attack", "char_hero_flame_attack_512x512.png", "Characters", 512, 512, GameAssetCategory.Character, true, "Flame hero attack placeholder"),
            new AssetSpec("char_hero_flame_cast", "char_hero_flame_cast_512x512.png", "Characters", 512, 512, GameAssetCategory.Character, true, "Flame hero cast placeholder"),
            new AssetSpec("char_hero_flame_hurt", "char_hero_flame_hurt_512x512.png", "Characters", 512, 512, GameAssetCategory.Character, true, "Flame hero hurt placeholder"),

            new AssetSpec("enemy_meadow_slime", "enemy_meadow_slime_512x512.png", "Enemies", 512, 512, GameAssetCategory.Enemy, true, "Meadow slime placeholder"),
            new AssetSpec("boss_slime_king", "boss_slime_king_768x768.png", "Enemies", 768, 768, GameAssetCategory.Enemy, true, "Slime king boss placeholder"),

            new AssetSpec("icon_token_sword", "icon_token_sword_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Sword token icon"),
            new AssetSpec("icon_token_heart", "icon_token_heart_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Heart token icon"),
            new AssetSpec("icon_token_coin", "icon_token_coin_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Coin token icon"),
            new AssetSpec("icon_token_food", "icon_token_food_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Food token icon"),
            new AssetSpec("icon_token_book", "icon_token_book_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Book token icon"),
            new AssetSpec("icon_token_mana", "icon_token_mana_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Mana token icon"),
            new AssetSpec("icon_token_shield", "icon_token_shield_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Shield token icon"),
            new AssetSpec("icon_token_star", "icon_token_star_128x128.png", "Tokens", 128, 128, GameAssetCategory.Token, true, "Star token icon"),

            new AssetSpec("ui_panel_main", "ui_panel_main_768x512.png", "UI", 768, 512, GameAssetCategory.UI, true, "Main UI panel"),
            new AssetSpec("ui_panel_popup", "ui_panel_popup_768x512.png", "UI", 768, 512, GameAssetCategory.UI, true, "Popup UI panel"),
            new AssetSpec("ui_btn_primary", "ui_btn_primary_384x128.png", "UI", 384, 128, GameAssetCategory.UI, true, "Primary button"),
            new AssetSpec("ui_btn_secondary", "ui_btn_secondary_384x128.png", "UI", 384, 128, GameAssetCategory.UI, true, "Secondary button"),
            new AssetSpec("ui_btn_close", "ui_btn_close_128x128.png", "UI", 128, 128, GameAssetCategory.UI, true, "Close button"),
            new AssetSpec("ui_bar_hp_bg", "ui_bar_hp_bg_512x64.png", "UI", 512, 64, GameAssetCategory.UI, true, "HP bar background"),
            new AssetSpec("ui_bar_hp_fill", "ui_bar_hp_fill_512x64.png", "UI", 512, 64, GameAssetCategory.UI, true, "HP bar fill"),
            new AssetSpec("ui_bar_mana_bg", "ui_bar_mana_bg_512x64.png", "UI", 512, 64, GameAssetCategory.UI, true, "Mana bar background"),
            new AssetSpec("ui_bar_mana_fill", "ui_bar_mana_fill_512x64.png", "UI", 512, 64, GameAssetCategory.UI, true, "Mana bar fill"),

            new AssetSpec("currency_gold", "currency_gold_128x128.png", "Items", 128, 128, GameAssetCategory.Currency, true, "Gold currency icon"),
            new AssetSpec("currency_soul_gem", "currency_soul_gem_128x128.png", "Items", 128, 128, GameAssetCategory.Currency, true, "Soul gem currency icon"),

            new AssetSpec("logo_game_main", "logo_game_main_768x384.png", "Loading", 768, 384, GameAssetCategory.Loading, true, "Game logo placeholder"),
            new AssetSpec("loading_bar_frame", "loading_bar_frame_768x96.png", "Loading", 768, 96, GameAssetCategory.Loading, true, "Loading bar frame"),
            new AssetSpec("loading_bar_fill", "loading_bar_fill_768x96.png", "Loading", 768, 96, GameAssetCategory.Loading, true, "Loading bar fill"),
            new AssetSpec("icon_app", "icon_app_1024x1024.png", "Loading", 1024, 1024, GameAssetCategory.Loading, false, "App icon placeholder"),

            new AssetSpec("missing_sprite", "missing_sprite_128x128.png", "UI", 128, 128, GameAssetCategory.UI, true, "Missing sprite fallback")
        };

        [MenuItem("Tools/Isekai 12 Realms/Asset PNG Generator")]
        public static void Open()
        {
            GetWindow<GameAssetPngGeneratorWindow>("Asset PNG Generator");
        }

        [MenuItem("Tools/Isekai 12 Realms/Generate Priority 1 Placeholder PNGs")]
        public static void GeneratePriority1Placeholders()
        {
            EnsureFolders();
            int count = 0;
            foreach (AssetSpec spec in Priority1Assets)
            {
                string pngPath = spec.Path;
                File.WriteAllBytes(pngPath, DrawPlaceholder(spec));
                File.WriteAllText($"{MetaPath}/{spec.id}_{spec.width}x{spec.height}.json", CreateMetadata(spec));
                count++;
            }

            AssetDatabase.Refresh();
            foreach (AssetSpec spec in Priority1Assets)
            {
                ConfigureTextureImporter(spec.Path, spec.transparent, spec.category == GameAssetCategory.Token || spec.category == GameAssetCategory.Tileset);
            }

            RebuildAssetManifest();
            Debug.Log($"[Assets] Generated {count} priority 1 placeholder PNGs and metadata files.");
        }

        [MenuItem("Tools/Isekai 12 Realms/Rebuild Asset Manifest")]
        public static void RebuildAssetManifest()
        {
            EnsureFolders();
            string manifestDir = Path.GetDirectoryName(ManifestPath);
            if (!Directory.Exists(manifestDir)) Directory.CreateDirectory(manifestDir);

            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath);
            if (manifest == null)
            {
                manifest = CreateInstance<GameAssetManifest>();
                AssetDatabase.CreateAsset(manifest, ManifestPath);
            }

            Regex sizeRegex = new Regex(@"^(?<id>[a-z0-9_]+)_(?<w>\d+)x(?<h>\d+)\.png$");
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { GeneratedRoot });
            manifest.entries.Clear();
            int pngCount = 0;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
                pngCount++;
                string fileName = Path.GetFileName(path);
                Match match = sizeRegex.Match(fileName);
                if (!match.Success) continue;
                string id = match.Groups["id"].Value;
                int width = int.Parse(match.Groups["w"].Value);
                int height = int.Parse(match.Groups["h"].Value);
                GameAssetCategory category = CategoryFromPath(path, id);
                bool transparent = category != GameAssetCategory.Background && id != "icon_app";
                ConfigureTextureImporter(path, transparent, category == GameAssetCategory.Token || category == GameAssetCategory.Tileset);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                manifest.entries.Add(new GameAssetEntry
                {
                    id = id,
                    fileName = fileName,
                    relativePath = path,
                    width = width,
                    height = height,
                    category = category,
                    transparent = transparent,
                    priority = Priority1Assets.Any(a => a.id == id) ? 1 : 5,
                    sprite = sprite
                });
            }

            manifest.entries = manifest.entries.OrderBy(e => e.category).ThenBy(e => e.id).ToList();
            manifest.missingSprite = manifest.entries.FirstOrDefault(e => e.id == "missing_sprite")?.sprite;
            manifest.RebuildLookup();
            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Assets] Rebuilt manifest. PNGs found: {pngCount}, entries created: {manifest.entries.Count}, missing sprite: {(manifest.missingSprite != null ? "linked" : "missing")}");
        }

        [MenuItem("Tools/Isekai 12 Realms/Apply Placeholder Art To Current UI")]
        public static void ApplyPlaceholderArtToCurrentUi()
        {
            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath);
            AssetSpriteBinder.SetManifest(manifest);
            GameSceneBootstrapper bootstrapper = FindObjectOfType<GameSceneBootstrapper>();
            bootstrapper?.RepairSceneUi();
            foreach (AssetSpriteBinder binder in Resources.FindObjectsOfTypeAll<AssetSpriteBinder>())
            {
                binder.Apply();
            }
            Debug.Log("[Assets] Applied placeholder art binders to current UI.");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Isekai 12 Realms Asset Pipeline", EditorStyles.boldLabel);
            if (GUILayout.Button("Create / Repair Generated Art Folders", GUILayout.Height(34))) EnsureFolders();
            if (GUILayout.Button("Generate Priority 1 Placeholder PNGs", GUILayout.Height(34))) GeneratePriority1Placeholders();
            if (GUILayout.Button("Rebuild Asset Manifest", GUILayout.Height(34))) RebuildAssetManifest();
            if (GUILayout.Button("Apply Placeholder Art To Current UI", GUILayout.Height(34))) ApplyPlaceholderArtToCurrentUi();
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists(GeneratedRoot)) Directory.CreateDirectory(GeneratedRoot);
            foreach (string folder in RequiredFolders)
            {
                string path = $"{GeneratedRoot}/{folder}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
        }

        private static void ConfigureTextureImporter(string path, bool transparent, bool point)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.alphaIsTransparency = transparent;
            importer.mipmapEnabled = false;
            importer.filterMode = point ? FilterMode.Point : FilterMode.Bilinear;
            TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static GameAssetCategory CategoryFromPath(string path, string id)
        {
            if (path.Contains("/Backgrounds/")) return GameAssetCategory.Background;
            if (path.Contains("/Characters/")) return GameAssetCategory.Character;
            if (path.Contains("/Enemies/")) return GameAssetCategory.Enemy;
            if (path.Contains("/NPCs/")) return GameAssetCategory.NPC;
            if (path.Contains("/Tokens/")) return GameAssetCategory.Token;
            if (path.Contains("/Skills/")) return GameAssetCategory.Skill;
            if (path.Contains("/Equipment/")) return GameAssetCategory.Equipment;
            if (path.Contains("/Items/") && id.StartsWith("currency_")) return GameAssetCategory.Currency;
            if (path.Contains("/Items/")) return GameAssetCategory.Item;
            if (path.Contains("/UI/")) return GameAssetCategory.UI;
            if (path.Contains("/Tilesets/")) return GameAssetCategory.Tileset;
            if (path.Contains("/VFX/")) return GameAssetCategory.VFX;
            if (path.Contains("/Maps/")) return GameAssetCategory.Map;
            if (path.Contains("/Loading/")) return GameAssetCategory.Loading;
            return GameAssetCategory.Misc;
        }

        private static byte[] DrawPlaceholder(AssetSpec spec)
        {
            Texture2D texture = new Texture2D(spec.width, spec.height, TextureFormat.RGBA32, false);
            Color[] pixels = Enumerable.Repeat(spec.transparent ? Color.clear : Color.white, spec.width * spec.height).ToArray();
            if (spec.category == GameAssetCategory.Background || spec.id == "icon_app") DrawBackground(pixels, spec);
            else if (spec.id == "missing_sprite") DrawMissing(pixels, spec.width, spec.height);
            else if (spec.category == GameAssetCategory.UI || spec.category == GameAssetCategory.Loading) DrawUi(pixels, spec);
            else if (spec.category == GameAssetCategory.Token) DrawToken(pixels, spec);
            else if (spec.category == GameAssetCategory.Currency) DrawCurrency(pixels, spec);
            else DrawCharacterOrEnemy(pixels, spec);
            texture.SetPixels(pixels);
            texture.Apply();
            byte[] png = texture.EncodeToPNG();
            DestroyImmediate(texture);
            return png;
        }

        private static void DrawBackground(Color[] p, AssetSpec s)
        {
            Color top = new Color(0.38f, 0.83f, 1f, 1f);
            Color bottom = new Color(0.12f, 0.22f, 0.55f, 1f);
            if (s.id.Contains("town")) { top = new Color(0.52f, 0.95f, 0.82f, 1f); bottom = new Color(0.2f, 0.62f, 0.28f, 1f); }
            if (s.id.Contains("scroll")) { top = new Color(1f, 0.96f, 0.78f, 1f); bottom = new Color(0.74f, 0.54f, 0.28f, 1f); }
            if (s.id.Contains("battle")) { top = new Color(0.35f, 0.76f, 0.98f, 1f); bottom = new Color(0.24f, 0.75f, 0.26f, 1f); }
            if (s.id == "icon_app") { top = new Color(0.22f, 0.75f, 0.95f, 1f); bottom = new Color(0.45f, 0.18f, 0.75f, 1f); }
            for (int y = 0; y < s.height; y++)
            {
                Color c = Color.Lerp(bottom, top, (float)y / Mathf.Max(1, s.height - 1));
                for (int x = 0; x < s.width; x++) p[y * s.width + x] = c;
            }
            FillCircle(p, s.width, s.height, s.width * 0.78f, s.height * 0.78f, s.width * 0.18f, new Color(1f, 0.88f, 0.32f, 0.85f));
            FillCircle(p, s.width, s.height, s.width * 0.25f, s.height * 0.24f, s.width * 0.28f, new Color(1f, 1f, 1f, 0.18f));
        }

        private static void DrawMissing(Color[] p, int w, int h)
        {
            FillRect(p, w, h, 0, 0, w, h, new Color(0.45f, 0.08f, 0.7f, 1f));
            FillRect(p, w, h, 10, 10, w - 20, h - 20, new Color(0.72f, 0.32f, 0.95f, 1f));
            FillCircle(p, w, h, w * 0.5f, h * 0.62f, w * 0.18f, Color.white);
            FillRect(p, w, h, w / 2 - 8, h / 4, 16, 18, Color.white);
        }

        private static void DrawUi(Color[] p, AssetSpec s)
        {
            Color fill = new Color(1f, 0.93f, 0.74f, 0.95f);
            Color border = new Color(0.16f, 0.19f, 0.35f, 1f);
            if (s.id.Contains("primary")) fill = new Color(0.18f, 0.76f, 0.82f, 1f);
            if (s.id.Contains("secondary")) fill = new Color(1f, 0.72f, 0.28f, 1f);
            if (s.id.Contains("close")) fill = new Color(1f, 0.35f, 0.28f, 1f);
            if (s.id.Contains("hp_bg")) fill = new Color(0.16f, 0.04f, 0.05f, 1f);
            if (s.id.Contains("hp_fill")) { fill = new Color(0.55f, 0.9f, 0.34f, 1f); border = Color.clear; }
            if (s.id.Contains("mana_bg")) fill = new Color(0.04f, 0.05f, 0.2f, 1f);
            if (s.id.Contains("mana_fill")) { fill = new Color(0.42f, 0.62f, 1f, 1f); border = Color.clear; }
            if (s.id.Contains("loading_bar_fill")) { fill = new Color(0.42f, 0.9f, 1f, 1f); border = Color.clear; }
            FillRect(p, s.width, s.height, 0, 0, s.width, s.height, border == Color.clear ? fill : border);
            int b = Mathf.Max(4, Mathf.RoundToInt(Mathf.Min(s.width, s.height) * 0.08f));
            FillRect(p, s.width, s.height, b, b, s.width - b * 2, s.height - b * 2, fill);
            if (s.id == "logo_game_main")
            {
                FillCircle(p, s.width, s.height, s.width * 0.25f, s.height * 0.52f, s.height * 0.28f, new Color(1f, 0.78f, 0.28f, 1f));
                FillCircle(p, s.width, s.height, s.width * 0.5f, s.height * 0.5f, s.height * 0.36f, new Color(0.35f, 0.88f, 1f, 1f));
                FillCircle(p, s.width, s.height, s.width * 0.74f, s.height * 0.52f, s.height * 0.28f, new Color(0.8f, 0.44f, 1f, 1f));
            }
        }

        private static void DrawToken(Color[] p, AssetSpec s)
        {
            Color c = new Color(0.5f, 0.85f, 1f, 1f);
            if (s.id.Contains("heart")) c = new Color(1f, 0.32f, 0.42f, 1f);
            if (s.id.Contains("coin")) c = new Color(1f, 0.78f, 0.22f, 1f);
            if (s.id.Contains("food")) c = new Color(0.56f, 0.9f, 0.36f, 1f);
            if (s.id.Contains("book")) c = new Color(0.7f, 0.45f, 1f, 1f);
            if (s.id.Contains("mana")) c = new Color(0.35f, 0.55f, 1f, 1f);
            if (s.id.Contains("shield")) c = new Color(0.62f, 0.68f, 0.78f, 1f);
            if (s.id.Contains("star")) c = new Color(1f, 0.94f, 0.24f, 1f);
            FillCircle(p, s.width, s.height, s.width * 0.5f, s.height * 0.5f, s.width * 0.47f, new Color(0.08f, 0.1f, 0.18f, 1f));
            FillCircle(p, s.width, s.height, s.width * 0.5f, s.height * 0.5f, s.width * 0.38f, c);
            FillRect(p, s.width, s.height, s.width / 2 - 8, 22, 16, 84, Color.white);
            if (s.id.Contains("star")) FillCircle(p, s.width, s.height, s.width * 0.5f, s.height * 0.5f, 18, Color.white);
        }

        private static void DrawCurrency(Color[] p, AssetSpec s)
        {
            Color c = s.id.Contains("gem") ? new Color(0.65f, 0.38f, 1f, 1f) : new Color(1f, 0.75f, 0.18f, 1f);
            FillCircle(p, s.width, s.height, 58, 66, 38, new Color(0.08f, 0.1f, 0.18f, 1f));
            FillCircle(p, s.width, s.height, 58, 66, 30, c);
            FillCircle(p, s.width, s.height, 78, 50, 25, c);
        }

        private static void DrawCharacterOrEnemy(Color[] p, AssetSpec s)
        {
            Color c = s.category == GameAssetCategory.Enemy ? new Color(0.35f, 0.85f, 0.42f, 1f) : new Color(1f, 0.42f, 0.24f, 1f);
            if (s.id.Contains("boss")) c = new Color(0.55f, 0.32f, 0.9f, 1f);
            FillCircle(p, s.width, s.height, s.width * 0.5f, s.height * 0.48f, s.width * 0.34f, new Color(0.08f, 0.1f, 0.18f, 1f));
            FillCircle(p, s.width, s.height, s.width * 0.5f, s.height * 0.5f, s.width * 0.29f, c);
            FillCircle(p, s.width, s.height, s.width * 0.4f, s.height * 0.57f, s.width * 0.035f, Color.white);
            FillCircle(p, s.width, s.height, s.width * 0.6f, s.height * 0.57f, s.width * 0.035f, Color.white);
        }

        private static void FillRect(Color[] p, int w, int h, int x, int y, int rw, int rh, Color c)
        {
            for (int yy = Mathf.Max(0, y); yy < Mathf.Min(h, y + rh); yy++)
            for (int xx = Mathf.Max(0, x); xx < Mathf.Min(w, x + rw); xx++) p[yy * w + xx] = c;
        }

        private static void FillCircle(Color[] p, int w, int h, float cx, float cy, float r, Color c)
        {
            float rr = r * r;
            for (int y = Mathf.Max(0, Mathf.FloorToInt(cy - r)); y < Mathf.Min(h, Mathf.CeilToInt(cy + r)); y++)
            for (int x = Mathf.Max(0, Mathf.FloorToInt(cx - r)); x < Mathf.Min(w, Mathf.CeilToInt(cx + r)); x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                if (dx * dx + dy * dy <= rr) p[y * w + x] = c;
            }
        }

        private static string CreateMetadata(AssetSpec spec)
        {
            return $@"{{
  ""id"": ""{spec.id}"",
  ""file"": ""{spec.fileName}"",
  ""size"": [{spec.width}, {spec.height}],
  ""category"": ""{spec.category.ToString().ToLowerInvariant()}"",
  ""usage"": ""{spec.usage}"",
  ""style"": ""bright chibi fantasy placeholder 2D"",
  ""transparent"": {spec.transparent.ToString().ToLowerInvariant()},
  ""replaceable"": true,
  ""prompt"": ""original procedural placeholder for {spec.usage}, no copied art, no watermark""
}}";
        }

        private readonly struct AssetSpec
        {
            public readonly string id;
            public readonly string fileName;
            public readonly string folder;
            public readonly int width;
            public readonly int height;
            public readonly GameAssetCategory category;
            public readonly bool transparent;
            public readonly string usage;
            public string Path => $"{GeneratedRoot}/{folder}/{fileName}";

            public AssetSpec(string id, string fileName, string folder, int width, int height, GameAssetCategory category, bool transparent, string usage)
            {
                this.id = id;
                this.fileName = fileName;
                this.folder = folder;
                this.width = width;
                this.height = height;
                this.category = category;
                this.transparent = transparent;
                this.usage = usage;
            }
        }
    }
}
