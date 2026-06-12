using Isekai12Realms.Battle;
using Isekai12Realms.Board;
using UnityEngine;

namespace Isekai12Realms.Skills
{
    public class SkillEffectResolver
    {
        public SkillResolveResult Apply(SkillDefinition definition, int level, BattleState state, BoardController board)
        {
            SkillResolveResult result = new SkillResolveResult();
            if (definition == null || state == null) return result;

            if (definition.effects == null) return result;
            foreach (SkillEffectData effect in definition.effects)
            {
                if (effect == null) continue;
                int value = FinalValue(effect, level);
                switch (effect.effectType)
                {
                    case SkillEffectType.Damage:
                        value = Mathf.RoundToInt(value + state.atk * 0.5f + state.mag * 0.25f);
                        int beforeEnemy = state.enemyHp;
                        DamageEnemy(state, value);
                        result.damageDealt += beforeEnemy - state.enemyHp;
                        result.messages.Add($"Damage {result.damageDealt}");
                        break;
                    case SkillEffectType.Heal:
                        value = Mathf.RoundToInt(value + state.mag * 0.6f);
                        int beforeHp = state.hp;
                        state.hp = Mathf.Min(state.maxHp, state.hp + value);
                        result.healingDone += state.hp - beforeHp;
                        result.messages.Add($"Heal {result.healingDone}");
                        break;
                    case SkillEffectType.Shield:
                        value = Mathf.RoundToInt(value + state.def * 0.4f + state.mag * 0.2f);
                        state.shield += value;
                        result.shieldGained += value;
                        result.messages.Add($"Shield {value}");
                        break;
                    case SkillEffectType.GainMana:
                        int beforeMana = state.mana;
                        state.mana = Mathf.Min(state.maxMana, state.mana + value);
                        result.manaGained += state.mana - beforeMana;
                        break;
                    case SkillEffectType.ShuffleBoard:
                        board?.ShuffleBoard();
                        result.boardShuffled = true;
                        result.messages.Add("Board Shuffled");
                        break;
                    case SkillEffectType.DestroyRandomTiles:
                        board?.DestroyRandomTiles(Mathf.Max(1, effect.tileCount));
                        result.tilesDestroyed += Mathf.Max(1, effect.tileCount);
                        break;
                    case SkillEffectType.DestroyArea:
                        board?.DestroyRandomArea(Mathf.Max(1, effect.areaSize));
                        result.tilesDestroyed += Mathf.Max(1, effect.areaSize * effect.areaSize);
                        break;
                    case SkillEffectType.ExtraTurn:
                        result.extraTurnGranted = true;
                        result.messages.Add("Extra Turn!");
                        break;
                    case SkillEffectType.CleanseDebuff:
                        result.messages.Add("Cleanse ready for future debuffs.");
                        break;
                }
            }

            return result;
        }

        private static int FinalValue(SkillEffectData effect, int level)
        {
            if (effect == null) return 0;
            int value = Mathf.RoundToInt(effect.baseValue * Mathf.Max(0.01f, effect.multiplier));
            if (!effect.scalesWithLevel) return value;
            int perLevel = 0;
            if (effect.effectType == SkillEffectType.Damage) perLevel = 8;
            if (effect.effectType == SkillEffectType.Heal || effect.effectType == SkillEffectType.Shield) perLevel = 6;
            return value + Mathf.Max(0, level - 1) * perLevel;
        }

        private static void DamageEnemy(BattleState state, int amount)
        {
            float enemyDefense = state.stage != null && state.stage.enemy != null ? state.stage.enemy.defense : 0f;
            int remaining = Mathf.Max(1, Mathf.RoundToInt(amount - enemyDefense * 0.5f));
            if (state.enemyShield > 0)
            {
                int absorbed = Mathf.Min(state.enemyShield, remaining);
                state.enemyShield -= absorbed;
                remaining -= absorbed;
            }
            if (remaining > 0) state.enemyHp = Mathf.Max(0, state.enemyHp - remaining);
        }
    }
}
