using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Isekai12Realms.Editor.UI
{
    public static class PopupDiagnosticsMenu
    {
        [MenuItem("Tools/Isekai 12 Realms/UI/Print Active Popup Objects")]
        public static void PrintActivePopupObjects()
        {
            Transform popupLayer = FindPopupLayer();
            if (popupLayer == null)
            {
                Debug.LogWarning("[UI] PopupLayer not found.");
                return;
            }

            Debug.Log($"[UI] PopupLayer children: {popupLayer.childCount}");
            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform child = popupLayer.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                RectTransform rect = child as RectTransform;
                CanvasGroup canvasGroup = child.GetComponent<CanvasGroup>();
                Image image = child.GetComponent<Image>();
                string size = rect != null ? rect.sizeDelta.ToString() : "n/a";
                Debug.Log($"[UI] Popup child '{child.name}' active={child.gameObject.activeSelf} sibling={child.GetSiblingIndex()} size={size} canvasGroup={(canvasGroup != null ? "yes" : "no")} blocksRaycasts={(canvasGroup != null && canvasGroup.blocksRaycasts)} image={(image != null ? "yes" : "no")}");
            }
        }

        [MenuItem("Tools/Isekai 12 Realms/UI/Close All Popups In Scene")]
        public static void CloseAllPopupsInScene()
        {
            Transform popupLayer = FindPopupLayer();
            if (popupLayer == null)
            {
                Debug.LogWarning("[UI] PopupLayer not found.");
                return;
            }

            for (int i = 0; i < popupLayer.childCount; i++)
            {
                Transform child = popupLayer.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }

            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
        }

        private static Transform FindPopupLayer()
        {
            GameObject root = GameObject.Find("RootCanvas");
            if (root == null)
            {
                return null;
            }

            Transform safeAreaRoot = root.transform.Find("SafeAreaRoot");
            return safeAreaRoot != null ? safeAreaRoot.Find("PopupLayer") : null;
        }
    }
}
