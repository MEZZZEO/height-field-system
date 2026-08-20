using Features.MaterialAccumulation.Runtime;
using Utilities.Lifetimes;
using Utilities.Reactive;
using View.Core;

namespace Features.MaterialAccumulation.View
{
    public sealed class MaterialAccumulationPanelInteractor : IInteractor<MaterialAccumulationPanelProtocol>
    {
        private readonly AccumulationRuntimeSettings _settings;
        private readonly MaterialAccumulationController _controller;

        public MaterialAccumulationPanelInteractor(AccumulationRuntimeSettings settings, MaterialAccumulationController controller)
        {
            _settings = settings;
            _controller = controller;
        }

        public MaterialAccumulationPanelProtocol Get(Lifetime lifetime)
        {
            var protocol = new MaterialAccumulationPanelProtocol(_settings);
            
            protocol.ResetSurfaceCommand.Execute.Advise(lifetime, _ =>
            {
                _controller.ResetSurface();
            });
            
            return protocol;
        }
    }
}
