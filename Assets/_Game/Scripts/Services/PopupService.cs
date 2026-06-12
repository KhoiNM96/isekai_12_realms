using System;
using System.Collections.Generic;
using UnityEngine;

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
        GameObject Show(string popupId, object payload = null);
        void CloseTop();
        void CloseAll();
    }

    public class PopupService : IPopupService
    {
        private Transform _popupLayer;
        private readonly Stack<GameObject> _activePopups = new Stack<GameObject>();
        private readonly Dictionary<string, GameObject> _popupPrefabs = new Dictionary<string, GameObject>();

        public void SetPopupLayer(Transform popupLayer)
        {
            _popupLayer = popupLayer;
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

        public GameObject Show(string popupId, object payload = null)
        {
            if (_popupLayer == null)
            {
                Debug.LogError("PopupService: PopupLayer transform has not been set!");
                return null;
            }

            if (!_popupPrefabs.TryGetValue(popupId, out GameObject prefab) || prefab == null)
            {
                Debug.LogError($"PopupService: Popup prefab not registered for id '{popupId}'.");
                return null;
            }

            GameObject popupInstance = UnityEngine.Object.Instantiate(prefab, _popupLayer);
            popupInstance.SetActive(true);

            IPopup popupComponent = popupInstance.GetComponent<IPopup>();
            if (popupComponent != null)
            {
                popupComponent.Open(payload);
            }
            else
            {
                Debug.LogWarning($"PopupInstance {popupId} does not have an IPopup component.");
            }

            _activePopups.Push(popupInstance);
            return popupInstance;
        }

        public void CloseTop()
        {
            if (_activePopups.Count > 0)
            {
                GameObject topPopup = _activePopups.Pop();
                if (topPopup != null)
                {
                    IPopup popupComponent = topPopup.GetComponent<IPopup>();
                    if (popupComponent != null)
                    {
                        popupComponent.Close();
                    }
                    UnityEngine.Object.Destroy(topPopup);
                }
            }
        }

        public void CloseAll()
        {
            while (_activePopups.Count > 0)
            {
                CloseTop();
            }
        }
    }
}
