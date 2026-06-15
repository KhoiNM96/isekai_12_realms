using System;
using System.Collections.Generic;
using Isekai12Realms.Equipment;
using Isekai12Realms.Inventory;
using Isekai12Realms.Quests;
using Isekai12Realms.Shop;
using Isekai12Realms.Skills;
using Isekai12Realms.Stages;

namespace Isekai12Realms.Data
{
    [Serializable]
    public class PlayerSaveData
    {
        public int schemaVersion = 1;
        public long saveVersion;
        public string playerId = "";
        public string localGuestId = "";
        public string playerName = "Guest Hero";

        public int level = 1;
        public long exp = 0;
        public int gold = 0;
        public int soulGem = 0;
        public int maxHp = 100;
        public int hp = 100;
        public int maxMana = 100;
        public int mana = 0;
        public string selectedClassId = "flame_squire";

        public string currentRealmId = "realm_01_meadow";
        public string currentStageId = "stage_01_01";
        public List<string> completedStageIds = new List<string>();
        public List<StageProgressData> stageProgress = new List<StageProgressData>();
        public InventorySaveData inventory = new InventorySaveData();
        public EquipmentLoadoutData equipment = new EquipmentLoadoutData();
        public List<PlayerSkillData> skills = new List<PlayerSkillData>();
        public string equippedSkill1Id = "skill_flame_spark_slash";
        public string equippedSkill2Id = "skill_flame_shuffle_bell";
        public string equippedUltimateId = "skill_flame_realm_burst";
        public List<PlayerQuestData> quests = new List<PlayerQuestData>();
        public List<string> completedTutorialStepIds = new List<string>();
        public bool tutorialEnabled = true;
        public string activeTutorialId = "";
        public string activeTutorialStepId = "";
        public string lastDailyResetDate = "";
        public List<PurchaseRecord> purchaseRecords = new List<PurchaseRecord>();
        public List<ShopPurchaseLimitData> shopPurchaseLimits = new List<ShopPurchaseLimitData>();
        public int inventoryExtraSlots;
        public string lastDailyShopRefreshDate = "";
        public string firebaseUid = "";
        public string authProvider = "LocalOnly";
        public string cloudSaveId = "default";
        public long lastCloudUploadAt;
        public long lastCloudDownloadAt;
        public string lastSyncedDeviceId = "";
        public bool cloudSyncEnabled = true;
        public string deviceId = "";
        public long totalPlaySeconds;

        public long createdAt;
        public long updatedAt;
        public string checksum = "";
    }
}
