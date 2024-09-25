using System;
using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public class ContextServiceBuilder : IContextServiceBuilder
    {
        private readonly ContextServiceRegisterImpl _serviceImpl;

        public ContextServiceBuilder(
            Logger logger,
            Lifetime lifetime,
            IInjector injector,
            IServicesObserverRegister observer = null,
            Action<ContextServiceBuilderOptions> options = null
        )
        {
            Logger = logger;
            Lifetime = lifetime;
            Injector = injector;
            _serviceImpl = new ContextServiceRegisterImpl(injector, observer, options);
        }

        public Logger Logger { get; }
        public Lifetime Lifetime { get; }
        public IInjector Injector { get; }

        public void Register(IServiceResolver resolver)
        {
            _serviceImpl.Register(resolver);
        }

        public T Resolve<T>() => Injector.Resolve<T>();

        public async Task AwakeServices()
        {
            await _serviceImpl.Awake(Lifetime, Logger);
        }

        public async Task InitializeServices()
        {
            await _serviceImpl.Initialize(Logger);
        }
    }
}