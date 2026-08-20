using Features.MaterialAccumulation.View;
using Zenject;

namespace Installers.Gameplay
{
    public static class GameplayUIInstaller
    {
        public static void Register(DiContainer container)
        {
            container.BindInterfacesTo<MaterialAccumulationPanelInteractor>().AsSingle();
        }
    }
}
