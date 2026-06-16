using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.DropTables;
using Isekai12Realms.Enemies;
using Isekai12Realms.Inventory;
using Isekai12Realms.Realms;
using UnityEngine;

namespace Isekai12Realms.Battle
{
    public class RealmRewardService
    {
        public int GetBaseExp(RealmDefinition realm, EnemyDefinition enemy, bool isBoss)
        {
            int realmOrder = realm != null ? Mathf.Max(1, realm.order) : 1;
            int enemyLevel = enemy != null ? Mathf.Max(1, enemy.level) : 1;
            int normalExp = 30 + realmOrder * 20 + enemyLevel * 10;
            return isBoss ? normalExp * 3 : normalExp;
        }

        public int GetBaseGold(RealmDefinition realm, EnemyDefinition enemy, bool isBoss)
        {
            int realmOrder = realm != null ? Mathf.Max(1, realm.order) : 1;
            int enemyLevel = enemy != null ? Mathf.Max(1, enemy.level) : 1;
            int normalGold = 20 + realmOrder * 15 + enemyLevel * 5;
            return isBoss ? normalGold * 3 : normalGold;
        }

        public int GetSoulGemFirstClearBonus(int realmOrder)
        {
            if (realmOrder <= 3) return 5;
            if (realmOrder <= 6) return 10;
            if (realmOrder <= 9) return 15;
            return 25;
        }

        public List<DropRollResult> RollDrops(BattleEncounterData encounter, float luckBonus, float dropBonus)
        {
            List<DropRollResult> results = new List<DropRollResult>();
            DropTableDefinition table = encounter != null ? encounter.dropTable : null;
            if (table != null && table.drops != null && table.drops.Count > 0)
            {
                RollTable(table, luckBonus, dropBonus, results);
                return results;
            }

            results.Add(new DropRollResult { itemId = GetFallbackRealmMaterial(encounter), amount = Random.Range(1, 3) });
            if (Random.value <= 0.2f)
            {
                results.Add(new DropRollResult { itemId = "item_potion_small", amount = 1 });
            }

            return results;
        }

        private static void RollTable(DropTableDefinition table, float luckBonus, float dropBonus, List<DropRollResult> results)
        {
            foreach (DropEntry drop in table.drops)
            {
                if (drop == null)
                {
                    continue;
                }

                float effectiveChance = drop.chance >= 1f ? drop.chance : Mathf.Min(0.95f, drop.chance * (1f + luckBonus + dropBonus));
                if (Random.value > effectiveChance)
                {
                    continue;
                }

                results.Add(new DropRollResult
                {
                    itemId = drop.isEquipment ? drop.equipmentId : drop.itemId,
                    amount = Random.Range(drop.minAmount, drop.maxAmount + 1),
                    isEquipment = drop.isEquipment
                });
            }
        }

        private static string GetFallbackRealmMaterial(BattleEncounterData encounter)
        {
            if (encounter == null || encounter.realm == null)
            {
                return "mat_realm_fragment";
            }

            return $"mat_{encounter.realm.id}_fragment";
        }
    }

    public struct DropRollResult
    {
        public string itemId;
        public int amount;
        public bool isEquipment;
    }
}
