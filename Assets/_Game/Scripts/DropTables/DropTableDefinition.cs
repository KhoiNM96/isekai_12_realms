using System;
using System.Collections.Generic;
using UnityEngine;

namespace Isekai12Realms.DropTables
{
    [Serializable]
    public class DropEntry
    {
        public string itemId;
        public string equipmentId;
        public int minAmount = 1;
        public int maxAmount = 1;
        [Range(0f, 1f)] public float chance = 1f;
        public bool isEquipment;
    }

    [CreateAssetMenu(menuName = "Isekai 12 Realms/Drop Table Definition")]
    public class DropTableDefinition : ScriptableObject
    {
        public string id;
        public List<DropEntry> drops = new List<DropEntry>();
    }
}
