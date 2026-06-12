using System;
using System.Collections.Generic;
using Isekai12Realms.Equipment;

namespace Isekai12Realms.Inventory
{
    [Serializable]
    public class InventorySaveData
    {
        public List<ItemStackData> items = new List<ItemStackData>();
        public List<EquipmentInstanceData> equipments = new List<EquipmentInstanceData>();
        public int capacity = 80;
    }
}
