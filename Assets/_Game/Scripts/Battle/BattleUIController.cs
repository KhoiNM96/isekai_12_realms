using Isekai12Realms.Board;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using Isekai12Realms.DropTables;
using Isekai12Realms.Inventory;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using TMPro;
using UnityEngine;
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
        private TextMeshProUGUI goldRewardText;
        private TextMeshProUGUI expRewardText;
        private Image enemyHpFill;
        private Image playerHpFill;
        private Image playerManaFill;
        private Button skill1Button;
        private Button skill2Button;
        private Button ultimateButton;
        private bool rewardGranted;
        private StageDefinition selectedStage;

        public void SetStage(StageDefinition stage)
        {
            selectedStage = stage;
        }

        public void Initialize(UIScreenManager manager, BoardController board)
        {
            screenManager = manager;
            progressionService = FindObjectOfType<PlayerProgressionService>();
            boardController = board;
            CacheReferences();

            battleService.StateChanged -= UpdateView;
            battleService.StateChanged += UpdateView;
            battleService.BattleEnded -= OnBattleEnded;
            battleService.BattleEnded += OnBattleEnded;

            if (boardController != null)
            {
                boardController.MatchesResolved -= battleService.ApplyPlayerMatchResult;
                boardController.MatchesResolved += battleService.ApplyPlayerMatchResult;
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
            if (boardController != null)
            {
                boardController.MatchesResolved -= battleService.ApplyPlayerMatchResult;
            }
        }

        public void StartBattle()
        {
            if (boardController == null)
            {
                CacheReferences();
            }

            battleService.StartBattle(boardController, selectedStage);
            rewardGranted = false;
            UpdateView();
        }

        public void BackToWorldMap()
        {
            battleService.EndBattle();
            screenManager?.ShowScreen(GameUIScreen.WorldMap);
        }

        public void UseSkill1()
        {
            bool hadMana = battleService.State.mana >= 30;
            battleService.UseSkill1();
            if (!hadMana)
            {
                screenManager?.ToastService?.ShowToast("Spark Slash needs 30 mana.");
            }
        }

        public void UseSkill2()
        {
            bool hadMana = battleService.State.mana >= 20;
            battleService.UseSkill2();
            if (!hadMana)
            {
                screenManager?.ToastService?.ShowToast("Shuffle Bell needs 20 mana.");
            }
        }

        public void UseUltimate()
        {
            bool hadMana = battleService.State.mana >= 100;
            battleService.UseUltimate();
            if (!hadMana)
            {
                screenManager?.ToastService?.ShowToast("Realm Burst needs 100 mana.");
            }
        }

        public void WinTest()
        {
            GrantVictoryRewards();
        }

        public void LoseTest()
        {
            screenManager?.OpenBattleResult(false);
        }

        private void OnBattleEnded(BattleResultType result)
        {
            if (result == BattleResultType.Victory)
            {
                GrantVictoryRewards();
            }
            else if (result == BattleResultType.Defeat)
            {
                screenManager?.OpenBattleResult(false);
            }
        }

        private void GrantVictoryRewards()
        {
            if (rewardGranted)
            {
                return;
            }

            rewardGranted = true;
            if (progressionService == null)
            {
                progressionService = FindObjectOfType<PlayerProgressionService>();
            }

            int exp = 50 + battleService.State.expReward;
            int gold = 30 + battleService.State.goldReward;
            bool firstClear = selectedStage == null || progressionService == null || !progressionService.CurrentSave.completedStageIds.Contains(selectedStage.id);
            bool replay = !firstClear;
            if (selectedStage != null)
            {
                exp = selectedStage.baseExpReward;
                if (replay) exp = Mathf.FloorToInt(exp * 0.7f);
                gold = selectedStage.baseGoldReward;
            }
            exp += battleService.State.expReward;
            gold += battleService.State.goldReward;
            string drops = "Drops:";
            bool leveledUp = false;

            if (progressionService != null)
            {
                progressionService.AddGold(gold);
                leveledUp = progressionService.AddExp(exp);
                drops += RollDrops();
                string stageId = selectedStage != null ? selectedStage.id : "stage_01_01";
                progressionService.MarkStageCompleted(stageId);
                StageProgressionService stageProgression = FindObjectOfType<StageProgressionService>();
                stageProgression?.IncrementStageClearCount(stageId);
            }

            string stageName = selectedStage != null ? selectedStage.displayName : "First Slime";
            string enemyName = selectedStage != null && selectedStage.enemy != null ? selectedStage.enemy.displayName : battleService.State.enemyName;
            screenManager?.OpenBattleResult(true, exp, gold, $"Stage: {stageName}\nEnemy defeated: {enemyName}\n{(firstClear ? "First Clear!\n" : string.Empty)}{drops}", leveledUp);
        }

        private string RollDrops()
        {
            DropTableDefinition table = selectedStage != null ? selectedStage.dropTable : null;
            if (table == null)
            {
                progressionService.AddItem("mat_slime_jelly", 2);
                return "\nSlime Jelly x2";
            }

            string text = string.Empty;
            foreach (DropEntry drop in table.drops)
            {
                if (UnityEngine.Random.value > drop.chance) continue;
                int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);
                if (drop.isEquipment)
                {
                    EquipmentInstanceData equipment = PrototypeEquipmentFactory.Create(drop.equipmentId);
                    progressionService.AddEquipment(equipment);
                    text += $"\n{equipment.displayName}";
                }
                else
                {
                    progressionService.AddItem(drop.itemId, amount);
                    text += $"\n{PrototypeItemDatabase.Get(drop.itemId).displayName} x{amount}";
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
            SetText(goldRewardText, $"Gold: {state.goldReward}");
            SetText(expRewardText, $"EXP: {state.expReward}");

            SetFill(enemyHpFill, state.enemyHp, state.enemyMaxHp);
            SetFill(playerHpFill, state.hp, state.maxHp);
            SetFill(playerManaFill, state.mana, state.maxMana);

            SetButtonEnabled(skill1Button, state.mana >= 30 && state.battleResult == BattleResultType.None);
            SetButtonEnabled(skill2Button, state.mana >= 20 && state.battleResult == BattleResultType.None);
            SetButtonEnabled(ultimateButton, state.mana >= 100 && state.battleResult == BattleResultType.None);
        }

        private void CacheReferences()
        {
            boardController = boardController != null ? boardController : GetComponentInChildren<BoardController>(true);
            enemyNameText = FindText("EnemyName");
            playerNameText = FindText("PlayerName");
            foodText = FindText("FoodText");
            comboText = FindText("ComboText");
            turnText = FindText("TurnText");
            goldRewardText = FindText("GoldRewardText");
            expRewardText = FindText("ExpRewardText");
            enemyHpFill = FindImage("EnemyHp/Fill");
            playerHpFill = FindImage("PlayerHp/Fill");
            playerManaFill = FindImage("PlayerMana/Fill");
            skill1Button = FindButton("Button_Skill1");
            skill2Button = FindButton("Button_Skill2");
            ultimateButton = FindButton("Button_Ultimate");
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

        private static void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null) text.text = value;
        }

        private static void SetFill(Image fill, int value, int max)
        {
            if (fill == null || max <= 0) return;
            fill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)value / max), 1f);
        }

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
