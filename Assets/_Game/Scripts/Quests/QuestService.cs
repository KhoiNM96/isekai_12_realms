using System;
using System.Collections.Generic;
using System.Linq;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Equipment;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Quests
{
    public class QuestService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private PlayerProgressionService progressionService;
        private EquipmentService equipmentService;
        private ToastService toastService;

        public event Action QuestsChanged;
        public event Action<string> QuestCompleted;

        private PlayerSaveData Save => saveService?.CurrentSave;
        private GameContentDatabase Database => contentService?.Database;

        public void Initialize(ISaveService save, ContentDatabaseService content, PlayerProgressionService progression, EquipmentService equipment, ToastService toast)
        {
            saveService = save;
            contentService = content;
            progressionService = progression;
            equipmentService = equipment;
            toastService = toast;
            EnsureSaveState();
            ResetDailyIfNeeded();
            AutoStartEligibleQuests();
            SaveNow();
        }

        public QuestStatus GetQuestStatus(string questId) => GetOrCreateQuestData(questId)?.status ?? QuestStatus.Locked;

        public bool StartQuest(string questId)
        {
            QuestDefinition quest = Database?.GetQuestById(questId);
            PlayerQuestData data = GetOrCreateQuestData(questId);
            if (quest == null || data == null || !RequirementsMet(quest)) return false;
            if (data.status == QuestStatus.Locked || data.status == QuestStatus.Available)
            {
                data.status = QuestStatus.Active;
                data.startedAt = Now();
                EnsureProgressEntries(quest, data);
                SaveNowChanged();
                return true;
            }
            return false;
        }

        public void TrackProgress(QuestObjectiveType type, string targetId, int amount = 1)
        {
            if (Save == null || Database == null || amount <= 0) return;
            AutoStartEligibleQuests();
            bool changed = false;
            foreach (QuestDefinition quest in Database.quests ?? new List<QuestDefinition>())
            {
                if (quest == null) continue;
                PlayerQuestData data = GetOrCreateQuestData(quest.id);
                if (data == null || data.status != QuestStatus.Active) continue;
                bool changedThisQuest = false;
                EnsureProgressEntries(quest, data);
                for (int i = 0; i < quest.objectives.Count; i++)
                {
                    QuestObjectiveData objective = quest.objectives[i];
                    if (objective == null || objective.objectiveType != type) continue;
                    if (!TargetMatches(objective.targetId, targetId)) continue;
                    QuestObjectiveProgressData progress = data.objectiveProgress.Find(p => p.objectiveIndex == i);
                    if (progress == null || progress.completed) continue;
                    progress.currentAmount = Mathf.Min(Mathf.Max(1, objective.requiredAmount), progress.currentAmount + amount);
                    progress.completed = progress.currentAmount >= Mathf.Max(1, objective.requiredAmount);
                    changed = true;
                    changedThisQuest = true;
                }
                if (changedThisQuest && IsQuestComplete(quest, data)) CompleteQuest(quest.id);
            }
            if (changed) SaveNowChanged();
        }

        public void CompleteQuest(string questId)
        {
            QuestDefinition quest = Database?.GetQuestById(questId);
            PlayerQuestData data = GetOrCreateQuestData(questId);
            if (quest == null || data == null || data.status == QuestStatus.Completed || data.status == QuestStatus.Claimed) return;
            data.status = QuestStatus.Completed;
            data.completedAt = Now();
            QuestCompleted?.Invoke(questId);
            toastService?.ShowToast("Quest complete: " + quest.displayName);
            if (quest.autoClaim) ClaimQuest(questId);
            UnlockNextQuests();
        }

        public bool CanClaimQuest(string questId) => GetOrCreateQuestData(questId)?.status == QuestStatus.Completed;

        public bool ClaimQuest(string questId)
        {
            QuestDefinition quest = Database?.GetQuestById(questId);
            PlayerQuestData data = GetOrCreateQuestData(questId);
            if (quest == null || data == null || data.status != QuestStatus.Completed) return false;
            foreach (QuestRewardData reward in quest.rewards ?? new List<QuestRewardData>()) GrantReward(reward);
            data.status = QuestStatus.Claimed;
            data.claimedAt = Now();
            UnlockNextQuests();
            SaveNowChanged();
            toastService?.ShowToast("Quest reward claimed.");
            return true;
        }

        public List<QuestDefinition> GetActiveQuests() => GetQuestsByStatus(QuestStatus.Active);
        public List<QuestDefinition> GetAvailableQuests() => GetQuestsByStatus(QuestStatus.Available);
        public List<QuestDefinition> GetCompletedUnclaimedQuests() => GetQuestsByStatus(QuestStatus.Completed);
        public bool HasClaimableQuests() => GetCompletedUnclaimedQuests().Count > 0;

        public void UnlockNextQuests()
        {
            if (Database?.quests == null) return;
            foreach (QuestDefinition quest in Database.quests)
            {
                PlayerQuestData data = GetOrCreateQuestData(quest.id);
                if (data.status == QuestStatus.Locked && RequirementsMet(quest)) data.status = quest.autoStart ? QuestStatus.Active : QuestStatus.Available;
                if (data.status == QuestStatus.Active) EnsureProgressEntries(quest, data);
            }
            SaveNowChanged();
        }

        public QuestDefinition GetTrackerQuest()
        {
            List<QuestDefinition> active = GetActiveQuests();
            return active.OrderBy(q => TrackerPriority(q.questType)).ThenBy(q => q.order).FirstOrDefault();
        }

        public string BuildQuestProgressText(QuestDefinition quest)
        {
            if (quest == null) return "No active quest.";
            PlayerQuestData data = GetOrCreateQuestData(quest.id);
            EnsureProgressEntries(quest, data);
            string text = quest.displayName + "\n" + quest.description + "\n";
            for (int i = 0; i < quest.objectives.Count; i++)
            {
                QuestObjectiveData objective = quest.objectives[i];
                QuestObjectiveProgressData progress = data.objectiveProgress.Find(p => p.objectiveIndex == i);
                text += $"{(objective != null ? objective.description : "Objective")} {progress?.currentAmount ?? 0}/{Mathf.Max(1, objective != null ? objective.requiredAmount : 1)}\n";
            }
            return text.TrimEnd();
        }

        private void AutoStartEligibleQuests()
        {
            if (Database?.quests == null) return;
            foreach (QuestDefinition quest in Database.GetAutoStartQuests())
            {
                PlayerQuestData data = GetOrCreateQuestData(quest.id);
                if ((data.status == QuestStatus.Locked || data.status == QuestStatus.Available) && RequirementsMet(quest)) StartQuest(quest.id);
            }
        }

        private void ResetDailyIfNeeded()
        {
            if (Save == null) return;
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (Save.lastDailyResetDate == today) return;
            Save.lastDailyResetDate = today;
            foreach (QuestDefinition quest in Database?.GetQuestsByType(QuestType.Daily) ?? new List<QuestDefinition>())
            {
                PlayerQuestData data = GetOrCreateQuestData(quest.id);
                data.status = QuestStatus.Active;
                data.startedAt = Now();
                data.completedAt = 0;
                data.claimedAt = 0;
                data.objectiveProgress.Clear();
                EnsureProgressEntries(quest, data);
            }
        }

        private PlayerQuestData GetOrCreateQuestData(string questId)
        {
            if (Save == null || string.IsNullOrEmpty(questId)) return null;
            EnsureSaveState();
            PlayerQuestData data = Save.quests.Find(q => q.questId == questId);
            if (data != null) return data;
            QuestDefinition quest = Database?.GetQuestById(questId);
            data = new PlayerQuestData { questId = questId, status = quest != null && RequirementsMet(quest) ? (quest.autoStart ? QuestStatus.Active : QuestStatus.Available) : QuestStatus.Locked };
            if (data.status == QuestStatus.Active) data.startedAt = Now();
            Save.quests.Add(data);
            if (quest != null) EnsureProgressEntries(quest, data);
            return data;
        }

        private void EnsureProgressEntries(QuestDefinition quest, PlayerQuestData data)
        {
            if (quest == null || data == null) return;
            if (data.objectiveProgress == null) data.objectiveProgress = new List<QuestObjectiveProgressData>();
            for (int i = 0; i < (quest.objectives?.Count ?? 0); i++)
            {
                if (data.objectiveProgress.Exists(p => p.objectiveIndex == i)) continue;
                data.objectiveProgress.Add(new QuestObjectiveProgressData { objectiveIndex = i });
            }
        }

        private bool IsQuestComplete(QuestDefinition quest, PlayerQuestData data)
        {
            if (quest.objectives == null || quest.objectives.Count == 0) return false;
            EnsureProgressEntries(quest, data);
            return data.objectiveProgress.Where(p => p.objectiveIndex < quest.objectives.Count).All(p => p.completed);
        }

        private bool RequirementsMet(QuestDefinition quest)
        {
            foreach (string required in quest.requiredQuestIds ?? new List<string>())
            {
                QuestStatus status = GetQuestStatus(required);
                if (status != QuestStatus.Completed && status != QuestStatus.Claimed) return false;
            }
            return true;
        }

        private static bool TargetMatches(string objectiveTarget, string actualTarget)
        {
            return string.IsNullOrEmpty(objectiveTarget) || objectiveTarget == "any" || objectiveTarget == actualTarget;
        }

        private void GrantReward(QuestRewardData reward)
        {
            if (reward == null || progressionService == null) return;
            int amount = Mathf.Max(1, reward.amount);
            switch (reward.rewardType)
            {
                case QuestRewardType.Gold: progressionService.AddGold(amount); break;
                case QuestRewardType.SoulGem: progressionService.AddSoulGem(amount); break;
                case QuestRewardType.EXP: progressionService.AddExp(amount); break;
                case QuestRewardType.Item: progressionService.AddItem(reward.itemId, amount); break;
                case QuestRewardType.Equipment:
                    EquipmentInstanceData equipment = equipmentService != null ? equipmentService.CreateEquipmentInstance(reward.equipmentId) : null;
                    if (equipment != null) progressionService.AddEquipment(equipment);
                    break;
            }
        }

        private List<QuestDefinition> GetQuestsByStatus(QuestStatus status)
        {
            if (Database?.quests == null) return new List<QuestDefinition>();
            return Database.quests.Where(q => q != null && GetQuestStatus(q.id) == status).OrderBy(q => q.order).ToList();
        }

        private static int TrackerPriority(QuestType type)
        {
            if (type == QuestType.Tutorial) return 0;
            if (type == QuestType.Main) return 1;
            if (type == QuestType.Daily) return 2;
            return 3;
        }

        private void EnsureSaveState()
        {
            if (Save == null) return;
            if (Save.quests == null) Save.quests = new List<PlayerQuestData>();
            if (Save.completedTutorialStepIds == null) Save.completedTutorialStepIds = new List<string>();
            if (string.IsNullOrEmpty(Save.lastDailyResetDate)) Save.lastDailyResetDate = DateTime.Now.ToString("yyyy-MM-dd");
        }

        private void SaveNowChanged()
        {
            SaveNow();
            QuestsChanged?.Invoke();
        }

        private void SaveNow() => saveService?.SaveNow();
        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
