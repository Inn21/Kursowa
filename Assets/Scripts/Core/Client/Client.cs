using System.Collections.Generic;
using _PROJECT.Scripts.Core.Utils.MonoUtils;
using Core.Feature;
using Core.Installers;
using Core.Utils.MonoUtils;
using Features.Ui;
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
        private RuntimeUIFeature _runtimeUIFeature;
        #endregion

        #region Features
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
            //TODO: Add here other installers, maybe for services, divide features by core and the ones that depend on them, etc.

            _bindingsInitialized = true;
        }

        private void InjectorDiBindings()
        {
            DiInstanceGeneratorInstaller.Install(Container);
        }

        private void CoreFeatureBindings()
        {
            _monoService = new MonoFeature();
            _runtimeUIFeature = new RuntimeUIFeature();


            // var sceneConfig = Resources.Load<SceneFeatureConfig>("SceneConfig");
            // _sceneFeature.Initialize(sceneConfig);
            

            var coreFeatures = new List<BaseFeature>
            {
                _monoService,
                _runtimeUIFeature,
            };

            CoreFeatureInstaller.Install(Container, coreFeatures);
        }

        private void FeatureBindings()
        {

            var features = new List<BaseFeature>
            {
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
            _runtimeUIFeature.Initialize();
        }

        private void InitInitializableFeatures()
        {
        }

        private void AddLoadingScreen()
        {
        }

        private void OnDestroy()
        {
            // _sceneFeature.OnSceneLoaded -= AddLoadingScreen;
        }

        #endregion
    }

   
}