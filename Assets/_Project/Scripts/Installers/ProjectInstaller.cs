using Utilities.Lifetimes;
using Zenject;

namespace Installers
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            LifetimeInstaller.Register(Container);
        }
    }
}
