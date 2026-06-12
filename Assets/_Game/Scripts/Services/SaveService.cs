using System;
using System.IO;
using UnityEngine;
using Isekai12Realms.Data;

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
    }

    public class SaveService : ISaveService
    {
        private const string SaveFileName = "save_v1.json";
        private const string BackupFileName = "save_v1_backup.json";
        
        public PlayerSaveData CurrentSave { get; private set; }

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private string BackupFilePath => Path.Combine(Application.persistentDataPath, BackupFileName);

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
                    EnsureSaveDefaults(CurrentSave);
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
                            EnsureSaveDefaults(CurrentSave);
                            File.Copy(BackupFilePath, SaveFilePath, true);
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

            if (data.completedStageIds == null)
            {
                data.completedStageIds = new System.Collections.Generic.List<string>();
            }

            if (data.stageProgress == null)
            {
                data.stageProgress = new System.Collections.Generic.List<Isekai12Realms.Stages.StageProgressData>();
            }

            if (string.IsNullOrEmpty(data.playerName)) data.playerName = "Guest Hero";
            if (string.IsNullOrEmpty(data.selectedClassId)) data.selectedClassId = "flame_squire";
            if (string.IsNullOrEmpty(data.currentRealmId)) data.currentRealmId = "realm_01_meadow";
            if (string.IsNullOrEmpty(data.currentStageId)) data.currentStageId = "stage_01_01";
            if (data.maxHp <= 0) data.maxHp = 100;
            if (data.maxMana <= 0) data.maxMana = 100;
            if (data.level <= 0) data.level = 1;
        }
    }
}
