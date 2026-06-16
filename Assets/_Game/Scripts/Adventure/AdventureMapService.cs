using System;
using System.Collections.Generic;
using Isekai12Realms.Battle;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Enemies;
using Isekai12Realms.Realms;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Adventure
{
    public class AdventureMapService : MonoBehaviour
    {
        private UIScreenManager screenManager;
        private ContentDatabaseService contentService;
        private RealmProgressionService realmProgressionService;
        private PlayerProgressionService progressionService;
        private BattleUIController battleUIController;
        private RealmAdventureMapUIController realmMapController;
        private BattleEncounterData currentEncounter;
        private string currentRealmId;

        public void Initialize(UIScreenManager ui, ContentDatabaseService content, RealmProgressionService realmProgression, PlayerProgressionService progression, BattleUIController battleController, RealmAdventureMapUIController mapController)
        {
            screenManager = ui;
            contentService = content;
            realmProgressionService = realmProgression;
            progressionService = progression;
            battleUIController = battleController;
            realmMapController = mapController;
            currentRealmId = progression != null && progression.CurrentSave != null ? progression.CurrentSave.currentRealmId : string.Empty;
        }

        public bool EnterRealm(string realmId)
        {
            Debug.Log($"[Adventure] EnterRealm: {realmId}");
            RealmDefinition realm = GetRealmById(realmId);
            if (realm == null || realmProgressionService == null || !realmProgressionService.CanEnterRealm(realm))
            {
                return false;
            }

            currentRealmId = realmId;
            currentEncounter = null;
            realmProgressionService.MarkRealmEntered(realmId);
            realmMapController?.ShowRealm(realm);
            RefreshMap();
            return true;
        }

        public void ExitRealm()
        {
            currentEncounter = null;
        }

        public RealmDefinition GetCurrentRealm()
        {
            if (string.IsNullOrEmpty(currentRealmId) && progressionService != null && progressionService.CurrentSave != null)
            {
                currentRealmId = progressionService.CurrentSave.currentRealmId;
            }

            return GetRealmById(currentRealmId);
        }

        public bool StartEncounter(AdventureMonsterController monster)
        {
            if (monster == null)
            {
                return false;
            }

            RealmDefinition realm = GetCurrentRealm();
            BattleEncounterData encounter = CreateEncounter(realm, monster.EnemyDefinition, monster.IsBoss);
            if (encounter == null)
            {
                return false;
            }

            BeginEncounter(encounter);
            return true;
        }

        public bool StartEncounter(EnemyDefinition enemy, bool isBoss)
        {
            RealmDefinition realm = GetCurrentRealm();
            if (realm == null || enemy == null)
            {
                return false;
            }

            BattleEncounterData encounter = CreateEncounter(realm, enemy, isBoss);
            if (encounter == null)
            {
                return false;
            }

            BeginEncounter(encounter);
            return true;
        }

        public BattleEncounterData CreateEncounterForMonster(RealmDefinition realm, AdventureMonsterController monster)
        {
            return realm == null || monster == null ? null : CreateEncounter(realm, monster.EnemyDefinition, monster.IsBoss);
        }

        public void BeginEncounter(BattleEncounterData encounter)
        {
            if (encounter == null || battleUIController == null || screenManager == null)
            {
                return;
            }

            currentEncounter = encounter;
            realmMapController?.SetMovementEnabled(false);
            battleUIController.SetEncounter(encounter);
            screenManager.ShowScreen(GameUIScreen.Battle);
        }

        public void OnEncounterVictory()
        {
            if (realmProgressionService == null || currentEncounter == null)
            {
                return;
            }

            realmProgressionService.MarkMonsterDefeated(currentEncounter.realmId, currentEncounter.enemyId);
            if (currentEncounter.isBoss)
            {
                realmProgressionService.MarkBossDefeated(currentEncounter.realmId);
                realmProgressionService.MarkRealmCompleted(currentEncounter.realmId);
            }

            realmMapController?.OnEncounterVictory(currentEncounter);
            realmMapController?.SetMovementEnabled(true);
            currentEncounter = null;
            RefreshMap();
        }

        public void OnEncounterDefeat()
        {
            realmMapController?.OnEncounterDefeat(currentEncounter);
            realmMapController?.SetMovementEnabled(true);
            currentEncounter = null;
            RefreshMap();
        }

        public void RefreshMap()
        {
            realmMapController?.RefreshMap();
        }

        public BattleEncounterData GetCurrentEncounter()
        {
            return currentEncounter;
        }

        public RealmProgressData GetCurrentRealmProgress()
        {
            RealmDefinition realm = GetCurrentRealm();
            return realmProgressionService != null && realm != null ? realmProgressionService.GetCurrentRealmProgress(realm.id) : null;
        }

        public bool CanStartBossEncounter()
        {
            RealmProgressData progress = GetCurrentRealmProgress();
            return progress != null && progress.normalMonstersDefeated >= 3 && !progress.bossDefeated;
        }

        private RealmDefinition GetRealmById(string realmId)
        {
            return contentService != null ? contentService.GetRealmById(realmId) : null;
        }

        private BattleEncounterData CreateEncounter(RealmDefinition realm, EnemyDefinition enemy, bool isBoss)
        {
            if (realm == null || enemy == null)
            {
                return null;
            }

            RealmRewardService rewardService = new RealmRewardService();
            Isekai12Realms.DropTables.DropTableDefinition dropTable = null;
            if (contentService != null)
            {
                List<StageDefinition> stages = contentService.GetStagesForRealm(realm.id);
                for (int i = 0; i < stages.Count; i++)
                {
                    StageDefinition stage = stages[i];
                    if (stage != null && stage.enemy != null && string.Equals(stage.enemy.id, enemy.id, StringComparison.OrdinalIgnoreCase))
                    {
                        dropTable = stage.dropTable;
                        break;
                    }
                }
            }
            return new BattleEncounterData
            {
                encounterId = $"{realm.id}_{enemy.id}_{(isBoss ? "boss" : "normal")}",
                realmId = realm.id,
                enemyId = enemy.id,
                displayName = enemy.displayName,
                enemy = enemy,
                isBoss = isBoss,
                baseGoldReward = rewardService.GetBaseGold(realm, enemy, isBoss),
                baseExpReward = rewardService.GetBaseExp(realm, enemy, isBoss),
                dropTable = dropTable,
                battleBackgroundAssetId = realm.battleBackgroundAssetId,
                realm = realm
            };
        }
    }
}
