using System.Security.Cryptography;
using System.Text;

namespace Isekai12Realms.CloudSave
{
    public static class SaveChecksumUtility
    {
        public static string ComputeChecksum(string json)
        {
            if (string.IsNullOrEmpty(json)) return string.Empty;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public static bool VerifyChecksum(string json, string checksum)
        {
            return string.IsNullOrEmpty(checksum) || ComputeChecksum(json) == checksum;
        }
    }
}
