using Features.MaterialAccumulation.Config;
using Features.MaterialAccumulation.Input;
using Features.MaterialAccumulation.View;
using Zenject;

namespace Features.MaterialAccumulation.Runtime
{
    public static class MaterialAccumulationInstaller
    {
        public static void Register(
            DiContainer container,
            MaterialAccumulationSettings settings,
            MaterialAccumulationInput input,
            HeightFieldMeshView meshView,
            MaterialAccumulationZoneView zoneView)
        {
            settings.Validate();
            container.BindInstance(settings);
            container.BindInterfacesAndSelfTo<AccumulationRuntimeSettings>().AsSingle();
            container.BindInterfacesAndSelfTo<MaterialAccumulationInput>().FromInstance(input).AsSingle();
            container.BindInterfacesAndSelfTo<MaterialAccumulationController>().AsSingle();
            container.BindInterfacesAndSelfTo<HeightFieldMeshView>().FromInstance(meshView).AsSingle();
            container.BindInterfacesAndSelfTo<MaterialAccumulationZoneView>().FromInstance(zoneView).AsSingle();
        }
    }
}
