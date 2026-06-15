using System.Threading.Tasks;

namespace Isekai12Realms.IAP
{
    public interface IReceiptValidatorService
    {
        Task<ReceiptValidationResult> ValidateAsync(string productId, string transactionId, string receipt);
    }
}
