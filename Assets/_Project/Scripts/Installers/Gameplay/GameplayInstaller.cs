using Features.MaterialAccumulation.Config;
using Features.MaterialAccumulation.Input;
using Features.MaterialAccumulation.Runtime;
using Features.MaterialAccumulation.View;
using View.Core;
using UnityEngine;
using Zenject;

namespace Installers.Gameplay
{
    public sealed class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private MaterialAccumulationSettings _materialAccumulationSettings;
        [SerializeField] private MaterialAccumulationInput _materialAccumulationInput;
        [SerializeField] private HeightFieldMeshView _heightFieldMeshView;
        [SerializeField] private MaterialAccumulationZoneView _materialAccumulationZoneView;

        public override void InstallBindings()
        {
            ViewCoreInstaller.Register(Container);
            GameplayUIInstaller.Register(Container);
            
            MaterialAccumulationInstaller.Register(
                Container,
                _materialAccumulationSettings,
                _materialAccumulationInput,
                _heightFieldMeshView,
                _materialAccumulationZoneView);
        }
    }
}
