using Isekai12Realms.CloudSave;

namespace Isekai12Realms.IAP
{
    public static class ReceiptHashUtility
    {
        public static string HashReceipt(string receipt)
        {
            return SaveChecksumUtility.ComputeChecksum(receipt);
        }
    }
}
