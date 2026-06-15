using Isekai12Realms.Equipment;
using Isekai12Realms.Inventory;
using Isekai12Realms.Services;
using Isekai12Realms.UI;
using UnityEngine;

namespace Isekai12Realms.Crafting
{
    public class CraftingService : MonoBehaviour
    {
        private ISaveService saveService;
        private EquipmentService equipmentService;
        private ToastService toastService;

        public void Initialize(ISaveService save, EquipmentService equipment, ToastService toast)
        {
            saveService = save;
            equipmentService = equipment;
            toastService = toast;
        }

        public bool CanCraft(CraftingRecipeDefinition recipe)
        {
            return string.IsNullOrEmpty(GetCraftBlocker(recipe));
        }

        public bool CraftEquipment(CraftingRecipeDefinition recipe)
        {
            string blocker = GetCraftBlocker(recipe);
            if (!string.IsNullOrEmpty(blocker))
            {
                toastService?.ShowToast(blocker);
                return false;
            }

            saveService.CurrentSave.gold -= recipe.goldCost;
            foreach (CraftingRecipeItemCost cost in recipe.itemCosts)
            {
                if (cost == null || string.IsNullOrEmpty(cost.itemId) || cost.amount <= 0) continue;
                ItemStackData stack = saveService.CurrentSave.inventory.items.Find(i => i.itemId == cost.itemId);
                stack.amount -= cost.amount;
                if (stack.amount <= 0) saveService.CurrentSave.inventory.items.Remove(stack);
            }

            EquipmentInstanceData equipment = equipmentService.CreateEquipmentInstance(recipe.resultEquipmentId);
            saveService.CurrentSave.inventory.equipments.Add(equipment);
            saveService.SaveNow();
            toastService?.ShowToast("Equipment crafted!");
            return true;
        }

        private string GetCraftBlocker(CraftingRecipeDefinition recipe)
        {
            if (recipe == null) return "Recipe missing.";
            if (saveService?.CurrentSave == null) return "Save missing.";
            if (equipmentService == null) return "Equipment service missing.";
            if (string.IsNullOrEmpty(recipe.resultEquipmentId)) return "Recipe result missing.";
            if (saveService.CurrentSave.gold < recipe.goldCost) return "Not enough gold.";
            foreach (CraftingRecipeItemCost cost in recipe.itemCosts)
            {
                if (cost == null || string.IsNullOrEmpty(cost.itemId) || cost.amount <= 0) continue;
                ItemStackData stack = saveService.CurrentSave.inventory.items.Find(i => i.itemId == cost.itemId);
                if (stack == null || stack.amount < cost.amount) return "Not enough material.";
            }
            return string.Empty;
        }
    }
}
