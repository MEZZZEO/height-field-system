using System;
using Features.MaterialAccumulation.Config;
using UnityEngine;
using Utilities.Reactive;

namespace Features.MaterialAccumulation.Runtime
{
    public sealed class AccumulationRuntimeSettings : IDisposable
    {
        public IViewableProperty<float> MoveSpeed { get; }
        public IViewableProperty<float> BaseRadius { get; }
        public IViewableProperty<float> Amplitude { get; }
        public IViewableProperty<float> Frequency { get; }
        public IViewableProperty<float> AccumulationSpeed { get; }
        public AnimationCurve RadiusCurve { get; }
        public float SweepStepFactor { get; }

        public AccumulationRuntimeSettings(MaterialAccumulationSettings source)
        {
            source.Validate();

            MoveSpeed = new ViewableProperty<float>(source.MoveSpeed);
            BaseRadius = new ViewableProperty<float>(source.BaseRadius);
            Amplitude = new ViewableProperty<float>(source.Amplitude);
            Frequency = new ViewableProperty<float>(source.Frequency);
            AccumulationSpeed = new ViewableProperty<float>(source.AccumulationSpeed);
            RadiusCurve = source.RadiusCurve;
            SweepStepFactor = source.SweepStepFactor;
        }

        public void Dispose()
        {
            MoveSpeed.Dispose();
            BaseRadius.Dispose();
            Amplitude.Dispose();
            Frequency.Dispose();
            AccumulationSpeed.Dispose();
        }
    }
}
