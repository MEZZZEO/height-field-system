using Zenject;

namespace View.Core
{
    public sealed class ViewCoreInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Register(Container);
        }

        public static void Register(DiContainer container)
        {
            container.BindInterfacesTo<ProtocolDispatcher>().AsSingle().CopyIntoAllSubContainers();
        }
    }
}
