using Utilities.Reactive;

namespace Utilities.Lifetimes.Extensions
{
    public static class SetTrueLifetimedExtension
    {
        public static void SetTrueLifetimed(this IViewableProperty<bool> property, Lifetime lifetime)
        {
            property.Value = true;
            lifetime.OnTermination(() => property.Value = false);
        }
    }
}
