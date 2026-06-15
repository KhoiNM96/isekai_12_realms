using System.Threading.Tasks;

namespace Isekai12Realms.IAP
{
    public class LocalReceiptValidatorService : IReceiptValidatorService
    {
        public Task<ReceiptValidationResult> ValidateAsync(string productId, string transactionId, string receipt)
        {
            return Task.FromResult(ReceiptValidationResult.Valid("Local receipt accepted."));
        }
    }
}
