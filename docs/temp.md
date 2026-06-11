# AI RULES & ASSET MANIFEST — ISEKAI 12 REALMS REBORN

## 0. Mục tiêu tài liệu

Tài liệu này dùng cho **Codex**, **Unity AI**, hoặc AI coding agent khác để tự tạo game Unity mobile màn hình dọc.

Game lấy cảm hứng gameplay từ dòng RPG match-3 Java mobile cổ điển:

* đi map 2D;
* gặp NPC;
* nhận nhiệm vụ;
* vào trận chiến match-3 theo lượt;
* có máu, mana, lương thực, vàng, EXP;
* có skill, trang bị, nhiệm vụ, stage replay.

Nhưng toàn bộ art, tên gọi, nhân vật, bối cảnh, UI phải là **original isekai fantasy**, không sao chép asset, tên riêng, map, icon, logo, nhân vật, UI của game cũ.

---

# PART A — GLOBAL AI RULES

## 1. Rule bắt buộc cho Codex và Unity AI

### 1.1. Không copy nguyên bản

AI không được tạo lại:

* logo cũ;
* nhân vật cũ;
* tên lịch sử hoặc tên NPC của game cũ;
* map giống ảnh tham khảo;
* icon giống hệt;
* UI frame giống hệt;
* watermark, text, website, tên thương hiệu cũ;
* sprite lấy từ ảnh tham khảo.

AI chỉ được giữ **ý tưởng gameplay cấp cao**:

```text
2D RPG map + NPC + turn-based match-3 battle + progression + equipment + skills.
```

### 1.2. Bối cảnh mới

Bối cảnh bắt buộc là:

```text
Isekai fantasy, bright, cute, non-violent, family-friendly, colorful, chibi, magical, offline RPG.
```

Không dùng bối cảnh lịch sử thật.
Không dùng chiến tranh đẫm máu.
Không dùng hình ảnh phân biệt sắc tộc, tôn giáo, chính trị.

### 1.3. Màn hình mobile dọc

Tất cả UI phải thiết kế theo:

```text
Reference Resolution: 1080 x 1920
Orientation: Portrait only
Safe Area: required
One-hand friendly layout
Minimum button height: 88 px
Minimum readable text size: 28 px
```

### 1.4. Asset phải là PNG

Mọi ảnh game phải xuất ra PNG.

Format tên file bắt buộc:

```text
object_widthxheight.png
```

Ví dụ:

```text
bg_title_sky_1080x1920.png
char_hero_flame_idle_512x512.png
icon_token_sword_128x128.png
ui_btn_primary_384x128.png
```

Quy tắc tên file:

```text
lowercase_snake_case_widthxheight.png
```

Không dùng:

```text
space
uppercase
Vietnamese accents
special characters
random names
final.png
image1.png
new_sprite.png
```

### 1.5. Mỗi PNG cần có metadata sidecar

Mỗi ảnh nên có file `.json` đi kèm để sau này AI khác thay thế đúng.

Ví dụ:

```text
char_hero_flame_idle_512x512.png
char_hero_flame_idle_512x512.json
```

JSON format:

```json
{
  "id": "char_hero_flame_idle",
  "file": "char_hero_flame_idle_512x512.png",
  "size": [512, 512],
  "category": "character",
  "usage": "player idle sprite",
  "style": "chibi fantasy pixel-inspired 2D",
  "transparent": true,
  "replaceable": true,
  "prompt": "original chibi isekai flame hero, cute, non-violent, transparent background"
}
```

---

# PART B — ART GENERATION SKILL

## 2. Skill: Generate Game PNG Assets

### 2.1. Skill name

```text
skill_generate_game_png_assets
```

### 2.2. Purpose

AI phải tự tạo toàn bộ ảnh PNG cho game Unity, gồm:

* backgrounds;
* character sprites;
* enemy sprites;
* NPC sprites;
* token icons;
* skill icons;
* equipment icons;
* item icons;
* UI panels;
* buttons;
* popups;
* map tiles;
* VFX sprites;
* loading/title art;
* shop icons;
* quest icons.

### 2.3. Output folder

Tất cả ảnh phải đặt trong:

```text
Assets/_Game/Art/Generated/
```

Cấu trúc thư mục:

```text
Assets/_Game/Art/Generated/
  Backgrounds/
  Characters/
  Enemies/
  NPCs/
  Tokens/
  Skills/
  Equipment/
  Items/
  UI/
  Tilesets/
  VFX/
  Maps/
  Icons/
  Loading/
  Meta/
```

### 2.4. PNG import rule cho Unity

Khi import PNG vào Unity:

```text
Texture Type: Sprite (2D and UI)
Sprite Mode:
  Single for normal images
  Multiple for spritesheet
Pixels Per Unit:
  100 for UI/icons
  64 for pixel/chibi map sprites
Filter Mode:
  Point for pixel art
  Bilinear for soft UI/background
Compression:
  None for UI/icons during development
  Normal Quality for production
Generate Mip Maps: Off for UI
Alpha Is Transparency: On when transparent
```

### 2.5. Transparent background rule

Ảnh cần nền trong suốt:

```text
characters
enemies
NPCs
icons
tokens
skills
equipment
items
VFX
UI buttons
UI frames
```

Ảnh không cần trong suốt:

```text
full background
title background
battle background
world map background
loading screen
```

### 2.6. Style chung

Art direction:

```text
cute chibi isekai fantasy, bright color, pixel-inspired but clean, soft outline, mobile readable, family-friendly, non-violent, no blood, no gore, no realistic horror
```

Màu chính:

```text
primary_cyan: #4EDBD2
secondary_gold: #FFD166
panel_cream: #FFF2D6
panel_dark: #26324A
success_green: #7ED957
danger_orange: #FF7A45
magic_purple: #A66CFF
```

### 2.7. Không dùng text trong ảnh trừ logo

AI không được render text trực tiếp trong PNG, trừ:

```text
game logo
loading word optional
```

Lý do: text cần thay đổi ngôn ngữ trong Unity bằng TextMeshPro.

Các button không được ghi chữ sẵn trong PNG.
Button chỉ là frame/background. Text đặt trong Unity.

---

# PART C — CODING RULES FOR CODEX

## 3. Codex Rule: Unity Architecture

### 3.1. Codex phải đọc tài liệu trước

Trước khi code, Codex phải đọc:

```text
docs/spec.md
docs/ai_rules.md
docs/asset_manifest.md
```

Nếu file chưa tồn tại, Codex phải tạo file theo nội dung spec.

### 3.2. Không hard-code asset path rời rạc

Không được viết kiểu:

```csharp
Resources.Load<Sprite>("abc");
```

Ưu tiên:

```text
ScriptableObject reference
Addressables key
AssetRegistry
```

### 3.3. Tất cả asset phải có ID

Ví dụ:

```csharp
public class AssetId
{
    public const string TokenSword = "icon_token_sword";
    public const string HeroFlameIdle = "char_hero_flame_idle";
}
```

### 3.4. Dùng AssetManifest

Codex phải tạo:

```text
Assets/_Game/ScriptableObjects/AssetManifest.asset
```

Manifest chứa:

```csharp
[Serializable]
public class GameAssetEntry
{
    public string id;
    public string fileName;
    public string path;
    public Vector2Int size;
    public Sprite sprite;
    public GameAssetCategory category;
}
```

### 3.5. Không để UI logic xử lý gameplay

Sai:

```text
BattleButton tự trừ máu enemy.
```

Đúng:

```text
BattleButton gọi BattleService.
BattleService xử lý logic.
UI chỉ update view.
```

### 3.6. Offline-first

Mọi tính năng core phải chạy khi không có mạng:

```text
battle
inventory
equipment
skill
quest
stage replay
local save
shop bằng gold/gem đã có
```

Firebase chỉ dùng để:

```text
login
cloud save
purchase ledger backup
remote config optional
```

### 3.7. IAP rule

IAP chỉ được bán:

```text
Soul Gem currency packs
```

Không được tạo IAP bán:

```text
weapon
armor
legendary item
character power
stage unlock
skip campaign
```

### 3.8. Save rule

Sau các hành động này phải save ngay:

```text
battle win
battle lose nếu có thay đổi item
level up
equipment upgrade
skill upgrade
IAP grant
quest reward claim
shop purchase
cloud sync decision
```

### 3.9. Naming rule cho script

```text
PascalCase cho class
camelCase cho field private
UPPER_SNAKE_CASE không dùng trừ const đặc biệt
```

Ví dụ:

```text
BattleService.cs
BoardController.cs
TileView.cs
InventoryService.cs
CloudSaveService.cs
IAPService.cs
```

---

# PART D — UNITY AI RULES

## 4. Unity AI Rule: Scene Generation

Unity AI được phép tự tạo scene nhưng phải theo layout.

### 4.1. Scene list

```text
BootScene
GameScene
```

Không tạo quá nhiều scene ở MVP.

GameScene dùng state:

```text
Title
CharacterCreation
MainTown
WorldMap
Adventure
Battle
Result
```

### 4.2. UI hierarchy bắt buộc

```text
RootCanvas
  SafeAreaRoot
    BackgroundLayer
    MainLayer
    HudLayer
    NavigationLayer
    PopupLayer
    ToastLayer
    LoadingLayer
EventSystem
GameManager
AudioManager
```

### 4.3. Canvas rule

```text
Canvas Render Mode: Screen Space - Overlay
Canvas Scaler: Scale With Screen Size
Reference Resolution: 1080 x 1920
Match Width Or Height: 0.5
```

### 4.4. Battle board rule

Battle board phải nằm giữa màn hình.

```text
Board size: 8 x 8
Tile visual size: 96 x 96 px
Board visual size: 768 x 768 px
Board anchor: center
```

### 4.5. Popup layer rule

Mọi popup phải instantiate vào:

```text
RootCanvas/SafeAreaRoot/PopupLayer
```

Không được để popup trong scene root.

---

# PART E — IMAGE PROMPT RULES

## 5. Prompt format cho AI tạo ảnh

Mỗi prompt tạo ảnh phải dùng format:

```text
Create an original PNG game asset for a mobile portrait isekai RPG match-3 game.
Asset ID: {asset_id}
Size: {width}x{height}
Background: {transparent_or_full}
Style: cute chibi fantasy, bright colors, pixel-inspired, clean readable shape, soft outline, mobile UI friendly, non-violent, no blood, no gore.
Description: {description}
Do not include text, watermark, logo, copyrighted characters, historical real persons, or copied game assets.
Output filename: {asset_id}_{width}x{height}.png
```

### 5.1. Ví dụ prompt token

```text
Create an original PNG game asset for a mobile portrait isekai RPG match-3 game.
Asset ID: icon_token_sword
Size: 128x128
Background: transparent
Style: cute chibi fantasy, bright colors, pixel-inspired, clean readable shape, soft outline, mobile UI friendly, non-violent, no blood, no gore.
Description: a small glowing wooden training sword icon with cyan sparkle, readable at small size.
Do not include text, watermark, logo, copyrighted characters, historical real persons, or copied game assets.
Output filename: icon_token_sword_128x128.png
```

### 5.2. Ví dụ prompt character

```text
Create an original PNG game asset for a mobile portrait isekai RPG match-3 game.
Asset ID: char_hero_flame_idle
Size: 512x512
Background: transparent
Style: cute chibi fantasy, bright colors, pixel-inspired, clean readable shape, soft outline, mobile UI friendly, non-violent, no blood, no gore.
Description: original chibi isekai flame squire hero, short cape, small wooden sword, warm orange magical accent, friendly expression, idle pose, full body.
Do not include text, watermark, logo, copyrighted characters, historical real persons, or copied game assets.
Output filename: char_hero_flame_idle_512x512.png
```

### 5.3. Ví dụ prompt background

```text
Create an original PNG game background for a mobile portrait isekai RPG match-3 game.
Asset ID: bg_town_meadow
Size: 1080x1920
Background: full scene
Style: cute chibi fantasy, bright colors, pixel-inspired, clean readable shape, soft outline, mobile UI friendly, non-violent.
Description: peaceful isekai town in a floating meadow, tiny shops, magic gate, blue sky, soft clouds, warm lighting, no text.
Do not include text, watermark, logo, copyrighted characters, historical real persons, or copied game assets.
Output filename: bg_town_meadow_1080x1920.png
```

---

# PART F — ASSET MANIFEST

## 6. Tổng quan số lượng asset MVP

MVP cần khoảng:

```text
Backgrounds: 24
Characters: 36
NPCs: 24
Enemies: 48
Tokens: 16
Skills: 36
Equipment Icons: 72
Item Icons: 48
UI Assets: 80
Tilesets: 24
VFX: 40
Map Objects: 48
Logo/Loading: 8
Total: khoảng 504 PNG
```

Có thể bắt đầu bằng placeholder đơn giản trước, sau đó thay dần.

---

# 7. Background assets

## 7.1. Fullscreen backgrounds — 1080x1920

| ID                       | File                                   | Usage                       |
| ------------------------ | -------------------------------------- | --------------------------- |
| bg_splash_realm_gate     | bg_splash_realm_gate_1080x1920.png     | Splash/loading              |
| bg_title_sky_realm       | bg_title_sky_realm_1080x1920.png       | Title                       |
| bg_character_create_room | bg_character_create_room_1080x1920.png | Character creation          |
| bg_town_meadow           | bg_town_meadow_1080x1920.png           | Main town                   |
| bg_world_map_scroll      | bg_world_map_scroll_1080x1920.png      | World map                   |
| bg_shop_magic_room       | bg_shop_magic_room_1080x1920.png       | Shop                        |
| bg_inventory_table       | bg_inventory_table_1080x1920.png       | Inventory                   |
| bg_forge_room            | bg_forge_room_1080x1920.png            | Blacksmith                  |
| bg_skill_library         | bg_skill_library_1080x1920.png         | Skill screen                |
| bg_cloud_sync            | bg_cloud_sync_1080x1920.png            | Cloud save popup background |

## 7.2. Battle backgrounds — 1080x960

| ID                | File                           | Usage           |
| ----------------- | ------------------------------ | --------------- |
| bg_battle_meadow  | bg_battle_meadow_1080x960.png  | Realm 1 battle  |
| bg_battle_ember   | bg_battle_ember_1080x960.png   | Realm 2 battle  |
| bg_battle_tide    | bg_battle_tide_1080x960.png    | Realm 3 battle  |
| bg_battle_thunder | bg_battle_thunder_1080x960.png | Realm 4 battle  |
| bg_battle_forest  | bg_battle_forest_1080x960.png  | Realm 5 battle  |
| bg_battle_crystal | bg_battle_crystal_1080x960.png | Realm 6 battle  |
| bg_battle_bazaar  | bg_battle_bazaar_1080x960.png  | Realm 7 battle  |
| bg_battle_snow    | bg_battle_snow_1080x960.png    | Realm 8 battle  |
| bg_battle_clock   | bg_battle_clock_1080x960.png   | Realm 9 battle  |
| bg_battle_candy   | bg_battle_candy_1080x960.png   | Realm 10 battle |
| bg_battle_library | bg_battle_library_1080x960.png | Realm 11 battle |
| bg_battle_eclipse | bg_battle_eclipse_1080x960.png | Realm 12 battle |

---

# 8. Player character assets

## 8.1. Base hero sprites — 512x512

| ID                     | File                               |
| ---------------------- | ---------------------------------- |
| char_hero_base_idle    | char_hero_base_idle_512x512.png    |
| char_hero_base_walk_01 | char_hero_base_walk_01_512x512.png |
| char_hero_base_walk_02 | char_hero_base_walk_02_512x512.png |
| char_hero_base_attack  | char_hero_base_attack_512x512.png  |
| char_hero_base_cast    | char_hero_base_cast_512x512.png    |
| char_hero_base_hurt    | char_hero_base_hurt_512x512.png    |
| char_hero_base_victory | char_hero_base_victory_512x512.png |
| char_hero_base_defeat  | char_hero_base_defeat_512x512.png  |

## 8.2. Class hero sprites — 512x512

| ID                      | File                                | Class |
| ----------------------- | ----------------------------------- | ----- |
| char_hero_flame_idle    | char_hero_flame_idle_512x512.png    | Flame |
| char_hero_flame_attack  | char_hero_flame_attack_512x512.png  | Flame |
| char_hero_flame_cast    | char_hero_flame_cast_512x512.png    | Flame |
| char_hero_flame_victory | char_hero_flame_victory_512x512.png | Flame |
| char_hero_tide_idle     | char_hero_tide_idle_512x512.png     | Tide  |
| char_hero_tide_attack   | char_hero_tide_attack_512x512.png   | Tide  |
| char_hero_tide_cast     | char_hero_tide_cast_512x512.png     | Tide  |
| char_hero_tide_victory  | char_hero_tide_victory_512x512.png  | Tide  |
| char_hero_storm_idle    | char_hero_storm_idle_512x512.png    | Storm |
| char_hero_storm_attack  | char_hero_storm_attack_512x512.png  | Storm |
| char_hero_storm_cast    | char_hero_storm_cast_512x512.png    | Storm |
| char_hero_storm_victory | char_hero_storm_victory_512x512.png | Storm |

## 8.3. Portraits — 512x512

| ID                  | File                            |
| ------------------- | ------------------------------- |
| portrait_hero_base  | portrait_hero_base_512x512.png  |
| portrait_hero_flame | portrait_hero_flame_512x512.png |
| portrait_hero_tide  | portrait_hero_tide_512x512.png  |
| portrait_hero_storm | portrait_hero_storm_512x512.png |

---

# 9. NPC assets

## 9.1. NPC full body — 512x512

| ID                   | File                             | Role               |
| -------------------- | -------------------------------- | ------------------ |
| npc_mira_mail_cat    | npc_mira_mail_cat_512x512.png    | Mail/helper        |
| npc_brann_blacksmith | npc_brann_blacksmith_512x512.png | Forge              |
| npc_nami_healer      | npc_nami_healer_512x512.png      | Healer             |
| npc_lumo_star_sprite | npc_lumo_star_sprite_512x512.png | EXP helper         |
| npc_taro_food_golem  | npc_taro_food_golem_512x512.png  | Food helper        |
| npc_yuki_lantern_fox | npc_yuki_lantern_fox_512x512.png | Snow realm         |
| npc_quest_elder      | npc_quest_elder_512x512.png      | Main quest         |
| npc_shop_keeper      | npc_shop_keeper_512x512.png      | Shop               |
| npc_skill_librarian  | npc_skill_librarian_512x512.png  | Skill              |
| npc_gate_guardian    | npc_gate_guardian_512x512.png    | World gate         |
| npc_craft_apprentice | npc_craft_apprentice_512x512.png | Craft              |
| npc_daily_board      | npc_daily_board_512x512.png      | Daily quest mascot |

## 9.2. NPC portraits — 512x512

Dùng cùng ID thêm prefix `portrait_`.

| ID                            | File                                      |
| ----------------------------- | ----------------------------------------- |
| portrait_npc_mira_mail_cat    | portrait_npc_mira_mail_cat_512x512.png    |
| portrait_npc_brann_blacksmith | portrait_npc_brann_blacksmith_512x512.png |
| portrait_npc_nami_healer      | portrait_npc_nami_healer_512x512.png      |
| portrait_npc_lumo_star_sprite | portrait_npc_lumo_star_sprite_512x512.png |
| portrait_npc_taro_food_golem  | portrait_npc_taro_food_golem_512x512.png  |
| portrait_npc_yuki_lantern_fox | portrait_npc_yuki_lantern_fox_512x512.png |
| portrait_npc_quest_elder      | portrait_npc_quest_elder_512x512.png      |
| portrait_npc_shop_keeper      | portrait_npc_shop_keeper_512x512.png      |
| portrait_npc_skill_librarian  | portrait_npc_skill_librarian_512x512.png  |
| portrait_npc_gate_guardian    | portrait_npc_gate_guardian_512x512.png    |
| portrait_npc_craft_apprentice | portrait_npc_craft_apprentice_512x512.png |
| portrait_npc_daily_board      | portrait_npc_daily_board_512x512.png      |

---

# 10. Enemy assets

## 10.1. Normal enemies — 512x512

| ID                     | File                               | Realm |
| ---------------------- | ---------------------------------- | ----- |
| enemy_meadow_slime     | enemy_meadow_slime_512x512.png     | R01   |
| enemy_meadow_mushroom  | enemy_meadow_mushroom_512x512.png  | R01   |
| enemy_meadow_leaf_bug  | enemy_meadow_leaf_bug_512x512.png  | R01   |
| enemy_ember_boar       | enemy_ember_boar_512x512.png       | R02   |
| enemy_ember_sprite     | enemy_ember_sprite_512x512.png     | R02   |
| enemy_ember_pumpkin    | enemy_ember_pumpkin_512x512.png    | R02   |
| enemy_tide_bubble      | enemy_tide_bubble_512x512.png      | R03   |
| enemy_tide_crab        | enemy_tide_crab_512x512.png        | R03   |
| enemy_tide_jelly       | enemy_tide_jelly_512x512.png       | R03   |
| enemy_thunder_bird     | enemy_thunder_bird_512x512.png     | R04   |
| enemy_thunder_rat      | enemy_thunder_rat_512x512.png      | R04   |
| enemy_thunder_cloud    | enemy_thunder_cloud_512x512.png    | R04   |
| enemy_forest_treeling  | enemy_forest_treeling_512x512.png  | R05   |
| enemy_forest_sprite    | enemy_forest_sprite_512x512.png    | R05   |
| enemy_forest_beetle    | enemy_forest_beetle_512x512.png    | R05   |
| enemy_crystal_goblin   | enemy_crystal_goblin_512x512.png   | R06   |
| enemy_crystal_bat      | enemy_crystal_bat_512x512.png      | R06   |
| enemy_crystal_mimic    | enemy_crystal_mimic_512x512.png    | R06   |
| enemy_bazaar_mask      | enemy_bazaar_mask_512x512.png      | R07   |
| enemy_bazaar_coin_imp  | enemy_bazaar_coin_imp_512x512.png  | R07   |
| enemy_bazaar_carpet    | enemy_bazaar_carpet_512x512.png    | R07   |
| enemy_snow_penguin     | enemy_snow_penguin_512x512.png     | R08   |
| enemy_snow_wisp        | enemy_snow_wisp_512x512.png        | R08   |
| enemy_snow_fox         | enemy_snow_fox_512x512.png         | R08   |
| enemy_clock_gearling   | enemy_clock_gearling_512x512.png   | R09   |
| enemy_clock_mouse      | enemy_clock_mouse_512x512.png      | R09   |
| enemy_clock_hourglass  | enemy_clock_hourglass_512x512.png  | R09   |
| enemy_candy_gummy      | enemy_candy_gummy_512x512.png      | R10   |
| enemy_candy_cupcake    | enemy_candy_cupcake_512x512.png    | R10   |
| enemy_candy_syrup      | enemy_candy_syrup_512x512.png      | R10   |
| enemy_library_bookbat  | enemy_library_bookbat_512x512.png  | R11   |
| enemy_library_inkling  | enemy_library_inkling_512x512.png  | R11   |
| enemy_library_rune_cat | enemy_library_rune_cat_512x512.png | R11   |
| enemy_eclipse_shadow   | enemy_eclipse_shadow_512x512.png   | R12   |
| enemy_eclipse_orb      | enemy_eclipse_orb_512x512.png      | R12   |
| enemy_eclipse_knight   | enemy_eclipse_knight_512x512.png   | R12   |

## 10.2. Boss enemies — 768x768

| ID                  | File                            | Realm |
| ------------------- | ------------------------------- | ----- |
| boss_slime_king     | boss_slime_king_768x768.png     | R01   |
| boss_cinder_boar    | boss_cinder_boar_768x768.png    | R02   |
| boss_bubble_serpent | boss_bubble_serpent_768x768.png | R03   |
| boss_storm_roc      | boss_storm_roc_768x768.png      | R04   |
| boss_elder_treant   | boss_elder_treant_768x768.png   | R05   |
| boss_gem_golem      | boss_gem_golem_768x768.png      | R06   |
| boss_mask_merchant  | boss_mask_merchant_768x768.png  | R07   |
| boss_frost_kitsune  | boss_frost_kitsune_768x768.png  | R08   |
| boss_time_warden    | boss_time_warden_768x768.png    | R09   |
| boss_sugar_dragon   | boss_sugar_dragon_768x768.png   | R10   |
| boss_rune_owl       | boss_rune_owl_768x768.png       | R11   |
| boss_void_prince    | boss_void_prince_768x768.png    | R12   |

---

# 11. Match-3 token assets

## 11.1. Basic tokens — 128x128

| ID                | File                          | Effect         |
| ----------------- | ----------------------------- | -------------- |
| icon_token_sword  | icon_token_sword_128x128.png  | Attack         |
| icon_token_heart  | icon_token_heart_128x128.png  | Heal           |
| icon_token_coin   | icon_token_coin_128x128.png   | Gold           |
| icon_token_food   | icon_token_food_128x128.png   | Food           |
| icon_token_book   | icon_token_book_128x128.png   | EXP bonus      |
| icon_token_mana   | icon_token_mana_128x128.png   | Mana           |
| icon_token_shield | icon_token_shield_128x128.png | Shield         |
| icon_token_star   | icon_token_star_128x128.png   | Class resource |

## 11.2. Special tokens — 128x128

| ID                       | File                                 | Effect         |
| ------------------------ | ------------------------------------ | -------------- |
| icon_token_row_rune      | icon_token_row_rune_128x128.png      | Clear row      |
| icon_token_column_rune   | icon_token_column_rune_128x128.png   | Clear column   |
| icon_token_bomb_rune     | icon_token_bomb_rune_128x128.png     | 3x3 explosion  |
| icon_token_realm_crystal | icon_token_realm_crystal_128x128.png | Clear one type |
| icon_token_locked        | icon_token_locked_128x128.png        | Locked overlay |
| icon_token_ice_overlay   | icon_token_ice_overlay_128x128.png   | Frozen overlay |
| icon_token_thorn_overlay | icon_token_thorn_overlay_128x128.png | Thorn overlay  |
| icon_token_time_overlay  | icon_token_time_overlay_128x128.png  | Timed overlay  |

---

# 12. Skill icons

## 12.1. Flame class — 128x128

| ID                      | File                                |
| ----------------------- | ----------------------------------- |
| skill_flame_spark_slash | skill_flame_spark_slash_128x128.png |
| skill_flame_ember_combo | skill_flame_ember_combo_128x128.png |
| skill_flame_burst       | skill_flame_burst_128x128.png       |
| skill_flame_warm_heart  | skill_flame_warm_heart_128x128.png  |
| skill_flame_sword_rain  | skill_flame_sword_rain_128x128.png  |
| skill_flame_fire_rune   | skill_flame_fire_rune_128x128.png   |

## 12.2. Tide class — 128x128

| ID                      | File                                |
| ----------------------- | ----------------------------------- |
| skill_tide_aqua_heal    | skill_tide_aqua_heal_128x128.png    |
| skill_tide_bubble_guard | skill_tide_bubble_guard_128x128.png |
| skill_tide_moon_tide    | skill_tide_moon_tide_128x128.png    |
| skill_tide_gentle_flow  | skill_tide_gentle_flow_128x128.png  |
| skill_tide_cleanse_wave | skill_tide_cleanse_wave_128x128.png |
| skill_tide_shield_rain  | skill_tide_shield_rain_128x128.png  |

## 12.3. Storm class — 128x128

| ID                        | File                                  |
| ------------------------- | ------------------------------------- |
| skill_storm_quick_jab     | skill_storm_quick_jab_128x128.png     |
| skill_storm_static_step   | skill_storm_static_step_128x128.png   |
| skill_storm_thunder_chain | skill_storm_thunder_chain_128x128.png |
| skill_storm_lucky_spark   | skill_storm_lucky_spark_128x128.png   |
| skill_storm_tile_swap     | skill_storm_tile_swap_128x128.png     |
| skill_storm_extra_turn    | skill_storm_extra_turn_128x128.png    |

## 12.4. Enemy skills — 128x128

| ID                       | File                                 |
| ------------------------ | ------------------------------------ |
| skill_enemy_slam         | skill_enemy_slam_128x128.png         |
| skill_enemy_heal         | skill_enemy_heal_128x128.png         |
| skill_enemy_lock_tile    | skill_enemy_lock_tile_128x128.png    |
| skill_enemy_freeze_tile  | skill_enemy_freeze_tile_128x128.png  |
| skill_enemy_poison_thorn | skill_enemy_poison_thorn_128x128.png |
| skill_enemy_shuffle      | skill_enemy_shuffle_128x128.png      |
| skill_enemy_countdown    | skill_enemy_countdown_128x128.png    |
| skill_enemy_dark_wave    | skill_enemy_dark_wave_128x128.png    |

---

# 13. Equipment icons

## 13.1. Weapons — 128x128

| ID                         | File                                   |
| -------------------------- | -------------------------------------- |
| equip_weapon_wooden_sword  | equip_weapon_wooden_sword_128x128.png  |
| equip_weapon_copper_sword  | equip_weapon_copper_sword_128x128.png  |
| equip_weapon_flame_sword   | equip_weapon_flame_sword_128x128.png   |
| equip_weapon_tide_wand     | equip_weapon_tide_wand_128x128.png     |
| equip_weapon_storm_dagger  | equip_weapon_storm_dagger_128x128.png  |
| equip_weapon_crystal_staff | equip_weapon_crystal_staff_128x128.png |
| equip_weapon_moon_blade    | equip_weapon_moon_blade_128x128.png    |
| equip_weapon_star_scepter  | equip_weapon_star_scepter_128x128.png  |
| equip_weapon_candy_hammer  | equip_weapon_candy_hammer_128x128.png  |
| equip_weapon_void_lantern  | equip_weapon_void_lantern_128x128.png  |
| equip_weapon_training_bow  | equip_weapon_training_bow_128x128.png  |
| equip_weapon_rune_book     | equip_weapon_rune_book_128x128.png     |

## 13.2. Armor — 128x128

| ID                        | File                                  |
| ------------------------- | ------------------------------------- |
| equip_armor_traveler_coat | equip_armor_traveler_coat_128x128.png |
| equip_armor_leaf_vest     | equip_armor_leaf_vest_128x128.png     |
| equip_armor_ember_jacket  | equip_armor_ember_jacket_128x128.png  |
| equip_armor_tide_robe     | equip_armor_tide_robe_128x128.png     |
| equip_armor_storm_cloak   | equip_armor_storm_cloak_128x128.png   |
| equip_armor_crystal_mail  | equip_armor_crystal_mail_128x128.png  |
| equip_armor_snow_parka    | equip_armor_snow_parka_128x128.png    |
| equip_armor_clock_coat    | equip_armor_clock_coat_128x128.png    |
| equip_armor_candy_apron   | equip_armor_candy_apron_128x128.png   |
| equip_armor_eclipse_garb  | equip_armor_eclipse_garb_128x128.png  |

## 13.3. Headgear — 128x128

| ID                       | File                                 |
| ------------------------ | ------------------------------------ |
| equip_head_leaf_hood     | equip_head_leaf_hood_128x128.png     |
| equip_head_mage_hat      | equip_head_mage_hat_128x128.png      |
| equip_head_ember_cap     | equip_head_ember_cap_128x128.png     |
| equip_head_tide_crown    | equip_head_tide_crown_128x128.png    |
| equip_head_storm_goggles | equip_head_storm_goggles_128x128.png |
| equip_head_crystal_helm  | equip_head_crystal_helm_128x128.png  |
| equip_head_snow_earmuffs | equip_head_snow_earmuffs_128x128.png |
| equip_head_rune_glasses  | equip_head_rune_glasses_128x128.png  |

## 13.4. Boots — 128x128

| ID                   | File                             |
| -------------------- | -------------------------------- |
| equip_boots_traveler | equip_boots_traveler_128x128.png |
| equip_boots_leaf     | equip_boots_leaf_128x128.png     |
| equip_boots_ember    | equip_boots_ember_128x128.png    |
| equip_boots_tide     | equip_boots_tide_128x128.png     |
| equip_boots_storm    | equip_boots_storm_128x128.png    |
| equip_boots_crystal  | equip_boots_crystal_128x128.png  |
| equip_boots_snow     | equip_boots_snow_128x128.png     |
| equip_boots_clock    | equip_boots_clock_128x128.png    |

## 13.5. Rings and charms — 128x128

| ID                 | File                           |
| ------------------ | ------------------------------ |
| equip_ring_lucky   | equip_ring_lucky_128x128.png   |
| equip_ring_flame   | equip_ring_flame_128x128.png   |
| equip_ring_tide    | equip_ring_tide_128x128.png    |
| equip_ring_storm   | equip_ring_storm_128x128.png   |
| equip_ring_crystal | equip_ring_crystal_128x128.png |
| equip_ring_eclipse | equip_ring_eclipse_128x128.png |
| equip_charm_realm  | equip_charm_realm_128x128.png  |
| equip_charm_food   | equip_charm_food_128x128.png   |
| equip_charm_exp    | equip_charm_exp_128x128.png    |
| equip_charm_gold   | equip_charm_gold_128x128.png   |
| equip_charm_drop   | equip_charm_drop_128x128.png   |
| equip_charm_mana   | equip_charm_mana_128x128.png   |

---

# 14. Consumable and material icons

## 14.1. Consumables — 128x128

| ID                  | File                            |
| ------------------- | ------------------------------- |
| item_potion_small   | item_potion_small_128x128.png   |
| item_potion_medium  | item_potion_medium_128x128.png  |
| item_potion_large   | item_potion_large_128x128.png   |
| item_food_basket    | item_food_basket_128x128.png    |
| item_shuffle_bell   | item_shuffle_bell_128x128.png   |
| item_lucky_cookie   | item_lucky_cookie_128x128.png   |
| item_repair_hammer  | item_repair_hammer_128x128.png  |
| item_skill_scroll   | item_skill_scroll_128x128.png   |
| item_revive_feather | item_revive_feather_128x128.png |
| item_map_ticket     | item_map_ticket_128x128.png     |

## 14.2. Materials — 128x128

| ID                  | File                            |
| ------------------- | ------------------------------- |
| mat_slime_jelly     | mat_slime_jelly_128x128.png     |
| mat_ember_dust      | mat_ember_dust_128x128.png      |
| mat_tide_pearl      | mat_tide_pearl_128x128.png      |
| mat_thunder_feather | mat_thunder_feather_128x128.png |
| mat_rootwood_bark   | mat_rootwood_bark_128x128.png   |
| mat_crystal_shard   | mat_crystal_shard_128x128.png   |
| mat_moon_ticket     | mat_moon_ticket_128x128.png     |
| mat_snow_flake      | mat_snow_flake_128x128.png      |
| mat_clock_gear      | mat_clock_gear_128x128.png      |
| mat_candy_sugar     | mat_candy_sugar_128x128.png     |
| mat_star_ink        | mat_star_ink_128x128.png        |
| mat_eclipse_core    | mat_eclipse_core_128x128.png    |

## 14.3. Currency icons — 128x128

| ID                     | File                               |
| ---------------------- | ---------------------------------- |
| currency_gold          | currency_gold_128x128.png          |
| currency_soul_gem      | currency_soul_gem_128x128.png      |
| currency_realm_token   | currency_realm_token_128x128.png   |
| currency_material_pack | currency_material_pack_128x128.png |

---

# 15. UI assets

## 15.1. Panels — 512x512 or 768x512

| ID                   | File                             |
| -------------------- | -------------------------------- |
| ui_panel_main        | ui_panel_main_768x512.png        |
| ui_panel_popup       | ui_panel_popup_768x512.png       |
| ui_panel_dark        | ui_panel_dark_768x512.png        |
| ui_panel_light       | ui_panel_light_768x512.png       |
| ui_panel_reward      | ui_panel_reward_768x512.png      |
| ui_panel_item_detail | ui_panel_item_detail_768x512.png |
| ui_panel_battle_info | ui_panel_battle_info_768x256.png |
| ui_panel_stage_card  | ui_panel_stage_card_768x512.png  |
| ui_panel_cloud_save  | ui_panel_cloud_save_768x512.png  |
| ui_panel_shop_card   | ui_panel_shop_card_512x512.png   |

## 15.2. Buttons — 384x128

| ID               | File                         |
| ---------------- | ---------------------------- |
| ui_btn_primary   | ui_btn_primary_384x128.png   |
| ui_btn_secondary | ui_btn_secondary_384x128.png |
| ui_btn_danger    | ui_btn_danger_384x128.png    |
| ui_btn_disabled  | ui_btn_disabled_384x128.png  |
| ui_btn_green     | ui_btn_green_384x128.png     |
| ui_btn_blue      | ui_btn_blue_384x128.png      |
| ui_btn_gold      | ui_btn_gold_384x128.png      |
| ui_btn_purple    | ui_btn_purple_384x128.png    |

## 15.3. Small buttons — 128x128

| ID              | File                        |
| --------------- | --------------------------- |
| ui_btn_close    | ui_btn_close_128x128.png    |
| ui_btn_back     | ui_btn_back_128x128.png     |
| ui_btn_settings | ui_btn_settings_128x128.png |
| ui_btn_info     | ui_btn_info_128x128.png     |
| ui_btn_plus     | ui_btn_plus_128x128.png     |
| ui_btn_minus    | ui_btn_minus_128x128.png    |
| ui_btn_refresh  | ui_btn_refresh_128x128.png  |
| ui_btn_lock     | ui_btn_lock_128x128.png     |
| ui_btn_unlock   | ui_btn_unlock_128x128.png   |
| ui_btn_sort     | ui_btn_sort_128x128.png     |

## 15.4. Bars — 512x64

| ID                  | File                           |
| ------------------- | ------------------------------ |
| ui_bar_hp_bg        | ui_bar_hp_bg_512x64.png        |
| ui_bar_hp_fill      | ui_bar_hp_fill_512x64.png      |
| ui_bar_mana_bg      | ui_bar_mana_bg_512x64.png      |
| ui_bar_mana_fill    | ui_bar_mana_fill_512x64.png    |
| ui_bar_exp_bg       | ui_bar_exp_bg_512x64.png       |
| ui_bar_exp_fill     | ui_bar_exp_fill_512x64.png     |
| ui_bar_food_bg      | ui_bar_food_bg_512x64.png      |
| ui_bar_food_fill    | ui_bar_food_fill_512x64.png    |
| ui_bar_loading_bg   | ui_bar_loading_bg_768x64.png   |
| ui_bar_loading_fill | ui_bar_loading_fill_768x64.png |

## 15.5. Navigation icons — 128x128

| ID               | File                         |
| ---------------- | ---------------------------- |
| ui_nav_adventure | ui_nav_adventure_128x128.png |
| ui_nav_hero      | ui_nav_hero_128x128.png      |
| ui_nav_bag       | ui_nav_bag_128x128.png       |
| ui_nav_quest     | ui_nav_quest_128x128.png     |
| ui_nav_shop      | ui_nav_shop_128x128.png      |
| ui_nav_home      | ui_nav_home_128x128.png      |

## 15.6. Popup icons — 128x128

| ID              | File                        |
| --------------- | --------------------------- |
| ui_icon_warning | ui_icon_warning_128x128.png |
| ui_icon_success | ui_icon_success_128x128.png |
| ui_icon_error   | ui_icon_error_128x128.png   |
| ui_icon_cloud   | ui_icon_cloud_128x128.png   |
| ui_icon_offline | ui_icon_offline_128x128.png |
| ui_icon_google  | ui_icon_google_128x128.png  |
| ui_icon_guest   | ui_icon_guest_128x128.png   |
| ui_icon_restore | ui_icon_restore_128x128.png |

## 15.7. Item frames — 128x128

| ID                 | File                           |
| ------------------ | ------------------------------ |
| ui_frame_common    | ui_frame_common_128x128.png    |
| ui_frame_uncommon  | ui_frame_uncommon_128x128.png  |
| ui_frame_rare      | ui_frame_rare_128x128.png      |
| ui_frame_epic      | ui_frame_epic_128x128.png      |
| ui_frame_legendary | ui_frame_legendary_128x128.png |
| ui_frame_selected  | ui_frame_selected_128x128.png  |
| ui_frame_locked    | ui_frame_locked_128x128.png    |
| ui_frame_new       | ui_frame_new_128x128.png       |

---

# 16. Tileset assets

## 16.1. Ground tiles — 256x256

| ID                  | File                            |
| ------------------- | ------------------------------- |
| tile_ground_meadow  | tile_ground_meadow_256x256.png  |
| tile_ground_ember   | tile_ground_ember_256x256.png   |
| tile_ground_tide    | tile_ground_tide_256x256.png    |
| tile_ground_thunder | tile_ground_thunder_256x256.png |
| tile_ground_forest  | tile_ground_forest_256x256.png  |
| tile_ground_crystal | tile_ground_crystal_256x256.png |
| tile_ground_bazaar  | tile_ground_bazaar_256x256.png  |
| tile_ground_snow    | tile_ground_snow_256x256.png    |
| tile_ground_clock   | tile_ground_clock_256x256.png   |
| tile_ground_candy   | tile_ground_candy_256x256.png   |
| tile_ground_library | tile_ground_library_256x256.png |
| tile_ground_eclipse | tile_ground_eclipse_256x256.png |

## 16.2. Platform tiles — 256x256

| ID                    | File                              |
| --------------------- | --------------------------------- |
| tile_platform_meadow  | tile_platform_meadow_256x256.png  |
| tile_platform_ember   | tile_platform_ember_256x256.png   |
| tile_platform_tide    | tile_platform_tide_256x256.png    |
| tile_platform_thunder | tile_platform_thunder_256x256.png |
| tile_platform_forest  | tile_platform_forest_256x256.png  |
| tile_platform_crystal | tile_platform_crystal_256x256.png |
| tile_platform_snow    | tile_platform_snow_256x256.png    |
| tile_platform_eclipse | tile_platform_eclipse_256x256.png |

## 16.3. Decoration tiles — 256x256

| ID                   | File                             |
| -------------------- | -------------------------------- |
| deco_tree_meadow     | deco_tree_meadow_256x256.png     |
| deco_flower_meadow   | deco_flower_meadow_256x256.png   |
| deco_lantern_town    | deco_lantern_town_256x256.png    |
| deco_shop_sign       | deco_shop_sign_256x256.png       |
| deco_magic_gate      | deco_magic_gate_256x256.png      |
| deco_crystal_cluster | deco_crystal_cluster_256x256.png |
| deco_snow_lantern    | deco_snow_lantern_256x256.png    |
| deco_clock_gear      | deco_clock_gear_256x256.png      |
| deco_candy_tree      | deco_candy_tree_256x256.png      |
| deco_library_shelf   | deco_library_shelf_256x256.png   |
| deco_eclipse_crystal | deco_eclipse_crystal_256x256.png |
| deco_treasure_chest  | deco_treasure_chest_256x256.png  |

---

# 17. World map assets

## 17.1. Realm nodes — 192x192

| ID                        | File                                  |
| ------------------------- | ------------------------------------- |
| map_node_realm_01_meadow  | map_node_realm_01_meadow_192x192.png  |
| map_node_realm_02_ember   | map_node_realm_02_ember_192x192.png   |
| map_node_realm_03_tide    | map_node_realm_03_tide_192x192.png    |
| map_node_realm_04_thunder | map_node_realm_04_thunder_192x192.png |
| map_node_realm_05_forest  | map_node_realm_05_forest_192x192.png  |
| map_node_realm_06_crystal | map_node_realm_06_crystal_192x192.png |
| map_node_realm_07_bazaar  | map_node_realm_07_bazaar_192x192.png  |
| map_node_realm_08_snow    | map_node_realm_08_snow_192x192.png    |
| map_node_realm_09_clock   | map_node_realm_09_clock_192x192.png   |
| map_node_realm_10_candy   | map_node_realm_10_candy_192x192.png   |
| map_node_realm_11_library | map_node_realm_11_library_192x192.png |
| map_node_realm_12_eclipse | map_node_realm_12_eclipse_192x192.png |

## 17.2. Stage node states — 128x128

| ID                  | File                            |
| ------------------- | ------------------------------- |
| map_stage_locked    | map_stage_locked_128x128.png    |
| map_stage_available | map_stage_available_128x128.png |
| map_stage_completed | map_stage_completed_128x128.png |
| map_stage_boss      | map_stage_boss_128x128.png      |
| map_stage_chest     | map_stage_chest_128x128.png     |
| map_stage_farm      | map_stage_farm_128x128.png      |

---

# 18. VFX assets

## 18.1. Battle VFX — 256x256

| ID                 | File                           |
| ------------------ | ------------------------------ |
| vfx_match_pop      | vfx_match_pop_256x256.png      |
| vfx_match_sparkle  | vfx_match_sparkle_256x256.png  |
| vfx_combo_burst    | vfx_combo_burst_256x256.png    |
| vfx_attack_slash   | vfx_attack_slash_256x256.png   |
| vfx_heal_heart     | vfx_heal_heart_256x256.png     |
| vfx_shield_bubble  | vfx_shield_bubble_256x256.png  |
| vfx_mana_glow      | vfx_mana_glow_256x256.png      |
| vfx_coin_pop       | vfx_coin_pop_256x256.png       |
| vfx_food_pop       | vfx_food_pop_256x256.png       |
| vfx_exp_book       | vfx_exp_book_256x256.png       |
| vfx_row_clear      | vfx_row_clear_256x256.png      |
| vfx_column_clear   | vfx_column_clear_256x256.png   |
| vfx_bomb_explosion | vfx_bomb_explosion_256x256.png |
| vfx_crystal_flash  | vfx_crystal_flash_256x256.png  |
| vfx_level_up       | vfx_level_up_256x256.png       |
| vfx_item_drop      | vfx_item_drop_256x256.png      |

## 18.2. Skill VFX — 512x512

| ID                    | File                              |
| --------------------- | --------------------------------- |
| vfx_skill_flame_burst | vfx_skill_flame_burst_512x512.png |
| vfx_skill_tide_wave   | vfx_skill_tide_wave_512x512.png   |
| vfx_skill_storm_chain | vfx_skill_storm_chain_512x512.png |
| vfx_skill_cleanse     | vfx_skill_cleanse_512x512.png     |
| vfx_skill_extra_turn  | vfx_skill_extra_turn_512x512.png  |
| vfx_skill_tile_swap   | vfx_skill_tile_swap_512x512.png   |
| vfx_skill_enemy_dark  | vfx_skill_enemy_dark_512x512.png  |
| vfx_skill_boss_phase  | vfx_skill_boss_phase_512x512.png  |

---

# 19. Logo and loading assets

| ID                    | File                              | Size      |
| --------------------- | --------------------------------- | --------- |
| logo_game_main        | logo_game_main_768x384.png        | 768x384   |
| logo_game_small       | logo_game_small_512x256.png       | 512x256   |
| loading_mascot        | loading_mascot_512x512.png        | 512x512   |
| loading_spinner_realm | loading_spinner_realm_256x256.png | 256x256   |
| loading_bar_frame     | loading_bar_frame_768x96.png      | 768x96    |
| loading_bar_fill      | loading_bar_fill_768x96.png       | 768x96    |
| icon_app              | icon_app_1024x1024.png            | 1024x1024 |
| icon_notification     | icon_notification_256x256.png     | 256x256   |

---

# 20. Asset generation priority

AI không cần tạo 500 ảnh ngay từ đầu. Làm theo thứ tự:

## Priority 1 — playable prototype

```text
bg_title_sky_realm_1080x1920.png
bg_town_meadow_1080x1920.png
bg_world_map_scroll_1080x1920.png
bg_battle_meadow_1080x960.png

char_hero_flame_idle_512x512.png
char_hero_flame_attack_512x512.png
char_hero_flame_cast_512x512.png
char_hero_flame_hurt_512x512.png

enemy_meadow_slime_512x512.png
boss_slime_king_768x768.png

icon_token_sword_128x128.png
icon_token_heart_128x128.png
icon_token_coin_128x128.png
icon_token_food_128x128.png
icon_token_book_128x128.png
icon_token_mana_128x128.png
icon_token_shield_128x128.png
icon_token_star_128x128.png

ui_panel_main_768x512.png
ui_panel_popup_768x512.png
ui_btn_primary_384x128.png
ui_btn_secondary_384x128.png
ui_btn_close_128x128.png

ui_bar_hp_bg_512x64.png
ui_bar_hp_fill_512x64.png
ui_bar_mana_bg_512x64.png
ui_bar_mana_fill_512x64.png

currency_gold_128x128.png
currency_soul_gem_128x128.png
```

## Priority 2 — MVP content

```text
R01-R03 backgrounds
3 hero classes
12 normal enemies
3 bosses
all basic UI panels
all skill icons for 3 classes
basic equipment icons
basic item/material icons
```

## Priority 3 — full campaign

```text
R04-R12 backgrounds
all enemies
all bosses
all realm map nodes
advanced VFX
cosmetics
seasonal assets
```

---

# 21. Prompt batch mẫu cho Codex/Unity AI

## 21.1. Prompt tạo asset placeholder bằng Unity/Codex

```text
Read docs/ai_rules.md and docs/asset_manifest.md.

Create a Unity Editor tool named GameAssetPngGenerator.

The tool must generate placeholder PNG files for all Priority 1 assets listed in the manifest.

Requirements:
- Output to Assets/_Game/Art/Generated/
- Use the exact filename format object_widthxheight.png
- Create transparent PNGs for characters, enemies, icons, UI, tokens, VFX
- Create full background PNGs for backgrounds
- Also create matching .json metadata files
- Use simple procedural shapes, gradients, and readable silhouettes
- Do not use copyrighted images or external downloads
- After generation, refresh AssetDatabase
- Set import settings to Sprite
```

## 21.2. Prompt tạo production art bằng Unity AI image generation

```text
Read docs/ai_rules.md and docs/asset_manifest.md.

Generate original production-quality PNG assets for all Priority 1 assets.

For every asset:
- Use the exact ID and filename from the manifest
- Use the specified size
- Use transparent background if the asset is character, enemy, icon, token, UI, VFX
- Use full background if the asset is bg_*
- Use cute chibi isekai fantasy style
- Do not include text except logo assets
- Do not include watermark
- Do not copy any existing game asset
- Save each PNG to the correct folder
- Create a sidecar JSON metadata file with id, filename, size, category, usage, style, transparent, prompt
```

## 21.3. Prompt tạo AssetManifest ScriptableObject

```text
Read docs/asset_manifest.md.

Create a Unity ScriptableObject called GameAssetManifest with entries for all assets.

Each entry must include:
- id
- fileName
- relativePath
- width
- height
- category
- transparent
- priority
- Sprite reference

Create an editor menu:
Tools/Isekai 12 Realms/Rebuild Asset Manifest

The menu scans Assets/_Game/Art/Generated and automatically links PNG sprites to manifest entries by filename.
```

## 21.4. Prompt tạo UI từ asset

```text
Read docs/spec.md, docs/ai_rules.md, and docs/asset_manifest.md.

Create the mobile portrait UI layout for:
- Title Screen
- Main Town
- World Map
- Battle Screen
- Inventory
- Hero
- Skill
- Equipment
- Shop
- Settings
- Battle Result Popup
- Cloud Save Popup

Use RootCanvas/SafeAreaRoot layer structure.
Use generated PNG assets from AssetManifest.
Do not hard-code sprite paths in UI scripts.
All text must use TextMeshPro, not baked into PNG.
```

---

# 22. Definition of Done

Asset generation is considered done when:

```text
1. Every generated PNG follows object_widthxheight.png.
2. Every PNG has matching .json metadata.
3. Unity imports every PNG as Sprite.
4. Transparent assets have alpha background.
5. No image contains watermark.
6. No image contains copied old game UI or old character.
7. UI can load sprites from AssetManifest.
8. Missing asset fallback exists.
9. Game can run with placeholder art.
10. Production art can replace placeholder art without code changes.
```

---

# 23. Fallback missing asset

Codex phải tạo fallback sprite:

```text
missing_sprite_128x128.png
```

Nếu asset không tồn tại, UI/game không được crash.
Hiển thị placeholder màu tím với icon dấu hỏi.

File:

```text
missing_sprite_128x128.png
```

---

# 24. Final note for AI agents

Always prioritize a playable game over perfect art.

Order:

```text
1. Generate placeholder PNGs.
2. Build UI and gameplay using placeholder PNGs.
3. Connect AssetManifest.
4. Replace placeholder with production PNGs later.
5. Never block gameplay implementation waiting for final art.
```
