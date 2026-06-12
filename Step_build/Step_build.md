# Unity
1. Tạo Unity project
2. Tạo folder chuẩn
3. Tạo docs/spec.md + docs/ai_rules.md + docs/asset_manifest.md
4. Bắt AI tạo asset pipeline placeholder PNG
5. Bắt AI tạo UI shell mobile dọc
6. Bắt AI tạo battle match-3 prototype
7. Sau đó mới Firebase/IAP

## Bước 1 — Tạo Unity project

Trong Unity Hub:

New Project
Template: 2D hoặc 2D URP
Project name: Isekai12Realms
Location: thư mục dễ quản lý, ví dụ D:/UnityProjects/Isekai12Realms

Khuyến nghị:

Nếu máy ổn: dùng 2D URP
Nếu muốn đơn giản, ít lỗi package: dùng 2D Core

Với game này, 2D Core là đủ cho MVP. Sau này vẫn có thể thêm URP nếu cần hiệu ứng đẹp hơn.

## Bước 2 — Cấu hình project ngay từ đầu

Vào:

Edit > Project Settings > Player

Set:

Company Name: tên studio của bạn
Product Name: Isekai 12 Realms
Default Orientation: Portrait
Allowed Orientations: chỉ Portrait

Android:

Package Name: com.yourstudio.isekai12realms
Scripting Backend: IL2CPP
Target Architectures: ARM64
Minimum API Level: Android 6.0 hoặc cao hơn

Canvas sau này dùng:

Reference Resolution: 1080 x 1920
Screen Match Mode: Match Width Or Height
Match: 0.5


## Bước 3 — Tạo folder chuẩn trong Unity

Trong Assets, tạo cấu trúc này trước:

Assets/
  _Game/
    Art/
      Generated/
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
        Loading/
        Meta/
    Audio/
    Prefabs/
    Scenes/
    Scripts/
      Core/
      Services/
      Data/
      Battle/
      Board/
      Character/
      Inventory/
      Quest/
      UI/
      Firebase/
      IAP/
      Editor/
    ScriptableObjects/
      AssetManifest/
      Characters/
      Skills/
      Items/
      Equipment/
      Stages/
      Enemies/
      DropTables/
      Economy/

Ngoài Unity project, ở root project tạo:

docs/
  spec.md
  ai_rules.md
  asset_manifest.md

## Bước 4 — Đưa spec/rule vào docs

Bạn nên tạo 3 file:

docs/spec.md
docs/ai_rules.md
docs/asset_manifest.md

Trong đó:

File	Nội dung
spec.md	Toàn bộ gameplay, UI, Firebase, IAP, hệ thống battle
ai_rules.md	Rule cho Codex/Unity AI, naming PNG, không copy asset cũ
asset_manifest.md	Danh sách toàn bộ ảnh cần tạo

Sau đó mới cho Codex hoặc Unity AI đọc.

## Bước 5 — Prompt đầu tiên cho Codex

Dùng prompt này trước, chưa cần làm gameplay:

### Prompt 1

Read docs/spec.md, docs/ai_rules.md, and docs/asset_manifest.md.

This is a new Unity mobile portrait 2D offline RPG match-3 project.

First, create the project foundation only:
- Create the folder structure under Assets/_Game
- Create BootScene and GameScene
- Create RootCanvas with SafeAreaRoot and layers:
  BackgroundLayer, MainLayer, HudLayer, NavigationLayer, PopupLayer, ToastLayer, LoadingLayer
- Create GameManager
- Create ServiceLocator
- Create GameStateMachine
- Create PopupService
- Create SaveService with local JSON save placeholder
- Create GameAssetManifest ScriptableObject
- Create an Editor tool named GameAssetPngGenerator
- The editor tool must generate Priority 1 placeholder PNG assets using the exact filename format object_widthxheight.png
- Generate matching .json metadata for each PNG
- Import generated PNGs as Sprite
- Do not implement battle yet
- Do not implement Firebase yet
- Do not implement IAP yet
- Keep all systems modular and replaceable

### Prompt 2

Read docs/spec.md, docs/ai_rules.md, and the current Unity project.

The game currently runs without errors but only shows a blue screen. This means the camera renders but the visible UI shell is missing or inactive.

Fix the project foundation so the game visibly shows a playable UI shell.

Requirements:

1. Inspect the current BootScene and GameScene.
2. Do not implement battle, Firebase, IAP, inventory, or full gameplay yet.
3. Ensure both scenes exist:
   - Assets/_Game/Scenes/BootScene.unity
   - Assets/_Game/Scenes/GameScene.unity

4. Ensure the scenes are added to Build Settings in this order:
   - BootScene
   - GameScene

5. BootScene must contain:
   - Main Camera
   - EventSystem
   - GameManager
   - RootCanvas
   - A visible loading screen with:
     - full-screen background Image
     - centered TextMeshPro text: "Isekai 12 Realms"
     - TextMeshPro text: "Loading..."
     - visible loading bar placeholder
   - After 1 second in Play Mode, load GameScene.

6. GameScene must contain:
   - Main Camera
   - EventSystem
   - GameManager if needed
   - RootCanvas
   - SafeAreaRoot
   - BackgroundLayer
   - MainLayer
   - HudLayer
   - NavigationLayer
   - PopupLayer
   - ToastLayer
   - LoadingLayer

7. In GameScene, create a visible Title Screen UI by default:
   - full-screen fantasy colored background, can be a simple Image color if sprite is missing
   - title text: "ISEKAI 12 REALMS"
   - subtitle text: "Offline Match-3 RPG"
   - button: "Start Game"
   - button: "Settings"
   - button: "Quit"
   - bottom small version text: "v0.1.0"

8. Pressing "Start Game" must switch to a visible Main Town placeholder in the same GameScene:
   - top HUD with player name "Guest Hero"
   - level text "Lv. 1"
   - gold text "Gold: 0"
   - gem text "Gems: 0"
   - center text "Main Town Placeholder"
   - bottom navigation buttons:
     - Adventure
     - Hero
     - Bag
     - Quest
     - Shop

9. All UI must be visible even if generated PNG assets are missing.
   - Use Unity UI Image colors as fallback.
   - Use TextMeshPro for all text.
   - Do not bake text into PNG.
   - Do not depend on Addressables for this first visible UI shell.

10. Canvas settings:
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1080 x 1920
   - Match Width Or Height: 0.5

11. RectTransform rules:
   - RootCanvas fills screen.
   - SafeAreaRoot fills screen.
   - BackgroundLayer fills screen.
   - MainLayer fills screen.
   - TitleScreenUI fills screen.
   - MainTownUI fills screen.

12. Add debug logs:
   - "[Boot] BootScene started"
   - "[Boot] Loading GameScene"
   - "[Game] GameScene started"
   - "[UI] TitleScreen shown"
   - "[UI] MainTown shown"

13. Create an editor menu:
   Tools/Isekai 12 Realms/Repair Core Scenes

   This menu should recreate/repair BootScene and GameScene with the required visible UI hierarchy.

14. After implementation, automatically save the scenes.

Acceptance criteria:
- Press Play from BootScene.
- The loading screen is visible immediately.
- After 1 second, GameScene loads.
- The Title Screen is visible.
- Pressing Start Game shows Main Town placeholder.
- The result must not be a blank blue screen.


### Prompt 3

Read docs/spec.md, docs/ai_rules.md, and inspect the current Unity project.

The current GameScene only shows a blue screen. In the Hierarchy, RootCanvas and layers exist, but BackgroundLayer, MainLayer, HudLayer, NavigationLayer, PopupLayer, ToastLayer, and LoadingLayer are empty. That is why no UI is visible.

Fix this immediately.

Requirements:

1. Do not implement battle, Firebase, IAP, inventory, shop logic, or progression yet.

2. Repair GameScene so it visibly shows a Title Screen by default.

3. Under RootCanvas/SafeAreaRoot/BackgroundLayer, create:
   - Background_Image
   - Type: UnityEngine.UI.Image
   - Anchor: stretch full screen
   - Color: fantasy blue/purple gradient fallback is okay. If gradient is hard, use solid dark blue.
   - Must be visible in Play Mode.

4. Under RootCanvas/SafeAreaRoot/MainLayer, create:
   - TitleScreenUI
     - Anchor: stretch full screen
     - Active: true

5. Inside TitleScreenUI, create visible UI elements:
   - TMP title text: "ISEKAI 12 REALMS"
     - anchored top center
     - font size around 64
     - color white or gold
   - TMP subtitle text: "Offline Match-3 RPG"
     - below title
     - font size around 36
   - Button_Start
     - center lower area
     - size 520x120
     - visible Image background
     - TMP child text: "Start Game"
   - Button_Settings
     - below Start
     - size 520x100
     - TMP child text: "Settings"
   - Version_Text
     - bottom center
     - text: "v0.1.0"
     - font size 28

6. Pressing Button_Start must hide TitleScreenUI and show MainTownUI.

7. Also create MainTownUI under MainLayer:
   - Active: false by default
   - Anchor: stretch full screen
   - visible background panel
   - top HUD:
     - "Guest Hero"
     - "Lv. 1"
     - "Gold: 0"
     - "Gems: 0"
   - center TMP text:
     - "Main Town Placeholder"
   - bottom navigation buttons:
     - Adventure
     - Hero
     - Bag
     - Quest
     - Shop

8. Create or fix a script named GameSceneBootstrapper.cs.
   It must:
   - run on GameManager in GameScene
   - find RootCanvas/SafeAreaRoot/MainLayer
   - if TitleScreenUI does not exist, create it at runtime
   - if MainTownUI does not exist, create it at runtime
   - show TitleScreenUI on Start()
   - log:
     "[Game] GameSceneBootstrapper started"
     "[UI] TitleScreen shown"
     "[UI] MainTown shown"

9. Create or fix an editor menu:
   Tools/Isekai 12 Realms/Repair Core Scenes

   This menu must:
   - Open or create Assets/_Game/Scenes/GameScene.unity
   - Ensure Main Camera exists
   - Ensure EventSystem exists
   - Ensure RootCanvas exists
   - Ensure Canvas settings:
     Render Mode: Screen Space - Overlay
     Canvas Scaler: Scale With Screen Size
     Reference Resolution: 1080 x 1920
     Match: 0.5
   - Ensure SafeAreaRoot and all layers exist
   - Create the visible TitleScreenUI and MainTownUI described above
   - Attach GameSceneBootstrapper to GameManager
   - Save the scene

10. The UI must not depend on generated PNG, Addressables, AssetManifest, Firebase, or IAP.
    Use default Unity UI Image colors as fallback.
    Use TextMeshPro for text.

11. Acceptance criteria:
   - Open GameScene and press Play.
   - The screen must not be blank blue.
   - The title "ISEKAI 12 REALMS" must be visible.
   - Start Game button must be visible.
   - Clicking Start Game must show Main Town Placeholder.

### Prompt 4

Read docs/spec.md, docs/ai_rules.md, docs/asset_manifest.md, and inspect the current Unity project.

The project foundation now works:

* GameScene is visible
* Title Screen is visible
* Start Game shows Main Town placeholder

Next task: implement the full mobile portrait UI shell and navigation only.

Do not implement battle logic yet.
Do not implement Firebase yet.
Do not implement IAP yet.
Do not implement full inventory/equipment logic yet.
Do not create production art yet.

Requirements:

1. Keep the existing scene structure:

   * BootScene
   * GameScene

2. Keep the existing RootCanvas structure:
   RootCanvas
   SafeAreaRoot
   BackgroundLayer
   MainLayer
   HudLayer
   NavigationLayer
   PopupLayer
   ToastLayer
   LoadingLayer

3. Ensure Canvas settings:

   * Render Mode: Screen Space - Overlay
   * Canvas Scaler: Scale With Screen Size
   * Reference Resolution: 1080 x 1920
   * Match Width Or Height: 0.5

4. Create a UI navigation/state system.

Create enum:
GameUIScreen

* Title
* CharacterCreation
* MainTown
* WorldMap
* Adventure
* Battle
* Hero
* Skills
* Equipment
* Inventory
* Quest
* Shop
* Settings

Create script:
UIScreenManager.cs

UIScreenManager must:

* live on GameManager or a UIManager object
* register all screen root GameObjects
* show only one main screen at a time
* keep PopupLayer independent
* expose method ShowScreen(GameUIScreen screen)
* log every screen transition

5. Create these screen root objects under MainLayer:

   * TitleScreenUI
   * CharacterCreationUI
   * MainTownUI
   * WorldMapUI
   * AdventureUI
   * BattleUI
   * HeroUI
   * SkillsUI
   * EquipmentUI
   * InventoryUI
   * QuestUI
   * ShopUI

Each screen must:

* fill the full safe area
* have a visible background/panel
* have a TMP title at the top
* have at least one Back/Home button if appropriate
* use Unity UI Image color fallback
* use TextMeshPro for all text
* not depend on PNG assets, Addressables, Firebase, or IAP

6. TitleScreenUI:

   * Title: "ISEKAI 12 REALMS"
   * Subtitle: "Offline Match-3 RPG"
   * Button: "Start Game" -> show MainTownUI
   * Button: "New Hero" -> show CharacterCreationUI
   * Button: "Settings" -> open Settings popup
   * Version text: "v0.1.0"

7. CharacterCreationUI:

   * Title: "Create Your Reborn Hero"
   * Character preview placeholder
   * Name input placeholder or TMP text "Hero Name: Guest Hero"
   * Class buttons:

     * Flame Squire
     * Tide Acolyte
     * Storm Scout
   * Button: "Start Journey" -> show MainTownUI
   * Button: "Back" -> show TitleScreenUI

8. MainTownUI:

   * Top HUD:

     * Avatar placeholder
     * "Guest Hero"
     * "Lv. 1"
     * "Gold: 0"
     * "Gems: 0"
   * Center:

     * "Main Town Placeholder"
     * NPC placeholder buttons:

       * Quest Elder
       * Blacksmith
       * Shop Keeper
   * Bottom navigation in NavigationLayer:

     * Adventure -> WorldMapUI
     * Hero -> HeroUI
     * Bag -> InventoryUI
     * Quest -> QuestUI
     * Shop -> ShopUI

9. WorldMapUI:

   * Title: "World Map"
   * Scroll/content placeholder with 12 realm buttons:

     * Realm 01 Meadow Gate
     * Realm 02 Ember Village
     * Realm 03 Tide Shrine
     * Realm 04 Thunder Peak
     * Realm 05 Rootwood Forest
     * Realm 06 Crystal Mine
     * Realm 07 Moon Bazaar
     * Realm 08 Snow Lantern
     * Realm 09 Clock Ruin
     * Realm 10 Candy Citadel
     * Realm 11 Sky Library
     * Realm 12 Eclipse Throne
   * Selecting Realm 01 shows a simple Stage card:

     * "Stage 1-1: First Slime"
     * "Recommended Lv. 1"
     * Button: "Enter Battle" -> show BattleUI placeholder
   * Back/Home button -> MainTownUI

10. AdventureUI:

* Title: "Adventure Placeholder"
* 2D map placeholder panel
* Player placeholder
* NPC placeholder
* Button: "Start Battle" -> BattleUI
* Back button -> MainTownUI

11. BattleUI placeholder:

* Title: "Battle Placeholder"
* Enemy area:

  * enemy name "Meadow Slime"
  * enemy HP bar placeholder
* Battle info:

  * "Your Turn"
  * "Food: 20"
  * "Combo: 0"
* Center board placeholder:

  * create an 8x8 visual grid using UI Images
  * no match-3 logic yet
  * use colored squares or token placeholder labels
* Player area:

  * "Guest Hero"
  * HP bar placeholder
  * Mana bar placeholder
* Skill bar:

  * Skill 1
  * Skill 2
  * Ultimate
  * Item
* Button: "Win Test" -> open BattleResult popup with victory data
* Button: "Lose Test" -> open BattleResult popup with defeat data
* Back button -> WorldMapUI

12. HeroUI:

* Title: "Hero"
* Character preview placeholder
* Stats placeholder:

  * HP 100
  * ATK 10
  * MAG 8
  * DEF 5
  * SPD 5
  * LUCK 1
* Buttons:

  * Skills -> SkillsUI
  * Equipment -> EquipmentUI
* Back/Home -> MainTownUI

13. SkillsUI:

* Title: "Skills"
* Tabs or buttons:

  * Flame
  * Tide
  * Storm
* Skill list placeholder:

  * Skill icon placeholder
  * Skill name
  * Level 1
  * Upgrade button disabled placeholder
* Back -> HeroUI

14. EquipmentUI:

* Title: "Equipment"
* Equipment slots:

  * Weapon
  * Armor
  * Head
  * Boots
  * Ring
  * Charm
* Equipment detail placeholder
* Back -> HeroUI

15. InventoryUI:

* Title: "Bag"
* Tabs:

  * All
  * Equipment
  * Material
  * Consumable
  * Quest
* Grid placeholder, 5 columns
* Item detail placeholder
* Back/Home -> MainTownUI

16. QuestUI:

* Title: "Quest"
* Quest list placeholder:

  * Main Quest: Reach Meadow Gate
  * Daily Quest: Win 3 Battles
  * Achievement: First Match
* Claim button disabled placeholder
* Back/Home -> MainTownUI

17. ShopUI:

* Title: "Shop"
* Tabs:

  * Daily
  * Gold Shop
  * Gem Shop
  * IAP
* IAP tab must show placeholder gem packs only:

  * Tiny Gem Pack
  * Small Gem Pack
  * Medium Gem Pack
  * Large Gem Pack
* Add text:

  * "IAP will only sell Soul Gem currency."
* Do not implement real purchase yet.
* Back/Home -> MainTownUI

18. Settings popup:
    Create popup under PopupLayer:

* SettingsPopup
* Title: "Settings"
* Music toggle placeholder
* SFX toggle placeholder
* Cloud Save placeholder
* Button: "Close"

19. BattleResult popup:
    Create popup under PopupLayer:

* BattleResultPopup
* Victory mode:

  * "Victory!"
  * "EXP +50"
  * "Gold +30"
  * Buttons:

    * Continue -> WorldMapUI
    * Replay -> BattleUI
    * Town -> MainTownUI
* Defeat mode:

  * "Try Again"
  * Buttons:

    * Retry -> BattleUI
    * Upgrade -> HeroUI
    * Town -> MainTownUI

20. Toast system:
    Create ToastService.cs

* ShowToast(string message)
* Display a small TMP text panel under ToastLayer
* Auto hide after 2 seconds
* Use it when pressing disabled placeholder buttons

21. Loading overlay:
    Create LoadingOverlayUI under LoadingLayer

* hidden by default
* full-screen semi-transparent panel
* TMP text "Loading..."
* method ShowLoading(string message)
* method HideLoading()

22. Editor repair menu:
    Update existing menu:
    Tools/Isekai 12 Realms/Repair Core Scenes

It must recreate/repair all UI screens above without duplicating objects if they already exist.

23. Acceptance criteria:

* Open GameScene and press Play
* Title screen appears
* Start Game opens Main Town
* Bottom nav buttons open WorldMap, Hero, Inventory, Quest, Shop
* WorldMap Realm 01 Enter Battle opens Battle placeholder
* Battle placeholder shows an 8x8 grid
* Win Test opens Victory popup
* Lose Test opens Defeat popup
* Settings popup opens and closes
* No screen is blank blue
* No missing script errors
* No console errors


### Prompt 5

Read docs/spec.md, docs/ai_rules.md, docs/asset_manifest.md, and inspect the current Unity project.

The current project already has:

* BootScene and GameScene
* Visible UI shell
* Title Screen
* Main Town
* World Map
* Battle placeholder screen
* Battle screen has an 8x8 visual grid placeholder

Next task: implement the real match-3 battle prototype.

Do not implement Firebase yet.
Do not implement IAP yet.
Do not implement production art yet.
Do not implement full equipment/inventory progression yet.
Do not replace the whole UI shell.
Only implement the battle system and connect it to the existing BattleUI.

Requirements:

1. Create core battle folders if missing:
   Assets/_Game/Scripts/Battle
   Assets/_Game/Scripts/Board
   Assets/_Game/Scripts/Data

2. Create these enums:

TileType:

* Sword
* Heart
* Coin
* Food
* Book
* Mana
* Shield
* Star

SpecialTileType:

* None
* RowRune
* ColumnRune
* BombRune
* RealmCrystal

BattleTurnOwner:

* Player
* Enemy

BattleResultType:

* None
* Victory
* Defeat

3. Create TileData.cs:
   Fields:

* TileType type
* SpecialTileType specialType
* Vector2Int position
* bool locked
* int freezeTurns

4. Create TileView.cs:
   Responsibilities:

* Display one tile in the UI grid
* Hold reference to TileData
* Show different color/icon placeholder for each TileType
* Use TextMeshPro label or simple colored Image fallback
* Detect click/tap
* Notify BoardController when selected
* Highlight selected tile
* Move/swap visually when BoardController updates

5. Create BoardController.cs:
   Responsibilities:

* Generate an 8x8 board
* Ensure initial board has no automatic matches
* Ensure board has at least one valid move
* Handle tile selection
* Allow swap only between adjacent tiles
* Reject invalid swap and swap back
* Find horizontal and vertical matches
* Resolve matches
* Drop tiles down
* Spawn new tiles from top
* Continue cascade until no matches remain
* Shuffle board if no valid moves exist

Public methods:

* Initialize(int width, int height)
* TrySelectTile(TileView tileView)
* TrySwap(Vector2Int a, Vector2Int b)
* FindMatches()
* ResolveMatches()
* DropAndRefill()
* HasValidMove()
* ShuffleBoard()
* ClearBoard()

6. Match rules:

* Match 3: remove matched tiles and apply token effect
* Match 4: create special tile and grant extra turn
* Match 5: create RealmCrystal and grant extra turn
* L/T shape: create BombRune and grant extra turn
* Cascade matches also apply effects
* Every cascade increases combo count

7. Create MatchGroup.cs:
   Fields:

* TileType tileType
* List<Vector2Int> positions
* bool createsSpecial
* SpecialTileType specialCreated
* int count

8. Create BattleState.cs:
   Player fields:

* playerName = "Guest Hero"
* playerLevel = 1
* maxHp = 100
* hp = 100
* maxMana = 100
* mana = 0
* shield = 0
* food = 20
* goldReward = 0
* expReward = 0

Enemy fields:

* enemyName = "Meadow Slime"
* enemyLevel = 1
* maxHp = 80
* hp = 80
* maxMana = 100
* mana = 0
* shield = 0

Battle fields:

* currentTurnOwner
* comboCount
* turnCount
* battleResult

9. Create BattleResolver.cs:
   Convert matched tiles into battle effects.

Player match effects:

* Sword: damage enemy
* Heart: heal player
* Coin: add battle gold reward
* Food: increase food
* Book: add battle EXP reward
* Mana: increase player mana
* Shield: add shield
* Star: add mana and small bonus damage

Suggested prototype values:

* Sword: 5 damage per tile
* Heart: 4 heal per tile
* Coin: 3 gold per tile
* Food: 2 food per tile
* Book: 4 EXP per tile
* Mana: 8 mana per tile
* Shield: 3 shield per tile
* Star: 5 mana per tile and 2 damage per tile

Shield absorbs damage before HP.

10. Create BattleService.cs:
    Responsibilities:

* StartBattle()
* EndBattle()
* ApplyPlayerMatchResult()
* ExecuteEnemyTurn()
* CheckWinLose()
* UseSkill1()
* UseSkill2()
* UseUltimate()

Turn rules:

* Player starts first
* Player food decreases by 1 after a valid player move
* If food <= 0, player takes 5 damage each player turn
* Match 4+ grants extra turn
* If no extra turn, enemy acts
* Enemy action prototype:

  * 70% chance attack player
  * 20% chance gain shield
  * 10% chance heal itself
* Enemy attack damage: 8
* Enemy shield: 5
* Enemy heal: 6

Win:

* enemy HP <= 0

Defeat:

* player HP <= 0

11. Connect BattleService to existing BattleUI.

Create BattleUIController.cs if missing.

BattleUIController must update:

* Enemy name
* Enemy HP bar
* Player name
* Player HP bar
* Player mana bar
* Food text
* Combo text
* Turn text
* Gold reward text if available
* EXP reward text if available

BattleUIController must contain buttons:

* Restart Battle
* Back to World Map
* Skill 1
* Skill 2
* Ultimate

12. Replace Battle placeholder 8x8 grid with real generated TileViews.

The visual board must:

* Be 8 columns x 8 rows
* Fit inside the current BattleUI board panel
* Use GridLayoutGroup or controlled RectTransform positioning
* Use square tiles
* Be readable on portrait 1080x1920
* Not overflow the screen

13. Skill prototype:
    Skill 1:

* Name: Spark Slash
* Cost: 30 mana
* Effect: deal 20 damage

Skill 2:

* Name: Shuffle Bell
* Cost: 20 mana
* Effect: shuffle board

Ultimate:

* Name: Realm Burst
* Cost: 100 mana
* Effect: deal 50 damage and destroy a random 3x3 tile area

Buttons must be disabled or greyed if not enough mana.

14. Battle result popup:
    Use the existing BattleResultPopup if it exists.
    On Victory show:

* "Victory!"
* EXP gained
* Gold gained
* Continue button -> WorldMapUI
* Replay button -> restart Battle
* Town button -> MainTownUI

On Defeat show:

* "Try Again"
* Retry button -> restart Battle
* Upgrade button -> HeroUI
* Town button -> MainTownUI

15. Debug logs:
    Add useful logs:

* "[Battle] StartBattle"
* "[Board] Generated board"
* "[Board] Swap"
* "[Board] Match found"
* "[Battle] Player resolved match"
* "[Battle] Enemy turn"
* "[Battle] Victory"
* "[Battle] Defeat"

16. Do not use Addressables yet.

17. Do not require generated PNG assets.

18. Use simple colored Images and TextMeshPro labels as fallback.

19. Keep battle logic independent from UI view.

20. Avoid putting gameplay logic directly inside Button onClick lambdas except calling service/controller methods.

21. Acceptance criteria:

* Open GameScene and press Play
* Go Title -> Start Game -> Main Town
* Open Adventure/World Map
* Enter Battle
* A real 8x8 board appears
* Player can tap two adjacent tiles to swap
* Invalid swap is rejected
* Valid match removes tiles
* Tiles drop and refill
* Cascades work
* Sword damages enemy
* Heart heals player
* Food increases food
* Mana increases mana
* Enemy takes a turn after player unless extra turn
* Player can win
* Player can lose
* Victory popup works
* Defeat popup works
* Restart battle works
* No console errors
* No blank screen


### Prompt 6

Read docs/spec.md, docs/ai_rules.md, docs/asset_manifest.md, and inspect the current Unity project.

The project already has:

* Visible UI shell
* Main Town / World Map / Battle UI
* Real match-3 battle prototype
* Victory and Defeat popup

Next task: implement local save, battle rewards, player progression, inventory, and basic equipment.

Do not implement Firebase yet.
Do not implement IAP yet.
Do not implement production art yet.
Do not implement Addressables yet.
Do not create complex content editor yet.

Goal:
After winning a battle, the player receives EXP, Gold, and item drops.
The data is saved locally.
When closing and reopening the game, progress is restored.

Requirements:

1. Create or update save folder:
   Assets/_Game/Scripts/Save
   Assets/_Game/Scripts/Inventory
   Assets/_Game/Scripts/Character
   Assets/_Game/Scripts/Equipment

2. Create PlayerSaveData.cs

Fields:

* int schemaVersion = 1
* long saveVersion
* string playerId
* string localGuestId
* string playerName
* int level
* long exp
* int gold
* int soulGem
* int maxHp
* int hp
* int maxMana
* int mana
* string selectedClassId
* string currentRealmId
* string currentStageId
* List<string> completedStageIds
* InventorySaveData inventory
* EquipmentLoadoutData equipment
* long createdAt
* long updatedAt

Default values:

* playerName = "Guest Hero"
* level = 1
* exp = 0
* gold = 0
* soulGem = 0
* maxHp = 100
* hp = 100
* maxMana = 100
* mana = 0
* selectedClassId = "flame_squire"
* currentRealmId = "realm_01_meadow"
* currentStageId = "stage_01_01"

3. Create InventorySaveData.cs

Fields:

* List<ItemStackData> items
* List<EquipmentInstanceData> equipments
* int capacity

Default:

* capacity = 80

4. Create ItemStackData.cs

Fields:

* string itemId
* int amount

5. Create EquipmentInstanceData.cs

Fields:

* string instanceId
* string equipmentId
* string displayName
* EquipmentSlot slot
* EquipmentRarity rarity
* int level
* int hpBonus
* int atkBonus
* int magBonus
* int defBonus
* int spdBonus
* bool locked

6. Create EquipmentLoadoutData.cs

Fields:

* string weaponInstanceId
* string armorInstanceId
* string headInstanceId
* string bootsInstanceId
* string ringInstanceId
* string charmInstanceId

7. Create enums:

EquipmentSlot:

* Weapon
* Armor
* Head
* Boots
* Ring
* Charm

EquipmentRarity:

* Common
* Uncommon
* Rare
* Epic
* Legendary

8. Create SaveService.cs or update existing SaveService.

Save path:
Application.persistentDataPath + "/save_v1.json"

Backup path:
Application.persistentDataPath + "/save_v1_backup.json"

SaveService must support:

* LoadOrCreateSave()
* SaveNow()
* HasSave()
* DeleteSave()
* CreateNewSave()
* Backup current save before overwrite
* Restore backup if main save cannot load
* Increment saveVersion every save
* Update updatedAt timestamp every save

Use JSON serialization.
Do not use Firebase.
Do not encrypt yet.
Add checksum placeholder field or method, but do not block saving if checksum is missing.

Debug logs:

* "[Save] Created new save"
* "[Save] Loaded save"
* "[Save] Saved successfully"
* "[Save] Main save failed, trying backup"
* "[Save] Backup restored"

9. Create PlayerProgressionService.cs

Responsibilities:

* Hold current PlayerSaveData
* AddGold(int amount)
* AddExp(int amount)
* AddSoulGem(int amount)
* AddItem(string itemId, int amount)
* AddEquipment(EquipmentInstanceData equipment)
* Equip(string instanceId)
* Unequip(EquipmentSlot slot)
* CalculateTotalStats()
* Save after every important change

EXP curve:
EXPRequired(level) = floor(50 * level^1.45)

Level up:

* While exp >= required exp:

  * exp -= required
  * level += 1
  * maxHp += 18
  * maxMana += 2
  * hp = maxHp
  * mana = maxMana
  * show toast "Level Up!"

10. Connect save data to UI.

MainTownUI must show real:

* playerName
* level
* gold
* soulGem

HeroUI must show real:

* level
* current exp / required exp
* HP
* Mana
* total ATK
* total MAG
* total DEF
* total SPD

InventoryUI must show real item/equipment placeholders from save data.

EquipmentUI must show equipped slots from save data.

11. Create basic static item database in code for prototype.

Items:

* item_potion_small

  * displayName: Small Potion
  * type: Consumable
* mat_slime_jelly

  * displayName: Slime Jelly
  * type: Material
* item_skill_scroll

  * displayName: Skill Scroll
  * type: Material

12. Create basic equipment prototype factory.

Equipment examples:

* equip_weapon_wooden_sword

  * displayName: Wooden Sword
  * slot: Weapon
  * rarity: Common
  * atkBonus: 5

* equip_armor_traveler_coat

  * displayName: Traveler Coat
  * slot: Armor
  * rarity: Common
  * hpBonus: 20
  * defBonus: 3

* equip_ring_lucky

  * displayName: Lucky Ring
  * slot: Ring
  * rarity: Uncommon
  * spdBonus: 2

13. Connect battle rewards.

When BattleService ends with Victory:

* Add EXP reward to player progression
* Add Gold reward to player progression
* Add stage completion to completedStageIds
* Add item drops
* Save immediately

Prototype reward for Stage 1-1:

* EXP +50
* Gold +30
* mat_slime_jelly x2
* 25% chance to drop Small Potion
* 15% chance to drop Wooden Sword

For now, random drop is acceptable.
Later it will be replaced by DropTableDefinition.

14. Update BattleResultPopup.

Victory popup must display actual:

* EXP gained
* Gold gained
* dropped items
* dropped equipment
* level up notice if level increased

Defeat popup:

* Do not grant EXP
* Do not grant Gold
* Do not save battle reward
* Save only if player HP/other persistent fields changed

15. Restore player data on game start.

On GameScene start:

* SaveService.LoadOrCreateSave()
* PlayerProgressionService initializes from save
* UI refreshes from save
* MainTown displays loaded data

16. Add New Game behavior.

On TitleScreenUI:

* "Start Game" loads existing save or creates new save, then opens MainTown
* "New Hero" opens CharacterCreationUI
* CharacterCreation "Start Journey":

  * creates new save
  * applies selected class
  * applies hero name placeholder
  * saves
  * opens MainTown

17. Add Delete Save debug button only inside Settings popup.

Settings popup:

* Add button "Delete Local Save"
* On click, show confirmation popup:

  * "Are you sure?"
  * Confirm deletes save and returns to TitleScreen
  * Cancel closes confirmation

18. Add Toast messages.

Show toast for:

* Save completed
* Gold gained
* Item gained
* Equipment equipped
* Not enough item
* Level up

19. Do not make UI beautiful yet.
    Use placeholder rows/cards/buttons.
    Use TextMeshPro.
    Use simple Unity UI Images.
    No generated PNG dependency.

20. Acceptance criteria:

* Open GameScene and press Play
* Start Game creates or loads save
* Main Town shows Guest Hero Lv. 1 Gold 0 Gems 0
* Enter Battle
* Win battle
* Victory popup shows EXP +50 and Gold +30
* Continue to World Map or Town
* Main Town now shows updated level/gold if changed
* Inventory shows Slime Jelly after win
* Close Play Mode
* Press Play again
* Start Game loads previous gold/EXP/items
* Equipping Wooden Sword increases ATK if it dropped
* Delete Local Save works from Settings
* No console errors
* No missing script errors
* No blank screen


### Prompt 7

Read docs/spec.md, docs/ai_rules.md, docs/asset_manifest.md, and inspect the current Unity project.

The project already has:

* Visible UI shell
* Real match-3 battle prototype
* Local save
* Battle rewards
* EXP/Gold progression
* Basic inventory/equipment
* Save/load works after reopening Play Mode

Next task: implement data-driven Realm, Stage, Enemy, and DropTable progression.

Do not implement Firebase yet.
Do not implement IAP yet.
Do not implement production art yet.
Do not implement Addressables yet.
Do not create complex online features.

Goal:
World Map must show realms and stages from data.
Selecting a stage starts battle using that stage's enemy, rewards, and drop table.
Winning a stage unlocks the next stage.
Replay old stages must work.

Requirements:

1. Create folders if missing:
   Assets/_Game/Scripts/Stages
   Assets/_Game/Scripts/Realms
   Assets/_Game/Scripts/Enemies
   Assets/_Game/Scripts/DropTables
   Assets/_Game/ScriptableObjects/Realms
   Assets/_Game/ScriptableObjects/Stages
   Assets/_Game/ScriptableObjects/Enemies
   Assets/_Game/ScriptableObjects/DropTables

2. Create RealmDefinition.cs as ScriptableObject.

Fields:

* string id
* string displayName
* string description
* int order
* string backgroundAssetId
* List<StageDefinition> stages

Example:
id = "realm_01_meadow"
displayName = "Meadow Gate"
description = "A peaceful floating meadow where the reborn hero begins the journey."
order = 1

3. Create StageDefinition.cs as ScriptableObject.

Fields:

* string id
* string realmId
* string displayName
* string description
* int stageNumber
* int recommendedLevel
* EnemyDefinition enemy
* DropTableDefinition dropTable
* int baseGoldReward
* int baseExpReward
* List<string> requiredCompletedStageIds
* bool isBossStage
* bool replayable
* string battleBackgroundAssetId

Example:
id = "stage_01_01"
realmId = "realm_01_meadow"
displayName = "First Slime"
recommendedLevel = 1
baseGoldReward = 30
baseExpReward = 50
replayable = true

4. Create EnemyDefinition.cs as ScriptableObject.

Fields:

* string id
* string displayName
* int level
* int maxHp
* int attack
* int defense
* int maxMana
* string spriteAssetId
* EnemyAIDifficulty difficulty

Create enum EnemyAIDifficulty:

* Easy
* Normal
* Hard
* Boss

Prototype enemies:

* enemy_meadow_slime

  * Meadow Slime
  * Level 1
  * HP 80
  * Attack 8
  * Defense 0
  * Easy

* enemy_meadow_mushroom

  * Meadow Mushroom
  * Level 2
  * HP 100
  * Attack 10
  * Defense 1
  * Easy

* boss_slime_king

  * Slime King
  * Level 3
  * HP 160
  * Attack 14
  * Defense 2
  * Boss

5. Create DropTableDefinition.cs as ScriptableObject.

Fields:

* string id
* List<DropEntry> drops

DropEntry fields:

* string itemId
* string equipmentId
* int minAmount
* int maxAmount
* float chance
* bool isEquipment

Rules:

* if isEquipment = false, use itemId
* if isEquipment = true, use equipmentId
* chance is 0.0 to 1.0

6. Create StageDatabase.cs or GameContentDatabase.cs as ScriptableObject.

Fields:

* List<RealmDefinition> realms
* List<StageDefinition> stages
* List<EnemyDefinition> enemies
* List<DropTableDefinition> dropTables

Methods:

* GetRealmById(string id)
* GetStageById(string id)
* GetEnemyById(string id)
* GetStagesForRealm(string realmId)
* GetNextStage(string currentStageId)

7. Create a ContentDatabaseService.cs.

Responsibilities:

* Load the GameContentDatabase asset
* Provide access to realms, stages, enemies, drop tables
* If database asset is missing, create prototype data at runtime as fallback
* Never crash if content data is missing
* Log warnings for missing content

8. Create prototype ScriptableObject assets automatically using an editor menu.

Create menu:
Tools/Isekai 12 Realms/Create Prototype Content

It must create:

* RealmDefinition:

  * realm_01_meadow
  * realm_02_ember
  * realm_03_tide

* StageDefinition:

  * stage_01_01 First Slime
  * stage_01_02 Mushroom Trouble
  * stage_01_03 Slime King Trial
  * stage_02_01 Ember Piglet
  * stage_02_02 Tiny Flame
  * stage_02_03 Cinder Boar Trial
  * stage_03_01 Bubble Trouble
  * stage_03_02 Tide Crab
  * stage_03_03 Bubble Serpent Trial

* EnemyDefinition:

  * enemy_meadow_slime
  * enemy_meadow_mushroom
  * boss_slime_king
  * enemy_ember_piglet
  * enemy_ember_sprite
  * boss_cinder_boar
  * enemy_tide_bubble
  * enemy_tide_crab
  * boss_bubble_serpent

* DropTableDefinition:

  * drop_stage_01_01
  * drop_stage_01_02
  * drop_stage_01_03
  * drop_stage_02_01
  * drop_stage_02_02
  * drop_stage_02_03
  * drop_stage_03_01
  * drop_stage_03_02
  * drop_stage_03_03

* GameContentDatabase.asset

Do not duplicate assets if they already exist.
Update existing assets safely.

9. Prototype stage data:

stage_01_01:

* enemy: enemy_meadow_slime
* recommendedLevel: 1
* gold: 30
* exp: 50
* requiredCompletedStageIds: empty
* replayable: true

stage_01_02:

* enemy: enemy_meadow_mushroom
* recommendedLevel: 2
* gold: 45
* exp: 70
* requiredCompletedStageIds: stage_01_01
* replayable: true

stage_01_03:

* enemy: boss_slime_king
* recommendedLevel: 3
* gold: 100
* exp: 150
* requiredCompletedStageIds: stage_01_02
* isBossStage: true
* replayable: true

stage_02_01:

* requiredCompletedStageIds: stage_01_03

stage_03_01:

* requiredCompletedStageIds: stage_02_03

10. Drop table prototype:

drop_stage_01_01:

* mat_slime_jelly x1-2, chance 1.0
* item_potion_small x1, chance 0.25
* equip_weapon_wooden_sword, chance 0.15

drop_stage_01_02:

* mat_slime_jelly x2-3, chance 1.0
* item_potion_small x1, chance 0.35
* equip_armor_traveler_coat, chance 0.15

drop_stage_01_03:

* mat_slime_jelly x3-5, chance 1.0
* item_skill_scroll x1, chance 0.25
* equip_ring_lucky, chance 0.2

11. Update PlayerSaveData.

Ensure it has:

* List<string> completedStageIds
* string currentRealmId
* string currentStageId
* Dictionary<string, int> stageClearCounts or serializable equivalent list

If Dictionary serialization is inconvenient, create:
StageProgressData:

* string stageId
* int clearCount
* long firstClearedAt
* long lastClearedAt

12. Create StageProgressionService.cs.

Responsibilities:

* IsStageUnlocked(StageDefinition stage)
* IsStageCompleted(string stageId)
* MarkStageCompleted(string stageId)
* GetStageClearCount(string stageId)
* IncrementStageClearCount(string stageId)
* GetUnlockedStagesForRealm(string realmId)
* GetCurrentAvailableStage()
* Save after stage completion

Unlock rule:

* A stage is unlocked if all requiredCompletedStageIds are completed.
* First stage of Realm 1 is unlocked by default.
* Realm 2 unlocks after stage_01_03.
* Realm 3 unlocks after stage_02_03.

13. Update WorldMapUI.

WorldMapUI must:

* Build realm buttons from ContentDatabaseService
* Show realm lock/unlock state
* When selecting a realm, show its stages
* Stage button visual states:

  * Locked
  * Available
  * Completed
  * Boss
* Selecting locked stage shows toast:
  "Complete previous stages first."
* Selecting unlocked stage shows Stage Detail Card:

  * stage name
  * recommended level
  * enemy name
  * base EXP
  * base Gold
  * replay count if completed
  * Enter Battle button

14. Update battle start flow.

When pressing Enter Battle:

* Store selected StageDefinition in BattleService
* BattleService.StartBattle(StageDefinition stage)
* Enemy stats must come from stage.enemy
* Battle UI must show enemy displayName, level, HP
* Rewards must come from stage.baseGoldReward, stage.baseExpReward, and stage.dropTable

15. Update battle victory flow.

On victory:

* Grant stage rewards from the selected stage
* Roll drops using DropTableDefinition
* Mark stage completed
* Increment clear count
* Save immediately
* Refresh WorldMapUI after returning

16. Replay behavior.

If replaying a completed stage:

* Allow replay
* Give normal gold and item drops
* EXP reward should be 70% of baseExpReward
* Boss stage can still drop items
* Show replay count in stage detail

17. Update BattleResultPopup.

Victory result must show:

* Stage name
* Enemy defeated
* EXP gained
* Gold gained
* Drop list
* First clear bonus text if first clear:
  "First Clear!"
* Buttons:
  Continue -> WorldMapUI
  Replay -> restart same stage
  Town -> MainTownUI

18. Add content validation editor tool.

Create menu:
Tools/Isekai 12 Realms/Validate Content

It must check:

* Every stage has id
* Every stage has enemy
* Every stage has drop table
* Every requiredCompletedStageId exists
* No duplicate stage ids
* No duplicate realm ids
* Every realm has at least one stage
* Every drop entry has valid itemId or equipmentId
* Print report to Console

19. Do not break existing save compatibility.

If an old save does not have stage progress:

* initialize completedStageIds as empty list
* initialize stage progress list
* keep existing level/gold/items

20. Acceptance criteria:

* Run Tools/Isekai 12 Realms/Create Prototype Content
* Run Tools/Isekai 12 Realms/Validate Content
* Open GameScene and press Play
* Start Game
* Open World Map
* Realm 1 is unlocked
* Stage 1-1 is available
* Stage 1-2 is locked before Stage 1-1 is cleared
* Enter Stage 1-1
* Battle uses Meadow Slime stats
* Win Stage 1-1
* Victory popup shows stage reward and drops
* Return to World Map
* Stage 1-1 is completed
* Stage 1-2 is now unlocked
* Replay Stage 1-1 works
* Close Play Mode and reopen
* Completed stage progress is restored from local save
* No console errors
* No missing script errors


### Prompt 8

Read docs/spec.md, docs/ai_rules.md, docs/asset_manifest.md, and inspect the current Unity project.

The project already has:

* UI shell
* Match-3 battle prototype
* Local save
* EXP/Gold/reward progression
* Inventory/equipment basics
* Data-driven Realm, Stage, Enemy, DropTable
* World Map progression and stage unlock

Next task: create Unity Editor tools so designers and AI agents can add new realms, stages, enemies, and drop tables without editing code.

Do not implement Firebase yet.
Do not implement IAP yet.
Do not implement production art yet.
Do not implement Addressables yet.
Do not replace the current gameplay.
Do not break existing ScriptableObject data.

Goal:
Create editor windows for managing game content:

* Stage Editor
* Enemy Editor
* DropTable Editor
* Realm Editor
* Content Validator
* Content Export/Import JSON

All tools must work with the existing ScriptableObject data system.

Requirements:

1. Create editor folder if missing:
   Assets/_Game/Scripts/Editor/ContentTools

2. Create main editor window:
   IsekaiContentEditorWindow.cs

Menu:
Tools/Isekai 12 Realms/Content Editor

The window must have tabs:

* Realms
* Stages
* Enemies
* Drop Tables
* Validate
* Export / Import

3. Realm Editor tab

Must allow:

* View all RealmDefinition assets
* Create new realm
* Edit selected realm fields:

  * id
  * displayName
  * description
  * order
  * backgroundAssetId
  * stages list
* Add stage to realm
* Remove stage from realm
* Sort realms by order
* Save selected realm
* Ping asset in Project window

Required validation:

* id is not empty
* id uses lowercase_snake_case
* displayName is not empty
* order > 0
* no duplicate realm id

4. Stage Editor tab

Must allow:

* View all StageDefinition assets
* Filter by realmId
* Create new stage
* Duplicate selected stage
* Edit selected stage fields:

  * id
  * realmId
  * displayName
  * description
  * stageNumber
  * recommendedLevel
  * enemy
  * dropTable
  * baseGoldReward
  * baseExpReward
  * requiredCompletedStageIds
  * isBossStage
  * replayable
  * battleBackgroundAssetId
* Add/remove requiredCompletedStageIds
* Save selected stage
* Ping asset in Project window

Create stage button must:

* Ask for stage id
* Create ScriptableObject at:
  Assets/_Game/ScriptableObjects/Stages/{stage_id}.asset
* Add the stage to GameContentDatabase
* Add the stage to its RealmDefinition if realmId exists

Required validation:

* stage id is not empty
* stage id uses lowercase_snake_case
* stage has realmId
* stage has enemy
* stage has dropTable
* recommendedLevel >= 1
* baseGoldReward >= 0
* baseExpReward >= 0
* requiredCompletedStageIds all exist
* no duplicate stage id

5. Enemy Editor tab

Must allow:

* View all EnemyDefinition assets
* Create new enemy
* Duplicate selected enemy
* Edit selected enemy fields:

  * id
  * displayName
  * level
  * maxHp
  * attack
  * defense
  * maxMana
  * spriteAssetId
  * difficulty
* Save selected enemy
* Ping asset in Project window

Create enemy button must:

* Ask for enemy id
* Create ScriptableObject at:
  Assets/_Game/ScriptableObjects/Enemies/{enemy_id}.asset
* Add the enemy to GameContentDatabase

Required validation:

* enemy id is not empty
* enemy id uses lowercase_snake_case
* displayName is not empty
* level >= 1
* maxHp > 0
* attack >= 0
* defense >= 0
* no duplicate enemy id

6. DropTable Editor tab

Must allow:

* View all DropTableDefinition assets
* Create new drop table
* Duplicate selected drop table
* Edit selected drop table fields:

  * id
  * drops list

For each DropEntry, allow editing:

* isEquipment
* itemId
* equipmentId
* minAmount
* maxAmount
* chance

Add buttons:

* Add Item Drop
* Add Equipment Drop
* Remove Drop
* Test Roll 10 Times
* Test Roll 100 Times

Test roll must print simulated rewards to Console.

Create drop table button must:

* Ask for drop table id
* Create ScriptableObject at:
  Assets/_Game/ScriptableObjects/DropTables/{drop_table_id}.asset
* Add the drop table to GameContentDatabase

Required validation:

* drop table id is not empty
* id uses lowercase_snake_case
* every drop has chance between 0 and 1
* minAmount >= 1 for item drops
* maxAmount >= minAmount
* item drop must have itemId
* equipment drop must have equipmentId

7. Validate tab

Integrate and improve the existing content validation tool.

The Validate tab must show a readable report in the editor window and also log to Console.

Validation checks:

* GameContentDatabase exists
* Every realm has valid id
* No duplicate realm id
* Every realm has at least one stage
* Every stage has valid id
* No duplicate stage id
* Every stage has valid realmId
* Stage realmId exists
* Every stage has enemy
* Every stage has drop table
* Every requiredCompletedStageId exists
* Every enemy has valid id
* No duplicate enemy id
* Every enemy has maxHp > 0
* Every drop table has valid id
* No duplicate drop table id
* Every drop entry has valid chance
* Every itemId/equipmentId is not empty
* World progression has at least one unlocked first stage

Report format:

* Errors
* Warnings
* Summary

Add buttons:

* Run Validation
* Fix Safe Issues

Fix Safe Issues may:

* Remove null entries from database lists
* Sort realms by order
* Sort stages by realmId and stageNumber
* Remove duplicated references in database lists

Do not automatically delete assets.

8. Export / Import tab

Create JSON export/import for content data.

Export button:

* Export all realms, stages, enemies, and drop tables to:
  Assets/_Game/Export/content_export.json

Import button:

* Read:
  Assets/_Game/Export/content_export.json
* Create or update ScriptableObject assets
* Rebuild GameContentDatabase references
* Do not duplicate assets with same id
* Show confirmation before import

JSON structure:
{
"realms": [],
"stages": [],
"enemies": [],
"dropTables": []
}

Use serializable DTO classes:

* RealmDto
* StageDto
* EnemyDto
* DropTableDto
* DropEntryDto
* ContentExportDto

9. Create sample content generation tool.

Menu:
Tools/Isekai 12 Realms/Create 12 Realm Skeleton Content

This tool must create skeleton content for all 12 realms if missing:

Realms:

* realm_01_meadow
* realm_02_ember
* realm_03_tide
* realm_04_thunder
* realm_05_rootwood
* realm_06_crystal
* realm_07_bazaar
* realm_08_snow
* realm_09_clock
* realm_10_candy
* realm_11_library
* realm_12_eclipse

For each realm, create 3 placeholder stages:

* stage_{realmNumber}_01
* stage_{realmNumber}_02
* stage_{realmNumber}_03_boss

For each realm, create:

* 2 normal enemies
* 1 boss enemy
* 3 drop tables

Do not overwrite existing configured assets unless the user confirms.

10. Add content preview helpers.

In the Stage Editor tab, show a preview card:

* Stage name
* Realm name
* Enemy name
* Enemy level
* Enemy HP
* Base gold
* Base EXP
* Drop count
* Unlock requirements
* Boss marker
* Replayable marker

In the Enemy Editor tab, show estimated difficulty:

* Easy if maxHp <= 100 and attack <= 10
* Normal if maxHp <= 180 and attack <= 18
* Hard if maxHp <= 300 and attack <= 30
* Boss if difficulty == Boss or is used by a boss stage

11. Add playtest shortcut.

In Stage Editor tab:

* Add button "Playtest Selected Stage"

When clicked in Play Mode:

* Start the selected stage battle using BattleService
* Switch UI to Battle screen

When clicked outside Play Mode:

* Show dialog:
  "Enter Play Mode first to playtest this stage."

12. Database rebuild tool.

Menu:
Tools/Isekai 12 Realms/Rebuild Content Database

Must:

* Find all RealmDefinition assets
* Find all StageDefinition assets
* Find all EnemyDefinition assets
* Find all DropTableDefinition assets
* Rebuild GameContentDatabase.asset
* Save assets
* Log result summary

13. Keep runtime code safe.

Runtime must not depend on editor-only classes.
All editor scripts must be inside Editor folder or wrapped with UNITY_EDITOR.

14. Preserve existing content.

Do not delete existing RealmDefinition, StageDefinition, EnemyDefinition, DropTableDefinition assets.
Do not rename existing IDs.
Do not break existing WorldMapUI, BattleService, or StageProgressionService.

15. Acceptance criteria:

* Open Unity
* Run Tools/Isekai 12 Realms/Content Editor
* Realms tab shows existing realms
* Stages tab shows existing stages
* Enemies tab shows existing enemies
* Drop Tables tab shows existing drop tables
* Create a new enemy from the editor
* Create a new drop table from the editor
* Create a new stage using that enemy and drop table
* Add the stage to a realm
* Run validation and see no critical errors
* Run Rebuild Content Database
* Press Play
* Open World Map
* The new stage appears when unlocked
* Enter the new stage
* Battle uses the selected enemy stats
* Victory grants selected stage rewards and drop table rewards
* No console errors
* No missing script errors
* Runtime build does not include editor-only errors


### Prompt 9

Read docs/spec.md, docs/ai_rules.md, docs/asset_manifest.md, and inspect the current Unity project.

The project already has:

* Visible UI shell
* Real match-3 battle prototype
* Local save and progression
* Data-driven Realm/Stage/Enemy/DropTable
* Content Editor tools

Next task: improve the visual foundation by creating the PNG asset pipeline and applying placeholder game art to the UI.

Do not implement Firebase yet.
Do not implement IAP yet.
Do not implement Addressables yet.
Do not generate final production art yet.
Do not rewrite the gameplay systems.
Do not break the existing UI navigation and battle flow.

Goal:
Create placeholder PNG assets using the required naming format, register them in AssetManifest, and update the main UI screens to use these assets instead of plain debug colors where possible.

Requirements:

1. Create or repair asset folders:

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
Loading/
Meta/

2. Create GameAssetCategory enum if missing:

* Background
* Character
* Enemy
* NPC
* Token
* Skill
* Equipment
* Item
* UI
* Tileset
* VFX
* Map
* Loading
* Currency
* Misc

3. Create or update GameAssetEntry.cs:

Fields:

* string id
* string fileName
* string relativePath
* int width
* int height
* GameAssetCategory category
* bool transparent
* int priority
* Sprite sprite

4. Create or update GameAssetManifest.cs as ScriptableObject.

Fields:

* List<GameAssetEntry> entries
* Sprite missingSprite

Methods:

* GetSprite(string id)
* HasAsset(string id)
* GetEntry(string id)
* RebuildLookup()
* Return missingSprite if id is missing

Manifest path:
Assets/_Game/ScriptableObjects/AssetManifest/GameAssetManifest.asset

5. Create fallback missing sprite.

Generate:
Assets/_Game/Art/Generated/UI/missing_sprite_128x128.png

It should be a simple purple square with a question mark style placeholder.
Do not use external files.

6. Create Editor tool:

File:
Assets/_Game/Scripts/Editor/AssetTools/GameAssetPngGeneratorWindow.cs

Menu:
Tools/Isekai 12 Realms/Asset PNG Generator

The tool must:

* Generate placeholder PNGs for Priority 1 assets
* Generate matching .json metadata files
* Use exact filename format:
  object_widthxheight.png
* Use lowercase_snake_case
* Refresh AssetDatabase after generation
* Set import settings to Sprite
* Set alpha transparency for transparent assets
* Never download external images
* Never use copyrighted images
* Use procedural Unity Texture2D drawing only

7. Priority 1 assets to generate:

Backgrounds:

* bg_title_sky_realm_1080x1920.png
* bg_town_meadow_1080x1920.png
* bg_world_map_scroll_1080x1920.png
* bg_battle_meadow_1080x960.png

Characters:

* char_hero_flame_idle_512x512.png
* char_hero_flame_attack_512x512.png
* char_hero_flame_cast_512x512.png
* char_hero_flame_hurt_512x512.png

Enemies:

* enemy_meadow_slime_512x512.png
* boss_slime_king_768x768.png

Tokens:

* icon_token_sword_128x128.png
* icon_token_heart_128x128.png
* icon_token_coin_128x128.png
* icon_token_food_128x128.png
* icon_token_book_128x128.png
* icon_token_mana_128x128.png
* icon_token_shield_128x128.png
* icon_token_star_128x128.png

UI:

* ui_panel_main_768x512.png
* ui_panel_popup_768x512.png
* ui_btn_primary_384x128.png
* ui_btn_secondary_384x128.png
* ui_btn_close_128x128.png
* ui_bar_hp_bg_512x64.png
* ui_bar_hp_fill_512x64.png
* ui_bar_mana_bg_512x64.png
* ui_bar_mana_fill_512x64.png

Currency:

* currency_gold_128x128.png
* currency_soul_gem_128x128.png

Loading:

* logo_game_main_768x384.png
* loading_bar_frame_768x96.png
* loading_bar_fill_768x96.png
* icon_app_1024x1024.png

8. Placeholder art rules:

For backgrounds:

* Full rectangular PNG
* No transparency
* Use simple gradient/procedural shapes
* No text except logo_game_main

For characters/enemies/icons/UI:

* Transparent background
* Simple readable silhouette
* Bright fantasy colors
* Mobile-readable at small size
* No watermark
* No copied art
* No historical real persons
* No old game asset imitation

9. Metadata JSON rule:

For every generated PNG, create matching JSON in:
Assets/_Game/Art/Generated/Meta/

Example:
icon_token_sword_128x128.json

JSON fields:

* id
* file
* size
* category
* usage
* style
* transparent
* replaceable
* prompt

10. Create or update AssetManifest rebuild tool.

Menu:
Tools/Isekai 12 Realms/Rebuild Asset Manifest

It must:

* Scan Assets/_Game/Art/Generated
* Find all PNGs
* Parse filename size from object_widthxheight.png
* Create/update GameAssetManifest.asset
* Link Sprite references
* Assign category by folder or prefix
* Assign missingSprite
* Save assets
* Log summary:

  * total PNGs found
  * total entries created
  * missing sprite status

11. Update UI to use AssetManifest.

Create AssetSpriteBinder.cs.

Fields:

* string assetId
* Image targetImage

Behavior:

* On Awake or Start, load sprite from GameAssetManifest
* Assign to targetImage
* If missing, assign missingSprite
* Never crash on missing asset

12. Apply assets to TitleScreenUI:

* Background uses bg_title_sky_realm
* Logo area uses logo_game_main if available
* Main buttons use ui_btn_primary or ui_btn_secondary
* Keep all button text as TextMeshPro, not baked into PNG

13. Apply assets to MainTownUI:

* Background uses bg_town_meadow
* Main panels use ui_panel_main
* Gold icon uses currency_gold
* Gem icon uses currency_soul_gem
* Hero placeholder uses char_hero_flame_idle

14. Apply assets to WorldMapUI:

* Background uses bg_world_map_scroll
* Stage cards use ui_panel_main
* Buttons use ui_btn_primary/ui_btn_secondary

15. Apply assets to BattleUI:

* Background uses bg_battle_meadow
* Player sprite placeholder uses char_hero_flame_idle
* Enemy sprite uses enemy_meadow_slime or selected enemy spriteAssetId
* HP bars use ui_bar_hp_bg and ui_bar_hp_fill
* Mana bars use ui_bar_mana_bg and ui_bar_mana_fill
* Board tiles use token sprites:

  * Sword -> icon_token_sword
  * Heart -> icon_token_heart
  * Coin -> icon_token_coin
  * Food -> icon_token_food
  * Book -> icon_token_book
  * Mana -> icon_token_mana
  * Shield -> icon_token_shield
  * Star -> icon_token_star

16. Update TileView.

TileView must:

* Prefer token sprite from AssetManifest
* Fallback to colored square + TMP label if sprite missing
* Keep click/tap behavior unchanged
* Keep match-3 logic unchanged

17. Apply assets to BattleResultPopup:

* Popup panel uses ui_panel_popup
* Buttons use ui_btn_primary / ui_btn_secondary
* Reward icons use currency_gold and item placeholders if available

18. Add UI polish without breaking layout.

Improve:

* Button sizes
* Spacing
* Top HUD readability
* Bottom navigation readability
* Battle board centered
* Text contrast
* Popup dim background
* Safe area usage

Reference resolution remains:

* 1080 x 1920
* Portrait
* Canvas Scaler: Scale With Screen Size
* Match: 0.5

19. Do not bake text into button PNGs.

All visible labels must remain TextMeshPro:

* Start Game
* Settings
* Main Town
* World Map
* Victory
* Gold
* EXP

20. Add debug menu entries:

Tools/Isekai 12 Realms/Generate Priority 1 Placeholder PNGs
Tools/Isekai 12 Realms/Rebuild Asset Manifest
Tools/Isekai 12 Realms/Apply Placeholder Art To Current UI

21. Acceptance criteria:

* Run Tools/Isekai 12 Realms/Generate Priority 1 Placeholder PNGs
* Run Tools/Isekai 12 Realms/Rebuild Asset Manifest
* Open GameScene and press Play
* Title screen uses generated background/logo/button sprites
* Main Town uses generated background/panel/currency sprites
* World Map uses generated background/panels
* Battle screen uses generated battle background
* Match-3 board tiles show token icons instead of only debug colors
* Enemy sprite placeholder appears
* Player sprite placeholder appears
* Missing assets do not crash the game
* All text remains TextMeshPro
* Existing battle flow still works
* Existing save/reward/progression still works
* No console errors
* No missing script errors


### Prompt 10

