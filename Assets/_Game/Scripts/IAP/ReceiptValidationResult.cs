namespace Isekai12Realms.IAP
{
    public class ReceiptValidationResult
    {
        public bool valid;
        public string message;

        public static ReceiptValidationResult Valid(string message = "Valid") => new ReceiptValidationResult { valid = true, message = message };
        public static ReceiptValidationResult Invalid(string message) => new ReceiptValidationResult { valid = false, message = message };
    }
}
