using Isekai12Realms.Board;
using Isekai12Realms.Character;
using Isekai12Realms.CloudSave;
using Isekai12Realms.Core;
using Isekai12Realms.Data;
using Isekai12Realms.Audio;
using Isekai12Realms.Adventure;
using Isekai12Realms.Equipment;
using Isekai12Realms.DropTables;
using Isekai12Realms.Inventory;
using Isekai12Realms.Quests;
using Isekai12Realms.Performance;
using Isekai12Realms.RemoteConfig;
using Isekai12Realms.Realms;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using Isekai12Realms.Tutorial;
using Isekai12Realms.UI;
using Isekai12Realms.VFX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;

namespace Isekai12Realms.Battle
{
    public class BattleUIController : MonoBehaviour
    {
        private readonly BattleService battleService = new BattleService();

        private UIScreenManager screenManager;
        private PlayerProgressionService progressionService;
        private BoardController boardController;
        private TextMeshProUGUI enemyNameText;
        private TextMeshProUGUI playerNameText;
        private TextMeshProUGUI foodText;
        private TextMeshProUGUI comboText;
        private TextMeshProUGUI turnText;
        private TextMeshProUGUI timerText;
        private TextMeshProUGUI goldRewardText;
        private TextMeshProUGUI expRewardText;
        private TextMeshProUGUI debugText;
        private GameObject debugPanel;
        private Image enemyHpFill;
        private Image playerHpFill;
        private Image playerManaFill;
        private Image enemySpriteImage;
        private Image playerSpriteImage;
        private Image battleBackgroundImage;
        private BattleCharacterView enemyView;
        private BattleCharacterView playerView;
        private FloatingTextService floatingText;
        private VFXService vfxService;
        private AudioService audioService;
        private SkillService skillService;
        private EquipmentService equipmentService;
        private QuestService questService;
        private TutorialService tutorialService;
        private CloudSaveCoordinator cloudSaveCoordinator;
        private AdventureMapService adventureMapService;
        private RealmProgressionService realmProgressionService;
        private BattleAnimationSettings animationSettings;
        private Button skill1Button;
        private Button skill2Button;
        private Button ultimateButton;
        private bool rewardGranted;
        private bool enemyTurnRoutineRunning;
        private bool resultPopupOpening;
        private bool suppressMinorMatchVfx;
        private int previousEnemyHp = -1;
        private int previousPlayerHp = -1;
        private int previousMana = -1;
        private int previousFood = -1;
        private int previousCombo = -1;
        private StageDefinition selectedStage;
        private BattleEncounterData selectedEncounter;
        private readonly RealmRewardService rewardService = new RealmRewardService();
        private readonly EnemyBoardAI enemyBoardAI = new EnemyBoardAI();

        public void SetStage(StageDefinition stage)
        {
            selectedStage = stage;
            selectedEncounter = null;
        }

        public void SetEncounter(BattleEncounterData encounter)
        {
            selectedEncounter = encounter;
            selectedStage = null;
        }

        public void Initialize(UIScreenManager manager, BoardController board)
        {
            screenManager = manager;
            progressionService = FindObjectOfType<PlayerProgressionService>();
            boardController = board;
            floatingText = FindObjectOfType<FloatingTextService>();
            vfxService = FindObjectOfType<VFXService>();
            audioService = FindObjectOfType<AudioService>();
            skillService = FindObjectOfType<SkillService>();
            equipmentService = FindObjectOfType<EquipmentService>();
            questService = FindObjectOfType<QuestService>();
            tutorialService = FindObjectOfType<TutorialService>();
            cloudSaveCoordinator = FindObjectOfType<CloudSaveCoordinator>();
            adventureMapService = FindObjectOfType<AdventureMapService>();
            realmProgressionService = FindObjectOfType<RealmProgressionService>();
            battleService.SetSkillService(skillService);
            animationSettings = BattleAnimationSettings.CreateDefault();
            CacheReferences();

            battleService.StateChanged -= UpdateView;
            battleService.StateChanged += UpdateView;
            battleService.BattleEnded -= OnBattleEnded;
            battleService.BattleEnded += OnBattleEnded;
            battleService.SkillResolved -= OnSkillResolved;
            battleService.SkillResolved += OnSkillResolved;

            if (boardController != null)
            {
                boardController.MoveResolved -= OnBoardMoveResolved;
                boardController.MoveResolved += OnBoardMoveResolved;
                boardController.CascadeResolved -= OnCascadeResolved;
                boardController.CascadeResolved += OnCascadeResolved;
                boardController.BoardFeedback -= OnBoardFeedback;
                boardController.BoardFeedback += OnBoardFeedback;
            }
        }

        private void OnEnable()
        {
            if (boardController != null)
            {
                StartBattle();
            }
        }

        private void OnDestroy()
        {
            battleService.StateChanged -= UpdateView;
            battleService.BattleEnded -= OnBattleEnded;
            battleService.SkillResolved -= OnSkillResolved;
            if (boardController != null)
            {
                boardController.MoveResolved -= OnBoardMoveResolved;
                boardController.CascadeResolved -= OnCascadeResolved;
                boardController.BoardFeedback -= OnBoardFeedback;
            }
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (DebugTogglePressed() && debugPanel != null)
            {
                debugPanel.SetActive(!debugPanel.activeSelf);
            }

            if (debugPanel != null && debugPanel.activeSelf && debugText != null)
            {
                BattleState state = battleService.State;
                debugText.text = $"Player HP: {state.hp}/{state.maxHp}\nEnemy HP: {state.enemyHp}/{state.enemyMaxHp}\nMana: {state.mana}/{state.maxMana}\nFood: {state.food}\nTurn: {state.currentTurnOwner}\nTime: {Mathf.CeilToInt(Mathf.Max(0f, state.remainingTurnTime))}s\nResolving: {(boardController != null && boardController.IsResolving)}\nTurn Resolving: {state.isResolvingTurn}\nLast Player Move: {state.lastPlayerMove}\nLast Enemy Move: {state.lastEnemyMove}\nLast Extra: {state.lastMoveGrantedExtraTurn}\nLast Max Match: {state.lastMaxMatchSize}\nEncounter: {(state.encounter != null ? state.encounter.encounterId : selectedStage != null ? selectedStage.id : "fallback")}";
            }
#endif

            bool timerPaused = boardController == null || boardController.IsResolving || enemyTurnRoutineRunning || resultPopupOpening;
            bool expired = battleService.TickTurnTimer(Time.deltaTime, timerPaused);
            if (expired)
            {
                if (battleService.State.currentTurnOwner == BattleTurnOwner.Player)
                {
                    battleService.SkipCurrentTurn();
                }
                else if (!enemyTurnRoutineRunning)
                {
                    StartCoroutine(EnemyTurnFeedbackRoutine());
                }
            }
        }

        private static bool DebugTogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.f3Key.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.F3);
#else
            return false;
#endif
        }

        public void StartBattle()
        {
            EnsureQuestServices();
            if (boardController == null)
            {
                CacheReferences();
            }
            if (skillService == null)
            {
                skillService = FindObjectOfType<SkillService>();
                battleService.SetSkillService(skillService);
            }

            if (selectedEncounter != null)
            {
                battleService.StartBattle(boardController, selectedEncounter);
            }
            else
            {
                battleService.StartBattle(boardController, selectedStage);
            }
            tutorialService?.HandleBattleStarted();
            if (progressionService != null)
            {
                battleService.SetPlayerStats(progressionService.CalculateTotalStats());
            }
            rewardGranted = false;
            enemyTurnRoutineRunning = false;
            resultPopupOpening = false;
            previousEnemyHp = previousPlayerHp = previousMana = previousFood = previousCombo = -1;
            UpdateView();
        }

        public void BackToWorldMap()
        {
            battleService.EndBattle();
            adventureMapService?.OnEncounterDefeat();
            screenManager?.ShowScreen(GameUIScreen.RealmAdventureMap);
        }

        public void UseSkill1()
        {
            TryUseSkill(SkillSlotType.Skill1);
        }

        public void UseSkill2()
        {
            TryUseSkill(SkillSlotType.Skill2);
        }

        public void UseUltimate()
        {
            TryUseSkill(SkillSlotType.Ultimate);
        }

        public void WinTest()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GrantVictoryRewards();
#endif
        }

        public void LoseTest()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StartCoroutine(OpenResultAfterDelay(false, 0, 0, string.Empty, false));
#endif
        }

        private void OnBattleEnded(BattleResultType result)
        {
            if (result == BattleResultType.Victory)
            {
                GrantVictoryRewards();
                adventureMapService?.OnEncounterVictory();
            }
            else if (result == BattleResultType.Defeat)
            {
                audioService?.PlaySfx("sfx_defeat");
                playerView?.PlayDefeat();
                adventureMapService?.OnEncounterDefeat();
                StartCoroutine(OpenResultAfterDelay(false, 0, 0, string.Empty, false));
            }
        }

        private void GrantVictoryRewards()
        {
            if (rewardGranted)
            {
                return;
            }

            rewardGranted = true;
            EnsureQuestServices();
            if (progressionService == null)
            {
                progressionService = FindObjectOfType<PlayerProgressionService>();
            }

            BattleEncounterData encounter = GetCurrentEncounter();
            bool firstClearBoss = encounter != null && encounter.isBoss && realmProgressionService != null && !realmProgressionService.IsRealmCompleted(encounter.realmId);
            int exp = battleService.State.expReward > 0 ? battleService.State.expReward : 50;
            int gold = battleService.State.goldReward > 0 ? battleService.State.goldReward : 30;
            if (ServiceLocator.TryResolve<GameConfigService>(out GameConfigService config))
            {
                exp = Mathf.FloorToInt(exp * config.ExpRewardMultiplier);
                gold = Mathf.FloorToInt(gold * config.GoldRewardMultiplier);
            }
            string drops = "Drops:";
            bool leveledUp = false;

            if (progressionService != null)
            {
                progressionService.AddGold(gold);
                leveledUp = progressionService.AddExp(exp);
                drops += RollDrops(encounter);
                if (firstClearBoss && encounter != null && encounter.realm != null)
                {
                    int soulGemBonus = rewardService.GetSoulGemFirstClearBonus(encounter.realm.order);
                    progressionService.AddSoulGem(soulGemBonus);
                }
                questService?.TrackProgress(QuestObjectiveType.WinBattle, "any", 1);
                questService?.TrackProgress(QuestObjectiveType.EarnGold, "any", gold);
                if (leveledUp) questService?.TrackProgress(QuestObjectiveType.ReachLevel, "any", progressionService.CurrentSave.level);
                if (encounter != null)
                {
                    questService?.TrackProgress(QuestObjectiveType.DefeatEnemy, encounter.enemyId, 1);
                }
                else if (selectedStage != null && selectedStage.enemy != null)
                {
                    string stageId = selectedStage.id;
                    progressionService.MarkStageCompleted(stageId);
                    questService?.TrackProgress(QuestObjectiveType.CompleteStage, stageId, 1);
                    questService?.TrackProgress(QuestObjectiveType.DefeatEnemy, selectedStage.enemy.id, 1);
                    StageProgressionService stageProgression = FindObjectOfType<StageProgressionService>();
                    stageProgression?.IncrementStageClearCount(stageId);
                }
                cloudSaveCoordinator?.QueueCloudSync(1f);
            }

            string stageName = encounter != null ? encounter.displayName : selectedStage != null ? selectedStage.displayName : "First Slime";
            string enemyName = encounter != null && encounter.enemy != null ? encounter.enemy.displayName : selectedStage != null && selectedStage.enemy != null ? selectedStage.enemy.displayName : battleService.State.enemyName;
            audioService?.PlaySfx("sfx_victory");
            playerView?.PlayVictory();
            StartCoroutine(OpenResultAfterDelay(true, exp, gold, $"Realm: {stageName}\nEnemy defeated: {enemyName}\n{drops}", leveledUp));
        }

        private string RollDrops(BattleEncounterData encounter)
        {
            List<DropRollResult> drops = rewardService.RollDrops(encounter, battleService.State.luck * 0.005f, battleService.State.dropRateBonus);
            if (drops == null || drops.Count == 0)
            {
                return "\nNone";
            }

            string text = string.Empty;
            foreach (DropRollResult drop in drops)
            {
                if (drop.isEquipment)
                {
                    if (equipmentService == null) equipmentService = FindObjectOfType<EquipmentService>();
                    EquipmentInstanceData equipment = equipmentService != null ? equipmentService.CreateEquipmentInstance(drop.itemId) : PrototypeEquipmentFactory.Create(drop.itemId);
                    progressionService.AddEquipment(equipment);
                    questService?.TrackProgress(QuestObjectiveType.OwnEquipment, equipment.equipmentId, 1);
                    text += $"\n{equipment.displayName}";
                }
                else
                {
                    progressionService.AddItem(drop.itemId, drop.amount);
                    questService?.TrackProgress(QuestObjectiveType.CollectItem, drop.itemId, drop.amount);
                    string itemName = PrototypeItemDatabase.Get(drop.itemId) != null ? PrototypeItemDatabase.Get(drop.itemId).displayName : drop.itemId;
                    text += $"\n{itemName} x{drop.amount}";
                }
            }

            return string.IsNullOrEmpty(text) ? "\nNone" : text;
        }

        private void UpdateView()
        {
            BattleState state = battleService.State;
            SetText(enemyNameText, $"{state.enemyName} Lv. {state.enemyLevel}");
            SetText(playerNameText, state.playerName);
            SetText(foodText, $"Food: {state.food}");
            SetText(comboText, $"Combo: {state.comboCount}");
            SetText(turnText, state.currentTurnOwner == BattleTurnOwner.Player ? "Your Turn" : "Enemy Turn");
            SetText(timerText, $"{Mathf.CeilToInt(Mathf.Max(0f, state.remainingTurnTime))}s");
            SetText(goldRewardText, $"Gold: {state.goldReward}");
            SetText(expRewardText, $"EXP: {state.expReward}");
            if (turnText != null)
            {
                turnText.color = state.currentTurnOwner == BattleTurnOwner.Player ? new Color(0.48f, 1f, 0.58f, 1f) : new Color(1f, 0.72f, 0.3f, 1f);
            }
            if (timerText != null)
            {
                bool warning = state.remainingTurnTime <= 5f;
                timerText.color = warning ? Color.Lerp(Color.white, new Color(1f, 0.25f, 0.15f, 1f), Mathf.PingPong(Time.time * 5f, 1f)) : Color.white;
                timerText.transform.localScale = warning ? Vector3.one * Mathf.Lerp(1f, 1.12f, Mathf.PingPong(Time.time * 5f, 1f)) : Vector3.one;
            }

            playerView?.SetName(state.playerName);
            playerView?.SetLevel(state.playerLevel);
            playerView?.SetHp(state.hp, state.maxHp);
            playerView?.SetMana(state.mana, state.maxMana);
            playerView?.SetShield(state.shield);
            playerView?.SetSprite(ClassIdleSpriteAssetId(progressionService != null && progressionService.CurrentSave != null ? progressionService.CurrentSave.selectedClassId : string.Empty));
            playerView?.PlayIdle();
            enemyView?.SetName($"{state.enemyName} Lv. {state.enemyLevel}");
            enemyView?.SetLevel(state.enemyLevel);
            enemyView?.SetHp(state.enemyHp, state.enemyMaxHp);
            enemyView?.SetShield(state.enemyShield);
            string enemySpriteId = state.encounter != null && state.encounter.enemy != null && !string.IsNullOrEmpty(state.encounter.enemy.spriteAssetId)
                ? state.encounter.enemy.spriteAssetId
                : selectedStage != null && selectedStage.enemy != null && !string.IsNullOrEmpty(selectedStage.enemy.spriteAssetId)
                    ? selectedStage.enemy.spriteAssetId
                    : "enemy_meadow_slime";
            enemyView?.SetSprite(enemySpriteId);
            string backgroundAssetId = state.encounter != null && !string.IsNullOrEmpty(state.encounter.battleBackgroundAssetId)
                ? state.encounter.battleBackgroundAssetId
                : selectedStage != null && !string.IsNullOrEmpty(selectedStage.battleBackgroundAssetId)
                    ? selectedStage.battleBackgroundAssetId
                    : "bg_battle_meadow";
            SetBattleBackground(backgroundAssetId);
            enemyView?.PlayIdle();
            boardController?.SetInputLocked(state.currentTurnOwner != BattleTurnOwner.Player || state.inputLocked || state.isResolvingTurn || state.battleResult != BattleResultType.None);
            ShowStateDeltaFeedback(state);

            if (state.food <= 5 && foodText != null)
            {
                foodText.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.2f, 1f), Mathf.PingPong(Time.time * 3f, 1f));
            }
            else if (foodText != null)
            {
                foodText.color = Color.white;
            }

            if (state.currentTurnOwner == BattleTurnOwner.Enemy && !enemyTurnRoutineRunning && state.battleResult == BattleResultType.None)
            {
                StartCoroutine(EnemyTurnFeedbackRoutine());
            }

            UpdateSkillButton(skill1Button, SkillSlotType.Skill1, state);
            UpdateSkillButton(skill2Button, SkillSlotType.Skill2, state);
            UpdateSkillButton(ultimateButton, SkillSlotType.Ultimate, state);
        }

        private void TryUseSkill(SkillSlotType slot)
        {
            EnsureQuestServices();
            if (battleService.State.currentTurnOwner != BattleTurnOwner.Player)
            {
                screenManager?.ToastService?.ShowToast("Wait for your turn.");
                return;
            }
            SkillDefinition skill = skillService?.GetEquippedSkill(slot);
            if (skill == null || !battleService.UseEquippedSkill(slot))
            {
                screenManager?.ToastService?.ShowToast(skill != null ? $"{skill.displayName} is not ready." : "No skill equipped.");
                return;
            }

            questService?.TrackProgress(QuestObjectiveType.UseSkill, skill.id, 1);
        }

        private void OnSkillResolved(SkillDefinition skill, SkillResolveResult result)
        {
            audioService?.PlaySfx(skill.slotType == SkillSlotType.Ultimate ? "sfx_ultimate" : "sfx_skill");
            playerView?.PlayCast();
            vfxService?.PlaySkillVfx(skill.id, skill.targetType == SkillTargetType.Board ? BoardPosition() : EnemyPosition());
            if (skill.slotType == SkillSlotType.Ultimate)
            {
                vfxService?.PlayFullScreenFlash(new Color(1f, 0.82f, 0.2f, 0.45f), animationSettings.skillFlashDuration);
            }
            if (skill.slotType == SkillSlotType.Skill2 && result.boardShuffled)
            {
                vfxService?.PlayFullScreenFlash(new Color(0f, 0f, 0f, 0.18f), 0.12f);
            }
            if (result.damageDealt > 0) { enemyView?.PlayHurt(); floatingText?.Show("-" + result.damageDealt, EnemyPosition(), new Color(1f, 0.25f, 0.18f, 1f), 44); }
            if (result.healingDone > 0) floatingText?.Show("+" + result.healingDone, PlayerPosition(), new Color(0.35f, 1f, 0.45f, 1f), 40);
            if (result.shieldGained > 0) floatingText?.Show("+" + result.shieldGained + " Shield", PlayerPosition(), new Color(0.55f, 0.75f, 1f, 1f), 38);
            if (result.manaGained > 0) floatingText?.Show("+" + result.manaGained + " Mana", PlayerPosition(), new Color(0.45f, 0.62f, 1f, 1f), 38);
            if (result.boardShuffled) { boardController?.PulseAllTiles(); floatingText?.Show("Board Shuffled", BoardPosition(), new Color(0.65f, 0.85f, 1f, 1f), 40); }
            if (result.tilesDestroyed > 0) floatingText?.Show("Tiles -" + result.tilesDestroyed, BoardPosition(), new Color(1f, 0.82f, 0.25f, 1f), 40);
            if (result.extraTurnGranted) floatingText?.Show("Extra Turn!", BoardPosition(), new Color(1f, 0.86f, 0.28f, 1f), 42);
            floatingText?.Show(skill.displayName, BoardPosition(), new Color(1f, 0.82f, 0.2f, 1f), skill.slotType == SkillSlotType.Ultimate ? 52 : 40);
        }

        private void UpdateSkillButton(Button button, SkillSlotType slot, BattleState state)
        {
            SkillDefinition skill = skillService?.GetEquippedSkill(slot);
            if (button == null || skill == null)
            {
                SetButtonEnabled(button, false);
                return;
            }
            PlayerSkillData playerSkill = skillService.GetPlayerSkill(skill.id);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                int cooldown = playerSkill != null ? playerSkill.cooldownRemaining : 0;
                label.text = cooldown > 0 ? $"{skill.displayName}\nCD {cooldown}" : $"{skill.displayName}\n{skillService.GetManaCost(skill.id)} MP";
                label.margin = new Vector4(42f, 0f, 0f, 0f);
            }
            SetSkillIcon(button, skill);
            SetButtonEnabled(button, skillService.IsSkillUsable(skill.id, state) && boardController != null && !boardController.IsResolving && !boardController.InputLocked);
        }

        private static void SetSkillIcon(Button button, SkillDefinition skill)
        {
            if (button == null || skill == null) return;
            Transform iconTransform = button.transform.Find("Icon");
            RectTransform iconRect;
            if (iconTransform == null)
            {
                GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconObject.transform.SetParent(button.transform, false);
                iconRect = iconObject.GetComponent<RectTransform>();
            }
            else
            {
                iconRect = iconTransform.GetComponent<RectTransform>();
            }

            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(34f, 0f);
            iconRect.sizeDelta = new Vector2(46f, 46f);
            Image icon = iconRect.GetComponent<Image>();
            icon.raycastTarget = false;
            Sprite sprite = AssetSpriteBinder.GetSprite(skill.iconAssetId);
            icon.sprite = sprite;
            icon.enabled = sprite != null;
            icon.preserveAspect = true;
            icon.color = Color.white;
        }

        private void CacheReferences()
        {
            boardController = boardController != null ? boardController : GetComponentInChildren<BoardController>(true);
            enemyNameText = FindText("EnemyName");
            playerNameText = FindText("PlayerName");
            foodText = FindText("FoodText");
            comboText = FindText("ComboText");
            turnText = FindText("TurnText");
            timerText = FindText("TimerText");
            if (timerText == null)
            {
                timerText = CreateTimerText();
            }
            goldRewardText = FindText("GoldRewardText");
            expRewardText = FindText("ExpRewardText");
            enemyHpFill = FindImage("EnemyHp/Fill");
            playerHpFill = FindImage("PlayerHp/Fill");
            playerManaFill = FindImage("PlayerMana/Fill");
            enemySpriteImage = FindImage("EnemySprite");
            playerSpriteImage = FindImage("PlayerSprite");
            battleBackgroundImage = GetComponent<Image>();
            Transform debugTransform = transform.Find("BattleDebugPanel");
            debugPanel = debugTransform != null ? debugTransform.gameObject : null;
            debugText = debugTransform != null ? debugTransform.Find("DebugText")?.GetComponent<TextMeshProUGUI>() : null;
            skill1Button = FindButton("Button_Skill1");
            skill2Button = FindButton("Button_Skill2");
            ultimateButton = FindButton("Button_Ultimate");
            enemyView = enemySpriteImage != null ? enemySpriteImage.GetComponent<BattleCharacterView>() : null;
            if (enemyView == null && enemySpriteImage != null) enemyView = enemySpriteImage.gameObject.AddComponent<BattleCharacterView>();
            playerView = playerSpriteImage != null ? playerSpriteImage.GetComponent<BattleCharacterView>() : null;
            if (playerView == null && playerSpriteImage != null) playerView = playerSpriteImage.gameObject.AddComponent<BattleCharacterView>();
            enemyView?.Bind(enemyNameText, enemyHpFill, null, enemySpriteImage);
            playerView?.Bind(playerNameText, playerHpFill, playerManaFill, playerSpriteImage);
        }

        private TextMeshProUGUI FindText(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<TextMeshProUGUI>() : null;
        }

        private Image FindImage(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<Image>() : null;
        }

        private Button FindButton(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private TextMeshProUGUI CreateTimerText()
        {
            GameObject timerObject = new GameObject("TimerText", typeof(RectTransform));
            timerObject.transform.SetParent(transform, false);
            RectTransform rect = timerObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -285f);
            rect.sizeDelta = new Vector2(160f, 54f);
            TextMeshProUGUI text = timerObject.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = 34;
            text.color = Color.white;
            return text;
        }

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null) text.text = value;
        }

        private static void SetFill(Image fill, int value, int max)
        {
            if (fill == null || max <= 0) return;
            fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)value / max), 1f);
        }

        private static void SetSprite(Image image, string assetId)
        {
            if (image == null) return;
            Sprite sprite = AssetSpriteBinder.GetSprite(assetId);
            if (sprite == null) return;
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = Color.white;
        }

        private void SetBattleBackground(string assetId)
        {
            if (battleBackgroundImage == null) battleBackgroundImage = GetComponent<Image>();
            SetSprite(battleBackgroundImage, assetId);
        }

        private static string ClassIdleSpriteAssetId(string classId)
        {
            if (classId == "tide_acolyte") return "char_hero_tide_idle";
            if (classId == "storm_scout") return "char_hero_storm_idle";
            return "char_hero_flame_idle";
        }

        private IEnumerator EnemyTurnFeedbackRoutine()
        {
            enemyTurnRoutineRunning = true;

            while (battleService.State.currentTurnOwner == BattleTurnOwner.Enemy && battleService.State.battleResult == BattleResultType.None)
            {
                SetText(turnText, "Enemy Turn");
                battleService.MarkResolvingTurn(true);
                while (boardController != null && boardController.IsResolving)
                {
                    yield return null;
                }
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));

                EnemyMoveChoice choice = enemyBoardAI.ChooseMove(boardController != null ? boardController.GetBoardSnapshot() : null, battleService.State);
                if (choice == null && boardController != null)
                {
                    boardController.ShuffleBoard();
                    yield return new WaitForSeconds(0.25f);
                    choice = enemyBoardAI.ChooseMove(boardController.GetBoardSnapshot(), battleService.State);
                }

                if (choice == null)
                {
                    battleService.MarkResolvingTurn(false);
                    battleService.SkipCurrentTurn();
                    break;
                }

                battleService.State.lastEnemyMove = $"{choice.from}->{choice.to}";
                enemyView?.PlayAttack();
                yield return boardController.ExecuteMove(choice.from, choice.to, BattleTurnOwner.Enemy);
                BoardResolveResult result = boardController.LastResolveResult;
                battleService.ApplyMoveResult(result, BattleTurnOwner.Enemy);
                ShowEnemyResultFeedback(battleService.State);
                yield return new WaitForSeconds(0.2f);
            }

            enemyTurnRoutineRunning = false;
            if (battleService.State.currentTurnOwner == BattleTurnOwner.Enemy && battleService.State.battleResult == BattleResultType.None)
            {
                StartCoroutine(EnemyTurnFeedbackRoutine());
            }
        }

        private void ShowEnemyResultFeedback(BattleState state)
        {
            if (state.lastEnemyAction == "damage")
            {
                playerView?.PlayHurt();
                vfxService?.PlayDamageVfx(PlayerPosition());
                floatingText?.Show("-" + state.lastEnemyActionValue, PlayerPosition(), new Color(1f, 0.25f, 0.18f, 1f), 42);
                audioService?.PlaySfx("sfx_damage");
            }
            else if (state.lastEnemyAction == "heal")
            {
                enemyView?.PlayCast();
                vfxService?.PlayHealVfx(EnemyPosition());
                floatingText?.Show("+" + state.lastEnemyActionValue, EnemyPosition(), new Color(0.35f, 1f, 0.45f, 1f), 38);
            }
            else if (state.lastEnemyAction == "shield")
            {
                enemyView?.PlayCast();
                vfxService?.PlayShieldVfx(EnemyPosition());
                floatingText?.Show("+" + state.lastEnemyActionValue + " Shield", EnemyPosition(), new Color(0.55f, 0.75f, 1f, 1f), 36);
            }
        }

        private void OnBoardMoveResolved(BoardResolveResult result, BattleTurnOwner owner)
        {
            if (owner != BattleTurnOwner.Player || battleService.State.currentTurnOwner != BattleTurnOwner.Player)
            {
                return;
            }

            battleService.State.lastPlayerMove = result != null ? $"groups={result.allMatchGroups.Count}, max={result.maxMatchSize}" : string.Empty;
            battleService.ApplyMoveResult(result, BattleTurnOwner.Player);
        }

        private void OnCascadeResolved(List<MatchGroup> groups, int combo)
        {
            EnsureQuestServices();
            if (combo >= 2)
            {
                floatingText?.Show("Combo x" + combo, BoardPosition(), new Color(1f, 0.86f, 0.28f, 1f), 44);
            }

            foreach (MatchGroup group in groups)
            {
                if (battleService.State.currentTurnOwner == BattleTurnOwner.Player)
                {
                    string tileTarget = group.tileType.ToString().ToLowerInvariant();
                    questService?.TrackProgress(QuestObjectiveType.MatchTokenCount, tileTarget, group.count);
                    tutorialService?.HandleTileMatched(group.tileType);
                }
                suppressMinorMatchVfx = ShouldSuppressMinorMatchVfx(combo);
                ShowMatchFeedback(group, battleService.State.currentTurnOwner);
                suppressMinorMatchVfx = false;
            }
        }

        private static bool ShouldSuppressMinorMatchVfx(int combo)
        {
            return combo > 3 && ServiceLocator.TryResolve<PerformanceService>(out PerformanceService performance) && performance.ReduceEffects;
        }

        private void OnBoardFeedback(string text)
        {
            floatingText?.Show(text, BoardPosition(), new Color(1f, 0.86f, 0.28f, 1f), 42);
        }

        private void EnsureQuestServices()
        {
            if (questService == null) questService = FindObjectOfType<QuestService>();
            if (tutorialService == null) tutorialService = FindObjectOfType<TutorialService>();
            if (cloudSaveCoordinator == null) cloudSaveCoordinator = FindObjectOfType<CloudSaveCoordinator>();
        }

        private void ShowMatchFeedback(MatchGroup group, BattleTurnOwner owner)
        {
            int count = group.count;
            if (owner == BattleTurnOwner.Enemy)
            {
                ShowEnemyMatchFeedback(group, count);
                return;
            }
            switch (group.tileType)
            {
                case TileType.Sword:
                    enemyView?.PlayHurt();
                    vfxService?.PlayDamageVfx(EnemyPosition());
                    floatingText?.Show("-" + (5 * count), EnemyPosition(), new Color(1f, 0.25f, 0.18f, 1f), 40);
                    audioService?.PlaySfx("sfx_damage");
                    break;
                case TileType.Heart:
                    vfxService?.PlayHealVfx(PlayerPosition());
                    floatingText?.Show("+" + (4 * count), PlayerPosition(), new Color(0.35f, 1f, 0.45f, 1f), 38);
                    audioService?.PlaySfx("sfx_heal");
                    break;
                case TileType.Coin:
                    if (!suppressMinorMatchVfx) vfxService?.PlayTileMatchVfx(BoardPosition(), group.tileType);
                    floatingText?.Show("+" + (3 * count) + " Gold", BoardPosition(), new Color(1f, 0.78f, 0.22f, 1f), 36);
                    audioService?.PlaySfx("sfx_coin");
                    break;
                case TileType.Food:
                    if (!suppressMinorMatchVfx) vfxService?.PlayTileMatchVfx(BoardPosition(), group.tileType);
                    floatingText?.Show("+" + (2 * count) + " Food", BoardPosition(), new Color(0.45f, 0.85f, 0.32f, 1f), 36);
                    break;
                case TileType.Book:
                    if (!suppressMinorMatchVfx) vfxService?.PlayTileMatchVfx(BoardPosition(), group.tileType);
                    floatingText?.Show("+" + (4 * count) + " EXP", BoardPosition(), new Color(0.72f, 0.5f, 1f, 1f), 36);
                    break;
                case TileType.Mana:
                    vfxService?.PlayManaVfx(PlayerPosition());
                    floatingText?.Show("+" + (8 * count) + " Mana", PlayerPosition(), new Color(0.45f, 0.62f, 1f, 1f), 36);
                    audioService?.PlaySfx("sfx_mana");
                    break;
                case TileType.Shield:
                    vfxService?.PlayShieldVfx(PlayerPosition());
                    floatingText?.Show("+" + (3 * count) + " Shield", PlayerPosition(), new Color(0.55f, 0.75f, 1f, 1f), 36);
                    audioService?.PlaySfx("sfx_shield");
                    break;
                case TileType.Star:
                    vfxService?.PlayManaVfx(EnemyPosition());
                    floatingText?.Show("-" + (2 * count), EnemyPosition(), new Color(1f, 0.95f, 0.25f, 1f), 36);
                    break;
            }
        }

        private void ShowEnemyMatchFeedback(MatchGroup group, int count)
        {
            switch (group.tileType)
            {
                case TileType.Sword:
                    playerView?.PlayHurt();
                    vfxService?.PlayDamageVfx(PlayerPosition());
                    floatingText?.Show("-" + (5 * count), PlayerPosition(), new Color(1f, 0.25f, 0.18f, 1f), 40);
                    audioService?.PlaySfx("sfx_damage");
                    break;
                case TileType.Heart:
                    vfxService?.PlayHealVfx(EnemyPosition());
                    floatingText?.Show("+" + (4 * count), EnemyPosition(), new Color(0.35f, 1f, 0.45f, 1f), 38);
                    break;
                case TileType.Mana:
                    vfxService?.PlayManaVfx(EnemyPosition());
                    floatingText?.Show("+" + (8 * count) + " Mana", EnemyPosition(), new Color(0.45f, 0.62f, 1f, 1f), 36);
                    break;
                case TileType.Shield:
                    vfxService?.PlayShieldVfx(EnemyPosition());
                    floatingText?.Show("+" + (3 * count) + " Shield", EnemyPosition(), new Color(0.55f, 0.75f, 1f, 1f), 36);
                    break;
                case TileType.Star:
                    vfxService?.PlayManaVfx(EnemyPosition());
                    floatingText?.Show("-" + (2 * count), PlayerPosition(), new Color(1f, 0.95f, 0.25f, 1f), 36);
                    break;
            }
        }

        private void ShowStateDeltaFeedback(BattleState state)
        {
            if (previousCombo >= 0 && state.comboCount > previousCombo && state.comboCount > 1)
            {
                if (comboText != null) comboText.transform.localScale = Vector3.one * 1.15f;
            }
            else if (comboText != null)
            {
                comboText.transform.localScale = Vector3.one;
            }
            previousEnemyHp = state.enemyHp;
            previousPlayerHp = state.hp;
            previousMana = state.mana;
            previousFood = state.food;
            previousCombo = state.comboCount;
        }

        private IEnumerator OpenResultAfterDelay(bool victory, int exp, int gold, string drops, bool leveledUp)
        {
            resultPopupOpening = true;
            yield return new WaitForSeconds(animationSettings.resultPopupDelay);
            screenManager?.OpenBattleResult(victory, exp, gold, drops, leveledUp);
        }

        private BattleEncounterData GetCurrentEncounter()
        {
            return battleService != null && battleService.State != null && battleService.State.encounter != null ? battleService.State.encounter : selectedEncounter;
        }

        private Vector3 EnemyPosition() => enemySpriteImage != null ? enemySpriteImage.transform.position : transform.position;
        private Vector3 PlayerPosition() => playerSpriteImage != null ? playerSpriteImage.transform.position : transform.position;
        private Vector3 BoardPosition() => boardController != null ? boardController.transform.position : transform.position;

        private static void SetButtonEnabled(Button button, bool enabled)
        {
            if (button == null) return;
            button.interactable = enabled;
            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = enabled ? new Color(0.18f, 0.76f, 0.82f, 1f) : new Color(0.35f, 0.38f, 0.44f, 1f);
            }
        }
    }
}
