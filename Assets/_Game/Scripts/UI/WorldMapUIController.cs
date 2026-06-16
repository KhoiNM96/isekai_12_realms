using System;
using System.Collections.Generic;
using Isekai12Realms.Adventure;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Realms;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.UI
{
    public class WorldMapUIController : MonoBehaviour
    {
        private readonly Color normalCardColor = new Color(0.18f, 0.24f, 0.34f, 0.96f);
        private readonly Color lockedCardColor = new Color(0.28f, 0.3f, 0.34f, 0.9f);
        private readonly Color completedCardColor = new Color(0.18f, 0.38f, 0.24f, 0.96f);
        private readonly Color selectedCardColor = new Color(0.95f, 0.8f, 0.28f, 0.98f);
        private readonly Color selectedLockedCardColor = new Color(0.6f, 0.55f, 0.28f, 0.95f);

        private UIScreenManager screenManager;
        private AdventureMapService adventureMapService;
        private ContentDatabaseService contentService;
        private RealmProgressionService realmProgressionService;

        private RectTransform root;
        private RectTransform scrollRoot;
        private ScrollRect scrollRect;
        private RectTransform contentRoot;
        private RectTransform detailPanel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI footerHintText;
        private TextMeshProUGUI realmTitleText;
        private TextMeshProUGUI realmRankText;
        private TextMeshProUGUI realmRequirementText;
        private TextMeshProUGUI realmDescriptionText;
        private TextMeshProUGUI realmProgressText;
        private TextMeshProUGUI enemyPreviewText;
        private TextMeshProUGUI bossPreviewText;
        private Button enterRealmButton;
        private TextMeshProUGUI enterRealmButtonText;

        private readonly List<RealmCardView> realmCards = new List<RealmCardView>();
        private string selectedRealmId;

        public void Initialize(
            UIScreenManager ui,
            AdventureMapService adventureService,
            ContentDatabaseService content,
            RealmProgressionService realmProgression,
            PlayerProgressionService progression)
        {
            screenManager = ui;
            adventureMapService = adventureService;
            contentService = content;
            realmProgressionService = realmProgression;

            BuildOrRepair();
            RefreshView();
        }

        public void BuildOrRepair()
        {
            root = transform as RectTransform;
            if (root == null)
            {
                return;
            }

            ClearChildren(root);

            Background(root);
            BuildHeader(root);
            BuildRealmScrollView(root);
            BuildDetailPanel(root);
            BuildFooter(root);

            if (string.IsNullOrEmpty(selectedRealmId))
            {
                selectedRealmId = DefaultRealmId();
            }
        }

        public void RefreshView()
        {
            if (root == null)
            {
                root = transform as RectTransform;
            }

            if (root == null || realmCards.Count == 0 || realmTitleText == null)
            {
                BuildOrRepair();
            }

            List<RealmDefinition> realms = GetRealms();
            if (realms.Count == 0)
            {
                SetPlaceholderState();
                return;
            }

            if (string.IsNullOrEmpty(selectedRealmId) || GetRealmById(selectedRealmId) == null)
            {
                selectedRealmId = DefaultRealmId();
            }

            for (int i = 0; i < realmCards.Count; i++)
            {
                RealmCardView card = realmCards[i];
                RealmDefinition realm = i < realms.Count ? realms[i] : null;
                if (card == null)
                {
                    continue;
                }

                card.Root.gameObject.SetActive(realm != null);
                if (realm == null)
                {
                    continue;
                }

                bool unlocked = IsUnlocked(realm);
                bool completed = IsCompleted(realm);
                bool selected = string.Equals(selectedRealmId, realm.id, StringComparison.OrdinalIgnoreCase);
                string progressText = BuildProgressText(realm);
                string lockReason = GetLockReason(realm);

                card.Realm = realm;
                card.NameText.text = realm.displayName;
                card.RankText.text = $"Rank: {realm.rank}";
                card.RequirementText.text = unlocked ? $"Requires Lv. {Mathf.Max(1, realm.requiredPlayerLevel)}" : lockReason;
                card.ProgressText.text = progressText;
                card.StateText.text = completed ? "Cleared" : (unlocked ? "Unlocked" : "Locked");
                card.StateText.color = completed ? new Color(0.85f, 1f, 0.82f, 1f) : (unlocked ? Color.white : new Color(0.9f, 0.9f, 0.9f, 0.9f));

                Color cardColor = completed ? completedCardColor : (unlocked ? normalCardColor : lockedCardColor);
                if (selected)
                {
                    cardColor = unlocked ? selectedCardColor : selectedLockedCardColor;
                }

                card.Background.color = cardColor;
                card.Button.interactable = true;
                card.Highlight.gameObject.SetActive(selected);
                card.CompletedBadge.gameObject.SetActive(completed);
            }

            UpdateDetailPanel(GetRealmById(selectedRealmId));
        }

        private void BuildHeader(RectTransform parent)
        {
            RectTransform header = CreateRect(parent, "Header");
            StretchTop(header, 150f);
            EnsureImage(header.gameObject, new Color(0.08f, 0.09f, 0.12f, 0.94f));

            Button backButton = CreateButton(header, "Button_Back", "Back", new Color(0.22f, 0.33f, 0.47f, 1f), new Vector2(120f, 75f), new Vector2(180f, 70f), () => screenManager?.ShowScreen(GameUIScreen.MainTown));
            SetAnchored(backButton.GetComponent<RectTransform>(), new Vector2(120f, -75f), new Vector2(180f, 70f), AnchorPoint.TopLeft);

            titleText = CreateText(header, "Title_Text", "World Map", 46, Color.white, new Vector2(0f, -70f), new Vector2(520f, 70f), AnchorPoint.TopCenter);

            Button settingsButton = CreateButton(header, "Button_Settings", "Settings", new Color(0.18f, 0.28f, 0.48f, 1f), new Vector2(120f, 75f), new Vector2(180f, 70f), () => screenManager?.OpenSettings());
            SetAnchored(settingsButton.GetComponent<RectTransform>(), new Vector2(-120f, -75f), new Vector2(180f, 70f), AnchorPoint.TopRight);
        }

        private void BuildRealmScrollView(RectTransform parent)
        {
            scrollRoot = CreateRect(parent, "RealmScrollView");
            SetAnchored(scrollRoot, new Vector2(0f, 175f), new Vector2(960f, 1240f), AnchorPoint.Center);
            EnsureImage(scrollRoot.gameObject, new Color(0.05f, 0.06f, 0.09f, 0.42f));

            scrollRect = scrollRoot.gameObject.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            }
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            RectTransform viewport = CreateRect(scrollRoot, "Viewport");
            Stretch(viewport);
            Image viewportImage = EnsureImage(viewport.gameObject, new Color(1f, 1f, 1f, 0.02f));
            viewportImage.raycastTarget = false;
            Mask mask = viewport.gameObject.GetComponent<Mask>();
            if (mask == null)
            {
                mask = viewport.gameObject.AddComponent<Mask>();
            }
            mask.showMaskGraphic = false;

            contentRoot = CreateRect(viewport, "Content");
            SetContentRect(contentRoot);
            VerticalLayoutGroup layout = contentRoot.gameObject.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = contentRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            }
            layout.spacing = 18f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = contentRoot.gameObject.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = contentRoot.gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = contentRoot;
            scrollRect.verticalNormalizedPosition = 1f;

            BuildRealmCards();
        }

        private void BuildRealmCards()
        {
            ClearChildren(contentRoot);
            realmCards.Clear();

            List<RealmDefinition> realms = GetRealms();
            for (int i = 0; i < realms.Count; i++)
            {
                RealmDefinition realm = realms[i];
                if (realm == null)
                {
                    continue;
                }

                RectTransform cardRoot = CreateRect(contentRoot, $"RealmCard_{(i + 1):00}");
                LayoutElement layout = cardRoot.gameObject.GetComponent<LayoutElement>();
                if (layout == null)
                {
                    layout = cardRoot.gameObject.AddComponent<LayoutElement>();
                }
                layout.preferredHeight = 120f;
                layout.minHeight = 120f;
                layout.flexibleWidth = 1f;

                Image background = EnsureImage(cardRoot.gameObject, normalCardColor);

                Button button = cardRoot.gameObject.GetComponent<Button>();
                if (button == null)
                {
                    button = cardRoot.gameObject.AddComponent<Button>();
                }
                button.transition = Selectable.Transition.None;

                RectTransform highlight = CreateRect(cardRoot, "SelectedHighlight");
                Stretch(highlight);
                Image highlightImage = EnsureImage(highlight.gameObject, new Color(1f, 1f, 1f, 0.18f));
                highlightImage.raycastTarget = false;
                highlight.gameObject.SetActive(false);

                RectTransform badge = CreateRect(cardRoot, "CompletedBadge");
                SetAnchored(badge, new Vector2(-22f, -22f), new Vector2(140f, 36f), AnchorPoint.TopRight);
                Image badgeImage = EnsureImage(badge.gameObject, new Color(0.1f, 0.4f, 0.18f, 0.95f));
                badgeImage.raycastTarget = false;
                _ = CreateText(badge, "Text", "Cleared", 22, Color.white, Vector2.zero, new Vector2(140f, 36f), AnchorPoint.Center);
                badge.gameObject.SetActive(false);

                CreateText(cardRoot, "RealmName_Text", realm.displayName, 34, Color.white, new Vector2(-24f, -18f), new Vector2(420f, 40f), AnchorPoint.TopLeft);
                CreateText(cardRoot, "RealmRank_Text", $"Rank: {realm.rank}", 24, new Color(0.95f, 0.92f, 0.82f, 1f), new Vector2(-24f, -56f), new Vector2(240f, 32f), AnchorPoint.TopLeft);
                CreateText(cardRoot, "RealmRequirement_Text", $"Requires Lv. {Mathf.Max(1, realm.requiredPlayerLevel)}", 22, new Color(0.92f, 0.92f, 0.92f, 1f), new Vector2(-24f, -82f), new Vector2(340f, 28f), AnchorPoint.TopLeft);
                CreateText(cardRoot, "RealmProgress_Text", BuildProgressText(realm), 22, new Color(0.92f, 0.92f, 0.92f, 1f), new Vector2(380f, -18f), new Vector2(240f, 36f), AnchorPoint.TopRight);
                TextMeshProUGUI stateText = CreateText(cardRoot, "RealmState_Text", "", 22, Color.white, new Vector2(380f, -54f), new Vector2(240f, 36f), AnchorPoint.TopRight);

                RealmCardView view = new RealmCardView
                {
                    Realm = realm,
                    Root = cardRoot,
                    Button = button,
                    Background = background,
                    Highlight = highlight,
                    CompletedBadge = badge,
                    NameText = cardRoot.Find("RealmName_Text")?.GetComponent<TextMeshProUGUI>(),
                    RankText = cardRoot.Find("RealmRank_Text")?.GetComponent<TextMeshProUGUI>(),
                    RequirementText = cardRoot.Find("RealmRequirement_Text")?.GetComponent<TextMeshProUGUI>(),
                    ProgressText = cardRoot.Find("RealmProgress_Text")?.GetComponent<TextMeshProUGUI>(),
                    StateText = stateText
                };

                RealmDefinition capturedRealm = realm;
                button.onClick.AddListener(() => SelectRealm(capturedRealm.id, true));
                realmCards.Add(view);
            }
        }

        private void BuildDetailPanel(RectTransform parent)
        {
            detailPanel = CreateRect(parent, "RealmDetailPanel");
            SetAnchored(detailPanel, new Vector2(0f, 210f), new Vector2(960f, 430f), AnchorPoint.BottomCenter);
            EnsureImage(detailPanel.gameObject, new Color(0.07f, 0.09f, 0.13f, 0.96f));

            realmTitleText = CreateText(detailPanel, "RealmTitle_Text", "Select a realm", 38, Color.white, new Vector2(-24f, -22f), new Vector2(560f, 42f), AnchorPoint.TopLeft);
            realmRankText = CreateText(detailPanel, "RealmRank_Text", string.Empty, 24, new Color(0.92f, 0.92f, 0.92f, 1f), new Vector2(-24f, -66f), new Vector2(360f, 30f), AnchorPoint.TopLeft);
            realmRequirementText = CreateText(detailPanel, "RealmRequirement_Text", string.Empty, 24, new Color(0.92f, 0.92f, 0.92f, 1f), new Vector2(-24f, -96f), new Vector2(460f, 30f), AnchorPoint.TopLeft);
            realmDescriptionText = CreateText(detailPanel, "RealmDescription_Text", "Select a realm", 24, Color.white, new Vector2(-24f, -138f), new Vector2(640f, 90f), AnchorPoint.TopLeft);
            realmProgressText = CreateText(detailPanel, "RealmProgress_Text", string.Empty, 22, new Color(0.95f, 0.95f, 0.95f, 1f), new Vector2(-24f, -230f), new Vector2(520f, 28f), AnchorPoint.TopLeft);
            enemyPreviewText = CreateText(detailPanel, "EnemyPreview_Text", string.Empty, 22, new Color(0.95f, 0.95f, 0.95f, 1f), new Vector2(-24f, -260f), new Vector2(640f, 30f), AnchorPoint.TopLeft);
            bossPreviewText = CreateText(detailPanel, "BossPreview_Text", string.Empty, 22, new Color(0.95f, 0.95f, 0.95f, 1f), new Vector2(-24f, -290f), new Vector2(640f, 30f), AnchorPoint.TopLeft);

            enterRealmButton = CreateButton(detailPanel, "Button_EnterRealm", "Enter Realm", new Color(0.18f, 0.76f, 0.82f, 1f), new Vector2(-24f, 20f), new Vector2(300f, 82f), EnterSelectedRealm);
            SetAnchored(enterRealmButton.GetComponent<RectTransform>(), new Vector2(-24f, 24f), new Vector2(300f, 82f), AnchorPoint.BottomRight);
            enterRealmButtonText = enterRealmButton.GetComponentInChildren<TextMeshProUGUI>();
        }

        private void BuildFooter(RectTransform parent)
        {
            footerHintText = CreateText(parent, "FooterHint_Text", "Tap a realm to view details.", 18, new Color(1f, 1f, 1f, 0.8f), new Vector2(0f, 18f), new Vector2(620f, 26f), AnchorPoint.BottomCenter);
        }

        private void SelectRealm(string realmId, bool showToastIfLocked)
        {
            RealmDefinition realm = GetRealmById(realmId);
            if (realm == null)
            {
                return;
            }

            selectedRealmId = realm.id;
            RefreshView();

            if (!IsUnlocked(realm) && showToastIfLocked)
            {
                screenManager?.ToastService?.ShowToast(GetLockReason(realm));
            }
        }

        private void EnterSelectedRealm()
        {
            RealmDefinition realm = GetRealmById(selectedRealmId);
            if (realm == null)
            {
                return;
            }

            if (!IsUnlocked(realm))
            {
                screenManager?.ToastService?.ShowToast(GetLockReason(realm));
                RefreshView();
                return;
            }

            if (adventureMapService != null && adventureMapService.EnterRealm(realm.id))
            {
                screenManager?.ShowScreen(GameUIScreen.RealmAdventureMap);
            }
        }

        private void UpdateDetailPanel(RealmDefinition realm)
        {
            if (realm == null)
            {
                SetPlaceholderState();
                return;
            }

            bool unlocked = IsUnlocked(realm);
            bool completed = IsCompleted(realm);
            RealmProgressData progress = realmProgressionService != null ? realmProgressionService.GetCurrentRealmProgress(realm.id) : null;

            realmTitleText.text = realm.displayName;
            realmRankText.text = $"Rank: {realm.rank}";
            realmRequirementText.text = unlocked ? $"Unlocked at Lv. {Mathf.Max(1, realm.requiredPlayerLevel)}" : GetLockReason(realm);
            realmDescriptionText.text = string.IsNullOrEmpty(realm.description) ? "No description available." : realm.description;
            realmProgressText.text = $"Progress: {(progress != null ? progress.normalMonstersDefeated : 0)}/3 normals{(progress != null && progress.bossDefeated ? " | Boss cleared" : string.Empty)}{(completed ? " | Cleared" : string.Empty)}";
            enemyPreviewText.text = $"Monsters: {BuildMonsterNames(realm)}";
            bossPreviewText.text = $"Boss: {(realm.bossEnemy != null ? realm.bossEnemy.displayName : "Unknown")}";

            if (enterRealmButton != null)
            {
                enterRealmButton.interactable = unlocked;
            }

            if (enterRealmButtonText != null)
            {
                enterRealmButtonText.text = unlocked ? "Enter Realm" : "Locked";
            }
        }

        private void SetPlaceholderState()
        {
            if (realmTitleText != null) realmTitleText.text = "Select a realm";
            if (realmRankText != null) realmRankText.text = string.Empty;
            if (realmRequirementText != null) realmRequirementText.text = string.Empty;
            if (realmDescriptionText != null) realmDescriptionText.text = "Choose one of the 12 realms to view details.";
            if (realmProgressText != null) realmProgressText.text = string.Empty;
            if (enemyPreviewText != null) enemyPreviewText.text = string.Empty;
            if (bossPreviewText != null) bossPreviewText.text = string.Empty;
            if (enterRealmButton != null) enterRealmButton.interactable = false;
            if (enterRealmButtonText != null) enterRealmButtonText.text = "Locked";
            if (footerHintText != null) footerHintText.text = "No realms found.";
        }

        private List<RealmDefinition> GetRealms()
        {
            List<RealmDefinition> realms = contentService != null ? contentService.Realms : new List<RealmDefinition>();
            List<RealmDefinition> filtered = new List<RealmDefinition>();
            for (int i = 0; i < realms.Count; i++)
            {
                if (realms[i] != null)
                {
                    filtered.Add(realms[i]);
                }
            }

            filtered.Sort((a, b) =>
            {
                int orderCompare = a.order.CompareTo(b.order);
                return orderCompare != 0 ? orderCompare : string.Compare(a.id, b.id, StringComparison.Ordinal);
            });

            if (filtered.Count > 12)
            {
                filtered.RemoveRange(12, filtered.Count - 12);
            }

            return filtered;
        }

        private RealmDefinition GetRealmById(string realmId)
        {
            if (string.IsNullOrEmpty(realmId))
            {
                return null;
            }

            List<RealmDefinition> realms = GetRealms();
            for (int i = 0; i < realms.Count; i++)
            {
                if (string.Equals(realms[i].id, realmId, StringComparison.OrdinalIgnoreCase))
                {
                    return realms[i];
                }
            }

            return null;
        }

        private string DefaultRealmId()
        {
            List<RealmDefinition> realms = GetRealms();
            if (realms.Count == 0)
            {
                return string.Empty;
            }

            for (int i = 0; i < realms.Count; i++)
            {
                if (realms[i] != null && (realms[i].unlockedByDefault || realms[i].order <= 1 || string.Equals(realms[i].id, "realm_01_meadow", StringComparison.OrdinalIgnoreCase)))
                {
                    return realms[i].id;
                }
            }

            return realms[0].id;
        }

        private bool IsUnlocked(RealmDefinition realm)
        {
            return realm != null && (realmProgressionService == null || realmProgressionService.IsRealmUnlocked(realm));
        }

        private bool IsCompleted(RealmDefinition realm)
        {
            return realm != null && realmProgressionService != null && realmProgressionService.IsRealmCompleted(realm.id);
        }

        private string GetLockReason(RealmDefinition realm)
        {
            return realmProgressionService != null ? realmProgressionService.GetRealmLockReason(realm) : "Requires Lv. 1";
        }

        private string BuildProgressText(RealmDefinition realm)
        {
            RealmProgressData progress = realmProgressionService != null ? realmProgressionService.GetCurrentRealmProgress(realm.id) : null;
            int cleared = progress != null ? progress.normalMonstersDefeated : 0;
            return $"Progress: {cleared}/3{(progress != null && progress.bossDefeated ? " | Boss cleared" : string.Empty)}";
        }

        private static string BuildMonsterNames(RealmDefinition realm)
        {
            if (realm == null || realm.normalEnemies == null || realm.normalEnemies.Count == 0)
            {
                return "None";
            }

            List<string> names = new List<string>();
            for (int i = 0; i < realm.normalEnemies.Count && i < 3; i++)
            {
                if (realm.normalEnemies[i] != null)
                {
                    names.Add(realm.normalEnemies[i].displayName);
                }
            }

            return names.Count > 0 ? string.Join(", ", names.ToArray()) : "None";
        }

        private void ClearChildren(Transform parent)
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
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static RectTransform CreateRect(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Background(RectTransform parent)
        {
            RectTransform background = CreateRect(parent, "Background");
            Stretch(background);
            Image image = EnsureImage(background.gameObject, new Color(0.05f, 0.07f, 0.1f, 0.02f));
            image.raycastTarget = false;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        private static void StretchTop(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0.03f, 1f);
            rect.anchorMax = new Vector2(0.97f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, -height);
            rect.offsetMax = Vector2.zero;
        }

        private static void SetContentRect(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, 0f);
        }

        private static void SetAnchored(RectTransform rect, Vector2 pos, Vector2 size, AnchorPoint anchor)
        {
            switch (anchor)
            {
                case AnchorPoint.TopLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    rect.pivot = new Vector2(0f, 1f);
                    break;
                case AnchorPoint.TopRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
                    rect.pivot = new Vector2(1f, 1f);
                    break;
                case AnchorPoint.BottomLeft:
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
                    rect.pivot = new Vector2(0f, 0f);
                    break;
                case AnchorPoint.BottomRight:
                    rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
                    rect.pivot = new Vector2(1f, 0f);
                    break;
                case AnchorPoint.TopCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    break;
                case AnchorPoint.BottomCenter:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);
                    break;
                default:
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    break;
            }

            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
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

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, Color color, Vector2 pos, Vector2 rectSize, AnchorPoint anchor)
        {
            RectTransform rect = CreateRect(parent, name);
            SetAnchored(rect, pos, rectSize, anchor);
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

        private static Button CreateButton(Transform parent, string name, string text, Color color, Vector2 pos, Vector2 rectSize, UnityEngine.Events.UnityAction action)
        {
            RectTransform rect = CreateRect(parent, name);
            SetAnchored(rect, pos, rectSize, AnchorPoint.Center);
            Image image = EnsureImage(rect.gameObject, color);
            image.raycastTarget = true;

            Button button = rect.gameObject.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }

            TextMeshProUGUI label = CreateText(rect, "Text", text, Mathf.RoundToInt(rectSize.y * 0.34f), Color.white, Vector2.zero, rectSize, AnchorPoint.Center);
            label.margin = new Vector4(12f, 0f, 12f, 0f);
            return button;
        }

        private sealed class RealmCardView
        {
            public RealmDefinition Realm;
            public RectTransform Root;
            public Button Button;
            public Image Background;
            public RectTransform Highlight;
            public RectTransform CompletedBadge;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI RankText;
            public TextMeshProUGUI RequirementText;
            public TextMeshProUGUI ProgressText;
            public TextMeshProUGUI StateText;
        }

        private enum AnchorPoint
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
