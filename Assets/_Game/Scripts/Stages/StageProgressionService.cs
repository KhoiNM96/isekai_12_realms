using System;
using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.Services;
using UnityEngine;

namespace Isekai12Realms.Stages
{
    public class StageProgressionService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;

        private PlayerSaveData Save => saveService?.CurrentSave;

        public void Initialize(ISaveService save, ContentDatabaseService content)
        {
            saveService = save;
            contentService = content;
            EnsureStageProgressList();
        }

        public bool IsStageUnlocked(StageDefinition stage)
        {
            if (stage == null) return false;
            if (stage.id == "stage_01_01") return true;
            if (stage.requiredCompletedStageIds == null || stage.requiredCompletedStageIds.Count == 0) return stage.realmId == "realm_01_meadow";
            foreach (string required in stage.requiredCompletedStageIds)
            {
                if (!IsStageCompleted(required)) return false;
            }
            return true;
        }

        public bool IsStageCompleted(string stageId)
        {
            return Save != null && Save.completedStageIds != null && Save.completedStageIds.Contains(stageId);
        }

        public void MarkStageCompleted(string stageId)
        {
            if (Save == null) return;
            EnsureStageProgressList();
            if (!Save.completedStageIds.Contains(stageId)) Save.completedStageIds.Add(stageId);
            StageProgressData progress = GetOrCreateProgress(stageId);
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (progress.firstClearedAt == 0) progress.firstClearedAt = now;
            progress.lastClearedAt = now;
            saveService.SaveNow();
        }

        public int GetStageClearCount(string stageId)
        {
            StageProgressData progress = Save?.stageProgress?.Find(p => p.stageId == stageId);
            return progress != null ? progress.clearCount : 0;
        }

        public void IncrementStageClearCount(string stageId)
        {
            if (Save == null) return;
            StageProgressData progress = GetOrCreateProgress(stageId);
            progress.clearCount += 1;
            progress.lastClearedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            saveService.SaveNow();
        }

        public List<StageDefinition> GetUnlockedStagesForRealm(string realmId)
        {
            List<StageDefinition> result = new List<StageDefinition>();
            foreach (StageDefinition stage in contentService.GetStagesForRealm(realmId))
            {
                if (IsStageUnlocked(stage)) result.Add(stage);
            }
            return result;
        }

        public StageDefinition GetCurrentAvailableStage()
        {
            foreach (StageDefinition stage in contentService.Database.stages)
            {
                if (IsStageUnlocked(stage) && !IsStageCompleted(stage.id)) return stage;
            }
            return contentService.GetStageById(Save?.currentStageId ?? "stage_01_01");
        }

        private StageProgressData GetOrCreateProgress(string stageId)
        {
            EnsureStageProgressList();
            StageProgressData progress = Save.stageProgress.Find(p => p.stageId == stageId);
            if (progress == null)
            {
                progress = new StageProgressData { stageId = stageId };
                Save.stageProgress.Add(progress);
            }
            return progress;
        }

        private void EnsureStageProgressList()
        {
            if (Save == null) return;
            if (Save.completedStageIds == null) Save.completedStageIds = new List<string>();
            if (Save.stageProgress == null) Save.stageProgress = new List<StageProgressData>();
        }
    }
}
