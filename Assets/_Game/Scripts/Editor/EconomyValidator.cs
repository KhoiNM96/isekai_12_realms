using System.Collections.Generic;
using Isekai12Realms.Data;
using Isekai12Realms.Economy;
using Isekai12Realms.Inventory;
using Isekai12Realms.Shop;

namespace Isekai12Realms.Editor
{
    public static class EconomyValidator
    {
        public static void Validate(GameContentDatabase db, List<string> errors)
        {
            if (db == null || errors == null) return;
            HashSet<string> shopIds = new HashSet<string>();
            HashSet<string> itemIds = new HashSet<string>();
            foreach (ShopDefinition shop in db.shops ?? new List<ShopDefinition>())
            {
                if (shop == null) continue;
                if (string.IsNullOrEmpty(shop.id)) errors.Add("Shop missing id.");
                else if (!shopIds.Add(shop.id)) errors.Add("Duplicate shop id: " + shop.id);
                foreach (ShopItemDefinition item in shop.items ?? new List<ShopItemDefinition>())
                {
                    if (item == null) continue;
                    if (string.IsNullOrEmpty(item.id)) errors.Add("Shop item missing id in " + shop.id);
                    else if (!itemIds.Add(item.id)) errors.Add("Duplicate shop item id: " + item.id);
                    if (item.purchaseLimitPerDay < 0) errors.Add("Daily limit negative: " + item.id);
                    if (item.purchaseLimitLifetime < 0) errors.Add("Lifetime limit negative: " + item.id);
                    if (!item.enabled) continue;
                    if (item.priceAmount < 0) errors.Add("Shop item price negative: " + item.id);
                    if (shop.shopType == ShopType.GoldShop && item.priceCurrency != CurrencyType.Gold) errors.Add("GoldShop item must cost Gold: " + item.id);
                    if ((shop.shopType == ShopType.GemShop || shop.shopType == ShopType.Cosmetic) && item.priceCurrency != CurrencyType.SoulGem) errors.Add("Gem/Cosmetic item must cost SoulGem: " + item.id);
                    if (shop.shopType == ShopType.IAPPlaceholder && item.itemType != ShopItemType.SoulGem && item.itemType != ShopItemType.Placeholder) errors.Add("IAP placeholder cannot grant non-SoulGem shop item: " + item.id);
                    if (item.itemType == ShopItemType.InventorySlot && item.amount <= 0) errors.Add("Inventory slot purchase amount <= 0: " + item.id);
                    if (item.itemType == ShopItemType.Item && string.IsNullOrEmpty(item.itemId)) errors.Add("Shop item missing itemId: " + item.id);
                    if (item.itemType == ShopItemType.Item && !PrototypeItemDatabase.Exists(item.itemId)) errors.Add("Shop item references missing item: " + item.id);
                    if (item.itemType == ShopItemType.Equipment && db.GetEquipmentDefinitionById(item.equipmentId) == null) errors.Add("Shop item references missing equipment: " + item.id);
                    if (item.itemType == ShopItemType.Equipment && shop.shopType != ShopType.Daily && shop.shopType != ShopType.GoldShop) errors.Add("Normal shop should not sell equipment directly: " + item.id);
                }
            }

            HashSet<string> productIds = new HashSet<string>();
            foreach (IAPProductDefinition product in db.iapProducts ?? new List<IAPProductDefinition>())
            {
                if (product == null) continue;
                if (string.IsNullOrEmpty(product.productId)) errors.Add("IAP product missing productId.");
                else if (!productIds.Add(product.productId)) errors.Add("Duplicate IAP product id: " + product.productId);
                if (product.enabled && product.soulGemAmount + product.bonusSoulGemAmount <= 0) errors.Add("IAP product grants no Soul Gems: " + product.productId);
            }
        }
    }
}
