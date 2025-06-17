using System.Collections.Generic;
using _PROJECT.Scripts.Core.Utils.MonoUtils;
using Core.Feature;
using Core.Feature.PlayerStats;
using Core.Feature.Tasks;
using Core.Installers;
using Core.Utils.MonoUtils;
using Features.Character;
using Features.Room;
using Features.Tasks;
using UnityEngine;
using Zenject;

namespace _PROJECT.Scripts.Core.Client
{
    public class Client : MonoInstaller
    {
        private bool _bindingsInitialized;
        public static Client BaseInstance { get; private set; }

        private void Awake()
        {
            var success = SetupSingleton();
            if (!success)
            {
                return;
            }

            Container.InstantiateComponentOnNewGameObject<MonoServiceInjector>();
            InitializeFeatures();

            if (_bindingsInitialized) return;

            InitializeBindings();
            _bindingsInitialized = true;
        }

        private bool SetupSingleton()
        {
            if (BaseInstance != null)
            {
                Debug.LogError("Client singleton already exists! Destroying duplicate instance.");
                Destroy(gameObject);
                return false;
            }

            BaseInstance = this;
            DontDestroyOnLoad(gameObject);

            return true;
        }

        #region CoreFeatures
        private MonoFeature _monoService;
        TasksFeature _tasksFeature;
        #endregion

        #region Features
        PlayerStatsFeature _playerStatsFeature;
        TaskTypeFeature _taskTypeFeature;
        CharacterTaskHandler _characterTaskHandler;
        RoomInteractionFeature _roomInteractionFeature;
        #endregion

        #region Bindings

        public override void InstallBindings()
        {
            InitializeBindings();
        }

        private void InitializeBindings()
        {
            if (_bindingsInitialized) return;

            InjectorDiBindings();
            CoreFeatureBindings();
            FeatureBindings();
            

            _bindingsInitialized = true;
        }

        private void InjectorDiBindings()
        {
            DiInstanceGeneratorInstaller.Install(Container);
        }

        private void CoreFeatureBindings()
        {
            _monoService = new MonoFeature();
            _tasksFeature = new TasksFeature();
            
            var coreFeatures = new List<BaseFeature>
            {
                _monoService,
                _tasksFeature
            };

            CoreFeatureInstaller.Install(Container, coreFeatures);
        }

        private void FeatureBindings()
        {
            _playerStatsFeature = new PlayerStatsFeature();
            _taskTypeFeature = new TaskTypeFeature();
            _characterTaskHandler = new CharacterTaskHandler();
            _roomInteractionFeature = new RoomInteractionFeature();
            
            var features = new List<BaseFeature>
            {
                _playerStatsFeature,
                _taskTypeFeature,
                _characterTaskHandler,
                _roomInteractionFeature
            };

            FeatureInstaller.Install(Container, features);
        }

        #endregion

        #region Initialization

        private void InitializeFeatures()
        {
            InitInitializableCoreFeatures();
            InitInitializableFeatures();
        }

        private void InitInitializableCoreFeatures()
        {
            _monoService.Initialize();
            _tasksFeature.Initialize();
        }

        private void InitInitializableFeatures()
        {
            _playerStatsFeature.Initialize();
            _taskTypeFeature.Initialize();
            _characterTaskHandler.Initialize();
            _roomInteractionFeature.Initialize();
        }

        #endregion
    }

   
}