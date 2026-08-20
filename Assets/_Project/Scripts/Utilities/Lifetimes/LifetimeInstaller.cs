using Utilities.Lifetimes.Extensions;
using Zenject;

namespace Utilities.Lifetimes
{
    public sealed class LifetimeInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Register(Container);
        }

        public static void Register(DiContainer container)
        {
            container.Bind<Lifetime>()
                .FromMethod(GetLifetime)
                .AsSingle()
                .CopyIntoAllSubContainers();
            container.BindInterfacesTo<LifetimeStarter>()
                .FromNewComponentOnRoot()
                .AsSingle()
                .CopyIntoAllSubContainers()
                .NonLazy();
            container.Bind<LifetimeInitializer>()
                .AsSingle()
                .CopyIntoAllSubContainers();
        }

        private static Lifetime GetLifetime(InjectContext context)
        {
            return context.Container.Resolve<Context>().gameObject.GetLifetime();
        }
    }
}
