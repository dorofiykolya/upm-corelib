using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public interface IContextSetup
    {
        Logger Logger { get; }
        Lifetime Lifetime { get; }
        IInjector Injector { get; }
        T Resolve<T>();
    }

    public interface IContextServiceSetup : IContextSetup, IServiceRegister
    {
        
    }

    public interface IContextServiceBuilder : IContextServiceSetup
    {
        Task AwakeServices();
        Task InitializeServices();
    }
}