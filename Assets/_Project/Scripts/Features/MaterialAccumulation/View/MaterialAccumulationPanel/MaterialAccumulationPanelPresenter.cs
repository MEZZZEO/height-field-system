using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Lifetimes;
using Utilities.Reactive;
using View.Core;

namespace Features.MaterialAccumulation.View
{
    public sealed class MaterialAccumulationPanelPresenter : MonoPresenter
    {
        private const string SliderValueFormat = "{0:0.##}";

        [Header("Controls")]
        [SerializeField] private Slider _moveSpeedSlider;
        [SerializeField] private Slider _baseRadiusSlider;
        [SerializeField] private Slider _amplitudeSlider;
        [SerializeField] private Slider _frequencySlider;
        [SerializeField] private Slider _accumulationSpeedSlider;
        [SerializeField] private Button _resetSurfaceButton;

        [Header("Values")]
        [SerializeField] private TMP_Text _moveSpeedValue;
        [SerializeField] private TMP_Text _baseRadiusValue;
        [SerializeField] private TMP_Text _amplitudeValue;
        [SerializeField] private TMP_Text _frequencyValue;
        [SerializeField] private TMP_Text _accumulationSpeedValue;

        [Header("Curve")]
        [SerializeField] private CurveGraphView _curveGraph;

        protected override void Setup(Lifetime lifetime)
        {
            ValidateReferences();

            var protocol = ProtocolDispatcher.Get<MaterialAccumulationPanelProtocol>(lifetime);
            _moveSpeedSlider.BindTo(lifetime, protocol.MoveSpeed);
            _baseRadiusSlider.BindTo(lifetime, protocol.BaseRadius);
            _amplitudeSlider.BindTo(lifetime, protocol.Amplitude);
            _frequencySlider.BindTo(lifetime, protocol.Frequency);
            _accumulationSpeedSlider.BindTo(lifetime, protocol.AccumulationSpeed);
            _resetSurfaceButton.BindTo(lifetime, protocol.ResetSurfaceCommand);

            _moveSpeedValue.BindTo(lifetime, protocol.MoveSpeed, SliderValueFormat);
            _baseRadiusValue.BindTo(lifetime, protocol.BaseRadius, SliderValueFormat);
            _amplitudeValue.BindTo(lifetime, protocol.Amplitude, SliderValueFormat);
            _frequencyValue.BindTo(lifetime, protocol.Frequency, SliderValueFormat);
            _accumulationSpeedValue.BindTo(lifetime, protocol.AccumulationSpeed, SliderValueFormat);
            
            _curveGraph.SetCurve(protocol.RadiusCurve.Value);
            protocol.RadiusCurve.Advise(lifetime, _curveGraph.SetCurve);
        }

        private void ValidateReferences()
        {
            if (_moveSpeedSlider == null || _baseRadiusSlider == null || _amplitudeSlider == null ||
                _frequencySlider == null || _accumulationSpeedSlider == null || _resetSurfaceButton == null ||
                _moveSpeedValue == null || _baseRadiusValue == null || _amplitudeValue == null ||
                _frequencyValue == null || _accumulationSpeedValue == null || _curveGraph == null)
            {
                throw new System.InvalidOperationException(
                    "MaterialAccumulationPanelPresenter has missing serialized references.");
            }
        }
    }
}
