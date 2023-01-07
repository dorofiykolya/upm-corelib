using Injections;

namespace Framework.Runtime.Core
{
    public interface IInjectorProvider
    {
        IInjector Injector { get; }
    }
}
