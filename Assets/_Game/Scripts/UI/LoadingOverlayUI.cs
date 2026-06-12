using TMPro;
using UnityEngine;

namespace Isekai12Realms.UI
{
    public class LoadingOverlayUI : MonoBehaviour
    {
        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private TextMeshProUGUI loadingText;

        public void Initialize(GameObject root, TextMeshProUGUI text)
        {
            overlayRoot = root;
            loadingText = text;
            HideLoading();
        }

        public void ShowLoading(string message)
        {
            if (loadingText != null)
            {
                loadingText.text = string.IsNullOrEmpty(message) ? "Loading..." : message;
            }

            if (overlayRoot != null)
            {
                overlayRoot.SetActive(true);
            }
        }

        public void HideLoading()
        {
            if (overlayRoot != null)
            {
                overlayRoot.SetActive(false);
            }
        }
    }
}
