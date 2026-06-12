using System.Collections.Generic;
using Isekai12Realms.Board;
using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.Battle
{
    public class BattleResolver
    {
        public void ResolveMatchesForSide(BoardResolveResult result, BattleTurnOwner owner, BattleState state)
        {
            if (result == null || state == null) return;
            if (owner == BattleTurnOwner.Player)
            {
                ApplyPlayerMatches(state, result.allMatchGroups);
            }
            else
            {
                ApplyEnemyMatches(state, result.allMatchGroups);
            }
        }

        public void ApplyPlayerMatches(BattleState state, List<MatchGroup> groups)
        {
            foreach (MatchGroup group in groups)
            {
                int count = group.count;
                switch (group.tileType)
                {
                    case TileType.Sword:
                        DamageEnemy(state, Mathf.RoundToInt(5 * count + state.atk * 0.25f));
                        break;
                    case TileType.Heart:
                        state.hp = Mathf.Min(state.maxHp, state.hp + Mathf.RoundToInt(4 * count + state.mag * 0.20f + state.healBonus));
                        break;
                    case TileType.Coin:
                        state.goldReward += 3 * count;
                        break;
                    case TileType.Food:
                        state.food += 2 * count;
                        break;
                    case TileType.Book:
                        state.expReward += 4 * count;
                        break;
                    case TileType.Mana:
                        state.mana = Mathf.Min(state.maxMana, state.mana + 8 * count + state.manaGainBonus);
                        break;
                    case TileType.Shield:
                        state.shield += Mathf.RoundToInt(3 * count + state.def * 0.15f);
                        break;
                    case TileType.Star:
                        state.mana = Mathf.Min(state.maxMana, state.mana + 5 * count);
                        DamageEnemy(state, Mathf.RoundToInt(2 * count + state.mag * 0.10f));
                        break;
                }
            }
        }

        private void ApplyEnemyMatches(BattleState state, List<MatchGroup> groups)
        {
            foreach (MatchGroup group in groups)
            {
                int count = group.count;
                switch (group.tileType)
                {
                    case TileType.Sword:
                        DamagePlayer(state, 5 * count);
                        state.lastEnemyAction = "damage";
                        state.lastEnemyActionValue += 5 * count;
                        break;
                    case TileType.Heart:
                        state.enemyHp = Mathf.Min(state.enemyMaxHp, state.enemyHp + 4 * count);
                        state.lastEnemyAction = "heal";
                        state.lastEnemyActionValue += 4 * count;
                        break;
                    case TileType.Mana:
                        state.enemyMana = Mathf.Min(state.enemyMaxMana, state.enemyMana + 8 * count);
                        state.lastEnemyAction = "mana";
                        state.lastEnemyActionValue += 8 * count;
                        break;
                    case TileType.Shield:
                        state.enemyShield += 3 * count;
                        state.lastEnemyAction = "shield";
                        state.lastEnemyActionValue += 3 * count;
                        break;
                    case TileType.Star:
                        DamagePlayer(state, 2 * count);
                        state.enemyMana = Mathf.Min(state.enemyMaxMana, state.enemyMana + 5 * count);
                        state.lastEnemyAction = "damage";
                        state.lastEnemyActionValue += 2 * count;
                        break;
                    case TileType.Coin:
                    case TileType.Food:
                    case TileType.Book:
                        state.enemyMana = Mathf.Min(state.enemyMaxMana, state.enemyMana + count);
                        break;
                }
            }
        }

        public void DamageEnemy(BattleState state, int amount)
        {
            float enemyDefense = state.stage != null && state.stage.enemy != null ? state.stage.enemy.defense : 0f;
            int remaining = Mathf.Max(1, Mathf.RoundToInt(amount - enemyDefense * 0.5f));
            if (state.enemyShield > 0)
            {
                int absorbed = Mathf.Min(state.enemyShield, remaining);
                state.enemyShield -= absorbed;
                remaining -= absorbed;
            }

            if (remaining > 0)
            {
                state.enemyHp = Mathf.Max(0, state.enemyHp - remaining);
            }
        }

        public void DamagePlayer(BattleState state, int amount)
        {
            int remaining = Mathf.Max(1, Mathf.RoundToInt(amount - state.def * 0.4f));
            if (state.shield > 0)
            {
                int absorbed = Mathf.Min(state.shield, remaining);
                state.shield -= absorbed;
                remaining -= absorbed;
            }

            if (remaining > 0)
            {
                state.hp = Mathf.Max(0, state.hp - remaining);
            }
        }
    }
}
