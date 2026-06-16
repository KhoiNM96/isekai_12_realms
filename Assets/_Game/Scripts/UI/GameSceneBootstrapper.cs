using System;
using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Auth;
using Isekai12Realms.Battle;
using Isekai12Realms.Board;
using Isekai12Realms.Build;
using Isekai12Realms.Character;
using Isekai12Realms.CloudSave;
using Isekai12Realms.Addressables;
using Isekai12Realms.Adventure;
using Isekai12Realms.ContentPacks;
using Isekai12Realms.Core;
using Isekai12Realms.Crafting;
using Isekai12Realms.Data;
using Isekai12Realms.Diagnostics;
using Isekai12Realms.Audio;
using Isekai12Realms.Economy;
using Isekai12Realms.Equipment;
using Isekai12Realms.Enemies;
using Isekai12Realms.Inventory;
using Isekai12Realms.IAP;
using Isekai12Realms.Purchases;
using Isekai12Realms.Performance;
using Isekai12Realms.Quests;
using Isekai12Realms.QA;
using Isekai12Realms.RemoteConfig;
using Isekai12Realms.Services;
using Isekai12Realms.Shop;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using Isekai12Realms.Tutorial;
using Isekai12Realms.Realms;
using Isekai12Realms.VFX;
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
        private RealmProgressionService realmProgressionService;
        private SkillService skillService;
        private EquipmentService equipmentService;
        private CraftingService craftingService;
        private QuestService questService;
        private TutorialService tutorialService;
        private TutorialOverlayUI tutorialOverlay;
        private ShopService shopService;
        private IIAPService iapService;
        private PurchaseLedgerService purchaseLedgerService;
        private CurrencyGrantService currencyGrantService;
        private CloudSaveCoordinator cloudSaveCoordinator;
        private IAssetLoadService assetLoadService;
        private ContentPackService contentPackService;
        private AdventureMapService adventureMapService;
        private RealmAdventureMapUIController realmAdventureMapController;
        private WorldMapUIController worldMapController;
        private GameConfigService gameConfigService;
        private BuildConfigService buildConfigService;
        private DiagnosticsService diagnosticsService;
        [SerializeField] private GameAssetManifest assetManifest;
        [SerializeField] private GameConfigData gameConfigDefaults;
        [SerializeField] private BuildConfig buildConfig;
        private StageDefinition selectedStage;
        private string selectedRealmId;
        private string selectedCreationClassId = "flame_squire";
        private string selectedSkillClassId = "flame_squire";
        private string selectedSkillId;
        private string selectedEquipmentInstanceId;
        private ShopType selectedShopType = ShopType.Daily;
        private string selectedShopItemId;
        private string selectedContentPackId;
        private RectTransform mainLayer;
        private RectTransform navigationLayer;
        private RectTransform popupLayer;
        private RectTransform toastLayer;
        private RectTransform loadingLayer;
        private AudioService audioService;
        private IPopupService popupService;

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
            popupService?.CloseAll();
            screenManager.ShowScreen(GameUIScreen.Title);
            EnsureVisibleScreenOrRecover();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != GameSceneName)
            {
                return;
            }

            RepairSceneUi();
            popupService?.CloseAll();
            screenManager.ShowScreen(GameUIScreen.Title);
            EnsureVisibleScreenOrRecover();
        }

        public void RepairSceneUi()
        {
            EnsureEventSystem();
            Canvas canvas = EnsureRootCanvas();
            canvas.gameObject.SetActive(true);
            RectTransform safeAreaRoot = EnsureChildRect(canvas.transform, "SafeAreaRoot");
            Stretch(safeAreaRoot);
            safeAreaRoot.gameObject.SetActive(true);

            RectTransform backgroundLayer = EnsureLayer(safeAreaRoot, "BackgroundLayer");
            mainLayer = EnsureLayer(safeAreaRoot, "MainLayer");
            RectTransform hudLayer = EnsureLayer(safeAreaRoot, "HudLayer");
            navigationLayer = EnsureLayer(safeAreaRoot, "NavigationLayer");
            popupLayer = EnsureLayer(safeAreaRoot, "PopupLayer");
            toastLayer = EnsureLayer(safeAreaRoot, "ToastLayer");
            loadingLayer = EnsureLayer(safeAreaRoot, "LoadingLayer");
            backgroundLayer.gameObject.SetActive(true);
            mainLayer.gameObject.SetActive(true);
            hudLayer.gameObject.SetActive(true);
            navigationLayer.gameObject.SetActive(true);
            popupLayer.gameObject.SetActive(true);
            toastLayer.gameObject.SetActive(true);
            loadingLayer.gameObject.SetActive(true);
            EnsureLayerOrder(safeAreaRoot);
            ResolvePopupService();
            if (popupService != null)
            {
                popupService.SetPopupLayer(popupLayer);
            }
            tutorialOverlay = GetComponent<TutorialOverlayUI>();
            if (tutorialOverlay == null)
            {
                tutorialOverlay = gameObject.AddComponent<TutorialOverlayUI>();
            }
            tutorialOverlay.Initialize(popupLayer);

            RegisterAssetManifest();
            EnsureBackground(backgroundLayer);
            screenManager = EnsureUIScreenManager();
            RegisterServices();
            RegisterContentServices();
            RegisterMainScreens();
            DisableUnregisteredMainLayerChildren();
            screenManager.RegisterNavigationRoot(EnsureBottomNavigation(navigationLayer).gameObject);
            RegisterPopups();
            RegisterProgression();
            RegisterReleaseHardeningServices();
        }

        private void RegisterAssetManifest()
        {
#if UNITY_EDITOR
            if (assetManifest == null)
            {
                assetManifest = UnityEditor.AssetDatabase.LoadAssetAtPath<GameAssetManifest>("Assets/_Game/ScriptableObjects/GameAssetManifest.asset");
            }
#endif
            AssetSpriteBinder.SetManifest(assetManifest);
#if USE_ADDRESSABLES
            assetLoadService = new AddressableAssetLoadService(assetManifest);
#else
            assetLoadService = new MockAssetLoadService(assetManifest);
#endif
            AssetSpriteBinder.SetAssetLoadService(assetLoadService);
            ServiceLocator.Register<IAssetLoadService>(assetLoadService);
        }

        private void RegisterContentServices()
        {
            contentService = GetComponent<ContentDatabaseService>();
            if (contentService == null)
            {
                contentService = gameObject.AddComponent<ContentDatabaseService>();
            }
            contentService.Initialize();
            contentPackService = new ContentPackService(contentService.Database, assetLoadService);
            ServiceLocator.Register(contentPackService);

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
                progressionService.Initialize(saveService, screenManager.ToastService, gameConfigService);
                stageProgressionService.Initialize(saveService, contentService);
                realmProgressionService = GetComponent<RealmProgressionService>();
                if (realmProgressionService == null)
                {
                    realmProgressionService = gameObject.AddComponent<RealmProgressionService>();
                }
                realmProgressionService.Initialize(saveService, contentService, progressionService);
                equipmentService = GetComponent<EquipmentService>();
                if (equipmentService == null)
                {
                    equipmentService = gameObject.AddComponent<EquipmentService>();
                }
                equipmentService.Initialize(saveService, contentService, screenManager.ToastService);
                craftingService = GetComponent<CraftingService>();
                if (craftingService == null)
                {
                    craftingService = gameObject.AddComponent<CraftingService>();
                }
                craftingService.Initialize(saveService, equipmentService, screenManager.ToastService);
                skillService = GetComponent<SkillService>();
                if (skillService == null)
                {
                    skillService = gameObject.AddComponent<SkillService>();
                }
                skillService.Initialize(saveService, contentService, screenManager.ToastService);
                questService = GetComponent<QuestService>();
                if (questService == null)
                {
                    questService = gameObject.AddComponent<QuestService>();
                }
                questService.Initialize(saveService, contentService, progressionService, equipmentService, screenManager.ToastService);
                shopService = GetComponent<ShopService>();
                if (shopService == null)
                {
                    shopService = gameObject.AddComponent<ShopService>();
                }
                shopService.Initialize(saveService, contentService, progressionService, equipmentService, questService, screenManager.ToastService, gameConfigService);
                cloudSaveCoordinator = GetComponent<CloudSaveCoordinator>();
                if (cloudSaveCoordinator == null)
                {
                    cloudSaveCoordinator = gameObject.AddComponent<CloudSaveCoordinator>();
                }
                cloudSaveCoordinator.Initialize(saveService, screenManager.ToastService);
                purchaseLedgerService = new PurchaseLedgerService(saveService);
                currencyGrantService = new CurrencyGrantService(saveService, progressionService, purchaseLedgerService, cloudSaveCoordinator, screenManager.ToastService);
#if USE_UNITY_IAP
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                iapService = new UnityIAPService(contentService, currencyGrantService, purchaseLedgerService, new LocalReceiptValidatorService(), screenManager.ToastService);
#else
                iapService = buildConfigService != null && buildConfigService.RequireServerReceiptValidationForProduction
                    ? new UnityIAPService(contentService, currencyGrantService, purchaseLedgerService, new ServerReceiptValidatorService(string.Empty, saveService.CurrentSave != null ? saveService.CurrentSave.firebaseUid : string.Empty), screenManager.ToastService)
                    : new UnityIAPService(contentService, currencyGrantService, purchaseLedgerService, new LocalReceiptValidatorService(), screenManager.ToastService);
#endif
#else
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                iapService = new MockIAPService(contentService, currencyGrantService, purchaseLedgerService, screenManager.ToastService);
#else
                iapService = new UnityIAPService(contentService, currencyGrantService, purchaseLedgerService, new LocalReceiptValidatorService(), screenManager.ToastService);
#endif
#endif
                iapService.OnPurchaseStarted -= OnIapPurchaseStarted;
                iapService.OnPurchaseStarted += OnIapPurchaseStarted;
                iapService.OnPurchaseSucceeded -= OnIapPurchaseSucceeded;
                iapService.OnPurchaseSucceeded += OnIapPurchaseSucceeded;
                iapService.OnPurchaseFailed -= OnIapPurchaseFailed;
                iapService.OnPurchaseFailed += OnIapPurchaseFailed;
                iapService.OnRestoreCompleted -= OnIapRestoreCompleted;
                iapService.OnRestoreCompleted += OnIapRestoreCompleted;
                iapService.OnIAPStatusChanged -= OnIapStatusChanged;
                iapService.OnIAPStatusChanged += OnIapStatusChanged;
                if (gameConfigService == null || gameConfigService.IapEnabled)
                {
                    _ = iapService.InitializeAsync();
                }
                tutorialService = GetComponent<TutorialService>();
                if (tutorialService == null)
                {
                    tutorialService = gameObject.AddComponent<TutorialService>();
                }
                tutorialService.Initialize(saveService, contentService, tutorialOverlay, gameConfigService);
                questService.QuestsChanged -= RefreshSaveBackedUi;
                questService.QuestsChanged += RefreshSaveBackedUi;
                questService.QuestCompleted -= OnQuestCompleted;
                questService.QuestCompleted += OnQuestCompleted;
                shopService.Changed -= RefreshSaveBackedUi;
                shopService.Changed += RefreshSaveBackedUi;
                cloudSaveCoordinator.StatusChanged -= RefreshSaveBackedUi;
                cloudSaveCoordinator.StatusChanged += RefreshSaveBackedUi;
                cloudSaveCoordinator.ConflictDetected -= OnCloudConflictDetected;
                cloudSaveCoordinator.ConflictDetected += OnCloudConflictDetected;
                progressionService.Changed -= RefreshSaveBackedUi;
                progressionService.Changed += RefreshSaveBackedUi;

                adventureMapService = GetComponent<AdventureMapService>();
                if (adventureMapService == null)
                {
                    adventureMapService = gameObject.AddComponent<AdventureMapService>();
                }

                BattleUIController battleController = mainLayer != null ? mainLayer.Find("BattleUI")?.GetComponent<BattleUIController>() : null;
                RealmAdventureMapUIController adventureController = mainLayer != null ? mainLayer.Find("RealmAdventureMapUI")?.GetComponent<RealmAdventureMapUIController>() : null;
                worldMapController = mainLayer != null ? mainLayer.Find("WorldMapUI")?.GetComponent<WorldMapUIController>() : null;
                realmAdventureMapController = adventureController;
                adventureMapService.Initialize(screenManager, contentService, realmProgressionService, progressionService, battleController, adventureController);
                adventureController?.Initialize(screenManager, adventureMapService, contentService, realmProgressionService, progressionService);
                worldMapController?.Initialize(screenManager, adventureMapService, contentService, realmProgressionService, progressionService);
            }

            screenManager.ScreenChanged -= OnScreenChanged;
            screenManager.ScreenChanged += OnScreenChanged;
            RefreshSaveBackedUi();
        }

        private void OnScreenChanged(GameUIScreen previous, GameUIScreen current)
        {
            string screenTargetId = ScreenTargetId(current);
            questService?.TrackProgress(QuestObjectiveType.OpenScreen, screenTargetId, 1);
            tutorialService?.HandleScreenOpened(screenTargetId);
            RefreshSaveBackedUi();
            if (current == GameUIScreen.WorldMap)
            {
                RefreshWorldMapState();
            }
            if (current == GameUIScreen.RealmAdventureMap)
            {
                RefreshAdventureMapState();
            }
        }

        private void OnQuestCompleted(string questId)
        {
            tutorialService?.HandleQuestCompleted(questId);
            RefreshSaveBackedUi();
        }

        private static string ScreenTargetId(GameUIScreen screen)
        {
            return "screen_" + screen.ToString().ToLowerInvariant();
        }

        public static void EnsureEventSystem()
        {
            EventSystem existing = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (existing == null)
            {
                EventSystem[] all = Resources.FindObjectsOfTypeAll<EventSystem>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].gameObject.scene.IsValid())
                    {
                        existing = all[i];
                        break;
                    }
                }
            }
            if (existing != null)
            {
                AddAvailableInputModule(existing.gameObject);
                existing.gameObject.SetActive(true);
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
                Canvas[] all = Resources.FindObjectsOfTypeAll<Canvas>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != null && all[i].gameObject.scene.IsValid() && all[i].gameObject.name == "RootCanvas")
                    {
                        canvasObject = all[i].gameObject;
                        break;
                    }
                }
            }
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
            BindImage(image, "bg_title_sky_realm");
            image.raycastTarget = false;
            bg.SetAsFirstSibling();
        }

        private void RegisterMainScreens()
        {
            screenManager.RegisterScreen(GameUIScreen.Title, CreateTitleScreen());
            screenManager.RegisterScreen(GameUIScreen.CharacterCreation, CreateCharacterCreation());
            screenManager.RegisterScreen(GameUIScreen.MainTown, CreateMainTown());
            screenManager.RegisterScreen(GameUIScreen.WorldMap, CreateWorldMap());
            screenManager.RegisterScreen(GameUIScreen.RealmAdventureMap, CreateAdventure());
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
                                      child.name == "RealmAdventureMapUI" ||
                                      child.name == "AdventureMapUI" ||
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
            ImageAsset(root, "Logo_Image", "logo_game_main", Anchor.TopCenter, new Vector2(0f, -185f), new Vector2(620f, 300f));
            Text(root, "Title_Text", "ISEKAI 12 REALMS", 68, new Color(1f, 0.86f, 0.38f, 1f), Anchor.TopCenter, new Vector2(0f, -250f), new Vector2(940f, 110f));
            Text(root, "Subtitle_Text", "Offline Match-3 RPG", 38, Color.white, Anchor.TopCenter, new Vector2(0f, -340f), new Vector2(760f, 70f));
            Text(root, "AccountStatus_Text", "Local Save", 28, Color.white, Anchor.TopCenter, new Vector2(0f, -405f), new Vector2(760f, 50f));
            ImageAsset(root, "Hero_Preview", "char_hero_flame_idle", Anchor.Center, new Vector2(0f, 105f), new Vector2(360f, 360f));

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
            Text(root, "CharacterPreview_Text", "Character Preview", 38, textDark, Anchor.Center, new Vector2(0f, 230f), new Vector2(650f, 80f));
            Text(root, "HeroName_Text", "Hero Name: Guest Hero", 34, textDark, Anchor.Center, new Vector2(0f, 70f), new Vector2(650f, 70f));

            Button(root, "Button_Class_Flame", "Flame Squire", danger, Anchor.Center, new Vector2(-260f, -70f), new Vector2(240f, 88f), () => SelectCreationClass("flame_squire"));
            Button(root, "Button_Class_Tide", "Tide Acolyte", primary, Anchor.Center, new Vector2(0f, -70f), new Vector2(240f, 88f), () => SelectCreationClass("tide_acolyte"));
            Button(root, "Button_Class_Storm", "Storm Scout", secondary, Anchor.Center, new Vector2(260f, -70f), new Vector2(240f, 88f), () => SelectCreationClass("storm_scout"));
            ImageAsset(root, "Hero_Class_Preview", ClassIdleSpriteAssetId(selectedCreationClassId), Anchor.Center, new Vector2(0f, 210f), new Vector2(300f, 300f));
            Button(root, "Button_StartJourney", "Start Journey", primary, Anchor.BottomCenter, new Vector2(0f, 190f), new Vector2(520f, 112f), StartNewHero);
            Button(root, "Button_Back", "Back", secondary, Anchor.BottomCenter, new Vector2(0f, 70f), new Vector2(520f, 96f), () => screenManager.ShowScreen(GameUIScreen.Title));
            return root.gameObject;
        }

        private GameObject CreateMainTown()
        {
            RectTransform root = CreateScreenRoot("MainTownUI", new Color(0.04f, 0.12f, 0.16f, 0.35f));
            Panel(root, "TopHud", panelDark, Anchor.TopCenter, new Vector2(0f, -70f), new Vector2(1040f, 140f));
            ImageAsset(root, "AvatarPlaceholder", "char_hero_flame_idle", Anchor.TopLeft, new Vector2(65f, -70f), new Vector2(96f, 96f));
            Text(root, "Name_Text", "Guest Hero", 32, Color.white, Anchor.TopLeft, new Vector2(130f, -48f), new Vector2(240f, 48f));
            Text(root, "Level_Text", "Lv. 1", 30, Color.white, Anchor.TopLeft, new Vector2(130f, -92f), new Vector2(160f, 42f));
            ImageAsset(root, "Gold_Icon", "currency_gold", Anchor.TopCenter, new Vector2(55f, -70f), new Vector2(56f, 56f));
            Text(root, "Gold_Text", "Gold: 0", 30, Color.white, Anchor.TopCenter, new Vector2(185f, -70f), new Vector2(220f, 52f));
            ImageAsset(root, "Gems_Icon", "currency_soul_gem", Anchor.TopCenter, new Vector2(300f, -70f), new Vector2(56f, 56f));
            Text(root, "Gems_Text", "Gems: 0", 30, Color.white, Anchor.TopCenter, new Vector2(430f, -70f), new Vector2(220f, 52f));
            Button(root, "Button_Settings", "Settings", primary, Anchor.TopRight, new Vector2(-110f, -70f), new Vector2(170f, 82f), screenManager.OpenSettings);

            Panel(root, "TownPanel", panelCream, Anchor.Center, new Vector2(0f, 60f), new Vector2(900f, 980f));
            Text(root, "Town_Title", "Main Town", 48, textDark, Anchor.Center, new Vector2(0f, 200f), new Vector2(760f, 90f));
            Panel(root, "QuestTracker", panelDark, Anchor.Center, new Vector2(0f, 390f), new Vector2(820f, 170f));
            Text(root, "QuestTracker_Text", "Quest tracker loading...", 28, Color.white, Anchor.Center, new Vector2(-80f, 390f), new Vector2(600f, 120f));
            Button(root, "Button_QuestTracker", "Go", primary, Anchor.Center, new Vector2(340f, 390f), new Vector2(120f, 78f), OpenTrackedQuest);
            Button(root, "Button_QuestElder", "Quest Elder", secondary, Anchor.Center, new Vector2(-260f, -40f), new Vector2(240f, 104f), () => screenManager.ShowScreen(GameUIScreen.Quest));
            Button(root, "Button_Blacksmith", "Blacksmith", new Color(0.5f, 0.62f, 0.72f, 1f), Anchor.Center, new Vector2(0f, -40f), new Vector2(240f, 104f), () => screenManager.ShowScreen(GameUIScreen.Equipment));
            Button(root, "Button_ShopKeeper", "Shop Keeper", primary, Anchor.Center, new Vector2(260f, -40f), new Vector2(240f, 104f), () => screenManager.ShowScreen(GameUIScreen.Shop));
            return root.gameObject;
        }

        private GameObject CreateWorldMap()
        {
            RectTransform root = CreateScreenRoot("WorldMapUI", new Color(0.14f, 0.1f, 0.05f, 0.35f));
            WorldMapUIController controller = root.GetComponent<WorldMapUIController>();
            if (controller == null)
            {
                controller = root.gameObject.AddComponent<WorldMapUIController>();
            }

            worldMapController = controller;
            return root.gameObject;
        }

        private void BuildWorldMapContent(RectTransform root)
        {
            if (contentService == null || contentService.Realms == null || contentService.Realms.Count == 0) return;
            for (int i = 0; i < contentService.Realms.Count; i++)
            {
                RealmDefinition realm = contentService.Realms[i];
                if (realm == null) continue;
                bool unlocked = realmProgressionService == null || realmProgressionService.IsRealmUnlocked(realm);
                RealmProgressData progress = realmProgressionService != null ? realmProgressionService.GetCurrentRealmProgress(realm.id) : null;
                string progressText = progress != null ? $"{progress.normalMonstersDefeated}/3" : "0/3";
                string lockState = unlocked ? (progress != null && progress.bossDefeated ? "Cleared" : "Open") : "Locked";
                string label = $"{realm.displayName}\n{realm.rank} Lv {realm.requiredPlayerLevel}\nProgress {progressText}  {lockState}";
                Color color = unlocked ? (progress != null && progress.bossDefeated ? secondary : primary) : new Color(0.45f, 0.48f, 0.55f, 1f);
                string capturedRealmId = realm.id;
                Button button = Button(root, "Button_Realm_" + realm.id, label, color, Anchor.TopCenter, new Vector2(-255f, -210f - i * 96f), new Vector2(390f, 82f), () => SelectRealm(capturedRealmId));
                AddButtonIcon(button, !string.IsNullOrEmpty(realm.mapNodeAssetId) ? realm.mapNodeAssetId : !string.IsNullOrEmpty(realm.backgroundAssetId) ? realm.backgroundAssetId : MapNodeAssetId(realm.id));
            }

            if (contentService.Realms.Count > 0 && string.IsNullOrEmpty(selectedRealmId))
            {
                SelectRealm(contentService.Realms[0].id);
            }
        }

        private void SelectRealm(string realmId)
        {
            selectedRealmId = realmId;
            RefreshWorldMapState();
        }

        private void EnterSelectedRealm()
        {
            RealmDefinition realm = GetSelectedRealm();
            if (realm == null)
            {
                return;
            }

            if (realmProgressionService != null && !realmProgressionService.CanEnterRealm(realm))
            {
                screenManager.ToastService?.ShowToast(realmProgressionService.GetRealmLockReason(realm));
                return;
            }

            if (adventureMapService != null && adventureMapService.EnterRealm(realm.id))
            {
                screenManager.ShowScreen(GameUIScreen.RealmAdventureMap);
            }
        }

        private void RefreshWorldMapState()
        {
            worldMapController?.RefreshView();
        }

        private void RefreshAdventureMapState()
        {
            adventureMapService?.RefreshMap();
        }

        private RealmDefinition GetSelectedRealm()
        {
            if (contentService == null || contentService.Realms == null || contentService.Realms.Count == 0)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(selectedRealmId))
            {
                RealmDefinition selected = contentService.Realms.Find(r => r != null && r.id == selectedRealmId);
                if (selected != null)
                {
                    return selected;
                }
            }

            return contentService.Realms[0];
        }

        private static string BuildRealmMonsterPreview(RealmDefinition realm)
        {
            if (realm == null || realm.normalEnemies == null || realm.normalEnemies.Count == 0)
            {
                return "None";
            }

            List<string> names = new List<string>();
            for (int i = 0; i < Mathf.Min(3, realm.normalEnemies.Count); i++)
            {
                EnemyDefinition enemy = realm.normalEnemies[i];
                if (enemy != null)
                {
                    names.Add(enemy.displayName);
                }
            }

            return names.Count > 0 ? string.Join(", ", names) : "None";
        }

        private GameObject CreateAdventure()
        {
            RectTransform root = CreateScreenRoot("RealmAdventureMapUI", new Color(0.05f, 0.16f, 0.14f, 0.35f));
            RealmAdventureMapUIController controller = root.GetComponent<RealmAdventureMapUIController>();
            if (controller == null)
            {
                controller = root.gameObject.AddComponent<RealmAdventureMapUIController>();
            }

            controller.Initialize(screenManager, adventureMapService, contentService, realmProgressionService, progressionService);
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

            Header(root, "Battle", controller.BackToWorldMap);

            Panel(root, "EnemyArea", panelDark, Anchor.TopCenter, new Vector2(0f, -190f), new Vector2(900f, 170f));
            ImageAsset(root, "EnemySprite", "enemy_meadow_slime", Anchor.TopCenter, new Vector2(-405f, -190f), new Vector2(150f, 150f));
            Text(root, "EnemyName", "Meadow Slime", 34, Color.white, Anchor.TopCenter, new Vector2(-210f, -165f), new Vector2(360f, 60f));
            Bar(root, "EnemyHp", Anchor.TopCenter, new Vector2(190f, -165f), new Vector2(360f, 42f), danger);
            Text(root, "TurnText", "Your Turn", 30, Color.white, Anchor.TopCenter, new Vector2(-300f, -285f), new Vector2(230f, 54f));
            Text(root, "FoodText", "Food: 20", 30, Color.white, Anchor.TopCenter, new Vector2(0f, -285f), new Vector2(230f, 54f));
            Text(root, "ComboText", "Combo: 0", 30, Color.white, Anchor.TopCenter, new Vector2(300f, -285f), new Vector2(230f, 54f));
            BoardController boardController = CreateBoardGrid(root);
            Panel(root, "PlayerArea", panelDark, Anchor.BottomCenter, new Vector2(0f, 340f), new Vector2(900f, 150f));
            ImageAsset(root, "PlayerSprite", "char_hero_flame_idle", Anchor.BottomCenter, new Vector2(-420f, 340f), new Vector2(130f, 130f));
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
            Button(root, "Button_BackWorldMap", "Back", primary, Anchor.BottomCenter, new Vector2(-70f, 90f), new Vector2(230f, 90f), controller.BackToWorldMap);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Button(root, "Button_WinTest", "Win Test", new Color(0.5f, 0.85f, 0.34f, 1f), Anchor.BottomCenter, new Vector2(170f, 90f), new Vector2(190f, 90f), controller.WinTest);
            Button(root, "Button_LoseTest", "Lose Test", danger, Anchor.BottomCenter, new Vector2(385f, 90f), new Vector2(190f, 90f), controller.LoseTest);
            RectTransform debug = EnsureChildRect(root, "BattleDebugPanel");
            SetRect(debug, Anchor.TopRight, new Vector2(-160f, -360f), new Vector2(300f, 220f));
            EnsureImage(debug.gameObject, new Color(0f, 0f, 0f, 0.68f));
            Text(debug, "DebugText", "Battle Debug\nHidden by default", 22, Color.white, Anchor.Center, Vector2.zero, new Vector2(280f, 200f));
            debug.gameObject.SetActive(false);
#endif
            controller.Initialize(screenManager, boardController);
            return root.gameObject;
        }

        private GameObject CreateHero()
        {
            RectTransform root = CreateScreenRoot("HeroUI", new Color(0.07f, 0.11f, 0.2f, 0.35f));
            Header(root, "Hero", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            Panel(root, "HeroPreview", panelCream, Anchor.Center, new Vector2(-230f, 160f), new Vector2(360f, 460f));
            ImageAsset(root, "HeroPortrait", "portrait_hero_flame", Anchor.Center, new Vector2(-230f, 175f), new Vector2(310f, 310f));
            Text(root, "HeroPreview_Text", "Character\nPreview", 28, textDark, Anchor.Center, new Vector2(-230f, -70f), new Vector2(300f, 80f));
            Text(root, "Stats_Text", "HP 100\nATK 10\nMAG 8\nDEF 5\nSPD 5\nLUCK 1", 36, Color.white, Anchor.Center, new Vector2(240f, 160f), new Vector2(360f, 340f));
            Text(root, "Exp_Text", "EXP 0 / 50", 30, Color.white, Anchor.Center, new Vector2(240f, -65f), new Vector2(420f, 70f));
            Text(root, "Class_Text", "Class: Flame Squire", 30, Color.white, Anchor.Center, new Vector2(240f, -130f), new Vector2(420f, 60f));
            Text(root, "EquippedSkills_Text", "Skills: Spark Slash / Shuffle Bell / Realm Burst", 26, Color.white, Anchor.Center, new Vector2(240f, -200f), new Vector2(520f, 90f));
            Button(root, "Button_Skills", "Skills", primary, Anchor.BottomCenter, new Vector2(-170f, 240f), new Vector2(300f, 104f), () => screenManager.ShowScreen(GameUIScreen.Skills));
            Button(root, "Button_Equipment", "Equipment", secondary, Anchor.BottomCenter, new Vector2(170f, 240f), new Vector2(300f, 104f), () => screenManager.ShowScreen(GameUIScreen.Equipment));
            return root.gameObject;
        }

        private GameObject CreateSkills()
        {
            RectTransform root = CreateScreenRoot("SkillsUI", new Color(0.06f, 0.08f, 0.2f, 0.35f));
            Header(root, "Skills", () => screenManager.ShowScreen(GameUIScreen.Hero));
            Button(root, "Tab_Flame", "Flame", danger, Anchor.TopCenter, new Vector2(-260f, -210f), new Vector2(220f, 86f), () => SelectSkillClass("flame_squire"));
            Button(root, "Tab_Tide", "Tide", primary, Anchor.TopCenter, new Vector2(0f, -210f), new Vector2(220f, 86f), () => SelectSkillClass("tide_acolyte"));
            Button(root, "Tab_Storm", "Storm", secondary, Anchor.TopCenter, new Vector2(260f, -210f), new Vector2(220f, 86f), () => SelectSkillClass("storm_scout"));
            Panel(root, "SkillList", panelCream, Anchor.Center, new Vector2(0f, 80f), new Vector2(860f, 860f));
            Text(root, "SkillList_Text", "Skill data loading...", 28, textDark, Anchor.Center, new Vector2(0f, 130f), new Vector2(760f, 620f));
            Button(root, "Button_UpgradeSelected", "Upgrade Selected", secondary, Anchor.BottomCenter, new Vector2(-180f, 210f), new Vector2(340f, 100f), UpgradeSelectedSkill);
            Button(root, "Button_EquipSelected", "Equip Selected", primary, Anchor.BottomCenter, new Vector2(210f, 210f), new Vector2(340f, 100f), EquipSelectedSkill);
            return root.gameObject;
        }

        private GameObject CreateEquipment()
        {
            RectTransform root = CreateScreenRoot("EquipmentUI", new Color(0.08f, 0.1f, 0.17f, 0.35f));
            Header(root, "Equipment", () => screenManager.ShowScreen(GameUIScreen.Hero));
            string[] slots = { "Weapon", "Armor", "Head", "Boots", "Ring", "Charm" };
            for (int i = 0; i < slots.Length; i++)
            {
                EquipmentSlot slot = (EquipmentSlot)Enum.Parse(typeof(EquipmentSlot), slots[i]);
                Button(root, "Slot_" + slots[i], slots[i], secondary, Anchor.TopCenter, new Vector2(i % 2 == 0 ? -230f : 230f, -240f - (i / 2) * 120f), new Vector2(360f, 92f), () => SelectEquippedSlot(slot));
            }
            Panel(root, "EquipmentDetail", panelCream, Anchor.BottomCenter, new Vector2(0f, 270f), new Vector2(860f, 400f));
            Text(root, "EquipmentDetail_Text", "Equipment details", 38, textDark, Anchor.BottomCenter, new Vector2(0f, 270f), new Vector2(720f, 140f));
            Button(root, "Button_EquipChange", "Change", primary, Anchor.BottomCenter, new Vector2(-300f, 90f), new Vector2(180f, 72f), () => screenManager.ShowScreen(GameUIScreen.Inventory));
            Button(root, "Button_Unequip", "Unequip", secondary, Anchor.BottomCenter, new Vector2(-100f, 90f), new Vector2(180f, 72f), UnequipSelectedEquipment);
            Button(root, "Button_UpgradeEquipment", "Upgrade", secondary, Anchor.BottomCenter, new Vector2(100f, 90f), new Vector2(180f, 72f), UpgradeSelectedEquipment);
            Button(root, "Button_LockEquipment", "Lock", primary, Anchor.BottomCenter, new Vector2(300f, 90f), new Vector2(180f, 72f), ToggleSelectedEquipmentLock);
            return root.gameObject;
        }

        private GameObject CreateInventory()
        {
            RectTransform root = CreateScreenRoot("InventoryUI", new Color(0.06f, 0.11f, 0.17f, 0.35f));
            Header(root, "Bag", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            string[] tabs = { "All", "Equipment", "Material", "Consumable", "Quest" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Button(root, "Tab_" + tabs[i], tabs[i], primary, Anchor.TopCenter, new Vector2(-400f + i * 200f, -205f), new Vector2(180f, 82f), RefreshSaveBackedUi);
            }
            CreateInventoryGrid(root);
            Panel(root, "ItemDetail", panelCream, Anchor.BottomCenter, new Vector2(0f, 200f), new Vector2(860f, 230f));
            Text(root, "ItemDetail_Text", "Item details", 34, textDark, Anchor.BottomCenter, new Vector2(0f, 200f), new Vector2(720f, 90f));
            Text(root, "InventoryList_Text", "Inventory empty", 28, textDark, Anchor.Center, new Vector2(0f, 120f), new Vector2(720f, 500f));
            Button(root, "Button_EquipFirst", "Equip First Equipment", secondary, Anchor.BottomCenter, new Vector2(0f, 335f), new Vector2(460f, 86f), EquipFirstEquipment);
            Button(root, "Button_EquipSelected", "Equip Selected", primary, Anchor.BottomCenter, new Vector2(-310f, 335f), new Vector2(250f, 76f), EquipSelectedEquipment);
            Button(root, "Button_UpgradeSelectedEquipment", "Upgrade", secondary, Anchor.BottomCenter, new Vector2(-55f, 335f), new Vector2(210f, 76f), UpgradeSelectedEquipment);
            Button(root, "Button_SellSelectedEquipment", "Sell", danger, Anchor.BottomCenter, new Vector2(175f, 335f), new Vector2(170f, 76f), SellSelectedEquipment);
            Button(root, "Button_LockSelectedEquipment", "Lock", primary, Anchor.BottomCenter, new Vector2(365f, 335f), new Vector2(170f, 76f), ToggleSelectedEquipmentLock);
            return root.gameObject;
        }

        private GameObject CreateQuest()
        {
            RectTransform root = CreateScreenRoot("QuestUI", new Color(0.07f, 0.09f, 0.17f, 0.35f));
            Header(root, "Quest", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            string[] tabs = { "Main", "Tutorial", "Daily", "Achievement", "Completed" };
            for (int i = 0; i < tabs.Length; i++)
            {
                Button(root, "Tab_" + tabs[i], tabs[i], primary, Anchor.TopCenter, new Vector2(-400f + i * 200f, -205f), new Vector2(180f, 82f), RefreshSaveBackedUi);
            }
            Panel(root, "QuestPanel", panelCream, Anchor.Center, new Vector2(0f, 110f), new Vector2(860f, 860f));
            Text(root, "QuestList_Text", "Quest data loading...", 28, textDark, Anchor.Center, new Vector2(0f, 160f), new Vector2(740f, 610f));
            Button(root, "Button_ClaimQuest", "Claim Completed", secondary, Anchor.BottomCenter, new Vector2(-190f, 230f), new Vector2(340f, 100f), ClaimFirstCompletedQuest);
            Button(root, "Button_GoQuest", "Go To Quest", primary, Anchor.BottomCenter, new Vector2(190f, 230f), new Vector2(300f, 100f), OpenTrackedQuest);
            return root.gameObject;
        }

        private GameObject CreateShop()
        {
            RectTransform root = CreateScreenRoot("ShopUI", new Color(0.08f, 0.07f, 0.15f, 0.35f));
            Header(root, "Shop", () => screenManager.ShowScreen(GameUIScreen.MainTown));
            string[] tabs = { "Daily", "Gold Shop", "Gem Shop", "Cosmetic", "IAP" };
            ShopType[] types = { ShopType.Daily, ShopType.GoldShop, ShopType.GemShop, ShopType.Cosmetic, ShopType.IAPPlaceholder };
            for (int i = 0; i < tabs.Length; i++)
            {
                ShopType capturedType = types[i];
                Button(root, "Tab_" + tabs[i].Replace(" ", ""), tabs[i], primary, Anchor.TopCenter, new Vector2(-400f + i * 200f, -205f), new Vector2(180f, 82f), () => SelectShopTab(capturedType));
            }
            Panel(root, "ShopPanel", panelCream, Anchor.Center, new Vector2(0f, 70f), new Vector2(860f, 860f));
            Text(root, "ShopCurrency_Text", "Gold: 0   Soul Gems: 0", 30, textDark, Anchor.TopCenter, new Vector2(0f, -300f), new Vector2(760f, 54f));
            Text(root, "ShopInfo_Text", "Shop data loading...", 28, textDark, Anchor.BottomCenter, new Vector2(0f, 215f), new Vector2(760f, 110f));
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
            EnsureModalBlocker(popupLayer);
            GameObject settings = CreateSettingsPopup();
            screenManager.RegisterSettingsPopup(settings);
            CreateDeleteConfirmPopup();
            CreateShopPurchaseConfirmPopup();
            CreateIapPurchaseSuccessPopup();
            CreateCloudConflictPopup();
            CreateContentDownloadPopup();

            TextMeshProUGUI resultTitle;
            TextMeshProUGUI resultRewards;
            GameObject victoryButtons;
            GameObject defeatButtons;
            GameObject battleResult = CreateBattleResultPopup(out resultTitle, out resultRewards, out victoryButtons, out defeatButtons);
            screenManager.RegisterBattleResultPopup(battleResult, resultTitle, resultRewards, victoryButtons, defeatButtons);
            popupService?.CloseAll();
        }

        private GameObject CreateSettingsPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "SettingsPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            root.SetAsLastSibling();
            DeactivateLegacyPopupChildren(root, "ModalPanel");

            RectTransform modalPanel = EnsureChildRect(root, "ModalPanel");
            SetRect(modalPanel, Anchor.Center, Vector2.zero, new Vector2(900f, 1400f));
            Image panelImage = EnsureImage(modalPanel.gameObject, panelCream);
            BindImage(panelImage, "ui_panel_popup");

            RectTransform header = EnsureChildRect(modalPanel, "Header");
            SetRect(header, Anchor.TopCenter, new Vector2(0f, -70f), new Vector2(820f, 120f));
            EnsureImage(header.gameObject, new Color(1f, 1f, 1f, 0.02f));
            Text(header, "Title_Text", "Settings", 54, textDark, Anchor.TopCenter, new Vector2(0f, -12f), new Vector2(520f, 72f));
            Button(header, "Button_Close", "Close", primary, Anchor.TopRight, new Vector2(-80f, -50f), new Vector2(170f, 68f), screenManager.CloseSettings);

            RectTransform scrollView = EnsureChildRect(modalPanel, "ScrollView");
            SetRect(scrollView, Anchor.Center, new Vector2(0f, -10f), new Vector2(820f, 1120f));
            EnsureImage(scrollView.gameObject, new Color(1f, 1f, 1f, 0.03f));
            ScrollRect scrollRect = scrollView.GetComponent<ScrollRect>();
            if (scrollRect == null) scrollRect = scrollView.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            RectTransform viewport = EnsureChildRect(scrollView, "Viewport");
            Stretch(viewport);
            Image viewportImage = EnsureImage(viewport.gameObject, new Color(1f, 1f, 1f, 0.01f));
            viewportImage.raycastTarget = false;
            Mask viewportMask = viewport.gameObject.GetComponent<Mask>();
            if (viewportMask == null) viewportMask = viewport.gameObject.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            RectTransform content = EnsureChildRect(viewport, "Content");
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            VerticalLayoutGroup contentLayout = content.gameObject.GetComponent<VerticalLayoutGroup>();
            if (contentLayout == null) contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.spacing = 18f;
            contentLayout.padding = new RectOffset(24, 24, 24, 24);
            contentLayout.childAlignment = TextAnchor.UpperCenter;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;
            ContentSizeFitter contentFitter = content.gameObject.GetComponent<ContentSizeFitter>();
            if (contentFitter == null) contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.verticalNormalizedPosition = 1f;

            RectTransform audioSection = CreateSettingsSection(content, "Section_Audio", "Audio");
            Toggle(audioSection, "Toggle_Music", "Music", Anchor.Center, Vector2.zero, new Vector2(710f, 62f), audioService == null || audioService.MusicEnabled, value => audioService?.MuteMusic(!value));
            Toggle(audioSection, "Toggle_Sfx", "SFX", Anchor.Center, Vector2.zero, new Vector2(710f, 62f), audioService == null || audioService.SfxEnabled, value => audioService?.MuteSfx(!value));
            Slider(audioSection, "Slider_MusicVolume", "Music Volume", Anchor.Center, Vector2.zero, new Vector2(710f, 54f), audioService != null ? audioService.MusicVolume : 0.7f, value => audioService?.SetMusicVolume(value));
            Slider(audioSection, "Slider_SfxVolume", "SFX Volume", Anchor.Center, Vector2.zero, new Vector2(710f, 54f), audioService != null ? audioService.SfxVolume : 0.85f, value => audioService?.SetSfxVolume(value));

            RectTransform accountSection = CreateSettingsSection(content, "Section_Account", "Account");
            Text(accountSection, "AccountStatus", "Status: Local Save", 26, textDark, Anchor.Center, Vector2.zero, new Vector2(720f, 76f));
            Text(accountSection, "AccountText", "Account: Guest", 24, textDark, Anchor.Center, Vector2.zero, new Vector2(720f, 60f));
            Button(accountSection, "Button_SignInGuest", "Sign in as Guest", primary, Anchor.Center, Vector2.zero, new Vector2(710f, 60f), () => _ = SignInGuestCloud());
            Button(accountSection, "Button_SignInGoogle", "Sign in with Google", primary, Anchor.Center, Vector2.zero, new Vector2(710f, 60f), () => _ = SignInGoogleCloud());
            if (!CanShowGoogleSignIn()) accountSection.Find("Button_SignInGoogle")?.gameObject.SetActive(false);

            RectTransform cloudSection = CreateSettingsSection(content, "Section_Cloud", "Cloud");
            Toggle(cloudSection, "Toggle_CloudSync", "Cloud Sync Enabled", Anchor.Center, Vector2.zero, new Vector2(710f, 62f), true, SetCloudSyncEnabled);
            Button(cloudSection, "Button_SyncNow", "Sync Now", secondary, Anchor.Center, Vector2.zero, new Vector2(710f, 60f), () => _ = SyncCloudNow());
            Button(cloudSection, "Button_UploadLocal", "Upload Local", secondary, Anchor.Center, Vector2.zero, new Vector2(710f, 60f), () => _ = UploadLocalCloud());
            Button(cloudSection, "Button_DownloadCloud", "Download Cloud", secondary, Anchor.Center, Vector2.zero, new Vector2(710f, 60f), () => _ = DownloadCloudSave());
            Button(modalPanel, "Button_CloseFooter", "Close", primary, Anchor.BottomCenter, new Vector2(0f, 58f), new Vector2(260f, 68f), screenManager.CloseSettings);
            header.SetAsLastSibling();
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private RectTransform CreateSettingsSection(Transform parent, string name, string title)
        {
            RectTransform section = EnsureChildRect(parent, name);
            EnsureImage(section.gameObject, new Color(1f, 1f, 1f, 0.08f));
            VerticalLayoutGroup layout = section.gameObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null) layout = section.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.padding = new RectOffset(22, 22, 18, 18);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = section.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = section.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            LayoutElement element = section.gameObject.GetComponent<LayoutElement>();
            if (element == null) element = section.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 0f;
            element.preferredWidth = 760f;
            TextMeshProUGUI header = CreateSectionTitle(section, title);
            if (header != null)
            {
                header.margin = new Vector4(0f, 0f, 0f, 0f);
            }
            return section;
        }

        private TextMeshProUGUI CreateSectionTitle(Transform parent, string title)
        {
            return Text(parent, "SectionTitle", title, 32, new Color(0.12f, 0.18f, 0.28f, 1f), Anchor.Center, Vector2.zero, new Vector2(720f, 46f));
        }

        private void ResolvePopupService()
        {
            if (popupService == null)
            {
                ServiceLocator.TryResolve<IPopupService>(out popupService);
            }

            if (popupService != null && popupLayer != null)
            {
                popupService.SetPopupLayer(popupLayer);
            }
        }

        private static void EnsureLayerOrder(RectTransform safeAreaRoot)
        {
            string[] order = { "BackgroundLayer", "MainLayer", "HudLayer", "NavigationLayer", "PopupLayer", "ToastLayer", "LoadingLayer" };
            for (int i = 0; i < order.Length; i++)
            {
                Transform child = safeAreaRoot.Find(order[i]);
                if (child != null)
                {
                    child.SetSiblingIndex(i);
                }
            }
        }

        private static void EnsureModalBlocker(Transform popupRoot)
        {
            if (popupRoot == null)
            {
                return;
            }

            RectTransform blocker = EnsureChildRect(popupRoot, "ModalBlocker");
            Stretch(blocker);
            Image image = EnsureImage(blocker.gameObject, new Color(0f, 0f, 0f, 0.55f));
            image.raycastTarget = true;
            blocker.SetAsFirstSibling();
            blocker.gameObject.SetActive(false);
        }

        private static void DeactivateLegacyPopupChildren(Transform popupRoot, params string[] allowedChildren)
        {
            if (popupRoot == null)
            {
                return;
            }

            HashSet<string> allowed = new HashSet<string>(allowedChildren ?? new string[0]);
            for (int i = 0; i < popupRoot.childCount; i++)
            {
                Transform child = popupRoot.GetChild(i);
                if (child != null && !allowed.Contains(child.name))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static void EnsurePopupCanvasGroup(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = root.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        private GameObject ShowPopup(string popupId, object payload = null)
        {
            ResolvePopupService();
            if (popupService != null)
            {
                return popupService.ShowPopup(popupId, payload);
            }

            Transform popup = popupLayer != null ? popupLayer.Find(popupId) : null;
            if (popup == null)
            {
                return null;
            }

            popup.gameObject.SetActive(true);
            return popup.gameObject;
        }

        private void HidePopup(string popupId)
        {
            ResolvePopupService();
            if (popupService != null)
            {
                popupService.HidePopup(popupId);
                return;
            }

            Transform popup = popupLayer != null ? popupLayer.Find(popupId) : null;
            if (popup != null)
            {
                popup.gameObject.SetActive(false);
            }
        }

        private GameObject CreateDeleteConfirmPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "DeleteSaveConfirmPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.62f)).raycastTarget = true;
            Panel(root, "Panel", panelCream, Anchor.Center, Vector2.zero, new Vector2(760f, 520f));
            Text(root, "Title", "Are you sure?", 50, textDark, Anchor.Center, new Vector2(0f, 130f), new Vector2(620f, 80f));
            Text(root, "Body", "This deletes the local save on this device.", 32, textDark, Anchor.Center, new Vector2(0f, 40f), new Vector2(620f, 80f));
            Button(root, "Button_Confirm", "Confirm", danger, Anchor.Center, new Vector2(-150f, -145f), new Vector2(260f, 92f), ConfirmDeleteSave);
            Button(root, "Button_Cancel", "Cancel", primary, Anchor.Center, new Vector2(150f, -145f), new Vector2(260f, 92f), CloseDeleteConfirm);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateShopPurchaseConfirmPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "ShopPurchaseConfirmPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.62f)).raycastTarget = true;
            Panel(root, "Panel", panelCream, Anchor.Center, Vector2.zero, new Vector2(800f, 620f));
            Text(root, "Title", "Confirm Purchase", 48, textDark, Anchor.Center, new Vector2(0f, 210f), new Vector2(660f, 72f));
            Text(root, "Body", "Purchase details", 32, textDark, Anchor.Center, new Vector2(0f, 50f), new Vector2(660f, 250f));
            Text(root, "Warning", "", 30, danger, Anchor.Center, new Vector2(0f, -115f), new Vector2(660f, 62f));
            Button(root, "Button_Confirm", "Confirm", secondary, Anchor.Center, new Vector2(-160f, -220f), new Vector2(260f, 92f), ConfirmShopPurchase);
            Button(root, "Button_Cancel", "Cancel", primary, Anchor.Center, new Vector2(160f, -220f), new Vector2(260f, 92f), CloseShopPurchaseConfirm);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateIapPurchaseSuccessPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "IAPPurchaseSuccessPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.62f)).raycastTarget = true;
            Panel(root, "Panel", panelCream, Anchor.Center, Vector2.zero, new Vector2(800f, 620f));
            Text(root, "Title", "Purchase Complete", 52, textDark, Anchor.Center, new Vector2(0f, 210f), new Vector2(680f, 82f));
            Text(root, "Body", "Soul Gems granted", 34, textDark, Anchor.Center, new Vector2(0f, 45f), new Vector2(680f, 260f));
            Button(root, "Button_OK", "OK", primary, Anchor.Center, new Vector2(0f, -220f), new Vector2(300f, 92f), CloseIapPurchaseSuccessPopup);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateCloudConflictPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "CloudConflictPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.65f)).raycastTarget = true;
            Panel(root, "Panel", panelCream, Anchor.Center, Vector2.zero, new Vector2(920f, 820f));
            Text(root, "Title", "Cloud Save Found", 50, textDark, Anchor.Center, new Vector2(0f, 300f), new Vector2(760f, 80f));
            Panel(root, "LocalCard", panelDark, Anchor.Center, new Vector2(-225f, 80f), new Vector2(390f, 390f));
            Panel(root, "CloudCard", panelDark, Anchor.Center, new Vector2(225f, 80f), new Vector2(390f, 390f));
            Text(root, "Local_Text", "Local Save", 28, Color.white, Anchor.Center, new Vector2(-225f, 80f), new Vector2(340f, 320f));
            Text(root, "Cloud_Text", "Cloud Save", 28, Color.white, Anchor.Center, new Vector2(225f, 80f), new Vector2(340f, 320f));
            Button(root, "Button_UseLocal", "Use Local", secondary, Anchor.Center, new Vector2(-280f, -245f), new Vector2(240f, 88f), () => _ = ResolveCloudUseLocal());
            Button(root, "Button_UseCloud", "Use Cloud", primary, Anchor.Center, new Vector2(0f, -245f), new Vector2(240f, 88f), () => _ = ResolveCloudUseCloud());
            Button(root, "Button_Cancel", "Cancel", danger, Anchor.Center, new Vector2(280f, -245f), new Vector2(220f, 88f), CancelCloudConflict);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateContentDownloadPopup()
        {
            RectTransform root = EnsureChildRect(popupLayer, "ContentDownloadPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.62f)).raycastTarget = true;
            Panel(root, "Panel", panelCream, Anchor.Center, Vector2.zero, new Vector2(860f, 760f));
            Text(root, "Title", "Content Pack", 50, textDark, Anchor.Center, new Vector2(0f, 260f), new Vector2(700f, 80f));
            Text(root, "Description", "Pack details", 30, textDark, Anchor.Center, new Vector2(0f, 130f), new Vector2(700f, 170f));
            Text(root, "Status", "Status", 28, textDark, Anchor.Center, new Vector2(0f, 20f), new Vector2(700f, 58f));
            Slider(root, "Progress", "Progress", Anchor.Center, new Vector2(0f, -55f), new Vector2(620f, 54f), 0f, null);
            Button(root, "Button_Download", "Download", primary, Anchor.Center, new Vector2(-250f, -160f), new Vector2(250f, 82f), DownloadSelectedPack);
            Button(root, "Button_Cancel", "Cancel", secondary, Anchor.Center, new Vector2(0f, -160f), new Vector2(210f, 82f), CloseContentDownloadPopup);
            Button(root, "Button_ClearCache", "Clear Cache", danger, Anchor.Center, new Vector2(250f, -160f), new Vector2(250f, 82f), ClearOptionalCache);
            Button(root, "Button_Close", "Close", primary, Anchor.Center, new Vector2(0f, -275f), new Vector2(300f, 82f), CloseContentDownloadPopup);
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private GameObject CreateBattleResultPopup(out TextMeshProUGUI title, out TextMeshProUGUI rewards, out GameObject victoryButtons, out GameObject defeatButtons)
        {
            RectTransform root = EnsureChildRect(popupLayer, "BattleResultPopup");
            Stretch(root);
            EnsurePopupCanvasGroup(root.gameObject);
            EnsureImage(root.gameObject, new Color(0f, 0f, 0f, 0.55f)).raycastTarget = true;
            Panel(root, "ResultPanel", panelCream, Anchor.Center, Vector2.zero, new Vector2(820f, 820f));
            title = Text(root, "ResultTitle", "Victory!", 58, textDark, Anchor.Center, new Vector2(0f, 275f), new Vector2(680f, 90f));
            ImageAsset(root, "RewardGoldIcon", "currency_gold", Anchor.Center, new Vector2(-240f, 165f), new Vector2(64f, 64f));
            ImageAsset(root, "RewardItemIcon", "missing_sprite", Anchor.Center, new Vector2(240f, 165f), new Vector2(64f, 64f));
            rewards = Text(root, "ResultRewards", "EXP +50\nGold +30", 38, textDark, Anchor.Center, new Vector2(0f, 110f), new Vector2(680f, 180f));

            victoryButtons = EnsureChildRect(root, "VictoryButtons").gameObject;
            Stretch(victoryButtons.GetComponent<RectTransform>());
            Button(victoryButtons.transform, "Button_Continue", "Continue", primary, Anchor.Center, new Vector2(0f, -60f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.RealmAdventureMap));
            Button(victoryButtons.transform, "Button_Replay", "Replay", secondary, Anchor.Center, new Vector2(0f, -165f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.Battle));
            Button(victoryButtons.transform, "Button_Town", "Town", danger, Anchor.Center, new Vector2(0f, -270f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.MainTown));

            defeatButtons = EnsureChildRect(root, "DefeatButtons").gameObject;
            Stretch(defeatButtons.GetComponent<RectTransform>());
            Button(defeatButtons.transform, "Button_Retry", "Retry", primary, Anchor.Center, new Vector2(0f, -60f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.Battle));
            Button(defeatButtons.transform, "Button_Upgrade", "Upgrade", secondary, Anchor.Center, new Vector2(0f, -165f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.Hero));
            Button(defeatButtons.transform, "Button_Town", "Realm", danger, Anchor.Center, new Vector2(0f, -270f), new Vector2(420f, 92f), () => screenManager.ShowScreen(GameUIScreen.RealmAdventureMap));

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
            buildConfigService = BuildConfigService.GetOrCreate(buildConfig);
            buildConfigService.ApplyStartupSettings();
            PerformanceService performance = new PerformanceService(buildConfigService);
            performance.Apply();
            ServiceLocator.Register(performance);
            RegisterGameConfigService();

            audioService = GetComponent<AudioService>();
            if (audioService == null)
            {
                audioService = gameObject.AddComponent<AudioService>();
            }

            RectTransform battleFloatingTextLayer = EnsureChildRect(toastLayer, "BattleFloatingTextLayer");
            Stretch(battleFloatingTextLayer);
            FloatingTextService floatingText = GetComponent<FloatingTextService>();
            if (floatingText == null)
            {
                floatingText = gameObject.AddComponent<FloatingTextService>();
            }
            floatingText.Initialize(battleFloatingTextLayer, 0.75f);

            RectTransform vfxLayer = EnsureChildRect(toastLayer, "BattleVFXLayer");
            Stretch(vfxLayer);
            VFXService vfx = GetComponent<VFXService>();
            if (vfx == null)
            {
                vfx = gameObject.AddComponent<VFXService>();
            }
            vfx.Initialize(vfxLayer);
            battleFloatingTextLayer.SetAsLastSibling();
        }

        private void RegisterReleaseHardeningServices()
        {
            diagnosticsService = GetComponent<DiagnosticsService>();
            if (diagnosticsService == null) diagnosticsService = gameObject.AddComponent<DiagnosticsService>();
            diagnosticsService.Initialize(buildConfigService, contentService, cloudSaveCoordinator);
            ServiceLocator.Register(diagnosticsService);

            ErrorPopup errorPopup = GetComponent<ErrorPopup>();
            if (errorPopup == null) errorPopup = gameObject.AddComponent<ErrorPopup>();
            errorPopup.Initialize(popupLayer);
            if (assetManifest == null) errorPopup.Show("Some local art may be incomplete.", "ASSET_MANIFEST_MISSING");
            if (contentService == null || contentService.Database == null) errorPopup.Show("Content data is unavailable. Some screens may be limited.", "CONTENT_DATABASE_MISSING");
            if (buildConfigService == null || buildConfigService.Config == null) errorPopup.Show("Build configuration is missing. Safe defaults are active.", "BUILD_CONFIG_MISSING");

            AndroidBackButtonService backButton = GetComponent<AndroidBackButtonService>();
            if (backButton == null) backButton = gameObject.AddComponent<AndroidBackButtonService>();
            backButton.Initialize(screenManager, popupLayer);

            AppLifecycleService lifecycle = GetComponent<AppLifecycleService>();
            if (lifecycle == null) lifecycle = gameObject.AddComponent<AppLifecycleService>();
            lifecycle.Initialize(cloudSaveCoordinator, shopService, screenManager);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            QADebugPanelUI qaPanel = GetComponent<QADebugPanelUI>();
            if (qaPanel == null) qaPanel = gameObject.AddComponent<QADebugPanelUI>();
            qaPanel.Initialize(popupLayer, screenManager, shopService, cloudSaveCoordinator, diagnosticsService, buildConfigService);
#endif

            ProductionModeGuard productionGuard = GetComponent<ProductionModeGuard>();
            if (productionGuard == null) productionGuard = gameObject.AddComponent<ProductionModeGuard>();
            productionGuard.Apply(buildConfigService);
        }

        private void EnsureVisibleScreenOrRecover()
        {
            if (HasVisibleMainScreen())
            {
                return;
            }

            Debug.Log("[UI] No visible screen detected. Restoring TitleScreenUI.");
            RepairSceneUi();
            popupService?.CloseAll();
            screenManager?.ShowScreen(GameUIScreen.Title);
        }

        private bool HasVisibleMainScreen()
        {
            if (mainLayer == null)
            {
                return false;
            }

            foreach (Transform child in mainLayer)
            {
                if (child != null && child.gameObject.activeSelf)
                {
                    return true;
                }
            }

            return false;
        }

        private void RegisterGameConfigService()
        {
#if UNITY_EDITOR
            if (gameConfigDefaults == null)
            {
                gameConfigDefaults = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfigData>("Assets/_Game/ScriptableObjects/GameConfigData.asset");
            }
#endif
#if USE_FIREBASE_REMOTE_CONFIG
            IRemoteConfigService remoteConfig = new FirebaseRemoteConfigService();
#else
            IRemoteConfigService remoteConfig = new MockRemoteConfigService();
#endif
            gameConfigService = new GameConfigService(gameConfigDefaults, remoteConfig);
            ServiceLocator.Register(gameConfigService);
            _ = gameConfigService.InitializeAsync();
        }

        private void StartExistingGame()
        {
            progressionService?.LoadOrCreate();
            RefreshSaveBackedUi();
            screenManager.ShowScreen(GameUIScreen.MainTown);
        }

        private void StartNewHero()
        {
            progressionService?.CreateNewSave(selectedCreationClassId, "Guest Hero");
            skillService?.SetDefaultClassSkills(selectedCreationClassId);
            RefreshSaveBackedUi();
            screenManager.ShowScreen(GameUIScreen.MainTown);
        }

        private void SelectCreationClass(string classId)
        {
            selectedCreationClassId = classId;
            SetText("CharacterCreationUI/HeroName_Text", "Hero Class: " + DisplayClass(classId));
            SetBoundImageAsset("CharacterCreationUI/Hero_Class_Preview", ClassIdleSpriteAssetId(classId));
        }

        private void SelectSkillClass(string classId)
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            if (save != null && save.selectedClassId != classId)
            {
                screenManager.ToastService?.ShowToast("Class switching will be added later.");
                return;
            }
            selectedSkillClassId = classId;
            RefreshSkillsUi();
        }

        private void SelectSkill(string skillId)
        {
            selectedSkillId = skillId;
            RefreshSkillsUi();
        }

        private void UpgradeSelectedSkill()
        {
            SkillDefinition skill = SelectedSkillOrFirstVisible();
            if (skill != null && skillService != null)
            {
                int goldBefore = progressionService?.CurrentSave?.gold ?? 0;
                if (skillService.UpgradeSkill(skill.id))
                {
                    questService?.TrackProgress(QuestObjectiveType.UpgradeSkill, skill.id, 1);
                    int goldSpent = Mathf.Max(0, goldBefore - (progressionService?.CurrentSave?.gold ?? goldBefore));
                    if (goldSpent > 0) questService?.TrackProgress(QuestObjectiveType.SpendGold, "any", goldSpent);
                    cloudSaveCoordinator?.QueueCloudSync(1f);
                }
                RefreshSaveBackedUi();
            }
        }

        private void EquipSelectedSkill()
        {
            SkillDefinition skill = SelectedSkillOrFirstVisible();
            if (skill != null && skillService != null)
            {
                skillService.EquipSkill(skill.id, skill.slotType);
                RefreshSaveBackedUi();
            }
        }

        private SkillDefinition SelectedSkillOrFirstVisible()
        {
            SkillDefinition selected = !string.IsNullOrEmpty(selectedSkillId) ? contentService?.Database?.GetSkillById(selectedSkillId) : null;
            return selected != null && selected.classId == selectedSkillClassId ? selected : FirstVisibleSkill();
        }

        private SkillDefinition FirstVisibleSkill()
        {
            return contentService?.Database?.GetSkillsByClass(selectedSkillClassId).Find(s => s != null && s.activationType == SkillActivationType.Active);
        }

        private void EquipFirstEquipment()
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            if (save == null || save.inventory.equipments.Count == 0)
            {
                screenManager.ToastService?.ShowToast("Not enough item");
                return;
            }

            EquipmentInstanceData equipment = save.inventory.equipments[0];
            if (progressionService.Equip(equipment.instanceId))
            {
                questService?.TrackProgress(QuestObjectiveType.EquipEquipment, equipment.equipmentId, 1);
                cloudSaveCoordinator?.QueueCloudSync(1f);
            }
            selectedEquipmentInstanceId = equipment.instanceId;
            RefreshSaveBackedUi();
        }

        private void SelectEquippedSlot(EquipmentSlot slot)
        {
            EquipmentInstanceData equipment = equipmentService?.GetEquipped(slot);
            selectedEquipmentInstanceId = equipment != null ? equipment.instanceId : string.Empty;
            RefreshSaveBackedUi();
        }

        private void SelectEquipment(string instanceId)
        {
            selectedEquipmentInstanceId = instanceId;
            RefreshSaveBackedUi();
        }

        private void EquipSelectedEquipment()
        {
            if (string.IsNullOrEmpty(selectedEquipmentInstanceId))
            {
                screenManager.ToastService?.ShowToast("Select equipment first.");
                return;
            }
            if (equipmentService != null && equipmentService.Equip(selectedEquipmentInstanceId))
            {
                EquipmentInstanceData equipment = equipmentService.GetEquipmentByInstanceId(selectedEquipmentInstanceId);
                questService?.TrackProgress(QuestObjectiveType.EquipEquipment, equipment != null ? equipment.equipmentId : "any", 1);
                cloudSaveCoordinator?.QueueCloudSync(1f);
            }
            RefreshSaveBackedUi();
        }

        private void UnequipSelectedEquipment()
        {
            EquipmentInstanceData equipment = equipmentService?.GetEquipmentByInstanceId(selectedEquipmentInstanceId);
            if (equipment == null)
            {
                screenManager.ToastService?.ShowToast("No equipped item selected.");
                return;
            }
            equipmentService?.Unequip(equipment.slot);
            RefreshSaveBackedUi();
        }

        private void UpgradeSelectedEquipment()
        {
            if (string.IsNullOrEmpty(selectedEquipmentInstanceId))
            {
                screenManager.ToastService?.ShowToast("Select equipment first.");
                return;
            }
            int goldBefore = progressionService?.CurrentSave?.gold ?? 0;
            if (equipmentService != null && equipmentService.UpgradeEquipment(selectedEquipmentInstanceId))
            {
                EquipmentInstanceData equipment = equipmentService.GetEquipmentByInstanceId(selectedEquipmentInstanceId);
                questService?.TrackProgress(QuestObjectiveType.UpgradeEquipment, equipment != null ? equipment.equipmentId : "any", 1);
                int goldSpent = Mathf.Max(0, goldBefore - (progressionService?.CurrentSave?.gold ?? goldBefore));
                if (goldSpent > 0) questService?.TrackProgress(QuestObjectiveType.SpendGold, "any", goldSpent);
                cloudSaveCoordinator?.QueueCloudSync(1f);
            }
            RefreshSaveBackedUi();
        }

        private void SellSelectedEquipment()
        {
            if (string.IsNullOrEmpty(selectedEquipmentInstanceId))
            {
                screenManager.ToastService?.ShowToast("Select equipment first.");
                return;
            }
            if (equipmentService != null && equipmentService.SellEquipment(selectedEquipmentInstanceId))
            {
                selectedEquipmentInstanceId = string.Empty;
            }
            RefreshSaveBackedUi();
        }

        private void ToggleSelectedEquipmentLock()
        {
            EquipmentInstanceData equipment = equipmentService?.GetEquipmentByInstanceId(selectedEquipmentInstanceId);
            if (equipment == null)
            {
                screenManager.ToastService?.ShowToast("Select equipment first.");
                return;
            }
            equipmentService.LockEquipment(equipment.instanceId, !equipment.locked);
            RefreshSaveBackedUi();
        }

        private void ClaimFirstCompletedQuest()
        {
            QuestDefinition quest = FirstCompletedQuest();
            if (quest == null)
            {
                screenManager.ToastService?.ShowToast("No quest reward ready.");
                return;
            }

            if (questService.ClaimQuest(quest.id)) cloudSaveCoordinator?.QueueCloudSync(1f);
            RefreshSaveBackedUi();
        }

        private void OpenTrackedQuest()
        {
            QuestDefinition completed = FirstCompletedQuest();
            if (completed != null)
            {
                screenManager.ShowScreen(GameUIScreen.Quest);
                return;
            }

            QuestDefinition quest = questService?.GetTrackerQuest();
            if (quest == null)
            {
                screenManager.ToastService?.ShowToast("No active quest.");
                return;
            }

            screenManager.ShowScreen(GameUIScreen.Quest);
        }

        private QuestDefinition FirstCompletedQuest()
        {
            return questService != null ? questService.GetCompletedUnclaimedQuests().Find(q => q != null) : null;
        }

        private void SelectShopTab(ShopType type)
        {
            selectedShopType = type;
            selectedShopItemId = string.Empty;
            RefreshShopUi();
        }

        private void OpenShopPurchaseConfirm(string shopItemId)
        {
            ShopItemDefinition item = contentService?.Database?.GetShopItemById(shopItemId);
            if (item == null)
            {
                screenManager.ToastService?.ShowToast("Shop item missing.");
                return;
            }
            string blocker = shopService != null ? shopService.GetPurchaseBlocker(item) : "Shop not ready.";
            if (!string.IsNullOrEmpty(blocker))
            {
                screenManager.ToastService?.ShowToast(blocker);
                return;
            }

            selectedShopItemId = shopItemId;
            GameObject popupObject = ShowPopup("ShopPurchaseConfirmPopup");
            Transform popup = popupObject != null ? popupObject.transform : null;
            if (popup == null) return;
            SetPopupText(popup, "Title", item.displayName);
            SetPopupText(popup, "Body", $"{item.description}\nAmount: {Mathf.Max(1, item.amount)}\nPrice: {item.priceAmount} {ShopService.CurrencyDisplayName(item.priceCurrency)}");
            SetPopupText(popup, "Warning", item.priceCurrency == CurrencyType.SoulGem ? "Spending Soul Gems. Confirm carefully." : string.Empty);
        }

        private void ConfirmShopPurchase()
        {
            ShopItemDefinition item = contentService?.Database?.GetShopItemById(selectedShopItemId);
            if (shopService != null && item != null && shopService.Purchase(item))
            {
                cloudSaveCoordinator?.QueueCloudSync(1f);
                CloseShopPurchaseConfirm();
                RefreshSaveBackedUi();
            }
        }

        private void CloseShopPurchaseConfirm()
        {
            HidePopup("ShopPurchaseConfirmPopup");
        }

        private void PurchaseIapProduct(string productId)
        {
            if (iapService == null || !iapService.IsAvailable)
            {
                screenManager.ToastService?.ShowToast("Store is currently unavailable. Please try again later.");
                return;
            }
            iapService?.Purchase(productId);
        }

        private void RestorePurchases()
        {
            iapService?.RestorePurchases();
            purchaseLedgerService = purchaseLedgerService ?? (ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService) ? new PurchaseLedgerService(saveService) : null);
            screenManager.ToastService?.ShowToast("Purchase records refreshed.");
        }

        private void OnIapPurchaseStarted(string productId)
        {
            screenManager.LoadingOverlay?.ShowLoading("Processing purchase...");
        }

        private void OnIapPurchaseSucceeded(PurchaseRecord record)
        {
            screenManager.LoadingOverlay?.HideLoading();
            ShowIapPurchaseSuccessPopup(record);
            cloudSaveCoordinator?.QueuePurchaseLedgerSync();
            RefreshSaveBackedUi();
        }

        private void OnIapPurchaseFailed(string productId, string reason)
        {
            screenManager.LoadingOverlay?.HideLoading();
            string message = reason != null && reason.ToLowerInvariant().Contains("cancel") ? "Purchase cancelled." : "Purchase failed. Please try again.";
            screenManager.ToastService?.ShowToast(message);
            RefreshSaveBackedUi();
        }

        private void OnIapRestoreCompleted()
        {
            screenManager.ToastService?.ShowToast("Purchase records refreshed.");
            RefreshSaveBackedUi();
        }

        private void OnIapStatusChanged(string status)
        {
            if (!string.IsNullOrEmpty(status)) Debug.Log("[IAP] " + status);
            RefreshSaveBackedUi();
        }

        private void ShowIapPurchaseSuccessPopup(PurchaseRecord record)
        {
            GameObject popupObject = ShowPopup("IAPPurchaseSuccessPopup");
            Transform popup = popupObject != null ? popupObject.transform : null;
            if (popup == null || record == null) return;
            SetPopupText(popup, "Body", $"{ProductName(record.productId)}\nSoul Gems +{record.amount}\nBonus +{record.bonusAmount}\nTotal Granted {record.totalGranted}\nCurrent Soul Gems: {progressionService?.CurrentSave?.soulGem ?? 0}");
        }

        private void CloseIapPurchaseSuccessPopup()
        {
            HidePopup("IAPPurchaseSuccessPopup");
        }

        private string ProductName(string productId)
        {
            IAPProductDefinition product = contentService?.Database?.GetIAPProductById(productId);
            return product != null ? product.displayName : productId;
        }

        private async System.Threading.Tasks.Task SignInGuestCloud()
        {
            if (cloudSaveCoordinator == null) return;
            await cloudSaveCoordinator.SignInGuestAsync();
            RefreshSaveBackedUi();
        }

        private async System.Threading.Tasks.Task SignInGoogleCloud()
        {
            if (cloudSaveCoordinator == null) return;
            await cloudSaveCoordinator.SignInGoogleAsync();
            RefreshSaveBackedUi();
        }

        private async System.Threading.Tasks.Task SyncCloudNow()
        {
            if (cloudSaveCoordinator == null) return;
            await cloudSaveCoordinator.SyncNowAsync();
            RefreshSaveBackedUi();
        }

        private async System.Threading.Tasks.Task UploadLocalCloud()
        {
            if (cloudSaveCoordinator == null) return;
            await cloudSaveCoordinator.UploadLocalSaveAsync();
            RefreshSaveBackedUi();
        }

        private async System.Threading.Tasks.Task DownloadCloudSave()
        {
            if (cloudSaveCoordinator == null) return;
            await cloudSaveCoordinator.DownloadCloudSaveAsync();
            progressionService?.LoadOrCreate();
            RefreshSaveBackedUi();
        }

        private void SetCloudSyncEnabled(bool enabled)
        {
            cloudSaveCoordinator?.SetCloudSyncEnabled(enabled);
            RefreshSaveBackedUi();
        }

        private void OnCloudConflictDetected(CloudSaveMeta local, CloudSaveMeta cloud)
        {
            GameObject popupObject = ShowPopup("CloudConflictPopup");
            Transform popup = popupObject != null ? popupObject.transform : null;
            if (popup == null) return;
            SetPopupText(popup, "Local_Text", BuildCloudMetaText("Local Save", local));
            SetPopupText(popup, "Cloud_Text", BuildCloudMetaText("Cloud Save", cloud));
        }

        private async System.Threading.Tasks.Task ResolveCloudUseLocal()
        {
            if (cloudSaveCoordinator != null) await cloudSaveCoordinator.ResolveConflictUseLocalAsync();
            CloseCloudConflictPopup();
            RefreshSaveBackedUi();
        }

        private async System.Threading.Tasks.Task ResolveCloudUseCloud()
        {
            if (cloudSaveCoordinator != null) await cloudSaveCoordinator.ResolveConflictUseCloudAsync();
            progressionService?.LoadOrCreate();
            CloseCloudConflictPopup();
            RefreshSaveBackedUi();
        }

        private void CancelCloudConflict()
        {
            cloudSaveCoordinator?.CancelConflictForSession();
            CloseCloudConflictPopup();
            RefreshSaveBackedUi();
        }

        private void CloseCloudConflictPopup()
        {
            HidePopup("CloudConflictPopup");
        }

        private void OpenDeleteConfirm()
        {
            ShowPopup("DeleteSaveConfirmPopup");
        }

        private void CloseDeleteConfirm()
        {
            HidePopup("DeleteSaveConfirmPopup");
        }

        private void ConfirmDeleteSave()
        {
            progressionService?.DeleteSave();
            CloseDeleteConfirm();
            screenManager.CloseSettings();
            screenManager.ShowScreen(GameUIScreen.Title);
        }

        private void DebugAddGold(int amount)
        {
            progressionService?.AddGold(amount);
            questService?.TrackProgress(QuestObjectiveType.EarnGold, "any", amount);
            RefreshSaveBackedUi();
        }

        private void DebugAddSoulGem(int amount)
        {
            progressionService?.AddSoulGem(amount);
            RefreshSaveBackedUi();
        }

        private void DebugResetDailyShop()
        {
            shopService?.ResetDailyShop(true);
            RefreshSaveBackedUi();
        }

        private void DebugClearPurchaseRecords()
        {
            purchaseLedgerService?.ClearDebugRecords();
            screenManager.ToastService?.ShowToast("Debug purchase records cleared.");
            RefreshSaveBackedUi();
        }

        private void DebugPrintPurchaseLedger()
        {
            List<PurchaseRecord> records = purchaseLedgerService != null ? purchaseLedgerService.GetAllRecords() : new List<PurchaseRecord>();
            string text = "[IAP] Purchase Ledger (" + records.Count + ")";
            foreach (PurchaseRecord record in records)
            {
                if (record == null) continue;
                text += $"\n{record.transactionId} product={record.productId} total={record.totalGranted} granted={record.granted} cloudSynced={record.cloudSynced}";
            }
            Debug.Log(text);
        }

        private void DebugForceCloudConflict()
        {
            cloudSaveCoordinator?.ForceConflictTest();
        }

        private void DebugClearFirebaseUid()
        {
            cloudSaveCoordinator?.ClearFirebaseUidFromLocalSave();
            RefreshSaveBackedUi();
        }

        private void DebugPrintLocalSaveInfo()
        {
            if (ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService))
            {
                PlayerSaveData save = saveService.CurrentSave;
                Debug.Log($"[Cloud] Local Save Info\nSave path: {saveService.SaveFilePath}\nBackup path: {saveService.BackupFilePath}\nPlayer: {save?.playerName}\nLevel: {save?.level}\nSaveVersion: {save?.saveVersion}\nUpdatedAt: {save?.updatedAt}\nFirebase UID: {save?.firebaseUid}\nCloud Sync: {save?.cloudSyncEnabled}");
            }
        }

        private void DebugExportLocalSaveJson()
        {
            if (!ServiceLocator.TryResolve<ISaveService>(out ISaveService saveService)) return;
            string path = "Assets/_Game/Export/local_save_debug.json";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, saveService.ExportCurrentSaveJson());
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
            screenManager.ToastService?.ShowToast("Local save JSON exported.");
        }

        private void DebugAddItem(string itemId, int amount)
        {
            progressionService?.AddItem(itemId, amount);
            questService?.TrackProgress(QuestObjectiveType.CollectItem, itemId, amount);
            RefreshSaveBackedUi();
        }

        private void DebugAddEquipment(string equipmentId)
        {
            EquipmentInstanceData equipment = equipmentService != null ? equipmentService.CreateEquipmentInstance(equipmentId) : PrototypeEquipmentFactory.Create(equipmentId);
            progressionService?.AddEquipment(equipment);
            questService?.TrackProgress(QuestObjectiveType.OwnEquipment, equipment.equipmentId, 1);
            selectedEquipmentInstanceId = equipment.instanceId;
            RefreshSaveBackedUi();
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
            SetText("TitleScreenUI/AccountStatus_Text", BuildTitleAccountStatus());
            Transform settings = popupLayer != null ? popupLayer.Find("SettingsPopup") : null;
            SetPopupText(settings, "ModalPanel/ScrollView/Viewport/Content/Section_Account/AccountStatus", BuildSettingsAccountStatus(save));
            SetPopupText(settings, "ModalPanel/ScrollView/Viewport/Content/Section_Account/AccountText", BuildSettingsAccountText());
            string remoteStatus = gameConfigService != null && gameConfigService.RemoteContentEnabled ? "Enabled" : "Disabled";
            string configured = assetLoadService != null && assetLoadService.IsAvailable ? "Configured" : "Content is stored locally.";
            SetPopupText(settings, "ModalPanel/ScrollView/Viewport/Content/Section_Content/CurrentContentVersion", $"Current Content Version: {gameConfigService?.CurrentContentVersion ?? "0.1.0"}");
            SetPopupText(settings, "ModalPanel/ScrollView/Viewport/Content/Section_Content/ContentStatus", $"Remote Content: {remoteStatus}\n{configured}");
            Toggle cloudToggle = settings != null ? settings.Find("ModalPanel/ScrollView/Viewport/Content/Section_Cloud/Toggle_CloudSync")?.GetComponent<Toggle>() : null;
            if (cloudToggle != null) cloudToggle.SetIsOnWithoutNotify(save.cloudSyncEnabled);
            GameObject guestButton = settings != null ? settings.Find("ModalPanel/ScrollView/Viewport/Content/Section_Account/Button_SignInGuest")?.gameObject : null;
            if (guestButton != null) guestButton.SetActive(cloudSaveCoordinator == null || cloudSaveCoordinator.CurrentUser == null);
            GameObject googleButton = settings != null ? settings.Find("ModalPanel/ScrollView/Viewport/Content/Section_Account/Button_SignInGoogle")?.gameObject : null;
            if (googleButton != null) googleButton.SetActive(CanShowGoogleSignIn());

            PlayerStats stats = progressionService.CalculateTotalStats();
            SetText("HeroUI/Stats_Text", $"Lv. {save.level}\nHP {stats.maxHp}\nMana {stats.mana}\nATK {stats.atk}\nMAG {stats.mag}\nDEF {stats.def}\nSPD {stats.spd}\nLUCK {stats.luck}");
            SetText("HeroUI/Exp_Text", $"EXP {save.exp} / {progressionService.GetExpRequired(save.level)}");
            SetText("HeroUI/Class_Text", "Class: " + DisplayClass(save.selectedClassId));
            SetBoundImageAsset("HeroUI/HeroPortrait", ClassPortraitAssetId(save.selectedClassId));
            SetText("HeroUI/EquippedSkills_Text", "Skills: " + SkillName(save.equippedSkill1Id) + " / " + SkillName(save.equippedSkill2Id) + " / " + SkillName(save.equippedUltimateId));
            selectedSkillClassId = save.selectedClassId;
            RefreshSkillsUi();

            SetText("InventoryUI/InventoryList_Text", BuildInventoryText(save));
            SetText("InventoryUI/ItemDetail_Text", BuildSelectedEquipmentText());
            SetText("EquipmentUI/EquipmentDetail_Text", BuildEquipmentText(save));
            SetText("QuestUI/QuestList_Text", BuildQuestListText());
            SetText("MainTownUI/QuestTracker_Text", BuildQuestTrackerText());
            RefreshShopUi();
            RefreshEquipmentCards(save);
        }

        private void RefreshShopUi()
        {
            Transform root = mainLayer != null ? mainLayer.Find("ShopUI") : null;
            PlayerSaveData save = progressionService?.CurrentSave;
            if (root == null || save == null) return;
            SetText("ShopUI/ShopCurrency_Text", $"Gold: {save.gold}   Soul Gems: {save.soulGem}   Bag: {save.inventory.capacity} slots");
            for (int i = 0; i < 8; i++)
            {
                Transform card = root.Find("ShopItemCard_" + i);
                if (card != null) card.gameObject.SetActive(false);
            }
            for (int i = 0; i < 8; i++)
            {
                Transform card = root.Find("IapProductCard_" + i);
                if (card != null) card.gameObject.SetActive(false);
            }

            if (selectedShopType == ShopType.IAPPlaceholder)
            {
                if (iapService == null || !iapService.IsAvailable)
                {
                    SetText("ShopUI/ShopInfo_Text", "Soul Gem Packs\nStore is currently unavailable. Please try again later.");
                    return;
                }
                if (gameConfigService != null && !gameConfigService.IapEnabled)
                {
                    SetText("ShopUI/ShopInfo_Text", "Soul Gem Packs\nStore is currently unavailable. Please try again later.");
                    return;
                }
                RefreshIapShopUi(root);
                return;
            }

            ShopDefinition shop = contentService?.Database?.GetShopByType(selectedShopType);
            SetText("ShopUI/ShopInfo_Text", shop != null ? shop.displayName : "No shop content found. Run Create Prototype Shop Content, then rebuild the content database.");
            List<ShopItemDefinition> items = shopService != null ? shopService.GetShopItems(selectedShopType) : new List<ShopItemDefinition>();
            for (int i = 0; i < Mathf.Min(items.Count, 6); i++)
            {
                ShopItemDefinition item = items[i];
                if (item == null) continue;
                string capturedId = item.id;
                ShopPurchaseLimitData limit = shopService?.GetPurchaseLimitData(item.id);
                string limitText = BuildLimitText(item, limit);
                string blocker = shopService != null ? shopService.GetPurchaseBlocker(item) : "Shop not ready.";
                bool soldOut = blocker == "Sold out today." || blocker == "Sold out." || !item.enabled;
                string label = $"{item.displayName}\n{item.description}\nAmount: {Mathf.Max(1, item.amount)}   Price: {item.priceAmount}\n{limitText}";
                Button card = Button(root, "ShopItemCard_" + i, label, soldOut ? new Color(0.45f, 0.48f, 0.55f, 1f) : primary, Anchor.Center, new Vector2(i % 2 == 0 ? -225f : 225f, 345f - (i / 2) * 190f), new Vector2(420f, 170f), () => OpenShopPurchaseConfirm(capturedId));
                FormatCardLabel(card, 22);
                ImageAsset(card.transform, "Icon", string.IsNullOrEmpty(item.iconAssetId) ? item.itemId : item.iconAssetId, Anchor.Center, new Vector2(-170f, 35f), new Vector2(58f, 58f)).raycastTarget = false;
                ImageAsset(card.transform, "PriceIcon", item.priceCurrency == CurrencyType.SoulGem ? "currency_soul_gem" : "currency_gold", Anchor.Center, new Vector2(115f, -50f), new Vector2(42f, 42f)).raycastTarget = false;
                SetButtonEnabled(card, !soldOut);
                card.gameObject.SetActive(true);
            }
        }

        private void RefreshIapShopUi(Transform root)
        {
            SetText("ShopUI/ShopInfo_Text", iapService != null && iapService.IsAvailable ? "Soul Gem Packs\nIAP only sells Soul Gem currency." : "Soul Gem Packs\nStore is currently unavailable. Please try again later.");
            List<IAPProductViewData> products = iapService != null ? iapService.GetProducts() : new List<IAPProductViewData>();
            for (int i = 0; i < Mathf.Min(products.Count, 6); i++)
            {
                IAPProductViewData product = products[i];
                if (product == null) continue;
                string capturedId = product.productId;
                bool canBuy = product.available && iapService != null && iapService.IsAvailable;
                string label = $"{product.displayName}\nSoul Gems: {product.soulGemAmount}\nBonus: {product.bonusSoulGemAmount}\nTotal: {product.totalSoulGemAmount}\n{product.localizedPriceText}\n{(canBuy ? "Buy" : "Unavailable")}";
                UnityEngine.Events.UnityAction action = canBuy ? (UnityEngine.Events.UnityAction)(() => PurchaseIapProduct(capturedId)) : screenManager.ShowDisabledToast;
                Button card = Button(root, "IapProductCard_" + i, label, canBuy ? secondary : new Color(0.45f, 0.48f, 0.55f, 1f), Anchor.Center, new Vector2(i % 2 == 0 ? -225f : 225f, 345f - (i / 2) * 190f), new Vector2(420f, 170f), action);
                FormatCardLabel(card, 21);
                ImageAsset(card.transform, "Icon", "currency_soul_gem", Anchor.Center, new Vector2(-170f, 35f), new Vector2(58f, 58f)).raycastTarget = false;
                SetButtonEnabled(card, canBuy);
                card.gameObject.SetActive(true);
            }
        }

        private static string BuildLimitText(ShopItemDefinition item, ShopPurchaseLimitData limit)
        {
            string text = string.Empty;
            if (item.purchaseLimitPerDay > 0) text += $"Daily {limit?.dailyCount ?? 0}/{item.purchaseLimitPerDay} ";
            if (item.purchaseLimitLifetime > 0) text += $"Lifetime {limit?.lifetimeCount ?? 0}/{item.purchaseLimitLifetime}";
            return string.IsNullOrEmpty(text) ? "No limit" : text.TrimEnd();
        }

        private string BuildTitleAccountStatus()
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            if (save == null) return "Account: Local";
            if (cloudSaveCoordinator == null || cloudSaveCoordinator.GetStatus() == CloudSaveStatus.LocalOnly) return "Account: Local";
            if (save.authProvider == "Google") return "Account: Google";
            return "Account: Guest";
        }

        private string BuildSettingsAccountText()
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            string account = "Local";
            if (cloudSaveCoordinator != null && cloudSaveCoordinator.CurrentUser != null)
            {
                account = cloudSaveCoordinator.CurrentUser.providerType == AuthProviderType.Google ? "Google" : "Guest";
            }

            string syncState = cloudSaveCoordinator == null ? "Disabled" : cloudSaveCoordinator.GetStatus() == CloudSaveStatus.Error ? "Error" : save != null && save.cloudSyncEnabled ? "Enabled" : "Disabled";
            string lastSynced = save != null && save.lastCloudUploadAt > 0 ? DateTimeOffset.FromUnixTimeSeconds(save.lastCloudUploadAt).ToLocalTime().ToString("g") : "Never";
            return $"Account: {account}\nCloud Sync: {syncState}\nLast synced: {lastSynced}";
        }

        private bool CanShowGoogleSignIn()
        {
            return cloudSaveCoordinator != null && cloudSaveCoordinator.IsGoogleSignInConfigured;
        }

        private void OpenManageDownloads()
        {
            ContentPackDefinition pack = contentPackService?.GetDownloadablePacks().Find(p => p != null && !p.includedInBuild);
            if (pack == null) pack = contentPackService?.GetAllPacks().Find(p => p != null && p.downloadable);
            if (pack == null) pack = contentPackService?.GetAllPacks().Find(p => p != null);
            if (pack != null) OpenContentDownloadPopup(pack);
            else screenManager.ToastService?.ShowToast("No content packs found.");
        }

        private void OpenContentDownloadPopup(ContentPackDefinition pack)
        {
            if (pack == null) return;
            selectedContentPackId = pack.id;
            GameObject popupObject = ShowPopup("ContentDownloadPopup");
            Transform popup = popupObject != null ? popupObject.transform : null;
            if (popup == null) return;
            ContentPackDownloadStatus status = contentPackService != null ? contentPackService.GetPackDownloadStatus(pack.id) : ContentPackDownloadStatus.Unknown;
            string remoteText = assetLoadService != null && assetLoadService.IsAvailable && gameConfigService != null && gameConfigService.RemoteContentEnabled
                ? string.Empty
                : "\nOptional downloads are unavailable in this build.";
            SetPopupText(popup, "Title", pack.displayName);
            SetPopupText(popup, "Description", $"{pack.description}\nSize: {FormatBytes(pack.estimatedSizeBytes)}\nVersion: {pack.version}{remoteText}");
            SetPopupText(popup, "Status", "Status: " + status);
            Slider progress = popup.Find("Progress")?.GetComponent<Slider>();
            if (progress != null) progress.SetValueWithoutNotify(status == ContentPackDownloadStatus.Downloaded || status == ContentPackDownloadStatus.AvailableLocal ? 1f : 0f);
        }

        private async void DownloadSelectedPack()
        {
            if (string.IsNullOrEmpty(selectedContentPackId) || contentPackService == null) return;
            ContentPackDefinition pack = contentService?.Database?.GetContentPackById(selectedContentPackId);
            await contentPackService.DownloadPackAsync(selectedContentPackId);
            if (pack != null) OpenContentDownloadPopup(pack);
            RefreshWorldMapState();
        }

        private async void ClearOptionalCache()
        {
            if (contentPackService != null) await contentPackService.ClearOptionalPacksAsync();
            screenManager.ToastService?.ShowToast("Optional content cache cleared.");
            RefreshSaveBackedUi();
        }

        private void CloseContentDownloadPopup()
        {
            HidePopup("ContentDownloadPopup");
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "Unknown";
            if (bytes < 1024L * 1024L) return Mathf.CeilToInt(bytes / 1024f) + " KB";
            return (bytes / (1024f * 1024f)).ToString("0.0") + " MB";
        }

        private string BuildSettingsAccountStatus(PlayerSaveData save)
        {
            string status = cloudSaveCoordinator != null ? cloudSaveCoordinator.GetStatus().ToString() : "LocalOnly";
            string account = string.IsNullOrEmpty(save.firebaseUid) ? "Local" : (save.authProvider == "Google" ? "Google" : "Guest");
            string uid = ShortId(save.firebaseUid);
            string sync = status == "Error" ? "Error" : save.cloudSyncEnabled ? "Enabled" : "Disabled";
            return $"Status: {status}\nAccount: {account}\nUID: {uid}\nCloud Sync: {sync}";
        }

        private static string BuildCloudMetaText(string title, CloudSaveMeta meta)
        {
            if (meta == null) return title + "\nMissing";
            return $"{title}\n{meta.playerName}\nLv {meta.level}\nGold {meta.gold}\nGems {meta.soulGem}\nRealm {meta.currentRealmId}\nStage {meta.currentStageId}\nUpdated {meta.updatedAt}\nDevice {ShortId(meta.deviceId)}";
        }

        private static string ShortId(string id)
        {
            return string.IsNullOrEmpty(id) ? "-" : id.Substring(0, Mathf.Min(8, id.Length));
        }

        private static bool IsDebugBuild()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#else
            return false;
#endif
        }

        private static void FormatCardLabel(Button card, int fontSize)
        {
            TextMeshProUGUI label = card != null ? card.GetComponentInChildren<TextMeshProUGUI>() : null;
            if (label == null) return;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.margin = new Vector4(68f, 4f, 8f, 4f);
        }

        private static void SetDebugButton(Button button)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            SetButtonEnabled(button, true);
#else
            SetButtonEnabled(button, false);
#endif
        }

        private void RefreshSkillsUi()
        {
            PlayerSaveData save = progressionService?.CurrentSave;
            if (save == null || contentService?.Database == null) return;
            List<SkillDefinition> skills = contentService.Database.GetSkillsByClass(selectedSkillClassId);
            if (string.IsNullOrEmpty(selectedSkillId) || !skills.Exists(s => s != null && s.id == selectedSkillId))
            {
                selectedSkillId = skills.Find(s => s != null && s.activationType == SkillActivationType.Active)?.id;
            }

            string text = $"Equipped\nSkill 1: {SkillName(save.equippedSkill1Id)}\nSkill 2: {SkillName(save.equippedSkill2Id)}\nUltimate: {SkillName(save.equippedUltimateId)}\n\n";
            foreach (SkillDefinition skill in skills)
            {
                PlayerSkillData ps = skillService?.GetPlayerSkill(skill.id);
                int level = ps != null ? ps.level : 1;
                SkillLevelData next = skill.GetLevelData(Mathf.Min(level + 1, skill.maxLevel));
                string itemCost = next != null && !string.IsNullOrEmpty(next.requiredItemId) && next.requiredItemAmount > 0 ? $" + {PrototypeItemDatabase.Get(next.requiredItemId).displayName} x{next.requiredItemAmount}" : string.Empty;
                string selectedMarker = skill.id == selectedSkillId ? "> " : string.Empty;
                text += $"{selectedMarker}{skill.displayName} Lv {level}/{skill.maxLevel} ({skill.slotType})\n{skill.description}\nMana {skillService?.GetManaCost(skill.id) ?? skill.baseManaCost}  CD {skillService?.GetCooldown(skill.id) ?? skill.baseCooldown}\nUpgrade: {(level >= skill.maxLevel ? "MAX" : (next != null ? next.upgradeGoldCost + " gold" + itemCost : "Missing data"))}\n\n";
            }
            SetText("SkillsUI/SkillList_Text", text);
            RefreshSkillCards(skills, save);
        }

        private void RefreshSkillCards(List<SkillDefinition> skills, PlayerSaveData save)
        {
            Transform root = mainLayer != null ? mainLayer.Find("SkillsUI") : null;
            if (root == null) return;

            for (int i = 0; i < 6; i++)
            {
                Transform existing = root.Find("SkillCard_" + i);
                if (existing != null) existing.gameObject.SetActive(false);
            }

            int count = Mathf.Min(skills.Count, 6);
            for (int i = 0; i < count; i++)
            {
                SkillDefinition skill = skills[i];
                if (skill == null) continue;
                PlayerSkillData ps = skillService?.GetPlayerSkill(skill.id);
                int level = ps != null ? ps.level : 1;
                bool equipped = save.equippedSkill1Id == skill.id || save.equippedSkill2Id == skill.id || save.equippedUltimateId == skill.id;
                string label = $"{(skill.id == selectedSkillId ? "> " : string.Empty)}{skill.displayName} Lv {level}/{skill.maxLevel}  {skill.slotType}{(equipped ? "  Equipped" : string.Empty)}";
                string capturedSkillId = skill.id;
                Button card = Button(root, "SkillCard_" + i, label, skill.id == selectedSkillId ? secondary : primary, Anchor.Center, new Vector2(0f, 330f - i * 98f), new Vector2(760f, 82f), () => SelectSkill(capturedSkillId));
                card.gameObject.SetActive(true);
                Image icon = ImageAsset(card.transform, "Icon", skill.iconAssetId, Anchor.Center, new Vector2(-330f, 0f), new Vector2(58f, 58f));
                icon.raycastTarget = false;
                Button upgrade = Button(card.transform, "Button_Upgrade", "Upgrade", secondary, Anchor.Center, new Vector2(190f, -20f), new Vector2(145f, 44f), () => { selectedSkillId = capturedSkillId; UpgradeSelectedSkill(); });
                Button equip = Button(card.transform, "Button_Equip", equipped ? "Equipped" : "Equip", equipped ? new Color(0.35f, 0.38f, 0.44f, 1f) : primary, Anchor.Center, new Vector2(335f, -20f), new Vector2(120f, 44f), () => { selectedSkillId = capturedSkillId; EquipSelectedSkill(); });
                SetButtonEnabled(upgrade, level < skill.maxLevel);
                SetButtonEnabled(equip, !equipped);
            }
        }

        private string SkillName(string skillId)
        {
            SkillDefinition skill = contentService?.Database?.GetSkillById(skillId);
            return skill != null ? skill.displayName : skillId;
        }

        private static string DisplayClass(string classId)
        {
            if (classId == "tide_acolyte") return "Tide Acolyte";
            if (classId == "storm_scout") return "Storm Scout";
            return "Flame Squire";
        }

        private string BuildInventoryText(PlayerSaveData save)
        {
            string text = $"Capacity: {save.inventory.capacity} slots (+{save.inventoryExtraSlots} extra)\n\nItems:\n";
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
                    string equipped = equipmentService != null && equipmentService.IsEquipped(equipment.instanceId) ? " [E]" : string.Empty;
                    string locked = equipment.locked ? " [L]" : string.Empty;
                    text += $"- {equipment.displayName} Lv {equipment.level} {equipment.rarity} {equipment.slot}{equipped}{locked}\n";
                }
            }
            return text;
        }

        private string BuildQuestListText()
        {
            if (questService == null || contentService?.Database?.quests == null || contentService.Database.quests.Count == 0)
            {
                return "No quest content found. Run Create Prototype Quests, then rebuild the content database.";
            }

            string text = string.Empty;
            List<QuestDefinition> claimable = questService.GetCompletedUnclaimedQuests();
            if (claimable.Count > 0)
            {
                text += "Ready to claim:\n";
                foreach (QuestDefinition quest in claimable)
                {
                    if (quest != null) text += "- " + quest.displayName + "\n";
                }
                text += "\n";
            }

            List<QuestDefinition> active = questService.GetActiveQuests();
            text += "Active:\n";
            if (active.Count == 0)
            {
                text += "- None\n";
            }
            else
            {
                foreach (QuestDefinition quest in active)
                {
                    if (quest != null) text += questService.BuildQuestProgressText(quest) + "\n\n";
                }
            }

            List<QuestDefinition> available = questService.GetAvailableQuests();
            if (available.Count > 0)
            {
                text += "Available:\n";
                foreach (QuestDefinition quest in available)
                {
                    if (quest != null) text += "- " + quest.displayName + "\n";
                }
            }

            return text.TrimEnd();
        }

        private string BuildQuestTrackerText()
        {
            QuestDefinition claimable = FirstCompletedQuest();
            if (claimable != null)
            {
                return "Quest reward ready\n" + claimable.displayName;
            }

            QuestDefinition tracker = questService?.GetTrackerQuest();
            return tracker != null ? questService.BuildQuestProgressText(tracker) : "No active quest. Visit the Quest Elder.";
        }

        private string BuildEquipmentText(PlayerSaveData save)
        {
            string detail = "Equipped Slots:\n" +
                   $"Weapon: {GetEquippedName(save, save.equipment.weaponInstanceId)}\n" +
                   $"Armor: {GetEquippedName(save, save.equipment.armorInstanceId)}\n" +
                   $"Head: {GetEquippedName(save, save.equipment.headInstanceId)}\n" +
                   $"Boots: {GetEquippedName(save, save.equipment.bootsInstanceId)}\n" +
                   $"Ring: {GetEquippedName(save, save.equipment.ringInstanceId)}\n" +
                   $"Charm: {GetEquippedName(save, save.equipment.charmInstanceId)}";
            string selected = BuildSelectedEquipmentText();
            return string.IsNullOrEmpty(selected) ? detail : detail + "\n\n" + selected;
        }

        private string BuildSelectedEquipmentText()
        {
            EquipmentInstanceData equipment = equipmentService?.GetEquipmentByInstanceId(selectedEquipmentInstanceId);
            if (equipment == null) return "Select equipment to view details.";
            EquipmentDefinition definition = equipmentService.GetDefinition(equipment.equipmentId);
            EquipmentUpgradeCostData nextCost = definition != null ? equipmentService.GetUpgradeCost(definition, equipment.level + 1) : null;
            string upgrade = equipment.level >= (definition != null ? definition.maxLevel : equipment.level) ? "MAX" : nextCost != null ? $"{nextCost.goldCost} gold + {nextCost.materialItemId} x{nextCost.materialAmount}" : "Missing cost";
            return $"{equipment.displayName} {(equipment.locked ? "[Locked]" : string.Empty)}\n{equipment.rarity} {equipment.slot} Lv {equipment.level}/{(definition != null ? definition.maxLevel : 1)}\n{equipmentService.BuildStatText(equipment)}\nUpgrade: {upgrade}\nSell: {equipmentService.GetSellValue(equipment)} gold\n{equipmentService.BuildComparisonText(equipment)}";
        }

        private void RefreshEquipmentCards(PlayerSaveData save)
        {
            Transform inventoryRoot = mainLayer != null ? mainLayer.Find("InventoryUI") : null;
            if (inventoryRoot != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    Transform existing = inventoryRoot.Find("EquipmentCard_" + i);
                    if (existing != null) existing.gameObject.SetActive(false);
                }
                int count = Mathf.Min(save.inventory.equipments.Count, 12);
                for (int i = 0; i < count; i++)
                {
                    EquipmentInstanceData equipment = save.inventory.equipments[i];
                    string captured = equipment.instanceId;
                    bool equipped = equipmentService != null && equipmentService.IsEquipped(equipment.instanceId);
                    string label = $"{equipment.displayName}\nLv {equipment.level} {equipment.rarity} {equipment.slot}{(equipped ? " [E]" : string.Empty)}{(equipment.locked ? " [L]" : string.Empty)}";
                    Button card = Button(inventoryRoot, "EquipmentCard_" + i, label, equipment.instanceId == selectedEquipmentInstanceId ? secondary : primary, Anchor.Center, new Vector2(i % 2 == 0 ? -235f : 235f, 345f - (i / 2) * 95f), new Vector2(430f, 82f), () => SelectEquipment(captured));
                    EquipmentDefinition definition = equipmentService != null ? equipmentService.GetDefinition(equipment.equipmentId) : null;
                    Image icon = ImageAsset(card.transform, "Icon", definition != null && !string.IsNullOrEmpty(definition.iconAssetId) ? definition.iconAssetId : equipment.equipmentId, Anchor.Center, new Vector2(-180f, 0f), new Vector2(52f, 52f));
                    icon.raycastTarget = false;
                    card.gameObject.SetActive(true);
                }
            }

            Transform equipmentRoot = mainLayer != null ? mainLayer.Find("EquipmentUI") : null;
            if (equipmentRoot == null) return;
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                Transform slotTransform = equipmentRoot.Find("Slot_" + slot);
                TextMeshProUGUI label = slotTransform != null ? slotTransform.GetComponentInChildren<TextMeshProUGUI>() : null;
                EquipmentInstanceData equipped = equipmentService?.GetEquipped(slot);
                if (label != null) label.text = equipped != null ? $"{slot}\n{equipped.displayName} Lv {equipped.level}" : $"{slot}\nEmpty";
            }
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

        private static void SetPopupText(Transform popup, string path, string value)
        {
            Transform target = popup != null ? popup.Find(path) : null;
            TextMeshProUGUI text = target != null ? target.GetComponent<TextMeshProUGUI>() : null;
            if (text != null) text.text = value;
        }

        private BoardController CreateBoardGrid(RectTransform root)
        {
            RectTransform board = EnsureChildRect(root, "BoardGrid");
            SetRect(board, Anchor.Center, new Vector2(0f, -50f), new Vector2(768f, 768f));
            GridLayoutGroup grid = board.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.enabled = false;
            }
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
            Image image = EnsureImage(root.gameObject, backgroundColor);
            BindImage(image, GetScreenBackgroundAssetId(name));
            root.gameObject.SetActive(false);
            return root;
        }

        private Image Panel(Transform parent, string name, Color color, Anchor anchor, Vector2 pos, Vector2 size)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            Image image = EnsureImage(rect.gameObject, color);
            BindImage(image, name.Contains("Popup") || name.Contains("Result") || name.Contains("Settings") ? "ui_panel_popup" : "ui_panel_main");
            return image;
        }

        private Image ImageAsset(Transform parent, string name, string assetId, Anchor anchor, Vector2 pos, Vector2 size)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            Image image = EnsureImage(rect.gameObject, Color.white);
            image.raycastTarget = false;
            BindImage(image, assetId);
            return image;
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
            LayoutElement layout = rect.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<LayoutElement>();
            }
            layout.preferredWidth = rectSize.x;
            layout.preferredHeight = rectSize.y;
            return label;
        }

        private Button Button(Transform parent, string name, string text, Color color, Anchor anchor, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            Image image = EnsureImage(rect.gameObject, color);
            image.raycastTarget = true;
            BindImage(image, GetButtonAssetId(name, color));
            Button button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(() => audioService?.PlaySfx("sfx_button_click"));
                button.onClick.AddListener(action);
            }
            Text(rect, "Text", text, Mathf.RoundToInt(size.y * 0.34f), Color.white, Anchor.Center, Vector2.zero, size);
            LayoutElement layout = rect.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<LayoutElement>();
            }
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            return button;
        }

        private Toggle Toggle(Transform parent, string name, string text, Anchor anchor, Vector2 pos, Vector2 size, bool isOn, UnityEngine.Events.UnityAction<bool> action)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            Toggle toggle = rect.GetComponent<Toggle>();
            if (toggle == null) toggle = rect.gameObject.AddComponent<Toggle>();
            Image bg = EnsureImage(rect.gameObject, new Color(0.12f, 0.18f, 0.28f, 0.95f));
            toggle.targetGraphic = bg;
            Text(rect, "Text", text, 30, Color.white, Anchor.Center, new Vector2(20f, 0f), size);
            toggle.isOn = isOn;
            toggle.onValueChanged.RemoveAllListeners();
            if (action != null) toggle.onValueChanged.AddListener(action);
            LayoutElement layout = rect.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<LayoutElement>();
            }
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            return toggle;
        }

        private Slider Slider(Transform parent, string name, string label, Anchor anchor, Vector2 pos, Vector2 size, float value, UnityEngine.Events.UnityAction<float> action)
        {
            RectTransform rect = EnsureChildRect(parent, name);
            SetRect(rect, anchor, pos, size);
            Text(rect, "Label", label, 26, textDark, Anchor.TopLeft, new Vector2(130f, -8f), new Vector2(260f, 36f));
            Slider slider = rect.GetComponent<Slider>();
            if (slider == null) slider = rect.gameObject.AddComponent<Slider>();
            RectTransform background = EnsureChildRect(rect, "Background");
            SetRect(background, Anchor.Center, new Vector2(90f, -8f), new Vector2(360f, 18f));
            slider.targetGraphic = EnsureImage(background.gameObject, new Color(0.08f, 0.12f, 0.2f, 1f));
            RectTransform fill = EnsureChildRect(background, "Fill");
            fill.anchorMin = Vector2.zero; fill.anchorMax = new Vector2(0.5f, 1f); fill.offsetMin = Vector2.zero; fill.offsetMax = Vector2.zero;
            slider.fillRect = fill;
            EnsureImage(fill.gameObject, primary);
            slider.minValue = 0f; slider.maxValue = 1f; slider.value = value;
            slider.onValueChanged.RemoveAllListeners();
            if (action != null) slider.onValueChanged.AddListener(action);
            LayoutElement layout = rect.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<LayoutElement>();
            }
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;
            return slider;
        }

        private void Bar(Transform parent, string name, Anchor anchor, Vector2 pos, Vector2 size, Color fillColor)
        {
            RectTransform root = EnsureChildRect(parent, name);
            SetRect(root, anchor, pos, size);
            Image bg = EnsureImage(root.gameObject, new Color(0.04f, 0.05f, 0.08f, 1f));
            BindImage(bg, name.Contains("Mana") ? "ui_bar_mana_bg" : "ui_bar_hp_bg");
            RectTransform fill = EnsureChildRect(root, "Fill");
            fill.anchorMin = new Vector2(0f, 0f);
            fill.anchorMax = new Vector2(0.75f, 1f);
            fill.offsetMin = new Vector2(4f, 4f);
            fill.offsetMax = new Vector2(-4f, -4f);
            Image fillImage = EnsureImage(fill.gameObject, fillColor);
            BindImage(fillImage, name.Contains("Mana") ? "ui_bar_mana_fill" : "ui_bar_hp_fill");
        }

        private static string GetScreenBackgroundAssetId(string screenName)
        {
            switch (screenName)
            {
                case "TitleScreenUI": return "splash_game_main";
                case "MainTownUI": return "bg_town_meadow";
                case "WorldMapUI": return "bg_world_map_scroll";
                case "RealmAdventureMapUI": return "bg_world_map_scroll";
                case "AdventureMapUI": return "bg_world_map_scroll";
                case "BattleUI": return "bg_battle_meadow";
                default: return string.Empty;
            }
        }

        private static string MapNodeAssetId(string realmId)
        {
            if (realmId == "realm_02_ember") return "map_node_realm_02_ember";
            if (realmId == "realm_03_tide") return "map_node_realm_03_tide";
            return "map_node_realm_01_meadow";
        }

        private static string ClassIdleSpriteAssetId(string classId)
        {
            if (classId == "tide_acolyte") return "char_hero_tide_idle";
            if (classId == "storm_scout") return "char_hero_storm_idle";
            return "char_hero_flame_idle";
        }

        private static string ClassPortraitAssetId(string classId)
        {
            if (classId == "tide_acolyte") return "portrait_hero_tide";
            if (classId == "storm_scout") return "portrait_hero_storm";
            return "portrait_hero_flame";
        }

        private void AddButtonIcon(Button button, string assetId)
        {
            if (button == null || string.IsNullOrEmpty(assetId)) return;
            Image icon = ImageAsset(button.transform, "Icon", assetId, Anchor.Center, new Vector2(-126f, 0f), new Vector2(54f, 54f));
            icon.raycastTarget = false;
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.margin = new Vector4(52f, 0f, 0f, 0f);
        }

        private string GetButtonAssetId(string name, Color color)
        {
            if (name.Contains("Close")) return "ui_btn_close";
            if (ColorDistance(color, secondary) < 0.08f) return "ui_btn_secondary";
            return "ui_btn_primary";
        }

        private static float ColorDistance(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }

        private static void BindImage(Image image, string assetId)
        {
            if (image == null || string.IsNullOrEmpty(assetId)) return;
            AssetSpriteBinder binder = image.GetComponent<AssetSpriteBinder>();
            if (binder == null)
            {
                binder = image.gameObject.AddComponent<AssetSpriteBinder>();
            }
            binder.assetId = assetId;
            binder.targetImage = image;
            binder.Apply();
        }

        private void SetBoundImageAsset(string path, string assetId)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(assetId) || mainLayer == null) return;
            Transform target = mainLayer.Find(path);
            Image image = target != null ? target.GetComponent<Image>() : null;
            if (image != null) BindImage(image, assetId);
        }

        private static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button == null) return;
            button.interactable = enabled;
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = enabled ? image.color : new Color(0.35f, 0.38f, 0.44f, 1f);
            }
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
                case Anchor.BottomLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                    break;
                case Anchor.BottomRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
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
            BottomLeft,
            BottomRight,
            BottomCenter
        }
    }
}
