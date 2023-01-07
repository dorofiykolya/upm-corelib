using Common;
using Framework.Runtime.Core.Loggers;

namespace Framework.Runtime.Core.ContextBuilder
{
    public interface IContextSetup : IInjectorProvider
    {
        Logger Logger { get; }
        Lifetime Lifetime { get; }
        T Resolve<T>();
    }
}
