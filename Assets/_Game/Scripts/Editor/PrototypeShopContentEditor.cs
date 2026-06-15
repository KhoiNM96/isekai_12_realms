using System.Collections.Generic;
using System.IO;
using Isekai12Realms.Data;
using Isekai12Realms.Economy;
using Isekai12Realms.Shop;
using UnityEditor;
using UnityEngine;

namespace Isekai12Realms.Editor
{
    public static class PrototypeShopContentEditor
    {
        private const string ShopPath = "Assets/_Game/ScriptableObjects/Shop";
        private const string EconomyPath = "Assets/_Game/ScriptableObjects/Economy";
        private const string DatabasePath = "Assets/_Game/ScriptableObjects/GameContentDatabase.asset";

        [MenuItem("Tools/Isekai 12 Realms/Create Prototype Shop Content")]
        public static void CreatePrototypeShopContent()
        {
            EnsureFolders();
            ShopDefinition daily = Shop("shop_daily", "Daily Shop", ShopType.Daily, true, 4,
                Item("daily_potion_small", "Small Potion", "A simple potion for early adventures.", ShopItemType.Item, "item_potion_small", 1, CurrencyType.Gold, 50, 3, 0),
                Item("daily_food_basket", "Food Basket", "A packed basket for long battles.", ShopItemType.Item, "item_food_basket", 1, CurrencyType.Gold, 80, 2, 0),
                Item("daily_skill_scroll", "Skill Scroll", "A scroll used by skill upgrades.", ShopItemType.Item, "item_skill_scroll", 1, CurrencyType.Gold, 250, 1, 0));
            ShopDefinition gold = Shop("shop_gold", "Gold Shop", ShopType.GoldShop, false, 4,
                Item("gold_potion_small", "Small Potion", "A reliable low-cost potion.", ShopItemType.Item, "item_potion_small", 1, CurrencyType.Gold, 70, 0, 0),
                Item("gold_shuffle_bell", "Shuffle Bell", "A small bell with board-shuffling magic.", ShopItemType.Item, "item_shuffle_bell", 1, CurrencyType.Gold, 120, 0, 0));
            ShopDefinition gem = Shop("shop_gem", "Gem Shop", ShopType.GemShop, false, 4,
                Reward("gem_gold_pack_small", "Small Gold Pack", "Trade Soul Gems for 1000 Gold.", ShopItemType.Gold, 1000, CurrencyType.SoulGem, 50, 3, 0),
                Reward("gem_inventory_slots_10", "Inventory Slots +10", "Increase inventory capacity by 10.", ShopItemType.InventorySlot, 10, CurrencyType.SoulGem, 100, 0, 5),
                Item("gem_lucky_cookie", "Lucky Cookie", "A treat for future lucky effects.", ShopItemType.Item, "item_lucky_cookie", 1, CurrencyType.SoulGem, 30, 5, 0));
            ShopDefinition cosmetic = Shop("shop_cosmetic", "Cosmetic Shop", ShopType.Cosmetic, false, 4,
                Cosmetic("cosmetic_board_skin_meadow", "Meadow Board Skin", "Cosmetic placeholder board skin.", "cosmetic_board_skin_meadow", 120),
                Cosmetic("cosmetic_hero_aura_cyan", "Cyan Hero Aura", "Cosmetic placeholder hero aura.", "cosmetic_hero_aura_cyan", 150));
            ShopDefinition iap = Shop("shop_iap_placeholder", "IAP Placeholder", ShopType.IAPPlaceholder, false, 4);

            IAPProductDefinition tiny = Product("gems_tiny", "Tiny Gem Pack", "120 Soul Gems", "com.isekai12realms.gems_tiny", 120, 0, "$0.99");
            IAPProductDefinition small = Product("gems_small", "Small Gem Pack", "400 Soul Gems + 40 bonus", "com.isekai12realms.gems_small", 400, 40, "$2.99");
            IAPProductDefinition medium = Product("gems_medium", "Medium Gem Pack", "750 Soul Gems + 180 bonus", "com.isekai12realms.gems_medium", 750, 180, "$4.99");
            IAPProductDefinition large = Product("gems_large", "Large Gem Pack", "1650 Soul Gems + 500 bonus", "com.isekai12realms.gems_large", 1650, 500, "$9.99");
            IAPProductDefinition mega = Product("gems_mega", "Mega Gem Pack", "3600 Soul Gems + 1800 bonus", "com.isekai12realms.gems_mega", 3600, 1800, "$19.99");

            GameContentDatabase db = LoadOrCreate<GameContentDatabase>(DatabasePath);
            db.shops = new List<ShopDefinition> { daily, gold, gem, cosmetic, iap };
            db.iapProducts = new List<IAPProductDefinition> { tiny, small, medium, large, mega };
            EditorUtility.SetDirty(db);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Shop] Prototype shop content created/updated.");
        }

        private static ShopDefinition Shop(string id, string name, ShopType type, bool daily, int refreshHour, params ShopItemDefinition[] items)
        {
            ShopDefinition shop = LoadOrCreate<ShopDefinition>($"{ShopPath}/{id}.asset");
            shop.id = id; shop.displayName = name; shop.shopType = type; shop.refreshDaily = daily; shop.refreshHourLocal = refreshHour; shop.iconAssetId = id; shop.items = new List<ShopItemDefinition>(items);
            EditorUtility.SetDirty(shop);
            return shop;
        }

        private static ShopItemDefinition Item(string id, string name, string description, ShopItemType type, string itemId, int amount, CurrencyType priceCurrency, int price, int dailyLimit, int lifetimeLimit)
        {
            return new ShopItemDefinition { id = id, displayName = name, description = description, iconAssetId = itemId, itemType = type, itemId = itemId, amount = amount, priceCurrency = priceCurrency, priceAmount = price, purchaseLimitPerDay = dailyLimit, purchaseLimitLifetime = lifetimeLimit, enabled = true };
        }

        private static ShopItemDefinition Reward(string id, string name, string description, ShopItemType type, int amount, CurrencyType priceCurrency, int price, int dailyLimit, int lifetimeLimit)
        {
            return new ShopItemDefinition { id = id, displayName = name, description = description, iconAssetId = type == ShopItemType.Gold ? "currency_gold" : id, itemType = type, amount = amount, priceCurrency = priceCurrency, priceAmount = price, purchaseLimitPerDay = dailyLimit, purchaseLimitLifetime = lifetimeLimit, enabled = true };
        }

        private static ShopItemDefinition Cosmetic(string id, string name, string description, string cosmeticId, int price)
        {
            return new ShopItemDefinition { id = id, displayName = name, description = description, iconAssetId = cosmeticId, itemType = ShopItemType.Cosmetic, cosmeticId = cosmeticId, amount = 1, priceCurrency = CurrencyType.SoulGem, priceAmount = price, purchaseLimitLifetime = 1, enabled = true };
        }

        private static IAPProductDefinition Product(string id, string name, string description, string platformId, int gems, int bonus, string price)
        {
            IAPProductDefinition product = LoadOrCreate<IAPProductDefinition>($"{EconomyPath}/{id}.asset");
            product.productId = id; product.displayName = name; product.description = description; product.platformProductId = platformId; product.soulGemAmount = gems; product.bonusSoulGemAmount = bonus; product.priceTextPlaceholder = price; product.enabled = true;
            EditorUtility.SetDirty(product);
            return product;
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists(ShopPath)) Directory.CreateDirectory(ShopPath);
            if (!Directory.Exists(EconomyPath)) Directory.CreateDirectory(EconomyPath);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
