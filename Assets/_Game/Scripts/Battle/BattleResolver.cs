using System.Collections.Generic;
using Isekai12Realms.Board;
using Isekai12Realms.Data;
using UnityEngine;

namespace Isekai12Realms.Battle
{
    public class BattleResolver
    {
        public void ApplyPlayerMatches(BattleState state, List<MatchGroup> groups)
        {
            foreach (MatchGroup group in groups)
            {
                int count = group.count;
                switch (group.tileType)
                {
                    case TileType.Sword:
                        DamageEnemy(state, 5 * count);
                        break;
                    case TileType.Heart:
                        state.hp = Mathf.Min(state.maxHp, state.hp + 4 * count);
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
                        state.mana = Mathf.Min(state.maxMana, state.mana + 8 * count);
                        break;
                    case TileType.Shield:
                        state.shield += 3 * count;
                        break;
                    case TileType.Star:
                        state.mana = Mathf.Min(state.maxMana, state.mana + 5 * count);
                        DamageEnemy(state, 2 * count);
                        break;
                }
            }
        }

        public void DamageEnemy(BattleState state, int amount)
        {
            int remaining = amount;
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
            int remaining = amount;
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
