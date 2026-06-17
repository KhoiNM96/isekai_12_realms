using UnityEngine;
using UnityEngine.SceneManagement;
using Isekai12Realms.Build;
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

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void InitializeServices()
        {
            // 1. Game State Machine
            _stateMachine = new GameStateMachine();
            ServiceLocator.Register<IGameStateMachine>(_stateMachine);

            BuildConfigService buildConfigService = BuildConfigService.GetOrCreate();
            buildConfigService.ApplyStartupSettings();

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
            if (SceneManager.GetActiveScene().name != GameSceneBootstrapper.GameSceneName)
            {
                GetOrCreateBootLoader().BeginLoad(GameSceneBootstrapper.GameSceneName);
                return;
            }

            EnterTitleState();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == GameSceneBootstrapper.GameSceneName)
            {
                EnterTitleState();
            }
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

        private Isekai12Realms.Core.BootLoader GetOrCreateBootLoader()
        {
            Isekai12Realms.Core.BootLoader loader = FindObjectOfType<Isekai12Realms.Core.BootLoader>();
            if (loader == null)
            {
                loader = GetComponent<Isekai12Realms.Core.BootLoader>();
                if (loader == null)
                {
                    loader = gameObject.AddComponent<Isekai12Realms.Core.BootLoader>();
                }
            }

            return loader;
        }

        private void EnterTitleState()
        {
            if (_stateMachine != null && _stateMachine.CurrentState != GameState.Title)
            {
                _stateMachine.TransitionTo(GameState.Title);
            }
        }
    }
}
