using _PROJECT.Scripts.Core.InstanceGenerator;
using Zenject;

namespace Core.Installers
{
    public class DiInstanceGeneratorInstaller : Installer<DiInstanceGeneratorInstaller>
    {
        public override void InstallBindings()
        {
            var diInstanceGenerator = new DiInstanceGenerator(Container);
            Container.BindInstance(diInstanceGenerator).AsSingle();
            Container.QueueForInject(diInstanceGenerator);
        }
    }
}