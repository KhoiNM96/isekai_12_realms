using System;
using System.Collections.Generic;
using Isekai12Realms.Board;
using Isekai12Realms.Data;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Battle
{
    public class BattleService
    {
        private readonly BattleResolver resolver = new BattleResolver();
        private readonly System.Random random = new System.Random();
        private BoardController board;

        public BattleState State { get; private set; } = new BattleState();
        public event Action StateChanged;
        public event Action<BattleResultType> BattleEnded;

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
            State.currentTurnOwner = BattleTurnOwner.Player;
            Debug.Log("[Battle] StartBattle");
            board.Initialize(8, 8);
            StateChanged?.Invoke();
        }

        public void EndBattle()
        {
            board?.ClearBoard();
        }

        public void ApplyPlayerMatchResult(List<MatchGroup> groups, bool grantsExtraTurn, int comboCount)
        {
            if (State.battleResult != BattleResultType.None)
            {
                return;
            }

            State.comboCount = comboCount;
            resolver.ApplyPlayerMatches(State, groups);
            State.food -= 1;
            if (State.food <= 0)
            {
                resolver.DamagePlayer(State, 5);
            }

            Debug.Log("[Battle] Player resolved match");
            CheckWinLose();
            if (State.battleResult != BattleResultType.None)
            {
                StateChanged?.Invoke();
                return;
            }

            if (grantsExtraTurn)
            {
                State.currentTurnOwner = BattleTurnOwner.Player;
            }
            else
            {
                ExecuteEnemyTurn();
            }

            StateChanged?.Invoke();
        }

        public void ExecuteEnemyTurn()
        {
            if (State.battleResult != BattleResultType.None)
            {
                return;
            }

            State.currentTurnOwner = BattleTurnOwner.Enemy;
            Debug.Log("[Battle] Enemy turn");
            int roll = random.Next(100);
            if (roll < 70)
            {
                resolver.DamagePlayer(State, State.stage != null && State.stage.enemy != null ? State.stage.enemy.attack : 8);
            }
            else if (roll < 90)
            {
                State.enemyShield += 5;
            }
            else
            {
                State.enemyHp = Mathf.Min(State.enemyMaxHp, State.enemyHp + 6);
            }

            State.turnCount++;
            State.currentTurnOwner = BattleTurnOwner.Player;
            CheckWinLose();
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
            if (!SpendMana(30)) return;
            resolver.DamageEnemy(State, 20);
            CheckWinLose();
            StateChanged?.Invoke();
        }

        public void UseSkill2()
        {
            if (!SpendMana(20)) return;
            board?.ShuffleBoard();
            StateChanged?.Invoke();
        }

        public void UseUltimate()
        {
            if (!SpendMana(100)) return;
            resolver.DamageEnemy(State, 50);
            board?.DestroyRandomArea(3);
            CheckWinLose();
            StateChanged?.Invoke();
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
    }
}
