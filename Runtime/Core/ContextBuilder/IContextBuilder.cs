using System;
using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public interface IContextServiceBuilder : IContextServiceSetup
    {
        static IContextServiceBuilder Default(Logger logger, Lifetime lifetime, IInjector injector, Action<ContextServiceBuilderOptions> options = null) =>
            new ContextServiceBuilder(
                logger: logger,
                lifetime: lifetime,
                injector: injector,
                observer: null,
                options: options
            );

        static IContextServiceBuilder Default(Logger logger, Lifetime lifetime, IInjector injector, IServicesObserverRegister observerRegister, Action<ContextServiceBuilderOptions> options = null) =>
            new ContextServiceBuilder(
                logger: logger,
                lifetime: lifetime,
                injector: injector,
                observer: observerRegister,
                options: options
            );

        Task AwakeServices();
        Task InitializeServices();
    }
}