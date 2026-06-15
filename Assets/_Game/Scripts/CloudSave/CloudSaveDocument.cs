using System;
using System.Collections.Generic;
using Isekai12Realms.Shop;

namespace Isekai12Realms.CloudSave
{
    [Serializable]
    public class CloudSaveDocument
    {
        public CloudSaveMeta meta;
        public string saveJson;
        public string checksum;
        public List<PurchaseRecord> purchaseRecords = new List<PurchaseRecord>();
        public long uploadedAt;
    }
}
