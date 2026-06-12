using UnityEngine;
using Isekai12Realms.Services;
using Isekai12Realms.UI;

namespace Isekai12Realms.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;

        [Header("References")]
        [SerializeField] private Transform popupLayer;
        
        private GameStateMachine _stateMachine;
        private SaveService _saveService;
        private PopupService _popupService;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (popupLayer != null)
                {
                    _instance.SetPopupLayer(popupLayer);
                }

                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
            EnsureSceneBootstrapper();
        }

        private void InitializeServices()
        {
            // 1. Game State Machine
            _stateMachine = new GameStateMachine();
            ServiceLocator.Register<IGameStateMachine>(_stateMachine);

            // 2. Save Service
            _saveService = new SaveService();
            _saveService.LoadOrCreateSave();
            ServiceLocator.Register<ISaveService>(_saveService);

            // 3. Popup Service
            _popupService = new PopupService();
            if (popupLayer != null)
            {
                _popupService.SetPopupLayer(popupLayer);
            }
            ServiceLocator.Register<IPopupService>(_popupService);

            Debug.Log("[GameManager] Core Services initialized.");
        }

        private void Start()
        {
            _stateMachine.TransitionTo(GameState.Title);
        }
        
        public void SetPopupLayer(Transform layer)
        {
            popupLayer = layer;
            if (_popupService != null)
            {
                _popupService.SetPopupLayer(layer);
            }
        }

        private void EnsureSceneBootstrapper()
        {
            if (GetComponent<GameSceneBootstrapper>() == null)
            {
                gameObject.AddComponent<GameSceneBootstrapper>();
            }
        }
    }
}
