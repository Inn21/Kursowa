using System.Collections.Generic;
using Core.Feature;
using Core.Feature.Save;
using Zenject;

namespace Core.Installers
{
    public class CoreFeatureInstaller : Installer<List<BaseFeature>, CoreFeatureInstaller>
    {
        private readonly List<BaseFeature> _features;

        public CoreFeatureInstaller(List<BaseFeature> features)
        {
            _features = features;
        }

        public override void InstallBindings()
        {
            Container.Bind<ISaveFeature>().To<JsonSaveFeature>().AsSingle();
            foreach (var feature in _features)
            {
                Container.BindInterfacesAndSelfTo(feature.GetType())
                    .FromInstance(feature)
                    .AsSingle();
                Container.QueueForInject(feature);
            }
        }
    }
}