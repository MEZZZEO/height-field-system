using View.Core;
using Zenject;

namespace Installers.Gameplay
{
    public sealed class GameplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            ViewCoreInstaller.Register(Container);
            GameplayUIInstaller.Register(Container);
        }
    }
}
