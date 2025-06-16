using System.Collections.Generic;
using _PROJECT.Scripts.Core.Utils.MonoUtils;
using Core.Feature;
using Core.Feature.PlayerStats;
using Core.Feature.Tasks;
using Core.Installers;
using Core.Utils.MonoUtils;
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
        #endregion

        #region Features
        
        TasksFeature _tasksFeature;
        PlayerStatsFeature _playerStatsFeature;
        TaskTypeFeature _taskTypeFeature;
        
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
            
            var coreFeatures = new List<BaseFeature>
            {
                _monoService,
            };

            CoreFeatureInstaller.Install(Container, coreFeatures);
        }

        private void FeatureBindings()
        {
            _tasksFeature = new TasksFeature();
            _playerStatsFeature = new PlayerStatsFeature();
            _taskTypeFeature = new TaskTypeFeature();
            
            var features = new List<BaseFeature>
            {
                _tasksFeature,
                _playerStatsFeature,
                _taskTypeFeature
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
        }

        private void InitInitializableFeatures()
        {
            _tasksFeature.Initialize();
            _playerStatsFeature.Initialize();
            _taskTypeFeature.Initialize();
        }

        #endregion
    }

   
}