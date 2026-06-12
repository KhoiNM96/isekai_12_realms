using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Isekai12Realms.Data;

namespace Isekai12Realms.Editor
{
    public class GameAssetPngGenerator : EditorWindow
    {
        private const string GeneratedRoot = "Assets/_Game/Art/Generated";
        private const string ManifestPath = "Assets/_Game/ScriptableObjects/GameAssetManifest.asset";

        private struct AssetSpec
        {
            public string id;
            public string fileName;
            public Vector2Int size;
            public GameAssetCategory category;
            public bool transparent;
            public string description;
            public string prompt;

            public AssetSpec(string id, string fileName, Vector2Int size, GameAssetCategory category, bool transparent, string description, string prompt)
            {
                this.id = id;
                this.fileName = fileName;
                this.size = size;
                this.category = category;
                this.transparent = transparent;
                this.description = description;
                this.prompt = prompt;
            }
        }

        private static readonly List<AssetSpec> Priority1Assets = new List<AssetSpec>
        {
            // Backgrounds
            new AssetSpec("bg_title_sky_realm", "bg_title_sky_realm_1080x1920.png", new Vector2Int(1080, 1920), GameAssetCategory.Backgrounds, false, "Title sky realm background", "Beautiful sky gradient background"),
            new AssetSpec("bg_town_meadow", "bg_town_meadow_1080x1920.png", new Vector2Int(1080, 1920), GameAssetCategory.Backgrounds, false, "Town meadow background", "Floating meadow town background"),
            new AssetSpec("bg_world_map_scroll", "bg_world_map_scroll_1080x1920.png", new Vector2Int(1080, 1920), GameAssetCategory.Backgrounds, false, "World map scroll background", "Old scroll parchment map background"),
            new AssetSpec("bg_battle_meadow", "bg_battle_meadow_1080x960.png", new Vector2Int(1080, 960), GameAssetCategory.Backgrounds, false, "Battle meadow background", "Lush green grass meadow for battles"),

            // Characters
            new AssetSpec("char_hero_flame_idle", "char_hero_flame_idle_512x512.png", new Vector2Int(512, 512), GameAssetCategory.Characters, true, "Flame Hero Idle", "Chibi flame squire idle body"),
            new AssetSpec("char_hero_flame_attack", "char_hero_flame_attack_512x512.png", new Vector2Int(512, 512), GameAssetCategory.Characters, true, "Flame Hero Attack", "Chibi flame squire attacking"),
            new AssetSpec("char_hero_flame_cast", "char_hero_flame_cast_512x512.png", new Vector2Int(512, 512), GameAssetCategory.Characters, true, "Flame Hero Cast", "Chibi flame squire casting spell"),
            new AssetSpec("char_hero_flame_hurt", "char_hero_flame_hurt_512x512.png", new Vector2Int(512, 512), GameAssetCategory.Characters, true, "Flame Hero Hurt", "Chibi flame squire taking damage"),

            // Enemies
            new AssetSpec("enemy_meadow_slime", "enemy_meadow_slime_512x512.png", new Vector2Int(512, 512), GameAssetCategory.Enemies, true, "Meadow Slime", "Cute bouncing green slime"),
            new AssetSpec("boss_slime_king", "boss_slime_king_768x768.png", new Vector2Int(768, 768), GameAssetCategory.Enemies, true, "Slime King Boss", "Giant slime with crown"),

            // Tokens
            new AssetSpec("icon_token_sword", "icon_token_sword_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Sword Token", "Small glowing sword icon"),
            new AssetSpec("icon_token_heart", "icon_token_heart_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Heart Token", "Small crystal heart icon"),
            new AssetSpec("icon_token_coin", "icon_token_coin_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Coin Token", "Small golden coin icon"),
            new AssetSpec("icon_token_food", "icon_token_food_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Food Token", "Small red apple or food icon"),
            new AssetSpec("icon_token_book", "icon_token_book_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Book Token", "Small spellbook icon"),
            new AssetSpec("icon_token_mana", "icon_token_mana_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Mana Token", "Small blue mana orb icon"),
            new AssetSpec("icon_token_shield", "icon_token_shield_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Shield Token", "Small metal shield icon"),
            new AssetSpec("icon_token_star", "icon_token_star_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Tokens, true, "Star Token", "Small magical star icon"),

            // UI Elements
            new AssetSpec("ui_panel_main", "ui_panel_main_768x512.png", new Vector2Int(768, 512), GameAssetCategory.UI, true, "Main Panel", "Decorative UI panel cream base"),
            new AssetSpec("ui_panel_popup", "ui_panel_popup_768x512.png", new Vector2Int(768, 512), GameAssetCategory.UI, true, "Popup Panel", "Decorative UI popup background"),
            new AssetSpec("ui_btn_primary", "ui_btn_primary_384x128.png", new Vector2Int(384, 128), GameAssetCategory.UI, true, "Primary Button", "Cyan color button frame"),
            new AssetSpec("ui_btn_secondary", "ui_btn_secondary_384x128.png", new Vector2Int(384, 128), GameAssetCategory.UI, true, "Secondary Button", "Gold color button frame"),
            new AssetSpec("ui_btn_close", "ui_btn_close_128x128.png", new Vector2Int(128, 128), GameAssetCategory.UI, true, "Close Button", "Red cross close button"),
            new AssetSpec("ui_bar_hp_bg", "ui_bar_hp_bg_512x64.png", new Vector2Int(512, 64), GameAssetCategory.UI, true, "HP Bar Background", "Dark red container for HP"),
            new AssetSpec("ui_bar_hp_fill", "ui_bar_hp_fill_512x64.png", new Vector2Int(512, 64), GameAssetCategory.UI, true, "HP Bar Fill", "Bright green or red fill for HP"),
            new AssetSpec("ui_bar_mana_bg", "ui_bar_mana_bg_512x64.png", new Vector2Int(512, 64), GameAssetCategory.UI, true, "Mana Bar Background", "Dark blue container for Mana"),
            new AssetSpec("ui_bar_mana_fill", "ui_bar_mana_fill_512x64.png", new Vector2Int(512, 64), GameAssetCategory.UI, true, "Mana Bar Fill", "Bright blue fill for Mana"),

            // Currencies (Items)
            new AssetSpec("currency_gold", "currency_gold_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Items, true, "Gold Currency Icon", "Glowing heap of gold coins"),
            new AssetSpec("currency_soul_gem", "currency_soul_gem_128x128.png", new Vector2Int(128, 128), GameAssetCategory.Items, true, "Soul Gem Icon", "Shard of glowing purple gemstone"),

            // Fallback (UI)
            new AssetSpec("missing_sprite", "missing_sprite_128x128.png", new Vector2Int(128, 128), GameAssetCategory.UI, true, "Missing Sprite Fallback", "Magenta-black checkerboard placeholder")
        };

        [MenuItem("Tools/Isekai 12 Realms/Game Asset PNG Generator")]
        public static void ShowWindow()
        {
            GetWindow<GameAssetPngGenerator>("Asset PNG Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Isekai 12 Realms - Asset Generator & Manifest Rebuilder", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("1. Create Required Folder Structure", GUILayout.Height(40)))
            {
                CreateFolders();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("2. Generate Priority 1 Placeholders", GUILayout.Height(40)))
            {
                GeneratePlaceholders();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("3. Rebuild Asset Manifest", GUILayout.Height(40)))
            {
                RebuildManifest();
            }
        }

        [MenuItem("Tools/Isekai 12 Realms/Rebuild Asset Manifest")]
        public static void RebuildManifest()
        {
            // Find or create ScriptableObject
            GameAssetManifest manifest = AssetDatabase.LoadAssetAtPath<GameAssetManifest>(ManifestPath);
            if (manifest == null)
            {
                // Ensure directory exists
                string dir = Path.GetDirectoryName(ManifestPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                manifest = CreateInstance<GameAssetManifest>();
                AssetDatabase.CreateAsset(manifest, ManifestPath);
            }

            manifest.entries.Clear();

            foreach (var spec in Priority1Assets)
            {
                string categoryDir = spec.category.ToString();
                string relativePath = $"{GeneratedRoot}/{categoryDir}/{spec.fileName}";
                
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(relativePath);

                GameAssetEntry entry = new GameAssetEntry
                {
                    id = spec.id,
                    fileName = spec.fileName,
                    relativePath = relativePath,
                    size = spec.size,
                    category = spec.category,
                    transparent = spec.transparent,
                    sprite = sprite,
                    priority = 1
                };

                manifest.entries.Add(entry);
            }

            EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GameAssetManifest] Manifest rebuilt with {manifest.entries.Count} entries at {ManifestPath}");
        }

        public static void CreateFolders()
        {
            foreach (GameAssetCategory cat in Enum.GetValues(typeof(GameAssetCategory)))
            {
                string path = $"{GeneratedRoot}/{cat}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }

            // Ensure other standard paths exist
            string[] standardPaths = {
                "Assets/_Game/Art/Characters",
                "Assets/_Game/Art/Enemies",
                "Assets/_Game/Art/UI",
                "Assets/_Game/Art/Icons",
                "Assets/_Game/Art/Tilesets",
                "Assets/_Game/Art/VFX",
                "Assets/_Game/Audio/BGM",
                "Assets/_Game/Audio/SFX",
                "Assets/_Game/Scenes",
                "Assets/_Game/Scripts/Core",
                "Assets/_Game/Scripts/Services",
                "Assets/_Game/Scripts/Data",
                "Assets/_Game/Scripts/Battle",
                "Assets/_Game/Scripts/Board",
                "Assets/_Game/Scripts/Character",
                "Assets/_Game/Scripts/Inventory",
                "Assets/_Game/Scripts/Quest",
                "Assets/_Game/Scripts/UI",
                "Assets/_Game/Scripts/Firebase",
                "Assets/_Game/Scripts/IAP",
                "Assets/_Game/Scripts/Editor",
                "Assets/_Game/Prefabs/UI",
                "Assets/_Game/Prefabs/Characters",
                "Assets/_Game/Prefabs/Battle",
                "Assets/_Game/Prefabs/Map",
                "Assets/_Game/Prefabs/Popups",
                "Assets/_Game/ScriptableObjects",
                "Assets/_Game/ScriptableObjects/Characters",
                "Assets/_Game/ScriptableObjects/Skills",
                "Assets/_Game/ScriptableObjects/Items",
                "Assets/_Game/ScriptableObjects/Stages",
                "Assets/_Game/ScriptableObjects/Enemies",
                "Assets/_Game/ScriptableObjects/DropTables",
                "Assets/_Game/ScriptableObjects/Economy",
                "Assets/_Game/Addressables/Remote",
                "Assets/_Game/Addressables/Local"
            };

            foreach (var p in standardPaths)
            {
                if (!Directory.Exists(p))
                {
                    Directory.CreateDirectory(p);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Folder structure created successfully.");
        }

        public static void GeneratePlaceholders()
        {
            CreateFolders();

            int count = 0;
            foreach (var spec in Priority1Assets)
            {
                string categoryDir = spec.category.ToString();
                string targetDir = $"{GeneratedRoot}/{categoryDir}";
                string pngPath = $"{targetDir}/{spec.fileName}";
                string jsonPath = pngPath.Replace(".png", ".json");

                // Generate PNG
                byte[] pngBytes = DrawPlaceholder(spec);
                File.WriteAllBytes(pngPath, pngBytes);

                // Generate JSON sidecar
                string jsonContent = CreateJsonSidecar(spec);
                File.WriteAllText(jsonPath, jsonContent);

                count++;
            }

            AssetDatabase.Refresh();

            // Set Import settings for all generated PNGs
            foreach (var spec in Priority1Assets)
            {
                string categoryDir = spec.category.ToString();
                string relativePath = $"{GeneratedRoot}/{categoryDir}/{spec.fileName}";

                TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100;
                    
                    // Point for tokens/tiles/pixel-art, Bilinear for backgrounds/UI
                    if (spec.category == GameAssetCategory.Tokens || spec.category == GameAssetCategory.Tilesets)
                    {
                        importer.filterMode = FilterMode.Point;
                    }
                    else
                    {
                        importer.filterMode = FilterMode.Bilinear;
                    }

                    importer.mipmapEnabled = false;
                    importer.alphaIsTransparency = spec.transparent;
                    
                    // Texture Compression
                    TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SetPlatformTextureSettings(platformSettings);

                    importer.SaveAndReimport();
                }
            }

            Debug.Log($"Successfully generated {count} PNG placeholders and matching JSON metadata sidecars.");

            // Automatically link generated sprites to Manifest
            RebuildManifest();
        }

        private static byte[] DrawPlaceholder(AssetSpec spec)
        {
            int w = spec.size.x;
            int h = spec.size.y;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

            Color[] pixels = new Color[w * h];

            if (!spec.transparent)
            {
                // Full background gradient
                Color topColor = new Color(0.3f, 0.85f, 0.95f, 1.0f); // bright blue sky
                Color bottomColor = new Color(0.1f, 0.4f, 0.7f, 1.0f); // deeper blue

                if (spec.id == "bg_town_meadow")
                {
                    topColor = new Color(0.4f, 0.9f, 0.8f, 1.0f); // cyan tint
                    bottomColor = new Color(0.2f, 0.6f, 0.4f, 1.0f); // meadow green
                }
                else if (spec.id == "bg_world_map_scroll")
                {
                    topColor = new Color(0.98f, 0.95f, 0.84f, 1.0f); // scroll paper light
                    bottomColor = new Color(0.85f, 0.75f, 0.55f, 1.0f); // scroll paper shadow
                }
                else if (spec.id == "bg_battle_meadow")
                {
                    topColor = new Color(0.3f, 0.7f, 0.9f, 1.0f);
                    bottomColor = new Color(0.25f, 0.8f, 0.3f, 1.0f); // green ground
                }

                for (int y = 0; y < h; y++)
                {
                    float t = (float)y / h;
                    Color lerped = Color.Lerp(bottomColor, topColor, t);
                    for (int x = 0; x < w; x++)
                    {
                        pixels[y * w + x] = lerped;
                    }
                }
            }
            else
            {
                // Transparent background
                Color clear = new Color(0, 0, 0, 0);
                for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

                // Render placeholder shapes inside the transparent texture
                if (spec.id == "missing_sprite")
                {
                    // Magenta-black checkerboard
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            bool isMagenta = ((x / 16) + (y / 16)) % 2 == 0;
                            pixels[y * w + x] = isMagenta ? Color.magenta : Color.black;
                        }
                    }
                }
                else if (spec.category == GameAssetCategory.UI)
                {
                    // UI frames & panels: Rounded rect panel look
                    Color panelBg = new Color(0.99f, 0.95f, 0.84f, 1.0f); // panel_cream #FFF2D6
                    Color borderCol = new Color(0.15f, 0.2f, 0.3f, 1.0f); // dark frame

                    if (spec.id == "ui_btn_primary")
                    {
                        panelBg = new Color(0.3f, 0.85f, 0.82f, 1.0f); // primary_cyan #4EDBD2
                        borderCol = Color.white;
                    }
                    else if (spec.id == "ui_btn_secondary")
                    {
                        panelBg = new Color(1.0f, 0.82f, 0.4f, 1.0f); // secondary_gold #FFD166
                        borderCol = Color.white;
                    }
                    else if (spec.id == "ui_btn_close")
                    {
                        panelBg = new Color(1.0f, 0.48f, 0.27f, 1.0f); // danger_orange #FF7A45
                        borderCol = Color.white;
                    }
                    else if (spec.id.Contains("hp_bg"))
                    {
                        panelBg = new Color(0.2f, 0.05f, 0.05f, 1.0f); // dark red
                        borderCol = Color.black;
                    }
                    else if (spec.id.Contains("hp_fill"))
                    {
                        panelBg = new Color(0.5f, 0.85f, 0.34f, 1.0f); // success_green #7ED957
                        borderCol = Color.clear;
                    }
                    else if (spec.id.Contains("mana_bg"))
                    {
                        panelBg = new Color(0.05f, 0.05f, 0.2f, 1.0f); // dark blue
                        borderCol = Color.black;
                    }
                    else if (spec.id.Contains("mana_fill"))
                    {
                        panelBg = new Color(0.65f, 0.42f, 1.0f, 1.0f); // magic_purple #A66CFF
                        borderCol = Color.clear;
                    }

                    int borderWidth = spec.id.Contains("bar") ? 4 : 12;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            bool isBorder = (x < borderWidth || x >= w - borderWidth || y < borderWidth || y >= h - borderWidth);
                            if (isBorder && borderCol != Color.clear)
                            {
                                pixels[y * w + x] = borderCol;
                            }
                            else
                            {
                                pixels[y * w + x] = panelBg;
                            }
                        }
                    }
                }
                else if (spec.category == GameAssetCategory.Tokens)
                {
                    // Draw a nice colored circle with a symbol representing the token
                    Color tokenCol = Color.white;
                    if (spec.id.Contains("sword")) tokenCol = new Color(0.3f, 0.85f, 0.82f, 1.0f); // Cyan
                    else if (spec.id.Contains("heart")) tokenCol = new Color(1.0f, 0.48f, 0.27f, 1.0f); // Red
                    else if (spec.id.Contains("coin")) tokenCol = new Color(1.0f, 0.82f, 0.4f, 1.0f); // Gold
                    else if (spec.id.Contains("food")) tokenCol = new Color(0.5f, 0.85f, 0.34f, 1.0f); // Green
                    else if (spec.id.Contains("book")) tokenCol = new Color(0.65f, 0.42f, 1.0f, 1.0f); // Purple
                    else if (spec.id.Contains("mana")) tokenCol = new Color(0.3f, 0.6f, 1.0f, 1.0f); // Blue
                    else if (spec.id.Contains("shield")) tokenCol = new Color(0.6f, 0.6f, 0.6f, 1.0f); // Grey
                    else if (spec.id.Contains("star")) tokenCol = new Color(0.95f, 0.9f, 0.3f, 1.0f); // Yellow

                    float r = w * 0.45f;
                    float cx = w * 0.5f;
                    float cy = h * 0.5f;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            float dx = x - cx;
                            float dy = y - cy;
                            float dist = Mathf.Sqrt(dx * dx + dy * dy);

                            if (dist < r)
                            {
                                if (dist > r - 8)
                                {
                                    pixels[y * w + x] = Color.black;
                                }
                                else
                                {
                                    pixels[y * w + x] = tokenCol;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Default circle placeholder for characters / enemies
                    Color spriteCol = new Color(0.95f, 0.4f, 0.4f, 1.0f); // Fire Orange for Flame hero
                    if (spec.id.Contains("slime")) spriteCol = new Color(0.3f, 0.8f, 0.4f, 1.0f); // Slime Green
                    else if (spec.id.Contains("boss")) spriteCol = new Color(0.5f, 0.3f, 0.8f, 1.0f); // Boss Purple

                    float r = w * 0.4f;
                    float cx = w * 0.5f;
                    float cy = h * 0.5f;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            float dx = x - cx;
                            float dy = y - cy;
                            float dist = Mathf.Sqrt(dx * dx + dy * dy);

                            if (dist < r)
                            {
                                if (dist > r - 12)
                                {
                                    pixels[y * w + x] = Color.black;
                                }
                                else
                                {
                                    pixels[y * w + x] = spriteCol;
                                }
                            }
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            return bytes;
        }

        private static string CreateJsonSidecar(AssetSpec spec)
        {
            string catLower = spec.category.ToString().ToLower();
            string style = "chibi fantasy pixel-inspired 2D";
            
            return $@"{{
  ""id"": ""{spec.id}"",
  ""file"": ""{spec.fileName}"",
  ""size"": [{spec.size.x}, {spec.size.y}],
  ""category"": ""{catLower}"",
  ""usage"": ""{spec.description}"",
  ""style"": ""{style}"",
  ""transparent"": {spec.transparent.ToString().ToLower()},
  ""replaceable"": true,
  ""prompt"": ""{spec.prompt}""
}}";
        }
    }
}
