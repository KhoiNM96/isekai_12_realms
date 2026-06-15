using System;
using System.Collections.Generic;
using Isekai12Realms.Character;
using Isekai12Realms.Data;
using Isekai12Realms.Economy;
using Isekai12Realms.Equipment;
using Isekai12Realms.Inventory;
using Isekai12Realms.Quests;
using Isekai12Realms.RemoteConfig;
using Isekai12Realms.Services;
using Isekai12Realms.Stages;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Shop
{
    public class ShopService : MonoBehaviour
    {
        private ISaveService saveService;
        private ContentDatabaseService contentService;
        private PlayerProgressionService progressionService;
        private EquipmentService equipmentService;
        private QuestService questService;
        private ToastService toastService;
        private GameConfigService gameConfigService;

        public event Action Changed;

        private PlayerSaveData Save => saveService?.CurrentSave;
        private GameContentDatabase Database => contentService?.Database;

        public void Initialize(ISaveService save, ContentDatabaseService content, PlayerProgressionService progression, EquipmentService equipment, QuestService quests, ToastService toast, GameConfigService config = null)
        {
            saveService = save;
            contentService = content;
            progressionService = progression;
            equipmentService = equipment;
            questService = quests;
            toastService = toast;
            gameConfigService = config;
            EnsureSaveState();
            CheckDailyRefresh();
        }

        public List<ShopItemDefinition> GetShopItems(ShopType type)
        {
            ShopDefinition shop = Database?.GetShopByType(type);
            return shop != null && shop.items != null ? shop.items.FindAll(i => i != null) : new List<ShopItemDefinition>();
        }

        public bool CanPurchase(ShopItemDefinition item) => string.IsNullOrEmpty(GetPurchaseBlocker(item));

        public string GetPurchaseBlocker(ShopItemDefinition item)
        {
            EnsureSaveState();
            if (Save == null) return "Save not ready.";
            if (item == null || !item.enabled) return "Item unavailable.";
            if (string.IsNullOrEmpty(item.id)) return "Invalid shop item.";
            if (item.priceAmount < 0) return "Invalid price.";
            if (item.purchaseLimitPerDay > 0 && GetPurchaseLimitData(item.id).dailyCount >= item.purchaseLimitPerDay) return "Sold out today.";
            if (item.purchaseLimitLifetime > 0 && GetPurchaseLimitData(item.id).lifetimeCount >= item.purchaseLimitLifetime) return "Sold out.";
            if (GetCurrency(item.priceCurrency) < item.priceAmount) return "Not enough " + CurrencyDisplayName(item.priceCurrency) + ".";
            if (item.itemType == ShopItemType.InventorySlot && item.amount <= 0) return "Invalid inventory slot amount.";
            if (item.itemType == ShopItemType.Placeholder) return "Coming soon.";
            return string.Empty;
        }

        public bool Purchase(ShopItemDefinition item)
        {
            string blocker = GetPurchaseBlocker(item);
            if (!string.IsNullOrEmpty(blocker))
            {
                toastService?.ShowToast(blocker);
                return false;
            }

            int amount = Mathf.Max(1, item.amount);
            AddCurrency(item.priceCurrency, -item.priceAmount);
            Grant(item, amount);
            ShopPurchaseLimitData limit = GetPurchaseLimitData(item.id);
            limit.dailyCount += 1;
            limit.lifetimeCount += 1;
            limit.dateKey = TodayKey();
            saveService.SaveNow();
            toastService?.ShowToast("Purchased: " + item.displayName);
            Changed?.Invoke();
            return true;
        }

        public void CheckDailyRefresh()
        {
            EnsureSaveState();
            ResetDailyLimitsIfNeeded();
        }

        public void ResetDailyLimitsIfNeeded()
        {
            if (Save == null) return;
            string today = TodayKey();
            if (Save.lastDailyShopRefreshDate == today) return;
            ResetDailyShop(false);
        }

        public void ResetDailyShop(bool showToast)
        {
            EnsureSaveState();
            if (Save == null) return;
            string today = TodayKey();
            foreach (ShopPurchaseLimitData limit in Save.shopPurchaseLimits)
            {
                if (limit == null) continue;
                limit.dailyCount = 0;
                limit.dateKey = today;
            }
            Save.lastDailyShopRefreshDate = today;
            saveService.SaveNow();
            if (showToast) toastService?.ShowToast("Daily shop reset.");
            Changed?.Invoke();
        }

        public ShopPurchaseLimitData GetPurchaseLimitData(string shopItemId)
        {
            EnsureSaveState();
            if (Save == null || string.IsNullOrEmpty(shopItemId)) return null;
            string today = TodayKey();
            ShopPurchaseLimitData limit = Save.shopPurchaseLimits.Find(l => l != null && l.shopItemId == shopItemId);
            if (limit == null)
            {
                limit = new ShopPurchaseLimitData { shopItemId = shopItemId, dateKey = today };
                Save.shopPurchaseLimits.Add(limit);
            }
            if (limit.dateKey != today)
            {
                limit.dailyCount = 0;
                limit.dateKey = today;
            }
            return limit;
        }

        public int GetCurrency(CurrencyType currency)
        {
            if (Save == null) return 0;
            switch (currency)
            {
                case CurrencyType.Gold: return Save.gold;
                case CurrencyType.SoulGem: return Save.soulGem;
                default: return 0;
            }
        }

        public static string CurrencyDisplayName(CurrencyType currency)
        {
            switch (currency)
            {
                case CurrencyType.SoulGem: return "Soul Gems";
                case CurrencyType.RealmToken: return "Realm Tokens";
                case CurrencyType.Material: return "Materials";
                default: return "Gold";
            }
        }

        private void Grant(ShopItemDefinition item, int amount)
        {
            switch (item.itemType)
            {
                case ShopItemType.Item:
                    progressionService?.AddItem(item.itemId, amount);
                    questService?.TrackProgress(QuestObjectiveType.CollectItem, item.itemId, amount);
                    break;
                case ShopItemType.Equipment:
                    EquipmentInstanceData equipment = equipmentService != null ? equipmentService.CreateEquipmentInstance(item.equipmentId) : PrototypeEquipmentFactory.Create(item.equipmentId);
                    progressionService?.AddEquipment(equipment);
                    questService?.TrackProgress(QuestObjectiveType.OwnEquipment, equipment.equipmentId, 1);
                    break;
                case ShopItemType.Gold:
                    AddCurrency(CurrencyType.Gold, amount);
                    questService?.TrackProgress(QuestObjectiveType.EarnGold, "any", amount);
                    break;
                case ShopItemType.SoulGem:
                    AddCurrency(CurrencyType.SoulGem, amount);
                    break;
                case ShopItemType.InventorySlot:
                    Save.inventoryExtraSlots += amount;
                    if (Save.inventory != null) Save.inventory.capacity += amount;
                    break;
                case ShopItemType.Cosmetic:
                    Save.purchaseRecords.Add(new PurchaseRecord { transactionId = "cosmetic_" + Now() + "_" + item.id, productId = item.cosmeticId, source = "shop_cosmetic_placeholder", amount = 1, purchasedAt = Now(), granted = true });
                    toastService?.ShowToast("Cosmetic placeholder unlocked.");
                    break;
            }
        }

        private void AddCurrency(CurrencyType currency, int amount)
        {
            if (Save == null) return;
            if (currency == CurrencyType.Gold)
            {
                Save.gold = Mathf.Max(0, Save.gold + amount);
                if (amount < 0) questService?.TrackProgress(QuestObjectiveType.SpendGold, "any", -amount);
            }
            else if (currency == CurrencyType.SoulGem)
            {
                Save.soulGem = Mathf.Max(0, Save.soulGem + amount);
            }
        }

        private void EnsureSaveState()
        {
            if (Save == null) return;
            if (Save.inventory == null) Save.inventory = new InventorySaveData();
            if (Save.inventory.items == null) Save.inventory.items = new List<ItemStackData>();
            if (Save.inventory.equipments == null) Save.inventory.equipments = new List<EquipmentInstanceData>();
            if (Save.purchaseRecords == null) Save.purchaseRecords = new List<PurchaseRecord>();
            if (Save.shopPurchaseLimits == null) Save.shopPurchaseLimits = new List<ShopPurchaseLimitData>();
            if (string.IsNullOrEmpty(Save.lastDailyShopRefreshDate)) Save.lastDailyShopRefreshDate = TodayKey();
        }

        private string TodayKey()
        {
            DateTime now = DateTime.Now;
            int refreshHour = gameConfigService != null ? gameConfigService.DailyShopRefreshHour : 4;
            if (now.Hour < refreshHour) now = now.AddDays(-1);
            return now.ToString("yyyy-MM-dd");
        }
        private static long Now() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
