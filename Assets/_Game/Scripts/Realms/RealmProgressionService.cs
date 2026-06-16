using System;
using System.Collections.Generic;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Realms
{
    public class RealmProgressionService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private PlayerProgressionService progressionService;

        private PlayerSaveData Save => saveService != null ? saveService.CurrentSave : null;

        public void Initialize(ISaveService save, ContentDatabaseService content, PlayerProgressionService progression = null)
        {
            saveService = save;
            contentService = content;
            progressionService = progression;
            EnsureDefaults();
        }

        public bool IsRealmUnlocked(RealmDefinition realm)
        {
            if (realm == null)
            {
                return false;
            }

            if (realm.unlockedByDefault)
            {
                return true;
            }

            return MeetsRequirements(realm);
        }

        public bool CanEnterRealm(RealmDefinition realm)
        {
            return IsRealmUnlocked(realm);
        }

        public string GetRealmLockReason(RealmDefinition realm)
        {
            if (realm == null)
            {
                return string.Empty;
            }

            if (IsRealmUnlocked(realm))
            {
                return string.Empty;
            }

            int level = GetPlayerLevel();
            bool needsLevel = !realm.unlockedByDefault && level < realm.requiredPlayerLevel;
            bool needsRealm = !string.IsNullOrEmpty(realm.requiredCompletedRealmId) && !IsRealmCompleted(realm.requiredCompletedRealmId);

            if (!needsLevel && !needsRealm)
            {
                return string.Empty;
            }

            RealmDefinition requiredRealm = contentService != null ? contentService.GetRealmById(realm.requiredCompletedRealmId) : null;
            string requiredName = requiredRealm != null ? requiredRealm.displayName : realm.requiredCompletedRealmId;
            if (needsLevel && needsRealm)
            {
                return $"Requires Lv. {realm.requiredPlayerLevel} and {requiredName} cleared";
            }

            return needsLevel ? $"Requires Lv. {realm.requiredPlayerLevel}" : $"Complete {requiredName} first";
        }

        public void MarkRealmEntered(string realmId)
        {
            if (string.IsNullOrEmpty(realmId) || Save == null)
            {
                return;
            }

            EnsureDefaults();
            Save.currentRealmId = realmId;
            RealmProgressData progress = GetOrCreateProgress(realmId);
            if (progress.firstEnteredAt == 0)
            {
                progress.firstEnteredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            saveService.SaveNow();
        }

        public void MarkMonsterDefeated(string realmId, string enemyId)
        {
            if (string.IsNullOrEmpty(realmId) || Save == null)
            {
                return;
            }

            RealmDefinition realm = contentService != null ? contentService.GetRealmById(realmId) : null;
            if (realm != null && realm.bossEnemy != null && string.Equals(realm.bossEnemy.id, enemyId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RealmProgressData progress = GetOrCreateProgress(realmId);
            progress.normalMonstersDefeated += 1;
            saveService.SaveNow();
        }

        public void MarkBossDefeated(string realmId)
        {
            if (string.IsNullOrEmpty(realmId) || Save == null)
            {
                return;
            }

            RealmProgressData progress = GetOrCreateProgress(realmId);
            progress.bossDefeated = true;
            saveService.SaveNow();
        }

        public void MarkRealmCompleted(string realmId)
        {
            if (string.IsNullOrEmpty(realmId) || Save == null)
            {
                return;
            }

            EnsureDefaults();
            if (!Save.completedRealmIds.Contains(realmId))
            {
                Save.completedRealmIds.Add(realmId);
            }

            RealmProgressData progress = GetOrCreateProgress(realmId);
            if (progress.completedAt == 0)
            {
                progress.completedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            saveService.SaveNow();
        }

        public RealmProgressData GetCurrentRealmProgress(string realmId)
        {
            if (string.IsNullOrEmpty(realmId) || Save == null)
            {
                return null;
            }

            EnsureDefaults();
            return GetOrCreateProgress(realmId);
        }

        public bool IsRealmCompleted(string realmId)
        {
            return Save != null && Save.completedRealmIds != null && Save.completedRealmIds.Contains(realmId);
        }

        private bool MeetsRequirements(RealmDefinition realm)
        {
            if (realm == null)
            {
                return false;
            }

            if (GetPlayerLevel() < Mathf.Max(1, realm.requiredPlayerLevel))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(realm.requiredCompletedRealmId) && !IsRealmCompleted(realm.requiredCompletedRealmId))
            {
                return false;
            }

            return true;
        }

        private int GetPlayerLevel()
        {
            return progressionService != null && progressionService.CurrentSave != null ? progressionService.CurrentSave.level : (Save != null ? Save.level : 1);
        }

        private void EnsureDefaults()
        {
            if (Save == null)
            {
                return;
            }

            if (Save.completedRealmIds == null)
            {
                Save.completedRealmIds = new List<string>();
            }

            if (Save.realmProgress == null)
            {
                Save.realmProgress = new List<RealmProgressData>();
            }
        }

        private RealmProgressData GetOrCreateProgress(string realmId)
        {
            if (Save == null)
            {
                return null;
            }

            EnsureDefaults();
            RealmProgressData progress = Save.realmProgress.Find(entry => entry != null && entry.realmId == realmId);
            if (progress == null)
            {
                progress = new RealmProgressData { realmId = realmId };
                Save.realmProgress.Add(progress);
            }

            return progress;
        }
    }
}
