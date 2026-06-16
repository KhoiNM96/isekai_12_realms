using System.Collections.Generic;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Enemies;
using Isekai12Realms.Stages;
using Isekai12Realms.Realms;
using Isekai12Realms.Services;
using Isekai12Realms.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Adventure
{
    public class AdventureMapController : MonoBehaviour
    {
        [SerializeField] private RectTransform mapArea;
        [SerializeField] private RectTransform monsterLayer;
        [SerializeField] private RectTransform playerLayer;
        [SerializeField] private RectTransform headerRoot;
        [SerializeField] private RectTransform controlsRoot;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI realmNameText;
        [SerializeField] private TextMeshProUGUI realmProgressText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Button backButton;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private Button upButton;
        [SerializeField] private Button downButton;

        private UIScreenManager screenManager;
        private AdventureMapService adventureService;
        private ContentDatabaseService contentService;
        private RealmProgressionService realmProgressionService;
        private PlayerProgressionService progressionService;
        private readonly List<AdventureMonsterView> monsterViews = new List<AdventureMonsterView>();
        private AdventurePlayerController playerController;

        public void Initialize(UIScreenManager ui, AdventureMapService service, ContentDatabaseService content, RealmProgressionService realmProgression, PlayerProgressionService progression)
        {
            screenManager = ui;
            adventureService = service;
            contentService = content;
            realmProgressionService = realmProgression;
            progressionService = progression;
            BuildIfNeeded();
            RefreshMap();
        }

        public void RefreshMap()
        {
            BuildIfNeeded();
            RealmDefinition realm = adventureService != null ? adventureService.GetCurrentRealm() : null;
            if (realm == null)
            {
                if (realmNameText != null) realmNameText.text = "Adventure Map";
                if (realmProgressText != null) realmProgressText.text = "Enter a realm from the world map.";
                if (hintText != null) hintText.text = "Use the arrow buttons or tap the map to move.";
                ClearMonsters();
                return;
            }

            if (realmNameText != null)
            {
                realmNameText.text = realm.displayName;
            }

            RealmProgressData progress = realmProgressionService != null ? realmProgressionService.GetCurrentRealmProgress(realm.id) : null;
            if (realmProgressText != null)
            {
                int defeated = progress != null ? progress.normalMonstersDefeated : 0;
                realmProgressText.text = $"Realm progress: {defeated}/3 normals cleared{(progress != null && progress.bossDefeated ? " | Boss cleared" : string.Empty)}";
            }

            if (hintText != null)
            {
                hintText.text = "Tap monsters to battle. Use the arrow buttons or tap the map to move.";
            }

            SetBackground(!string.IsNullOrEmpty(realm.mapBackgroundAssetId) ? realm.mapBackgroundAssetId : realm.backgroundAssetId);
            if (playerController != null)
            {
                playerController.Initialize(mapArea);
            }
            SpawnRealmMonsters(realm, progress);
        }

        private void BuildIfNeeded()
        {
            if (mapArea == null || headerRoot == null || controlsRoot == null || realmNameText == null)
            {
                ClearChildren(transform);

                backgroundImage = EnsureImage(CreateChild("Background", transform).gameObject, new Color(0.05f, 0.14f, 0.12f, 0.02f));
                Stretch(backgroundImage.rectTransform);
                backgroundImage.raycastTarget = false;

                headerRoot = CreateChild("Header", transform);
                SetHeaderRect(headerRoot, 150f);
                EnsureImage(headerRoot.gameObject, new Color(0.07f, 0.09f, 0.12f, 0.92f));

                backButton = CreateButton("Button_BackToWorldMap", headerRoot, new Vector2(-420f, -75f), new Vector2(220f, 70f), "Back", () =>
                {
                    adventureService?.ExitRealm();
                    screenManager?.ShowScreen(GameUIScreen.WorldMap);
                }).GetComponent<Button>();

                realmNameText = CreateText("RealmName_Text", headerRoot, new Vector2(0f, -66f), new Vector2(520f, 54f), 40, Color.white);
                realmProgressText = CreateText("RealmProgress_Text", headerRoot, new Vector2(0f, -108f), new Vector2(700f, 32f), 22, new Color(0.92f, 0.95f, 0.98f, 1f));

                mapArea = CreateChild("MapArea", transform);
                SetMapRect(mapArea, new Vector2(0f, -40f), new Vector2(960f, 1120f));
                backgroundImage = EnsureImage(CreateChild("MapBackground", mapArea).gameObject, new Color(0.09f, 0.18f, 0.16f, 0.92f));
                Stretch(backgroundImage.rectTransform);
                backgroundImage.raycastTarget = false;

                monsterLayer = CreateChild("MonsterContainer", mapArea);
                Stretch(monsterLayer);

                playerLayer = CreateChild("Player", mapArea);
                playerLayer.anchorMin = playerLayer.anchorMax = new Vector2(0.5f, 0.5f);
                playerLayer.pivot = new Vector2(0.5f, 0.5f);

                Image playerImage = playerLayer.gameObject.GetComponent<Image>();
                if (playerImage == null)
                {
                    playerImage = playerLayer.gameObject.AddComponent<Image>();
                }
                playerImage.color = new Color(1f, 1f, 1f, 0f);
                AdventurePlayerController playerControllerComponent = playerLayer.gameObject.GetComponent<AdventurePlayerController>();
                if (playerControllerComponent == null)
                {
                    playerControllerComponent = playerLayer.gameObject.AddComponent<AdventurePlayerController>();
                }

                RectTransform playerRect = playerLayer;
                playerRect.sizeDelta = new Vector2(96f, 96f);
                playerRect.anchoredPosition = Vector2.zero;
                playerImage.raycastTarget = false;
                SetSprite(playerImage, ClassIdleSpriteAssetId(progressionService != null && progressionService.CurrentSave != null ? progressionService.CurrentSave.selectedClassId : string.Empty));
                playerController = playerControllerComponent;
                playerController.Initialize(mapArea);

                controlsRoot = CreateChild("Controls", transform);
                SetControlsRect(controlsRoot, new Vector2(0f, 320f), new Vector2(380f, 220f));
                EnsureImage(controlsRoot.gameObject, new Color(0.05f, 0.06f, 0.08f, 0.58f));
                GridLayoutGroup grid = controlsRoot.gameObject.GetComponent<GridLayoutGroup>();
                if (grid == null)
                {
                    grid = controlsRoot.gameObject.AddComponent<GridLayoutGroup>();
                }
                grid.cellSize = new Vector2(96f, 72f);
                grid.spacing = new Vector2(14f, 14f);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 2;

                leftButton = CreateButton("Button_Left", controlsRoot, Vector2.zero, new Vector2(96f, 72f), "Left", () => playerController?.MoveBy(new Vector2(-140f, 0f))).GetComponent<Button>();
                rightButton = CreateButton("Button_Right", controlsRoot, Vector2.zero, new Vector2(96f, 72f), "Right", () => playerController?.MoveBy(new Vector2(140f, 0f))).GetComponent<Button>();
                upButton = CreateButton("Button_Up", controlsRoot, Vector2.zero, new Vector2(96f, 72f), "Up", () => playerController?.MoveBy(new Vector2(0f, 140f))).GetComponent<Button>();
                downButton = CreateButton("Button_Down", controlsRoot, Vector2.zero, new Vector2(96f, 72f), "Down", () => playerController?.MoveBy(new Vector2(0f, -140f))).GetComponent<Button>();

                hintText = CreateText("Hint_Text", transform, new Vector2(0f, -860f), new Vector2(760f, 28f), 20, new Color(1f, 1f, 1f, 0.82f));
            }

            if (playerController == null)
            {
                Transform playerTransform = transform.Find("MapArea/Player");
                if (playerTransform != null)
                {
                    playerController = playerTransform.GetComponent<AdventurePlayerController>();
                    playerController?.Initialize(mapArea);
                }
            }
        }

        private void SpawnRealmMonsters(RealmDefinition realm, RealmProgressData progress)
        {
            ClearMonsters();
            if (realm == null)
            {
                return;
            }

            Vector2[] spawnPoints =
            {
                new Vector2(-260f, 120f),
                new Vector2(120f, 140f),
                new Vector2(260f, -40f)
            };

            List<EnemyDefinition> monsters = realm.normalEnemies != null ? realm.normalEnemies : new List<EnemyDefinition>();
            int defeatedNormals = progress != null ? Mathf.Clamp(progress.normalMonstersDefeated, 0, monsters.Count) : 0;
            for (int i = 0; i < monsters.Count; i++)
            {
                if (i < defeatedNormals)
                {
                    continue;
                }

                EnemyDefinition enemy = monsters[i];
                if (enemy == null)
                {
                    continue;
                }

                GameObject monsterObject = CreateMonsterObject(enemy, false, spawnPoints[i % spawnPoints.Length]);
                monsterViews.Add(monsterObject.GetComponent<AdventureMonsterView>());
            }

            if (realm.bossEnemy != null)
            {
                bool bossVisible = progress != null && progress.normalMonstersDefeated >= Mathf.Max(3, realm.monsterSpawnCount <= 0 ? 3 : realm.monsterSpawnCount) && !progress.bossDefeated;
                if (bossVisible)
                {
                    GameObject bossObject = CreateMonsterObject(realm.bossEnemy, true, new Vector2(0f, -210f));
                    monsterViews.Add(bossObject.GetComponent<AdventureMonsterView>());
                }
            }

            if (playerController != null)
            {
                playerController.SetSpawnPosition(new Vector2(0f, -260f));
            }
        }

        private GameObject CreateMonsterObject(EnemyDefinition enemy, bool boss, Vector2 spawnPosition)
        {
            GameObject monsterObject = new GameObject(string.IsNullOrEmpty(enemy.spriteAssetId) ? enemy.id : enemy.spriteAssetId, typeof(RectTransform), typeof(Image), typeof(Button), typeof(AdventureMonsterView));
            monsterObject.transform.SetParent(monsterLayer, false);
            RectTransform rect = monsterObject.GetComponent<RectTransform>();
            rect.sizeDelta = boss ? new Vector2(128f, 128f) : new Vector2(104f, 104f);
            rect.anchoredPosition = spawnPosition;
            Image image = monsterObject.GetComponent<Image>();
            image.raycastTarget = true;
            Sprite sprite = AssetSpriteBinder.GetSprite(enemy.spriteAssetId);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
            else
            {
                image.color = boss ? new Color(0.75f, 0.45f, 1f, 1f) : new Color(0.6f, 0.85f, 0.45f, 1f);
            }

            TextMeshProUGUI label = CreateText("Label", monsterObject.transform, new Vector2(0f, -88f), new Vector2(180f, 48f), boss ? 20 : 18, Color.white);
            label.text = boss ? $"Boss\n{enemy.displayName}" : enemy.displayName;
            AdventureMonsterView view = monsterObject.GetComponent<AdventureMonsterView>();
            view.Initialize(enemy, boss, adventureService, playerController);
            view.SetSpawnPosition(spawnPosition);
            monsterObject.GetComponent<Button>().onClick.AddListener(() => view.OnPointerClick(null));
            return monsterObject;
        }

        private void ClearMonsters()
        {
            for (int i = monsterLayer.childCount - 1; i >= 0; i--)
            {
                Destroy(monsterLayer.GetChild(i).gameObject);
            }

            monsterViews.Clear();
        }

        private static RectTransform CreateChild(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
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

        private static void SetHeaderRect(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0.03f, 1f);
            rect.anchorMax = new Vector2(0.97f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, height);
            rect.anchoredPosition = Vector2.zero;
        }

        private static void SetMapRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void SetControlsRect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private void SetBackground(string assetId)
        {
            if (backgroundImage == null)
            {
                return;
            }

            Sprite sprite = AssetSpriteBinder.GetSprite(assetId);
            if (sprite != null)
            {
                backgroundImage.sprite = sprite;
            }
        }

        private static void SetSprite(Image image, string assetId)
        {
            if (image == null)
            {
                return;
            }

            Sprite sprite = AssetSpriteBinder.GetSprite(assetId);
            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.76f, 0.82f, 1f);
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(onClick);
            TextMeshProUGUI text = CreateText("Text", buttonObject.transform, Vector2.zero, size - new Vector2(18f, 12f), 22, Color.white);
            text.text = label;
            return buttonObject;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static string ClassIdleSpriteAssetId(string classId)
        {
            if (classId == "tide_acolyte") return "char_hero_tide_idle";
            if (classId == "storm_scout") return "char_hero_storm_idle";
            return "char_hero_flame_idle";
        }
    }
}
