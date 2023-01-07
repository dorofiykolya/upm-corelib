using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Framework.Runtime.Core.Services;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{

    public class ContextServiceRegisterImpl : IServiceRegister
    {
        private readonly IServicesObserverRegister _observer;
        private readonly List<Binder> _serviceFactories = new List<Binder>();
        private List<Service> _services;

        public ContextServiceRegisterImpl(IServicesObserverRegister observer)
        {
            _observer = observer;
        }

        public ContextServiceRegisterImpl()
        {

        }

        public void Register<T>(Func<Service> factory)
        {
            _serviceFactories.Add(new Binder(typeof(T), (i) => factory()));
        }

        public void Register(Func<Service> factory)
        {
            _serviceFactories.Add(new Binder((i) => factory()));
        }

        public void Register<TInterface, TImpl>() where TImpl : Service
        {
            _serviceFactories.Add(new Binder(typeof(TInterface), typeof(TImpl)));
        }

        internal async Task Awake(Lifetime lifetime, Logger logger, IInjector injector)
        {
            if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
            {
                logger.V($"Begin to prepare services");
            }
            _services = new List<Service>(_serviceFactories.Count);
            foreach (var binder in _serviceFactories)
            {
                var service = binder.Factory(injector);
                if (_observer != null)
                {
                    _observer.Register(service);
                }
                if (service == null) throw new NullReferenceException($"Service {binder.Type} cannot be null");

                if (binder.Type != null)
                {
                    injector.ToValue(binder.Type, service);
                }
                else
                {
                    injector.ToValue(service);
                }

                Service.Internal.Inject(service, lifetime, logger.WithTag(service.GetType()), injector);
                _services.Add(service);
            }
            if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
            {
                logger.V($"Services [\n{string.Join(",\n", _services.Select(x => x.GetType().Name))}\n]");
            }

            foreach (var service in _services)
            {
                injector.Inject(service);
            }

            await Task.Yield();
            if (lifetime.IsTerminated) return;

            var tasks = new List<Task>(_services.Count);
            foreach (var service in _services)
            {
                if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
                {
                    logger.V($"{service.GetType().Name}.OnAwake");
                }
                var task = Service.Internal.OnAwake(service);
                tasks.Add(task);
                if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
                {
                    OnComplete(logger, task, $"{service.GetType().Name}.OnAwake->Completed");
                }
                if (lifetime.IsTerminated) return;
            }

            await Task.WhenAll(tasks);
            if (lifetime.IsTerminated) return;
        }

        internal async Task Initialize(Logger logger)
        {
            var tasks = new List<Task>(_services.Count);
            foreach (var service in _services)
            {
                if (service.Lifetime.IsTerminated) return;
                if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
                {
                    logger.V($"{service.GetType().Name}.OnInitialize");
                }
                var task = Service.Internal.OnInitialize(service);
                tasks.Add(task);
                if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
                {
                    OnComplete(logger, task, $"{service.GetType().Name}.OnInitialize->Completed");
                }
                if (service.Lifetime.IsTerminated) return;
            }

            await Task.WhenAll(tasks);
        }

        private async void OnComplete(Logger logger, Task task, string message)
        {
            await task;
            logger.V(message);
        }

        private class Binder
        {
            public Type Type { get; }
            public Func<IInjector, Service> Factory { get; }

            public Binder(Type type, Func<IInjector, Service> factory)
            {
                Type = type;
                Factory = factory;
            }

            public Binder(Func<IInjector, Service> factory)
            {
                Factory = factory;
            }

            public Binder(Type typeInterface, Type impl)
            {
                Type = typeInterface;
                Factory = (inj) =>
                {
                    inj.ToSingleton(typeInterface, impl);
                    return (Service)inj.Resolve(typeInterface);
                };
            }
        }
    }
}
