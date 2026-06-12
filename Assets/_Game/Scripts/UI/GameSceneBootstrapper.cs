using System;
using Isekai12Realms.Battle;
using Isekai12Realms.Board;
using Isekai12Realms.Character;
using Isekai12Realms.Core;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.Realms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Isekai12Realms.UI
{
    public class GameSceneBootstrapper : MonoBehaviour
    {
        public const string GameSceneName = "GameScene";

        private UIScreenManager screenManager;
        private PlayerProgressionService progressionService;
        private ContentDatabaseService contentService;
        private StageProgressionService stageProgressionService;
        private StageDefinition selectedStage;
        private string selectedRealmId;
        private RectTransform mainLayer;
        private RectTransform navigationLayer;
        private RectTransform popupLayer;
        private RectTransform toastLayer;
        private RectTransform loadingLayer;

        private readonly Color panelCream = new Color(1f, 0.95f, 0.84f, 0.94f);
        private readonly Color panelDark = new Color(0.08f, 0.11f, 0.28f, 0.94f);
        private readonly Color primary = new Color(0.18f, 0.76f, 0.82f, 1f);
        private readonly Color secondary = new Color(1f, 0.72f, 0.28f, 1f);
        private readonly Color danger = new Color(1f, 0.48f, 0.27f, 1f);
        private readonly Color textDark = new Color(0.12f, 0.18f, 0.28f, 1f);

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            Debug.Log("[Game] GameSceneBootstrapper started");
            if (SceneManager.GetActiveScene().name != GameSceneName && GameObject.Find("RootCanvas") == null)
            {
                return;
            }

            RepairSceneUi();
            screenManager.ShowScreen(GameUIScreen.Title);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameSceneName)
            {
                return;
            }

            RepairSceneUi();
            screenManager.ShowScreen(GameUIScreen.Title);
        }

        public void RepairSceneUi()
        {
            EnsureEventSystem();
            Canvas canvas = EnsureRootCanvas();
            RectTransform safeAreaRoot = EnsureChildRect(canvas.transform, "SafeAreaRoot");
            Stretch(safeAreaRoot);

            RectTransform backgroundLayer = EnsureLayer(safeAreaRoot, "BackgroundLayer");
            mainLayer = EnsureLayer(safeAreaRoot, "MainLayer");
            EnsureLayer(safeAreaRoot, "HudLayer");
            navigationLayer = EnsureLayer(safeAreaRoot, "NavigationLayer");
            popupLayer = EnsureLayer(safeAreaRoot, "PopupLayer");
            toastLayer = EnsureLayer(safeAreaRoot, "ToastLayer");
            loadingLayer = EnsureLayer(safeAreaRoot, "LoadingLayer");

            EnsureBackground(backgroundLayer);
            screenManager = EnsureUIScreenManager();
            RegisterServices();
            RegisterContentServices();
            RegisterMainScreens();
            DisableUnregisteredMainLayerChildren();
            screenManager.RegisterNavigationRoot(EnsureBottomNavigation(navigationLayer).gameObject);
            RegisterPopups();
            RegisterProgression();
        }

        private void RegisterContentServices()
        {
            contentService = GetComponent<ContentDatabaseService>();
            if (contentService == null)
            {
                contentService = gameObject.AddComponent<ContentDatabaseService>();
            }
            contentService.Initialize();

            stageProgressionService = GetComponent<StageProgressionService>();
            if (stageProgressionService == null)
            {
                stageProgressionService = gameObject.AddComponent<StageProgressionService>();
            }
        }

        private void RegisterProgression()
        {
            progressionService = GetComponent<PlayerProgressionService>();
            if (progressionService == null)
            {
                progressionService = gameObject.AddComponent<PlayerProgressionService>();
            }

            if (ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService))
            {
                progressionService.Initialize(saveService, screenManager.ToastService);
                stageProgressionService.Initialize(saveService, contentService);
                progressionService.Changed -= RefreshSaveBackedUi;
                progressionService.Changed += RefreshSaveBackedUi;
            }

            screenManager.ScreenChanged -= OnScreenChanged;
            screenManager.ScreenChanged += OnScreenChanged;
            RefreshSaveBackedUi();
        }

        private void OnScreenChanged(GameUIScreen previous, GameUIScreen current)
        {
            RefreshSaveBackedUi();
            if (current == GameUIScreen.WorldMap)
            {
                RefreshWorldMapState();
            }
        }

        public static void EnsureEventSystem()
        {
            EventSystem existing = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (existing != null)
            {
                AddAvailableInputModule(existing.gameObject);
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            AddAvailableInputModule(eventSystem);
        }

        private static void AddAvailableInputModule(GameObject eventSystem)
        {
            if (eventSystem.GetComponent<BaseInputModule>() != null)
            {
                return;
            }

            Type inputSystemModule = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null)
            {
                eventSystem.AddComponent(inputSystemModule);
                return;
            }

            eventSystem.AddComponent<StandaloneInputModule>();
        }

        public static Canvas EnsureRootCanvas()
        {
            GameObject canvasObject = GameObject.Find("RootCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("RootCanvas", typeof(RectTransform));
            }

            canvasObject.transform.localScale = Vector3.one;
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        public static RectTransform EnsureLayer(Transform parent, string layerName)
        {
            RectTransform layer = EnsureChildRect(parent, layerName);
            Stretch(layer);
            return layer;
        }

        private UIScreenManager EnsureUIScreenManager()
        {
            UIScreenManager manager = GetComponent<UIScreenManager>();
            if (manager == null)
            {
                manager = gameObject.AddComponent<UIScreenManager>();
            }
            return manager;
        }

        private void EnsureBackground(RectTransform parent)
        {
            RectTransform bg = EnsureChildRect(parent, "Background_Image");
            Stretch(bg);
            Image image = EnsureImage(bg.gameObject, new Color(0.08f, 0.11f, 0.28f, 1f));
            image.raycastTarget = false;
            bg.SetAsFirstSibling();
        }

        private void RegisterMainScreens()
        {
            screenManager.RegisterScreen(GameUIScreen.Title, CreateTitleScreen());
            screenManager.RegisterScreen(GameUIScreen.CharacterCreation, CreateCharacterCreation());
            screenManager.RegisterScreen(GameUIScreen.MainTown, CreateMainTown());
            screenManager.RegisterScreen(GameUIScreen.WorldMap, CreateWorldMap());
            screenManager.RegisterScreen(GameUIScreen.Adventure, CreateAdventure());
            screenManager.RegisterScreen(GameUIScreen.Battle, CreateBattle());
            screenManager.RegisterScreen(GameUIScreen.Hero, CreateHero());
            screenManager.RegisterScreen(GameUIScreen.Skills, CreateSkills());
            screenManager.RegisterScreen(GameUIScreen.Equipment, CreateEquipment());
            screenManager.RegisterScreen(GameUIScreen.Inventory, CreateInventory());
            screenManager.RegisterScreen(GameUIScreen.Quest, CreateQuest());
            screenManager.RegisterScreen(GameUIScreen.Shop, CreateShop());
        }

        private void DisableUnregisteredMainLayerChildren()
        {
            for (int i = 0; i < mainLayer.childCount; i++)
            {
                Transform child = mainLayer.GetChild(i);
                bool isKnownScreen = child.name == "TitleScreenUI" ||
                                     child.name == "CharacterCreationUI" ||
                                     child.name == "MainTownUI" ||
                                     child.name == "WorldMapUI" ||
                                     child.name == "AdventureUI" ||
                                     child.name == "BattleUI" ||
                                     child.name == "HeroUI" ||
                                     child.name == "SkillsUI" ||
                                     child.name == "EquipmentUI" ||
                                     child.name == "InventoryUI" ||
                                     child.name == "QuestUI" ||
                                     child.name == "ShopUI";

                child.gameObject.SetActive(isKnownScreen && child.name == "TitleScreenUI");
            }
        }

        private GameObject CreateTitleScreen()
        {
            RectTransform root = CreateScreenRoot("TitleScreenUI", new Color(0.06f, 0.08f, 0.2f, 0.35f));
            Text(root, "Title_Text", "ISEKAI 12 REALMS", 68, new Color(1f, 0.86f, 0.38f, 1f), Anchor.TopCenter, new Vector2(0f, -250f), new Vector2(940f, 110f));
            Text(root, "Subtitle_Text", "Offline Match-3 RPG", 38, Color.white, Anchor.TopCenter, new Vector2(0f, -340f), new Vector2(760f, 70f));
            Panel(root, "Hero_Preview", new Color(1f, 0.95f, 0.84f, 0.85f), Anchor.Center, new Vector2(0f, 120f), new Vector2(360f, 360f));
            Text(root, "Hero_Preview_Text", "Hero\nPreview", 36, textDark, Anchor.Center, new Vector2(0f, 120f), new Vector2(320f, 160f));

            Button(root, "Button_Start", "Start Game", primary, Anchor.Center, new Vector2(0f, -330f), new Vector2(520f, 112f), StartExistingGame);
            Button(root, "Button_NewHero", "New Hero", secondary, Anchor.Center, new Vector2(0f, -462f), new Vector2(520f, 104f), () => screenManager.ShowScreen(GameUIScreen.CharacterCreation));
            Button(root, "Button_Settings", "Settings", new Color(0.18f, 0.28f, 0.48f, 1f), Anchor.Center, new Vector2(0f, -584f), new Vector2(520f, 96f), screenManager.OpenSettings);
            Text(root, "Version_Text", "v0.1.0", 28, Color.white, Anchor.BottomCenter, new Vector2(0f, 55f), new Vector2(260f, 48f));
            return root.gameObject;
        }

        private GameObject CreateCharacterCreation()
        {
            RectTransform root = CreateScreenRoot("CharacterCreationUI", new Color(0.08f, 0.12f, 0.24f, 0.45f));
            Header(root, "Create Your Reborn Hero", () => screenManager.ShowScreen(GameUIScreen.Title));
            Panel(root, "CharacterPreviewPanel", panelCream, Anchor.Center, new Vector2(0f, 120f), new Vector2(760f, 640f));
            Text(root, "CharacterPreview_Text", "Character Preview Placeholder", 38, textDark, Anchor.Center, new Vector2(0f, 230f), new Vector2(650f, 80f));
            Text(root, "HeroName_Text", "Hero Name: Guest Hero", 34, textDark, Anchor.Center, new Vector2(0f, 70f), new Vector2(650f, 70f));

            Button(root, "Button_Class_Flame", "Flame Squire", danger, Anchor.Center, new Vector2(-260f, -70f), new Vector2(240f, 88f), screenManager.ShowDisabledToast);
            Button(root, "Button_Class_Tide", "Tide Acolyte", primary, Anchor.Center, new Vector2(0f, -70f), new Vector2(240f, 88f), screenManager.ShowDisabledToast);
            Button(root, "Button_Class_Storm", "Storm Scout", secondary, Anchor.Center, new Vector2(260f, -70f), new Vector2(240f, 88f), screenManager.ShowDisabledToast);
            Button(root, "Button_StartJourney", "Start Journey", primary, Anchor.BottomCenter, new Vector2(0f, 190f), new Vector2(520f, 112f), StartNewHero);
            Button(root, "Button_Back", "Back", secondary, Anchor.BottomCenter, new Vector2(0f, 70f), new Vector2(520f, 96f), () => screenManager.ShowScreen(GameUIScreen.Title));
            return root.gameObject;
        }

        private GameObject CreateMainTown()
        {
            RectTransform root = CreateScreenRoot("MainTownUI", new Color(0.04f, 0.12f, 0.16f, 0.35f));
            Panel(root, "TopHud", panelDark, Anchor.TopCenter, new Vector2(0f, -70f), new Vector2(1040f, 140f));
            Panel(root, "AvatarPlaceholder", secondary, Anchor.TopLeft, new Vector2(65f, -70f), new Vector2(90f, 90f));
            Text(root, "Name_Text", "Guest Hero", 32, Color.white, Anchor.TopLeft, new Vector2(130f, -48f), new Vector2(240f, 48f));
            Text(root, "Level_Text", "Lv. 1", 30, Color.white, Anchor.TopLeft, new Vector2(130f, -92f), new Vector2(160f, 42f));
            Text(root, "Gold_Text", "Gold: 0", 30, Color.white, Anchor.TopCenter, new Vector2(160f, -70f), new Vector2(220f, 52f));
            Text(root, "Gems_Text", "Gems: 0", 30, Color.white, Anchor.TopCenter, new Vector2(390f, -70f), new Vector2(220f, 52f));
            Button(root, "Button_Settings", "Settings", primary, Anchor.TopRight, new Vector2(-110f, -70f), new Vector2(170f, 82f), screenManager.OpenSettings);

            Panel(root, "TownPanel", panelCream, Anchor.Center, new Vector2(0f, 60f), new Vector2(900f, 980f));
            Text(root, "Town_Title", "Main Town Placeholder", 48, textDark, Anchor.Center, new Vector2(0f, 200f), new Vector2(760f, 90f));
            Button(root, "Button_QuestElder", "Quest Elder", secondary, Anchor.Center, new Vector2(-260f, -40f), new Vector2(240f, 104f), () => screenManager.ShowScreen(GameUIScreen.Quest));
            Button(root, "Button_Blacksmith", "Blacksmith", new Color(0.5f, 0.62f, 0.72f, 1f), Anchor.Center, new Vector2(0f, -40f), new Vector2(240f, 104f), () => screenManager.ShowScreen(GameUIScreen.Equipment));
            Button(root, "Button_ShopKeeper", "Shop Keeper", primary, Anchor.Center, new Vector2(260f, -40f), new Vector2(240f, 104f), () => screenManager.ShowScreen(GameUIScreen.Shop));
            return root.gameObject;
        }

        private GameObject CreateWorldMap()
        {
            RectTransform root = CreateScreenRoot("WorldMapUI", new Color(0.14f, 0.1f, 0.05f, 0.35f));
            Header(root, "World Map", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            Panel(root, "RealmListPanel", panelCream, Anchor.TopCenter, new Vector2(-260f, -520f), new Vector2(380f, 760f));
            Panel(root, "StageListPanel", panelCream, Anchor.TopCenter, new Vector2(210f, -520f), new Vector2(520f, 760f));

            Panel(root, "StageCard", panelDark, Anchor.BottomCenter, new Vector2(0f, 180f), new Vector2(850f, 260f));
            Text(root, "Stage_Title", "Select a stage", 34, Color.white, Anchor.BottomCenter, new Vector2(-190f, 255f), new Vector2(440f, 54f));
            Text(root, "Stage_Detail", "Stage details", 26, Color.white, Anchor.BottomCenter, new Vector2(-190f, 175f), new Vector2(470f, 110f));
            Button(root, "Button_EnterBattle", "Enter Battle", danger, Anchor.BottomCenter, new Vector2(250f, 200f), new Vector2(300f, 100f), EnterSelectedStage);
            BuildWorldMapContent(root);
            return root.gameObject;
        }

        private void BuildWorldMapContent(RectTransform root)
        {
            if (contentService == null || contentService.Database == null) return;
            for (int i = 0; i < contentService.Realms.Count; i++)
            {
                RealmDefinition realm = contentService.Realms[i];
                bool unlocked = RealmUnlocked(realm);
                Button(root, "Button_Realm_" + realm.id, realm.displayName, unlocked ? primary : new Color(0.45f, 0.48f, 0.55f, 1f), Anchor.TopCenter, new Vector2(-260f, -210f - i * 96f), new Vector2(320f, 76f), unlocked ? () => SelectRealm(realm.id) : () => screenManager.ToastService?.ShowToast("Complete previous stages first."));
            }

            if (contentService.Realms.Count > 0)
            {
                SelectRealm(contentService.Realms[0].id);
            }
        }

        private bool RealmUnlocked(RealmDefinition realm)
        {
            if (realm == null || realm.order <= 1) return true;
            if (realm.id == "realm_02_ember") return stageProgressionService != null && stageProgressionService.IsStageCompleted("stage_01_03");
            if (realm.id == "realm_03_tide") return stageProgressionService != null && stageProgressionService.IsStageCompleted("stage_02_03");
            return false;
        }

        private void SelectRealm(string realmId)
        {
            selectedRealmId = realmId;
            RectTransform root = mainLayer.Find("WorldMapUI") as RectTransform;
            if (root == null || contentService == null) return;
            foreach (StageDefinition stage in contentService.GetStagesForRealm(realmId))
            {
                int index = stage.stageNumber - 1;
                bool unlocked = stageProgressionService == null || stageProgressionService.IsStageUnlocked(stage);
                bool completed = stageProgressionService != null && stageProgressionService.IsStageCompleted(stage.id);
                Color color = !unlocked ? new Color(0.42f, 0.42f, 0.46f, 1f) : stage.isBossStage ? danger : completed ? secondary : primary;
                string label = (completed ? "Completed: " : unlocked ? "Available: " : "Locked: ") + stage.displayName;
                Button(root, "Button_Stage_" + stage.id, label, color, Anchor.TopCenter, new Vector2(210f, -210f - index * 96f), new Vector2(470f, 76f), unlocked ? () => SelectStage(stage) : () => screenManager.ToastService?.ShowToast("Complete previous stages first."));
            }
        }

        private void SelectStage(StageDefinition stage)
        {
            selectedStage = stage;
            int replayCount = stageProgressionService != null ? stageProgressionService.GetStageClearCount(stage.id) : 0;
            SetText("WorldMapUI/Stage_Title", stage.displayName);
            SetText("WorldMapUI/Stage_Detail", $"Recommended Lv. {stage.recommendedLevel}\nEnemy: {(stage.enemy != null ? stage.enemy.displayName : "Missing")}\nEXP {stage.baseExpReward}  Gold {stage.baseGoldReward}\nReplay Count: {replayCount}");
        }

        private void EnterSelectedStage()
        {
            if (selectedStage == null)
            {
                screenManager.ToastService?.ShowToast("Select a stage first.");
                return;
            }
            Transform battleRoot = mainLayer.Find("BattleUI");
            BattleUIController controller = battleRoot != null ? battleRoot.GetComponent<BattleUIController>() : null;
            if (controller != null)
            {
                controller.SetStage(selectedStage);
            }
            screenManager.ShowScreen(GameUIScreen.Battle);
        }

        private void RefreshWorldMapState()
        {
            if (contentService == null || contentService.Realms.Count == 0) return;
            RectTransform root = mainLayer.Find("WorldMapUI") as RectTransform;
            if (root != null)
            {
                for (int i = 0; i < contentService.Realms.Count; i++)
                {
                    RealmDefinition realm = contentService.Realms[i];
                    bool unlocked = RealmUnlocked(realm);
                    Button(root, "Button_Realm_" + realm.id, realm.displayName, unlocked ? primary : new Color(0.45f, 0.48f, 0.55f, 1f), Anchor.TopCenter, new Vector2(-260f, -210f - i * 96f), new Vector2(320f, 76f), unlocked ? () => SelectRealm(realm.id) : () => screenManager.ToastService?.ShowToast("Complete previous stages first."));
                }
            }
            SelectRealm(string.IsNullOrEmpty(selectedRealmId) ? contentService.Realms[0].id : selectedRealmId);
        }

        private GameObject CreateAdventure()
        {
            RectTransform root = CreateScreenRoot("AdventureUI", new Color(0.05f, 0.16f, 0.14f, 0.35f));
            Header(root, "Adventure Placeholder", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            Panel(root, "MapPanel", panelCream, Anchor.Center, new Vector2(0f, 80f), new Vector2(900f, 1020f));
            Text(root, "Map_Text", "2D Map Placeholder", 46, textDark, Anchor.Center, new Vector2(0f, 240f), new Vector2(680f, 90f));
            Panel(root, "PlayerPlaceholder", primary, Anchor.Center, new Vector2(-180f, 20f), new Vector2(140f, 180f));
            Text(root, "Player_Text", "Player", 28, Color.white, Anchor.Center, new Vector2(-180f, 20f), new Vector2(140f, 80f));
            Panel(root, "NpcPlaceholder", secondary, Anchor.Center, new Vector2(210f, 30f), new Vector2(150f, 180f));
            Text(root, "Npc_Text", "NPC", 28, textDark, Anchor.Center, new Vector2(210f, 30f), new Vector2(140f, 80f));
            Button(root, "Button_StartBattle", "Start Battle", danger, Anchor.BottomCenter, new Vector2(0f, 185f), new Vector2(520f, 110f), () => screenManager.ShowScreen(GameUIScreen.Battle));
            return root.gameObject;
        }

        private GameObject CreateBattle()
        {
            RectTransform root = CreateScreenRoot("BattleUI", new Color(0.04f, 0.06f, 0.12f, 0.35f));
            BattleUIController controller = root.GetComponent<BattleUIController>();
            if (controller == null)
            {
                controller = root.gameObject.AddComponent<BattleUIController>();
            }

            Header(root, "Battle Placeholder", controller.BackToWorldMap);

            Panel(root, "EnemyArea", panelDark, Anchor.TopCenter, new Vector2(0f, -190f), new Vector2(900f, 170f));
            Text(root, "EnemyName", "Meadow Slime", 34, Color.white, Anchor.TopCenter, new Vector2(-210f, -165f), new Vector2(360f, 60f));
            Bar(root, "EnemyHp", Anchor.TopCenter, new Vector2(190f, -165f), new Vector2(360f, 42f), danger);
            Text(root, "TurnText", "Your Turn", 30, Color.white, Anchor.TopCenter, new Vector2(-300f, -285f), new Vector2(230f, 54f));
            Text(root, "FoodText", "Food: 20", 30, Color.white, Anchor.TopCenter, new Vector2(0f, -285f), new Vector2(230f, 54f));
            Text(root, "ComboText", "Combo: 0", 30, Color.white, Anchor.TopCenter, new Vector2(300f, -285f), new Vector2(230f, 54f));
            BoardController boardController = CreateBoardGrid(root);
            Panel(root, "PlayerArea", panelDark, Anchor.BottomCenter, new Vector2(0f, 340f), new Vector2(900f, 150f));
            Text(root, "PlayerName", "Guest Hero", 32, Color.white, Anchor.BottomCenter, new Vector2(-300f, 360f), new Vector2(240f, 50f));
            Bar(root, "PlayerHp", Anchor.BottomCenter, new Vector2(0f, 380f), new Vector2(330f, 38f), new Color(0.5f, 0.85f, 0.34f, 1f));
            Bar(root, "PlayerMana", Anchor.BottomCenter, new Vector2(0f, 330f), new Vector2(330f, 38f), new Color(0.4f, 0.55f, 1f, 1f));
            Text(root, "GoldRewardText", "Gold: 0", 26, Color.white, Anchor.BottomCenter, new Vector2(290f, 372f), new Vector2(170f, 40f));
            Text(root, "ExpRewardText", "EXP: 0", 26, Color.white, Anchor.BottomCenter, new Vector2(290f, 324f), new Vector2(170f, 40f));
            Button(root, "Button_Skill1", "Spark Slash", primary, Anchor.BottomCenter, new Vector2(-315f, 220f), new Vector2(185f, 88f), controller.UseSkill1);
            Button(root, "Button_Skill2", "Shuffle Bell", primary, Anchor.BottomCenter, new Vector2(-105f, 220f), new Vector2(185f, 88f), controller.UseSkill2);
            Button(root, "Button_Ultimate", "Realm Burst", primary, Anchor.BottomCenter, new Vector2(105f, 220f), new Vector2(185f, 88f), controller.UseUltimate);
            Button(root, "Button_Item", "Item", primary, Anchor.BottomCenter, new Vector2(315f, 220f), new Vector2(185f, 88f), screenManager.ShowDisabledToast);
            Button(root, "Button_RestartBattle", "Restart Battle", secondary, Anchor.BottomCenter, new Vector2(-340f, 90f), new Vector2(250f, 90f), controller.StartBattle);
            Button(root, "Button_BackWorldMap", "World Map", primary, Anchor.BottomCenter, new Vector2(-70f, 90f), new Vector2(230f, 90f), controller.BackToWorldMap);
            Button(root, "Button_WinTest", "Win Test", new Color(0.5f, 0.85f, 0.34f, 1f), Anchor.BottomCenter, new Vector2(170f, 90f), new Vector2(190f, 90f), controller.WinTest);
            Button(root, "Button_LoseTest", "Lose Test", danger, Anchor.BottomCenter, new Vector2(385f, 90f), new Vector2(190f, 90f), controller.LoseTest);
            controller.Initialize(screenManager, boardController);
            return root.gameObject;
        }

        private GameObject CreateHero()
        {
            RectTransform root = CreateScreenRoot("HeroUI", new Color(0.07f, 0.11f, 0.2f, 0.35f));
            Header(root, "Hero", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            Panel(root, "HeroPreview", panelCream, Anchor.Center, new Vector2(-230f, 160f), new Vector2(360f, 460f));
            Text(root, "HeroPreview_Text", "Character\nPreview", 36, textDark, Anchor.Center, new Vector2(-230f, 160f), new Vector2(300f, 160f));
            Text(root, "Stats_Text", "HP 100\nATK 10\nMAG 8\nDEF 5\nSPD 5\nLUCK 1", 36, Color.white, Anchor.Center, new Vector2(240f, 160f), new Vector2(360f, 340f));
            Text(root, "Exp_Text", "EXP 0 / 50", 30, Color.white, Anchor.Center, new Vector2(240f, -65f), new Vector2(420f, 70f));
            Button(root, "Button_Skills", "Skills", primary, Anchor.BottomCenter, new Vector2(-170f, 240f), new Vector2(300f, 104f), () => screenManager.ShowScreen(GameUIScreen.Skills));
            Button(root, "Button_Equipment", "Equipment", secondary, Anchor.BottomCenter, new Vector2(170f, 240f), new Vector2(300f, 104f), () => screenManager.ShowScreen(GameUIScreen.Equipment));
            return root.gameObject;
        }

        private GameObject CreateSkills()
        {
            RectTransform root = CreateScreenRoot("SkillsUI", new Color(0.06f, 0.08f, 0.2f, 0.35f));
            Header(root, "Skills", () => screenManager.ShowScreen(GameUIScreen.Hero));
            Button(root, "Tab_Flame", "Flame", danger, Anchor.TopCenter, new Vector2(-260f, -210f), new Vector2(220f, 86f), screenManager.ShowDisabledToast);
            Button(root, "Tab_Tide", "Tide", primary, Anchor.TopCenter, new Vector2(0f, -210f), new Vector2(220f, 86f), screenManager.ShowDisabledToast);
            Button(root, "Tab_Storm", "Storm", secondary, Anchor.TopCenter, new Vector2(260f, -210f), new Vector2(220f, 86f), screenManager.ShowDisabledToast);
            Panel(root, "SkillList", panelCream, Anchor.Center, new Vector2(0f, 80f), new Vector2(860f, 860f));
            Text(root, "SkillList_Text", "[Icon] Spark Slash     Level 1\n[Icon] Aqua Heal       Level 1\n[Icon] Quick Jab       Level 1", 36, textDark, Anchor.Center, new Vector2(-40f, 140f), new Vector2(720f, 240f));
            Button(root, "Button_UpgradeDisabled", "Upgrade (Disabled)", new Color(0.45f, 0.48f, 0.55f, 1f), Anchor.BottomCenter, new Vector2(0f, 210f), new Vector2(520f, 100f), screenManager.ShowDisabledToast);
            return root.gameObject;
        }

        private GameObject CreateEquipment()
        {
            RectTransform root = CreateScreenRoot("EquipmentUI", new Color(0.08f, 0.1f, 0.17f, 0.35f));
            Header(root, "Equipment", () => screenManager.ShowScreen(GameUIScreen.Hero));
            string[] slots = { "Weapon", "Armor", "Head", "Boots", "Ring", "Charm" };
            for (int i = 0; i < slots.Length; i++)
            {
                Button(root, "Slot_" + slots[i], slots[i], secondary, Anchor.TopCenter, new Vector2(i % 2 == 0 ? -230f : 230f, -240f - (i / 2) * 120f), new Vector2(360f, 92f), screenManager.ShowDisabledToast);
            }
            Panel(root, "EquipmentDetail", panelCream, Anchor.BottomCenter, new Vector2(0f, 270f), new Vector2(860f, 400f));
            Text(root, "EquipmentDetail_Text", "Equipment detail placeholder", 38, textDark, Anchor.BottomCenter, new Vector2(0f, 270f), new Vector2(720f, 140f));
            return root.gameObject;
        }

        private GameObject CreateInventory()
        {
            RectTransform root = CreateScreenRoot("InventoryUI", new Color(0.06f, 0.11f, 0.17f, 0.35f));
            Header(root, "Bag", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            string[] tabs = { "All", "Equipment", "Material", "Consumable", "Quest" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Button(root, "Tab_" + tabs[i], tabs[i], primary, Anchor.TopCenter, new Vector2(-400f + i * 200f, -205f), new Vector2(180f, 82f), screenManager.ShowDisabledToast);
            }
            CreateInventoryGrid(root);
            Panel(root, "ItemDetail", panelCream, Anchor.BottomCenter, new Vector2(0f, 200f), new Vector2(860f, 230f));
            Text(root, "ItemDetail_Text", "Item detail placeholder", 34, textDark, Anchor.BottomCenter, new Vector2(0f, 200f), new Vector2(720f, 90f));
            Text(root, "InventoryList_Text", "Inventory empty", 28, textDark, Anchor.Center, new Vector2(0f, 120f), new Vector2(720f, 500f));
            Button(root, "Button_EquipFirst", "Equip First Equipment", secondary, Anchor.BottomCenter, new Vector2(0f, 335f), new Vector2(460f, 86f), EquipFirstEquipment);
            return root.gameObject;
        }

        private GameObject CreateQuest()
        {
            RectTransform root = CreateScreenRoot("QuestUI", new Color(0.07f, 0.09f, 0.17f, 0.35f));
            Header(root, "Quest", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            Panel(root, "QuestPanel", panelCream, Anchor.Center, new Vector2(0f, 110f), new Vector2(860f, 860f));
            Text(root, "QuestList_Text", "Main Quest: Reach Meadow Gate\nDaily Quest: Win 3 Battles\nAchievement: First Match", 38, textDark, Anchor.Center, new Vector2(0f, 160f), new Vector2(740f, 260f));
            Button(root, "Button_ClaimDisabled", "Claim (Disabled)", new Color(0.45f, 0.48f, 0.55f, 1f), Anchor.BottomCenter, new Vector2(0f, 230f), new Vector2(520f, 100f), screenManager.ShowDisabledToast);
            return root.gameObject;
        }

        private GameObject CreateShop()
        {
            RectTransform root = CreateScreenRoot("ShopUI", new Color(0.08f, 0.07f, 0.15f, 0.35f));
            Header(root, "Shop", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            string[] tabs = { "Daily", "Gold Shop", "Gem Shop", "IAP" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Button(root, "Tab_" + tabs[i].Replace(" ", ""), tabs[i], i == 3 ? secondary : primary, Anchor.TopCenter, new Vector2(-315f + i * 210f, -205f), new Vector2(190f, 82f), screenManager.ShowDisabledToast);
            }
            Panel(root, "ShopPanel", panelCream, Anchor.Center, new Vector2(0f, 70f), new Vector2(860f, 860f));
            Text(root, "GemPack_Text", "Tiny Gem Pack\nSmall Gem Pack\nMedium Gem Pack\nLarge Gem Pack", 38, textDark, Anchor.Center, new Vector2(0f, 160f), new Vector2(720f, 260f));
            Text(root, "IapRule_Text", "IAP will only sell Soul Gem currency.", 34, danger, Anchor.BottomCenter, new Vector2(0f, 230f), new Vector2(760f, 90f));
            return root.gameObject;
        }

        private RectTransform EnsureBottomNavigation(RectTransform parent)
        {
            RectTransform root = EnsureChildRect(parent, "BottomNavigation");
            Stretch(root);
            Panel(root, "NavPanel", panelDark, Anchor.BottomCenter, new Vector2(0f, 75f), new Vector2(1040f, 150f));
            Button(root, "Button_Adventure", "Adventure", primary, Anchor.BottomCenter, new Vector2(-420f, 75f), new Vector2(190f, 98f), () => screenManager.ShowScreen(GameUIScreen.WorldMap));
            Button(root, "Button_Hero", "Hero", primary, Anchor.BottomCenter, new Vector2(-210f, 75f), new Vector2(190f, 98f), () => screenManager.ShowScreen(GameUIScreen.Hero));
            Button(root, "Button_Bag", "Bag", primary, Anchor.BottomCenter, new Vector2(0f, 75f), new Vector2(190f, 98f), () => screenManager.ShowScreen(GameUIScreen.Inventory));
            Button(root, "Button_Quest", "Quest", primary, Anchor.BottomCenter, new Vector2(210f, 75f), new Vector2(190f, 98f), () => screenManager.ShowScreen(GameUIScreen.Quest));
            Button(root, "Button_Shop", "Shop", primary, Anchor.BottomCenter, new Vector2(420f, 75f), new Vector2(190f, 98f), () => screenManager.ShowScreen(GameUIScreen.Shop));
            root.gameObject.SetActive(false);
            return root;
        }

        private void RegisterPopups()
        {
            GameObject settings = CreateSettingsPopup();
            screenManager.RegisterSettingsPopup(settings);
            CreateDeleteConfirmPopup();

            TextMeshProUGUI resultTitle;
            TextMeshProUGUI resultRewards;
            GameObject victoryButtons;
            GameObject defeatButtons;
            GameObject battleResult = CreateBattleResultPopup(out resultTitle, out resultRewards, out victoryButtons, out defeatButtons);
            screenManager.RegisterBattleResultPopup(battleResult, resultTitle, resultRewards, victoryButtons, defeatButtons);
        }

        private GameObject CreateSettingsPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "SettingsPopup");
            Stretch(root);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.45f)).raycastTarget = true;
            Panel(root, "SettingsPanel", panelCream, Anchor.Center, Vector2.zero, new Vector2(820f, 760f));
            Text(root, "Title", "Settings", 54, textDark, Anchor.Center, new Vector2(0f, 260f), new Vector2(660f, 80f));
            Text(root, "Body", "Music Toggle Placeholder\nSFX Toggle Placeholder\nCloud Save Placeholder", 36, textDark, Anchor.Center, new Vector2(0f, 70f), new Vector2(680f, 260f));
            Button(root, "Button_DeleteSave", "Delete Local Save", danger, Anchor.Center, new Vector2(0f, -190f), new Vector2(420f, 90f), OpenDeleteConfirm);
            Button(root, "Button_Close", "Close", primary, Anchor.Center, new Vector2(0f, -300f), new Vector2(360f, 90f), screenManager.CloseSettings);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateDeleteConfirmPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "DeleteSaveConfirmPopup");
            Stretch(root);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.62f)).raycastTarget = true;
            Panel(root, "Panel", panelCream, Anchor.Center, Vector2.zero, new Vector2(760f, 520f));
            Text(root, "Title", "Are you sure?", 50, textDark, Anchor.Center, new Vector2(0f, 130f), new Vector2(620f, 80f));
            Text(root, "Body", "This deletes the local save on this device.", 32, textDark, Anchor.Center, new Vector2(0f, 40f), new Vector2(620f, 80f));
            Button(root, "Button_Confirm", "Confirm", danger, Anchor.Center, new Vector2(-150f, -145f), new Vector2(260f, 92f), ConfirmDeleteSave);
            Button(root, "Button_Cancel", "Cancel", primary, Anchor.Center, new Vector2(150f, -145f), new Vector2(260f, 92f), CloseDeleteConfirm);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateBattleResultPopup(out TextMeshProUGUI title, out TextMeshProUGUI rewards, out GameObject victoryButtons, out GameObject defeatButtons)
        {
            RectTransform root = EnsureChildRect(popupLayer, "BattleResultPopup");
            Stretch(root);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.55f)).raycastTarget = true;
            Panel(root, "ResultPanel", panelCream, Anchor.Center, Vector2.zero, new Vector2(820f, 820f));
            title = Text(root, "ResultTitle", "Victory!", 58, textDark, Anchor.Center, new Vector2(0f, 275f), new Vector2(680f, 90f));
            rewards = Text(root, "ResultRewards", "EXP +50\nGold +30", 38, textDark, Anchor.Center, new Vector2(0f, 110f), new Vector2(680f, 180f));

            victoryButtons = EnsureChildRect(root, "VictoryButtons").gameObject;
            Stretch(victoryButtons.GetComponent<RectTransform>());
            Button(victoryButtons.transform, "Button_Continue", "Continue", primary, Anchor.Center, new Vector2(0f, -60f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.WorldMap));
            Button(victoryButtons.transform, "Button_Replay", "Replay", secondary, Anchor.Center, new Vector2(0f, -165f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.Battle));
            Button(victoryButtons.transform, "Button_Town", "Town", danger, Anchor.Center, new Vector2(0f, -270f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.MainTown));

            defeatButtons = EnsureChildRect(root, "DefeatButtons").gameObject;
            Stretch(defeatButtons.GetComponent<RectTransform>());
            Button(defeatButtons.transform, "Button_Retry", "Retry", primary, Anchor.Center, new Vector2(0f, -60f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.Battle));
            Button(defeatButtons.transform, "Button_Upgrade", "Upgrade", secondary, Anchor.Center, new Vector2(0f, -165f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.Hero));
            Button(defeatButtons.transform, "Button_Town", "Town", danger, Anchor.Center, new Vector2(0f, -270f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.MainTown));

            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private void RegisterServices()
        {
            ToastService toast = GetComponent<ToastService>();
            if (toast == null)
            {
                toast = gameObject.AddComponent<ToastService>();
            }

            RectTransform toastRoot = EnsureChildRect(toastLayer, "ToastPanel");
            SetRect(toastRoot, Anchor.BottomCenter, new Vector2(0f, 260f), new Vector2(760f, 92f));
            EnsureImage(toastRoot.gameObject, new Color(0.05f, 0.07f, 0.12f, 0.9f));
            TextMeshProUGUI toastText = Text(toastRoot, "ToastText", "Toast", 30, Color.white, Anchor.Center, Vector2.zero, new Vector2(720f, 80f));
            toast.Initialize(toastRoot.gameObject, toastText);

            LoadingOverlayUI loading = GetComponent<LoadingOverlayUI>();
            if (loading == null)
            {
                loading = gameObject.AddComponent<LoadingOverlayUI>();
            }

            RectTransform loadingRoot = EnsureChildRect(loadingLayer, "LoadingOverlayUI");
            Stretch(loadingRoot);
            EnsureImage(loadingRoot.gameObject, new Color(0f, 0f, 0f, 0.65f)).raycastTarget = true;
            TextMeshProUGUI loadingText = Text(loadingRoot, "LoadingText", "Loading...", 44, Color.white, Anchor.Center, Vector2.zero, new Vector2(500f, 100f));
            loading.Initialize(loadingRoot.gameObject, loadingText);

            screenManager.RegisterServices(toast, loading);
        }

        private void StartExistingGame()
        {
            progressionService?.LoadOrCreate();
            RefreshSaveBackedUi();
            screenManager.ShowScreen(GameUIScreen.MainTown);
        }

        private void StartNewHero()
        {
            progressionService?.CreateNewSave("flame_squire", "Guest Hero");
            RefreshSaveBackedUi();
            screenManager.ShowScreen(GameUIScreen.MainTown);
        }

        private void EquipFirstEquipment()
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            if (save == null || save.inventory.equipments.Count == 0)
            {
                screenManager.ToastService?.ShowToast("Not enough item");
                return;
            }

            progressionService.Equip(save.inventory.equipments[0].instanceId);
            RefreshSaveBackedUi();
        }

        private void OpenDeleteConfirm()
        {
            Transform popup = popupLayer.Find("DeleteSaveConfirmPopup");
            if (popup != null)
            {
                popup.gameObject.SetActive(true);
            }
        }

        private void CloseDeleteConfirm()
        {
            Transform popup = popupLayer.Find("DeleteSaveConfirmPopup");
            if (popup != null)
            {
                popup.gameObject.SetActive(false);
            }
        }

        private void ConfirmDeleteSave()
        {
            progressionService?.DeleteSave();
            CloseDeleteConfirm();
            screenManager.CloseSettings();
            screenManager.ShowScreen(GameUIScreen.Title);
        }

        private void RefreshSaveBackedUi()
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            if (save == null)
            {
                return;
            }

            SetText("MainTownUI/Name_Text", save.playerName);
            SetText("MainTownUI/Level_Text", $"Lv. {save.level}");
            SetText("MainTownUI/Gold_Text", $"Gold: {save.gold}");
            SetText("MainTownUI/Gems_Text", $"Gems: {save.soulGem}");

            PlayerStats stats = progressionService.CalculateTotalStats();
            SetText("HeroUI/Stats_Text", $"Lv. {save.level}\nHP {stats.hp}\nMana {stats.mana}\nATK {stats.atk}\nMAG {stats.mag}\nDEF {stats.def}\nSPD {stats.spd}\nLUCK 1");
            SetText("HeroUI/Exp_Text", $"EXP {save.exp} / {progressionService.GetExpRequired(save.level)}");

            SetText("InventoryUI/InventoryList_Text", BuildInventoryText(save));
            SetText("EquipmentUI/EquipmentDetail_Text", BuildEquipmentText(save));
        }

        private string BuildInventoryText(PlayerSaveData save)
        {
            string text = "Items:\n";
            if (save.inventory.items.Count == 0)
            {
                text += "- None\n";
            }
            else
            {
                foreach (ItemStackData item in save.inventory.items)
                {
                    text += $"- {PrototypeItemDatabase.Get(item.itemId).displayName} x{item.amount}\n";
                }
            }

            text += "\nEquipment:\n";
            if (save.inventory.equipments.Count == 0)
            {
                text += "- None";
            }
            else
            {
                foreach (EquipmentInstanceData equipment in save.inventory.equipments)
                {
                    text += $"- {equipment.displayName} ({equipment.slot})\n";
                }
            }
            return text;
        }

        private string BuildEquipmentText(PlayerSaveData save)
        {
            return "Equipped Slots:\n" +
                   $"Weapon: {GetEquippedName(save, save.equipment.weaponInstanceId)}\n" +
                   $"Armor: {GetEquippedName(save, save.equipment.armorInstanceId)}\n" +
                   $"Head: {GetEquippedName(save, save.equipment.headInstanceId)}\n" +
                   $"Boots: {GetEquippedName(save, save.equipment.bootsInstanceId)}\n" +
                   $"Ring: {GetEquippedName(save, save.equipment.ringInstanceId)}\n" +
                   $"Charm: {GetEquippedName(save, save.equipment.charmInstanceId)}";
        }

        private string GetEquippedName(PlayerSaveData save, string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId)) return "Empty";
            EquipmentInstanceData equipment = save.inventory.equipments.Find(e => e.instanceId == instanceId);
            return equipment != null ? equipment.displayName : "Missing";
        }

        private void SetText(string path, string value)
        {
            Transform target = mainLayer != null ? mainLayer.Find(path) : null;
            TextMeshProUGUI text = target != null ? target.GetComponent<TextMeshProUGUI>() : null;
            if (text != null)
            {
                text.text = value;
            }
        }

        private BoardController CreateBoardGrid(RectTransform root)
        {
            RectTransform board = EnsureChildRect(root, "BoardGrid");
            SetRect(board, Anchor.Center, new Vector2(0f, -50f), new Vector2(768f, 768f));
            GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = board.gameObject.AddComponent<GridLayoutGroup>();
            }
            grid.cellSize = new Vector2(88f, 88f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            Image boardBackground = board.GetComponent<Image>();
            if (boardBackground == null)
            {
                boardBackground = board.gameObject.AddComponent<Image>();
            }
            boardBackground.color = new Color(0.05f, 0.06f, 0.11f, 0.75f);

            BoardController boardController = board.GetComponent<BoardController>();
            if (boardController == null)
            {
                boardController = board.gameObject.AddComponent<BoardController>();
            }

            return boardController;
        }

        private void CreateInventoryGrid(RectTransform root)
        {
            RectTransform gridRoot = EnsureChildRect(root, "InventoryGrid");
            SetRect(gridRoot, Anchor.Center, new Vector2(0f, 120f), new Vector2(780f, 620f));
            GridLayoutGroup grid = gridRoot.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = gridRoot.gameObject.AddComponent<GridLayoutGroup>();
            }
            grid.cellSize = new Vector2(128f, 128f);
            grid.spacing = new Vector2(22f, 22f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            for (int i = 0; i < 20; i++)
            {
                RectTransform slot = EnsureChildRect(gridRoot, "Slot_" + i.ToString("00"));
                EnsureImage(slot.gameObject, new Color(0.1f, 0.15f, 0.25f, 0.88f));
            }
        }

        private void Header(RectTransform root, string title, UnityEngine.Events.UnityAction backAction)
        {
            Panel(root, "HeaderPanel", panelDark, Anchor.TopCenter, new Vector2(0f, -65f), new Vector2(1040f, 130f));
            Text(root, "HeaderTitle", title, 42, Color.white, Anchor.TopCenter, new Vector2(0f, -65f), new Vector2(650f, 80f));
            Button(root, "Button_Back", "Back", secondary, Anchor.TopLeft, new Vector2(105f, -65f), new Vector2(170f, 82f), backAction);
            Button(root, "Button_Settings", "Settings", primary, Anchor.TopRight, new Vector2(-120f, -65f), new Vector2(190f, 82f), screenManager.OpenSettings);
        }

        private RectTransform CreateScreenRoot(string name, Color backgroundColor)
        {
            RectTransform root = EnsureChildRect(mainLayer, name);
            Stretch(root);
            EnsureImage(root.gameObject, backgroundColor);
            root.gameObject.SetActive(false);
            return root;
        }

        private Image Panel(Transform parent, string name, Color color, Anchor anchor, Vector2 pos, Vector2 size)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            return EnsureImage(rect.gameObject, color);
        }

        private TextMeshProUGUI Text(Transform parent, string name, string text, int size, Color color, Anchor anchor, Vector2 pos, Vector2 rectSize)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, rectSize);
            TextMeshProUGUI label = rect.GetComponent<TextMeshProUGUI>();
            if (label == null)
            {
                label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            }
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private Button Button(Transform parent, string name, string text, Color color, Anchor anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            EnsureImage(rect.gameObject, color).raycastTarget = true;
            Button button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }
            Text(rect, "Text", text, Mathf.RoundToInt(size.y * 0.34f), Color.white, Anchor.Center, Vector2.zero, size);
            return button;
        }

        private void Bar(Transform parent, string name, Anchor anchor, Vector2 pos, Vector2 size, Color fillColor)
        {
            RectTransform root = EnsureChildRect(parent, name);
            SetRect(root, anchor, pos, size);
            EnsureImage(root.gameObject, new Color(0.04f, 0.05f, 0.08f, 1f));
            RectTransform fill = EnsureChildRect(root, "Fill");
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0.75f, 1f);
            fill.offsetMin = new Vector2(4f, 4f);
            fill.offsetMax = new Vector2(-4f, -4f);
            EnsureImage(fill.gameObject, fillColor);
        }

        public static RectTransform EnsureChildRect(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                GameObject childObject = new GameObject(childName, typeof(RectTransform));
                childObject.transform.SetParent(parent, false);
                child = childObject.transform;
            }
            return child.GetComponent<RectTransform>();
        }

        private static Image EnsureImage(GameObject target, Color color)
        {
            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                image = target.AddComponent<Image>();
            }
            image.color = color;
            image.type = Image.Type.Simple;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void SetRect(RectTransform rect, Anchor anchor, Vector2 pos, Vector2 size)
        {
            switch (anchor)
            {
                case Anchor.TopLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    break;
                case Anchor.TopRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    break;
                case Anchor.TopCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                    break;
                case Anchor.BottomCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    break;
                default:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
            }
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private enum Anchor
        {
            Center,
            TopCenter,
            TopLeft,
            TopRight,
            BottomCenter
        }
    }
}
