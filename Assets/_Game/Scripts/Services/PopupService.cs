using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Isekai12Realms.Services
{
    public interface IPopup
    {
        void Open(object payload);
        void Close();
    }

    public interface IPopupService
    {
        void SetPopupLayer(Transform popupLayer);
        void RegisterPopupPrefab(string popupId, GameObject prefab);
        GameObject ShowPopup(string popupId, object payload = null);
        GameObject Show(string popupId, object payload = null);
        void HidePopup(string popupId);
        bool IsPopupOpen(string popupId);
        bool HasOpenPopup();
        void CloseTop();
        void CloseAll();
    }

    public class PopupService : IPopupService
    {
        private const string ModalBlockerName = "ModalBlocker";

        private sealed class PopupEntry
        {
            public string PopupId;
            public GameObject Instance;
        }

        private Transform _popupLayer;
        private GameObject _modalBlocker;
        private readonly List<PopupEntry> _activePopups = new List<PopupEntry>();
        private readonly Dictionary<string, GameObject> _popupPrefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, GameObject> _popupInstances = new Dictionary<string, GameObject>();

        public void SetPopupLayer(Transform popupLayer)
        {
            _popupLayer = popupLayer;
            CachePopupLayer();
        }

        public void RegisterPopupPrefab(string popupId, GameObject prefab)
        {
            if (string.IsNullOrWhiteSpace(popupId))
            {
                throw new ArgumentException("Popup id cannot be empty.", nameof(popupId));
            }

            if (prefab == null)
            {
                _popupPrefabs.Remove(popupId);
                return;
            }

            _popupPrefabs[popupId] = prefab;
        }

        public GameObject ShowPopup(string popupId, object payload = null)
        {
            EnsurePopupLayer();

            if (string.IsNullOrWhiteSpace(popupId))
            {
                return null;
            }

            GameObject popupInstance = GetPopupInstance(popupId);
            if (popupInstance == null)
            {
                Debug.LogError($"PopupService: Popup '{popupId}' is missing from PopupLayer and no prefab is registered.");
                return null;
            }

            if (IsPopupOpen(popupId))
            {
                UpdateModalBlockerState();
                return popupInstance;
            }

            DeactivateAllPopupRootsExcept(popupInstance);
            PushActivePopup(popupId, popupInstance);
            ApplyPopupState(popupInstance, true);

            IPopup popupComponent = popupInstance.GetComponent<IPopup>();
            if (popupComponent != null)
            {
                popupComponent.Open(payload);
            }

            popupInstance.transform.SetAsLastSibling();
            UpdateModalBlockerState();
            return popupInstance;
        }

        public GameObject Show(string popupId, object payload = null)
        {
            return ShowPopup(popupId, payload);
        }

        public void HidePopup(string popupId)
        {
            if (string.IsNullOrWhiteSpace(popupId))
            {
                return;
            }

            int index = _activePopups.FindLastIndex(entry => entry != null && entry.PopupId == popupId);
            if (index < 0)
            {
                GameObject popup = GetPopupInstance(popupId);
                if (popup != null)
                {
                    ApplyPopupState(popup, false);
                }
                UpdateModalBlockerState();
                return;
            }

            if (index == _activePopups.Count - 1)
            {
                CloseTop();
                return;
            }

            PopupEntry entry = _activePopups[index];
            _activePopups.RemoveAt(index);
            ApplyPopupState(entry.Instance, false);
            UpdateModalBlockerState();
        }

        public void CloseTop()
        {
            if (_activePopups.Count == 0)
            {
                UpdateModalBlockerState();
                return;
            }

            PopupEntry entry = _activePopups[_activePopups.Count - 1];
            _activePopups.RemoveAt(_activePopups.Count - 1);

            if (entry != null && entry.Instance != null)
            {
                IPopup popupComponent = entry.Instance.GetComponent<IPopup>();
                if (popupComponent != null)
                {
                    popupComponent.Close();
                }

                ApplyPopupState(entry.Instance, false);
            }

            if (_activePopups.Count > 0)
            {
                PopupEntry next = _activePopups[_activePopups.Count - 1];
                if (next != null && next.Instance != null)
                {
                    ApplyPopupState(next.Instance, true);
                    next.Instance.transform.SetAsLastSibling();
                }
            }

            UpdateModalBlockerState();
        }

        public void CloseAll()
        {
            for (int i = _activePopups.Count - 1; i >= 0; i--)
            {
                PopupEntry entry = _activePopups[i];
                if (entry != null && entry.Instance != null)
                {
                    IPopup popupComponent = entry.Instance.GetComponent<IPopup>();
                    if (popupComponent != null)
                    {
                        popupComponent.Close();
                    }

                    ApplyPopupState(entry.Instance, false);
                }
            }

            _activePopups.Clear();

            if (_popupLayer != null)
            {
                for (int i = 0; i < _popupLayer.childCount; i++)
                {
                    Transform child = _popupLayer.GetChild(i);
                    if (child != null && child.name != ModalBlockerName)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }

            UpdateModalBlockerState();
        }

        public bool IsPopupOpen(string popupId)
        {
            return _activePopups.Exists(entry => entry != null && entry.PopupId == popupId && entry.Instance != null);
        }

        public bool HasOpenPopup()
        {
            return _activePopups.Exists(entry => entry != null && entry.Instance != null);
        }

        private void EnsurePopupLayer()
        {
            if (_popupLayer == null)
            {
                Debug.LogError("PopupService: PopupLayer transform has not been set!");
                return;
            }

            CachePopupLayer();
            if (_modalBlocker == null)
            {
                _modalBlocker = CreateModalBlocker(_popupLayer);
            }
        }

        private void CachePopupLayer()
        {
            _popupInstances.Clear();

            if (_popupLayer == null)
            {
                _modalBlocker = null;
                return;
            }

            for (int i = 0; i < _popupLayer.childCount; i++)
            {
                Transform child = _popupLayer.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name == ModalBlockerName)
                {
                    _modalBlocker = child.gameObject;
                    continue;
                }

                EnsureCanvasGroup(child.gameObject);
                _popupInstances[child.name] = child.gameObject;
            }
        }

        private GameObject GetPopupInstance(string popupId)
        {
            if (_popupInstances.TryGetValue(popupId, out GameObject existing) && existing != null)
            {
                return existing;
            }

            if (_popupLayer != null)
            {
                Transform directChild = _popupLayer.Find(popupId);
                if (directChild != null)
                {
                    GameObject found = directChild.gameObject;
                    EnsureCanvasGroup(found);
                    _popupInstances[popupId] = found;
                    return found;
                }
            }

            if (_popupPrefabs.TryGetValue(popupId, out GameObject prefab) && prefab != null && _popupLayer != null)
            {
                GameObject popupInstance = UnityEngine.Object.Instantiate(prefab, _popupLayer);
                popupInstance.name = popupId;
                EnsureCanvasGroup(popupInstance);
                popupInstance.SetActive(false);
                _popupInstances[popupId] = popupInstance;
                return popupInstance;
            }

            return null;
        }

        private void PushActivePopup(string popupId, GameObject instance)
        {
            int existingIndex = _activePopups.FindLastIndex(entry => entry != null && entry.PopupId == popupId);
            if (existingIndex >= 0)
            {
                _activePopups.RemoveAt(existingIndex);
            }

            _activePopups.Add(new PopupEntry { PopupId = popupId, Instance = instance });
        }

        private static void ApplyPopupState(GameObject popup, bool active)
        {
            if (popup == null)
            {
                return;
            }

            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = active;
                canvasGroup.blocksRaycasts = active;
            }

            popup.SetActive(active);
        }

        private void DeactivateAllPopupRootsExcept(GameObject popup)
        {
            if (_popupLayer == null)
            {
                return;
            }

            for (int i = 0; i < _popupLayer.childCount; i++)
            {
                Transform child = _popupLayer.GetChild(i);
                if (child == null || child.gameObject == _modalBlocker || child.gameObject == popup)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private void UpdateModalBlockerState()
        {
            if (_modalBlocker == null)
            {
                return;
            }

            bool hasOpenPopup = HasOpenPopup();
            _modalBlocker.SetActive(hasOpenPopup);
            if (hasOpenPopup)
            {
                _modalBlocker.transform.SetAsFirstSibling();
            }
        }

        private static GameObject CreateModalBlocker(Transform popupLayer)
        {
            GameObject blocker = new GameObject(ModalBlockerName, typeof(RectTransform), typeof(Image));
            blocker.transform.SetParent(popupLayer, false);
            RectTransform rect = blocker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = blocker.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.55f);
            image.raycastTarget = true;
            blocker.SetActive(false);
            blocker.transform.SetAsFirstSibling();
            return blocker;
        }

        private static void EnsureCanvasGroup(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
