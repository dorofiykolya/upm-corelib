using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public interface IContextServiceBuilder : IContextServiceSetup
    {
        static IContextServiceBuilder Default(Logger logger, Lifetime lifetime, IInjector injector) =>
                new ContextServiceBuilder(logger, lifetime, injector);

        static IContextServiceBuilder Default(Logger logger, Lifetime lifetime, IInjector injector, IServicesObserverRegister observerRegister) =>
                new ContextServiceBuilder(logger, lifetime, injector, observerRegister);

        Task AwakeServices();
        Task InitializeServices();
    }
}
