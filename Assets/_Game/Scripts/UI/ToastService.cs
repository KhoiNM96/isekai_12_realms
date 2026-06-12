using System.Collections;
using TMPro;
using UnityEngine;

namespace Isekai12Realms.UI
{
    public class ToastService : MonoBehaviour
    {
        [SerializeField] private GameObject toastRoot;
        [SerializeField] private TextMeshProUGUI toastText;
        private Coroutine hideRoutine;

        public void Initialize(GameObject root, TextMeshProUGUI text)
        {
            toastRoot = root;
            toastText = text;
            if (toastRoot != null)
            {
                toastRoot.SetActive(false);
            }
        }

        public void ShowToast(string message)
        {
            if (toastRoot == null || toastText == null)
            {
                Debug.LogWarning($"[Toast] {message}");
                return;
            }

            toastText.text = message;
            toastRoot.SetActive(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(2f);
            if (toastRoot != null)
            {
                toastRoot.SetActive(false);
            }
            hideRoutine = null;
        }
    }
}
