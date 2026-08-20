using Features.MaterialAccumulation.Runtime;
using UnityEngine;
using Utilities.Reactive;
using View.Core;

namespace Features.MaterialAccumulation.View
{
    public sealed class MaterialAccumulationPanelProtocol : IProtocol
    {
        public IViewableProperty<float> MoveSpeed { get; }
        public IViewableProperty<float> BaseRadius { get; }
        public IViewableProperty<float> Amplitude { get; }
        public IViewableProperty<float> Frequency { get; }
        public IViewableProperty<float> AccumulationSpeed { get; }
        public IReadonlyProperty<AnimationCurve> RadiusCurve { get; }
        public Command ResetSurfaceCommand { get; }

        public MaterialAccumulationPanelProtocol(AccumulationRuntimeSettings settings)
        {
            MoveSpeed = settings.MoveSpeed;
            BaseRadius = settings.BaseRadius;
            Amplitude = settings.Amplitude;
            Frequency = settings.Frequency;
            AccumulationSpeed = settings.AccumulationSpeed;
            RadiusCurve = new ViewableProperty<AnimationCurve>(settings.RadiusCurve);
            ResetSurfaceCommand = new Command();
        }
    }
}
