using System;
using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Framework.Runtime.Core.Services;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public class ContextServiceBuilder : IContextServiceBuilder
    {
        private readonly ContextServiceRegisterImpl _serviceImpl;

        public ContextServiceBuilder(Logger logger, Lifetime lifetime, IInjector injector)
        {
            Logger = logger;
            Lifetime = lifetime;
            Injector = injector;
            _serviceImpl = new ContextServiceRegisterImpl();
        }

        public Logger Logger { get; }
        public Lifetime Lifetime { get; }
        public IInjector Injector { get; }

        public T Resolve<T>() => Injector.Resolve<T>();

        public async Task AwakeServices()
        {
            await _serviceImpl.Awake(Lifetime, Logger, Injector);
        }

        public async Task InitializeServices()
        {
            await _serviceImpl.Initialize();
        }

        public void Register(Func<Service> factory)
        {
            _serviceImpl.Register(factory);
        }

        public void Register<T>(Func<Service> factory)
        {
            _serviceImpl.Register<T>(factory);
        }

        public void Register<TInterface, TImpl>() where TImpl : Service
        {
            _serviceImpl.Register<TInterface, TImpl>();
        }
    }
}