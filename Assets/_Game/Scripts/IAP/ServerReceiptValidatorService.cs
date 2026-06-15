using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Isekai12Realms.IAP
{
    [Serializable]
    internal class ServerReceiptValidationRequest
    {
        public string uid;
        public string productId;
        public string purchaseToken;
        public string transactionId;
    }

    [Serializable]
    internal class ServerReceiptValidationResponse
    {
        public bool valid;
        public bool alreadyGranted;
        public int totalGranted;
        public string message;
    }

    public class ServerReceiptValidatorService : IReceiptValidatorService
    {
        private readonly string endpointUrl;
        private readonly string uid;

        public ServerReceiptValidatorService(string endpointUrl, string uid)
        {
            this.endpointUrl = endpointUrl;
            this.uid = uid;
        }

        public async Task<ReceiptValidationResult> ValidateAsync(string productId, string transactionId, string receipt)
        {
            if (string.IsNullOrEmpty(endpointUrl))
            {
                return ReceiptValidationResult.Invalid("Server receipt validation is not configured.");
            }

            ServerReceiptValidationRequest request = new ServerReceiptValidationRequest
            {
                uid = uid,
                productId = productId,
                purchaseToken = receipt,
                transactionId = transactionId
            };

            string payload = JsonUtility.ToJson(request);
            using (UnityWebRequest webRequest = new UnityWebRequest(endpointUrl, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] body = System.Text.Encoding.UTF8.GetBytes(payload);
                webRequest.uploadHandler = new UploadHandlerRaw(body);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");

                try
                {
                    UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }
                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        return ReceiptValidationResult.Invalid("Server receipt validation failed.");
                    }

                    ServerReceiptValidationResponse response = JsonUtility.FromJson<ServerReceiptValidationResponse>(webRequest.downloadHandler.text);
                    if (response != null && response.valid)
                    {
                        return ReceiptValidationResult.Valid(response.alreadyGranted ? "Already granted." : "Validated.");
                    }

                    return ReceiptValidationResult.Invalid(response != null && !string.IsNullOrEmpty(response.message) ? response.message : "Invalid receipt.");
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[IAP] Server receipt validation failed: " + e.Message);
                    return ReceiptValidationResult.Invalid("Server receipt validation unavailable.");
                }
            }
        }
    }
}
