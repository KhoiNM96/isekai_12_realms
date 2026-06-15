using System;
using System.IO;
using UnityEngine;
using Isekai12Realms.Data;
using Isekai12Realms.Quests;
using Isekai12Realms.Shop;
using Isekai12Realms.Skills;

namespace Isekai12Realms.Services
{
    public interface ISaveService
    {
        PlayerSaveData CurrentSave { get; }
        void LoadOrCreateSave();
        void SaveNow();
        void SaveGame();
        void DeleteSave();
        bool HasSave();
        PlayerSaveData CreateNewSave();
        string SaveFilePath { get; }
        string BackupFilePath { get; }
        string CloudDownloadBackupPath { get; }
        void BackupBeforeCloudDownload();
        bool ReplaceCurrentSave(PlayerSaveData save);
        string ExportCurrentSaveJson();
    }

    public class SaveService : ISaveService
    {
        private const string SaveFileName = "save_v1.json";
        private const string BackupFileName = "save_v1_backup.json";
        
        public PlayerSaveData CurrentSave { get; private set; }

        public string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        public string BackupFilePath => Path.Combine(Application.persistentDataPath, BackupFileName);
        public string CloudDownloadBackupPath => Path.Combine(Application.persistentDataPath, "save_v1_before_cloud_download.json");

        public bool HasSave()
        {
            return File.Exists(SaveFilePath);
        }

        public void LoadOrCreateSave()
        {
            if (HasSave())
            {
                try
                {
                    CurrentSave = LoadSaveFromFile(SaveFilePath);
                    bool skillMigrationNeeded = NeedsSkillMigration(CurrentSave);
                    EnsureSaveDefaults(CurrentSave);
                    if (skillMigrationNeeded) SaveNow();
                    Debug.Log("[Save] Loaded save");
                    return;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Save] Main save failed, trying backup: {e.Message}");
                    if (File.Exists(BackupFilePath))
                    {
                        try
                        {
                            CurrentSave = LoadSaveFromFile(BackupFilePath);
                            bool skillMigrationNeeded = NeedsSkillMigration(CurrentSave);
                            EnsureSaveDefaults(CurrentSave);
                            File.Copy(BackupFilePath, SaveFilePath, true);
                            if (skillMigrationNeeded) SaveNow();
                            Debug.Log("[Save] Backup restored");
                            return;
                        }
                        catch (Exception backupEx)
                        {
                            Debug.LogError($"Failed to load backup save file: {backupEx.Message}");
                        }
                    }
                }
            }

            CreateNewSave();
        }

        public void SaveNow()
        {
            if (CurrentSave == null)
            {
                Debug.LogWarning("Cannot save game: CurrentSave is null.");
                return;
            }

            CurrentSave.saveVersion++;
            CurrentSave.updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            CurrentSave.checksum = CalculateChecksum(CurrentSave);

            try
            {
                // Write backup first before overwriting primary
                if (File.Exists(SaveFilePath))
                {
                    File.Copy(SaveFilePath, BackupFilePath, true);
                }

                string json = JsonUtility.ToJson(CurrentSave, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log("[Save] Saved successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving game: {e.Message}");
            }
        }

        public void SaveGame()
        {
            SaveNow();
        }

        public void DeleteSave()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }
            if (File.Exists(BackupFilePath))
            {
                File.Delete(BackupFilePath);
            }
            CurrentSave = null;
            Debug.Log("[Save] Deleted local save");
        }

        public PlayerSaveData CreateNewSave()
        {
            CurrentSave = new PlayerSaveData
            {
                playerId = Guid.NewGuid().ToString(),
                localGuestId = "guest_" + Guid.NewGuid().ToString().Substring(0, 8),
                createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                updatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            EnsureSaveDefaults(CurrentSave);
            CurrentSave.checksum = CalculateChecksum(CurrentSave);
            SaveNow();
            Debug.Log("[Save] Created new save");
            return CurrentSave;
        }

        public void BackupBeforeCloudDownload()
        {
            try
            {
                if (File.Exists(SaveFilePath)) File.Copy(SaveFilePath, CloudDownloadBackupPath, true);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Save] Could not create cloud download backup: " + e.Message);
            }
        }

        public bool ReplaceCurrentSave(PlayerSaveData save)
        {
            if (save == null) return false;
            BackupBeforeCloudDownload();
            CurrentSave = save;
            EnsureSaveDefaults(CurrentSave);
            CurrentSave.lastCloudDownloadAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveNow();
            return true;
        }

        public string ExportCurrentSaveJson()
        {
            return CurrentSave != null ? JsonUtility.ToJson(CurrentSave, true) : string.Empty;
        }

        private PlayerSaveData LoadSaveFromFile(string path)
        {
            string json = File.ReadAllText(path);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

            if (!ValidateChecksum(data))
            {
                Debug.LogWarning("Save data checksum mismatch! File might have been modified.");
            }

            return data;
        }

        private string CalculateChecksum(PlayerSaveData data)
        {
            return $"hash_{data.playerId}_{data.saveVersion}_{data.gold}";
        }

        private bool ValidateChecksum(PlayerSaveData data)
        {
            return string.IsNullOrEmpty(data.checksum) || data.checksum == CalculateChecksum(data);
        }

        private static bool NeedsSkillMigration(PlayerSaveData data)
        {
            return data == null ||
                   data.skills == null ||
                   data.skills.Count == 0 ||
                   string.IsNullOrEmpty(data.equippedSkill1Id) ||
                   string.IsNullOrEmpty(data.equippedSkill2Id) ||
                   string.IsNullOrEmpty(data.equippedUltimateId);
        }

        private void EnsureSaveDefaults(PlayerSaveData data)
        {
            if (data.inventory == null)
            {
                data.inventory = new Isekai12Realms.Inventory.InventorySaveData();
            }

            if (data.equipment == null)
            {
                data.equipment = new Isekai12Realms.Equipment.EquipmentLoadoutData();
            }

            if (data.inventory.items == null)
            {
                data.inventory.items = new System.Collections.Generic.List<Isekai12Realms.Inventory.ItemStackData>();
            }

            if (data.inventory.equipments == null)
            {
                data.inventory.equipments = new System.Collections.Generic.List<Isekai12Realms.Equipment.EquipmentInstanceData>();
            }

            if (data.completedStageIds == null)
            {
                data.completedStageIds = new System.Collections.Generic.List<string>();
            }

            if (data.stageProgress == null)
            {
                data.stageProgress = new System.Collections.Generic.List<Isekai12Realms.Stages.StageProgressData>();
            }

            if (data.skills == null)
            {
                data.skills = new System.Collections.Generic.List<PlayerSkillData>();
            }

            if (data.quests == null)
            {
                data.quests = new System.Collections.Generic.List<PlayerQuestData>();
            }

            if (data.completedTutorialStepIds == null)
            {
                data.completedTutorialStepIds = new System.Collections.Generic.List<string>();
            }

            if (string.IsNullOrEmpty(data.lastDailyResetDate)) data.lastDailyResetDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (data.purchaseRecords == null) data.purchaseRecords = new System.Collections.Generic.List<PurchaseRecord>();
            if (data.shopPurchaseLimits == null) data.shopPurchaseLimits = new System.Collections.Generic.List<ShopPurchaseLimitData>();
            if (string.IsNullOrEmpty(data.lastDailyShopRefreshDate)) data.lastDailyShopRefreshDate = DateTime.Now.ToString("yyyy-MM-dd");
            bool missingCloudAuthFields = string.IsNullOrEmpty(data.authProvider);
            if (missingCloudAuthFields) data.cloudSyncEnabled = true;
            if (string.IsNullOrEmpty(data.authProvider)) data.authProvider = "LocalOnly";
            if (string.IsNullOrEmpty(data.cloudSaveId)) data.cloudSaveId = "default";
            if (string.IsNullOrEmpty(data.deviceId)) data.deviceId = "device_" + Guid.NewGuid().ToString("N");

            if (string.IsNullOrEmpty(data.playerName)) data.playerName = "Guest Hero";
            if (string.IsNullOrEmpty(data.selectedClassId)) data.selectedClassId = "flame_squire";
            EnsureSkillDefaults(data);
            if (string.IsNullOrEmpty(data.currentRealmId)) data.currentRealmId = "realm_01_meadow";
            if (string.IsNullOrEmpty(data.currentStageId)) data.currentStageId = "stage_01_01";
            if (data.maxHp <= 0) data.maxHp = 100;
            if (data.maxMana <= 0) data.maxMana = 100;
            if (data.level <= 0) data.level = 1;
            foreach (Isekai12Realms.Equipment.EquipmentInstanceData equipment in data.inventory.equipments)
            {
                if (equipment == null) continue;
                if (string.IsNullOrEmpty(equipment.instanceId)) equipment.instanceId = Guid.NewGuid().ToString();
                if (equipment.level <= 0) equipment.level = 1;
                if (equipment.acquiredAt <= 0) equipment.acquiredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            foreach (PurchaseRecord record in data.purchaseRecords)
            {
                if (record == null) continue;
                if (string.IsNullOrEmpty(record.platformProductId)) record.platformProductId = record.productId;
                if (record.totalGranted <= 0) record.totalGranted = record.amount + record.bonusAmount;
                if (record.totalGranted <= 0) record.totalGranted = record.amount;
                if (record.grantedAt <= 0 && record.granted) record.grantedAt = record.purchasedAt;
            }
        }

        private void EnsureSkillDefaults(PlayerSaveData data)
        {
            if (data.skills.Count == 0)
            {
                AddDefaultSkill(data, "skill_flame_spark_slash");
                AddDefaultSkill(data, "skill_flame_shuffle_bell");
                AddDefaultSkill(data, "skill_flame_realm_burst");
            }

            if (string.IsNullOrEmpty(data.equippedSkill1Id)) data.equippedSkill1Id = "skill_flame_spark_slash";
            if (string.IsNullOrEmpty(data.equippedSkill2Id)) data.equippedSkill2Id = "skill_flame_shuffle_bell";
            if (string.IsNullOrEmpty(data.equippedUltimateId)) data.equippedUltimateId = "skill_flame_realm_burst";
        }

        private void AddDefaultSkill(PlayerSaveData data, string skillId)
        {
            if (data.skills.Exists(s => s.skillId == skillId)) return;
            data.skills.Add(new PlayerSkillData { skillId = skillId, level = 1, unlocked = true, cooldownRemaining = 0 });
        }
    }
}
