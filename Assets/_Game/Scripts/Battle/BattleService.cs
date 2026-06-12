using System;
using System.Collections.Generic;
using Isekai12Realms.Board;
using Isekai12Realms.Data;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Battle
{
    public class BattleService
    {
        public const float TURN_TIME_LIMIT = 30f;

        private readonly BattleResolver resolver = new BattleResolver();
        private readonly SkillEffectResolver skillResolver = new SkillEffectResolver();
        private BoardController board;
        private SkillService skillService;

        public BattleState State { get; private set; } = new BattleState();
        public event Action StateChanged;
        public event Action<BattleResultType> BattleEnded;
        public event Action<SkillDefinition, SkillResolveResult> SkillResolved;

        public void SetPlayerStats(Isekai12Realms.Character.PlayerStats stats)
        {
            if (stats == null) return;
            State.maxHp = stats.maxHp > 0 ? stats.maxHp : stats.hp;
            State.hp = State.maxHp;
            State.maxMana = stats.mana > 0 ? stats.mana : State.maxMana;
            State.atk = stats.atk;
            State.mag = stats.mag;
            State.def = stats.def;
            State.spd = stats.spd;
            State.luck = stats.luck;
            State.foodBonus = stats.foodBonus;
            State.manaGainBonus = stats.manaGainBonus;
            State.dropRateBonus = stats.dropRateBonus;
            State.expBonus = stats.expBonus;
            State.goldBonus = stats.goldBonus;
            State.healBonus = stats.healBonus;
            State.critRate = stats.critRate;
            State.food = 20 + stats.foodBonus;
        }

        public void SetSkillService(SkillService service)
        {
            skillService = service;
        }

        public void StartBattle(BoardController boardController)
        {
            StartBattle(boardController, null);
        }

        public void StartBattle(BoardController boardController, StageDefinition stage)
        {
            board = boardController;
            State = new BattleState();
            State.stage = stage;
            if (stage != null && stage.enemy != null)
            {
                State.enemyName = stage.enemy.displayName;
                State.enemyLevel = stage.enemy.level;
                State.enemyMaxHp = stage.enemy.maxHp;
                State.enemyHp = stage.enemy.maxHp;
                State.enemyMaxMana = stage.enemy.maxMana;
                State.enemyMana = 0;
            }
            Debug.Log("[Battle] StartBattle");
            board.Initialize(8, 8);
            BeginPlayerTurn();
            StateChanged?.Invoke();
        }

        public void EndBattle()
        {
            board?.ClearBoard();
        }

        public void ApplyPlayerMatchResult(List<MatchGroup> groups, bool grantsExtraTurn, int comboCount)
        {
            BoardResolveResult result = new BoardResolveResult { cascadeCount = comboCount };
            result.AddGroups(groups);
            result.grantsExtraTurn = grantsExtraTurn;
            ApplyMoveResult(result, BattleTurnOwner.Player);
        }

        public void ApplyMoveResult(BoardResolveResult result, BattleTurnOwner owner)
        {
            if (State.battleResult != BattleResultType.None)
            {
                return;
            }

            State.isResolvingTurn = false;
            State.inputLocked = false;
            State.comboCount = result != null ? result.cascadeCount : 0;
            State.currentTurnHasExtraTurn = result != null && result.grantsExtraTurn;
            State.lastMoveGrantedExtraTurn = State.currentTurnHasExtraTurn;
            State.lastMaxMatchSize = result != null ? result.maxMatchSize : 0;
            resolver.ResolveMatchesForSide(result, owner, State);

            if (owner == BattleTurnOwner.Player)
            {
                skillService?.TickCooldownsAfterPlayerTurn();
                DrainPlayerFoodForAction();
            }

            Debug.Log($"[Battle] {owner} resolved board move");
            CheckWinLose();
            if (State.battleResult != BattleResultType.None)
            {
                StateChanged?.Invoke();
                return;
            }

            if (State.currentTurnHasExtraTurn)
            {
                Debug.Log("[Turn] Extra turn granted by match 5+");
                if (owner == BattleTurnOwner.Player) BeginPlayerTurn();
                else BeginEnemyTurn();
            }
            else
            {
                Debug.Log("[Turn] No extra turn, switching side");
                if (owner == BattleTurnOwner.Player) BeginEnemyTurn();
                else BeginPlayerTurn();
            }

            StateChanged?.Invoke();
        }

        public void BeginPlayerTurn()
        {
            State.currentTurnOwner = BattleTurnOwner.Player;
            State.remainingTurnTime = TURN_TIME_LIMIT;
            State.inputLocked = false;
            State.isResolvingTurn = false;
            State.currentTurnHasExtraTurn = false;
            State.turnCount++;
            board?.SetInputLocked(false);
            Debug.Log("[Turn] Begin Player Turn, timer=30");
            StateChanged?.Invoke();
        }

        public void BeginEnemyTurn()
        {
            State.currentTurnOwner = BattleTurnOwner.Enemy;
            State.remainingTurnTime = TURN_TIME_LIMIT;
            State.inputLocked = true;
            State.isResolvingTurn = false;
            State.currentTurnHasExtraTurn = false;
            State.lastEnemyAction = string.Empty;
            State.lastEnemyActionValue = 0;
            State.turnCount++;
            board?.SetInputLocked(true);
            Debug.Log("[Turn] Begin Enemy Turn, timer=30");
            StateChanged?.Invoke();
        }

        public bool TickTurnTimer(float deltaTime, bool paused)
        {
            if (paused || State.battleResult != BattleResultType.None || State.isResolvingTurn)
            {
                return false;
            }

            State.remainingTurnTime = Mathf.Max(0f, State.remainingTurnTime - deltaTime);
            if (State.remainingTurnTime > 0f)
            {
                StateChanged?.Invoke();
                return false;
            }

            Debug.Log($"[Turn] Timer expired for {State.currentTurnOwner}");
            return true;
        }

        public void MarkResolvingTurn(bool resolving)
        {
            State.isResolvingTurn = resolving;
            State.inputLocked = resolving || State.currentTurnOwner == BattleTurnOwner.Enemy;
            board?.SetInputLocked(State.inputLocked);
            StateChanged?.Invoke();
        }

        public void SkipCurrentTurn()
        {
            Debug.Log("[Turn] No extra turn, switching side");
            if (State.currentTurnOwner == BattleTurnOwner.Player) BeginEnemyTurn();
            else BeginPlayerTurn();
        }

        public void ExecuteEnemyTurn()
        {
            SkipCurrentTurn();
            StateChanged?.Invoke();
        }

        public BattleResultType CheckWinLose()
        {
            if (State.enemyHp <= 0)
            {
                State.battleResult = BattleResultType.Victory;
                Debug.Log("[Battle] Victory");
                BattleEnded?.Invoke(State.battleResult);
            }
            else if (State.hp <= 0)
            {
                State.battleResult = BattleResultType.Defeat;
                Debug.Log("[Battle] Defeat");
                BattleEnded?.Invoke(State.battleResult);
            }

            return State.battleResult;
        }

        public void UseSkill1()
        {
            UseEquippedSkill(SkillSlotType.Skill1);
        }

        public void UseSkill2()
        {
            UseEquippedSkill(SkillSlotType.Skill2);
        }

        public void UseUltimate()
        {
            UseEquippedSkill(SkillSlotType.Ultimate);
        }

        public bool UseEquippedSkill(SkillSlotType slotType)
        {
            if (skillService == null || board == null || board.IsResolving || board.InputLocked || State.currentTurnOwner != BattleTurnOwner.Player)
            {
                return false;
            }

            SkillDefinition skill = skillService.GetEquippedSkill(slotType);
            if (skill == null || !skillService.IsSkillUsable(skill.id, State))
            {
                return false;
            }

            int manaCost = skillService.GetManaCost(skill.id);
            if (!SpendMana(manaCost)) return false;
            int level = skillService.GetSkillLevel(skill.id);
            MarkResolvingTurn(true);
            SkillResolveResult result = skillResolver.Apply(skill, level, State, board);
            skillService.StartCooldown(skill.id);
            SkillResolved?.Invoke(skill, result);
            DrainPlayerFoodForAction();
            CheckWinLose();
            MarkResolvingTurn(false);
            if (State.battleResult == BattleResultType.None)
            {
                if (result.extraTurnGranted)
                {
                    Debug.Log("[Turn] Extra turn granted by match 5+");
                    BeginPlayerTurn();
                }
                else
                {
                    Debug.Log("[Turn] No extra turn, switching side");
                    BeginEnemyTurn();
                }
            }
            StateChanged?.Invoke();
            return true;
        }

        private bool SpendMana(int cost)
        {
            if (State.battleResult != BattleResultType.None || State.currentTurnOwner != BattleTurnOwner.Player || State.mana < cost)
            {
                return false;
            }

            State.mana -= cost;
            return true;
        }

        private void DrainPlayerFoodForAction()
        {
            State.food -= 1;
            if (State.food <= 0)
            {
                resolver.DamagePlayer(State, Mathf.Max(1, Mathf.RoundToInt(State.maxHp * 0.05f)));
            }
        }
    }
}
