using Utilities.Lifetimes;

namespace View.Core
{
    public interface IInteractor { }

    public interface IInteractor<TProtocol> : IInteractor where TProtocol : IProtocol
    {
        TProtocol Get(Lifetime lifetime);
    }
}