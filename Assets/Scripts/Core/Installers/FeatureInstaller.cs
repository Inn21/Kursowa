using System.Collections.Generic;
using Core.Feature;
using Zenject;

namespace Core.Installers
{
    public class FeatureInstaller : Installer<List<BaseFeature>, FeatureInstaller>
    {
        private readonly List<BaseFeature> _features;
        public FeatureInstaller(List<BaseFeature> features)
        {
            _features = features;
        }
        public override void InstallBindings()
        {
            
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