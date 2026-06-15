using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.RemoteConfig;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using UnityEngine;

namespace Isekai12Realms.Tutorial
{
    public class TutorialService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private TutorialOverlayUI overlay;
        private GameConfigService gameConfigService;

        private PlayerSaveData Save => saveService?.CurrentSave;
        private GameContentDatabase Database => contentService?.Database;

        public void Initialize(ISaveService save, ContentDatabaseService content, TutorialOverlayUI overlayUi, GameConfigService config = null)
        {
            saveService = save;
            contentService = content;
            overlay = overlayUi;
            gameConfigService = config;
            if (Save == null) return;
            if (Save.completedTutorialStepIds == null) Save.completedTutorialStepIds = new List<string>();
            if (gameConfigService != null && Save.completedTutorialStepIds.Count == 0 && string.IsNullOrEmpty(Save.activeTutorialId)) Save.tutorialEnabled = gameConfigService.TutorialEnabledDefault;
            if (Save.tutorialEnabled && string.IsNullOrEmpty(Save.activeTutorialId))
            {
                TutorialDefinition first = Database?.tutorials?.Find(t => t != null && t.autoStart);
                if (first != null) StartTutorial(first.id);
            }
        }

        public void StartTutorial(string tutorialId)
        {
            TutorialDefinition tutorial = Database?.GetTutorialById(tutorialId);
            if (Save == null || tutorial == null || tutorial.steps == null || tutorial.steps.Count == 0) return;
            Save.activeTutorialId = tutorial.id;
            Save.activeTutorialStepId = FirstIncompleteStep(tutorial)?.stepId ?? tutorial.steps[0].stepId;
            Save.tutorialEnabled = true;
            SaveNow();
        }

        public void CompleteStep(string stepId)
        {
            if (Save == null || string.IsNullOrEmpty(stepId)) return;
            if (!Save.completedTutorialStepIds.Contains(stepId)) Save.completedTutorialStepIds.Add(stepId);
            TutorialDefinition tutorial = Database?.GetTutorialById(Save.activeTutorialId);
            TutorialStepData step = tutorial?.steps?.Find(s => s.stepId == stepId);
            TutorialStepData next = null;
            if (tutorial != null)
            {
                next = !string.IsNullOrEmpty(step?.nextStepId) ? tutorial.steps.Find(s => s.stepId == step.nextStepId) : FirstIncompleteStep(tutorial);
            }
            Save.activeTutorialStepId = next != null ? next.stepId : string.Empty;
            if (string.IsNullOrEmpty(Save.activeTutorialStepId)) Save.activeTutorialId = string.Empty;
            SaveNow();
        }

        public void SkipTutorial(string tutorialId)
        {
            TutorialDefinition tutorial = Database?.GetTutorialById(tutorialId);
            if (Save == null || tutorial == null || !tutorial.skippable) return;
            foreach (TutorialStepData step in tutorial.steps ?? new List<TutorialStepData>())
            {
                if (!Save.completedTutorialStepIds.Contains(step.stepId)) Save.completedTutorialStepIds.Add(step.stepId);
            }
            Save.activeTutorialId = string.Empty;
            Save.activeTutorialStepId = string.Empty;
            Save.tutorialEnabled = false;
            overlay?.Hide();
            SaveNow();
        }

        public bool IsStepCompleted(string stepId) => Save != null && Save.completedTutorialStepIds != null && Save.completedTutorialStepIds.Contains(stepId);
        public void HandleScreenOpened(string screenId) => TryShow(TutorialTriggerType.OnScreenOpened, screenId, string.Empty);
        public void HandleBattleStarted() => TryShow(TutorialTriggerType.OnBattleStarted, "screen_battle", string.Empty);
        public void HandleTileMatched(Data.TileType tileType) => TryShow(TutorialTriggerType.OnTileMatched, string.Empty, tileType.ToString().ToLowerInvariant());
        public void HandleQuestCompleted(string questId) => TryShow(TutorialTriggerType.OnQuestCompleted, string.Empty, questId);

        private void TryShow(TutorialTriggerType trigger, string screenId, string targetId)
        {
            if (Save == null || !Save.tutorialEnabled || string.IsNullOrEmpty(Save.activeTutorialId)) return;
            TutorialDefinition tutorial = Database?.GetTutorialById(Save.activeTutorialId);
            TutorialStepData step = tutorial?.steps?.Find(s => s.stepId == Save.activeTutorialStepId);
            if (step == null || IsStepCompleted(step.stepId)) return;
            if (step.triggerType != trigger && step.triggerType != TutorialTriggerType.Manual) return;
            if (!string.IsNullOrEmpty(step.targetScreen) && step.targetScreen != screenId) return;
            overlay?.Show(tutorial, step, () => CompleteStep(step.stepId), () => SkipTutorial(tutorial.id));
        }

        private TutorialStepData FirstIncompleteStep(TutorialDefinition tutorial)
        {
            return tutorial.steps.Find(s => s != null && !IsStepCompleted(s.stepId));
        }

        private void SaveNow() => saveService?.SaveNow();
    }
}
