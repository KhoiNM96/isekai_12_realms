# AI RULES ISEKAI 12 REALMS REBORN

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