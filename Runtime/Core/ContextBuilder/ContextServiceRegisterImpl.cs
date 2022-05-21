using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Framework.Runtime.Core.Services;
using Injections;
using UnityEngine.Assertions;

namespace Framework.Runtime.Core.ContextBuilder
{
    public class ContextServiceRegisterImpl : IServiceRegister
    {
        private readonly List<Binder> _serviceFactories = new List<Binder>();
        private List<Service> _services;

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
            _services = new List<Service>(_serviceFactories.Count);
            foreach (var binder in _serviceFactories)
            {
                var service = binder.Factory(injector);
                Assert.IsNotNull(service);
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

            foreach (var service in _services)
            {
                injector.Inject(service);
            }

            await Task.Yield();
            if (lifetime.IsTerminated) return;

            var tasks = new List<Task>(_services.Count);
            foreach (var service in _services)
            {
                tasks.Add(Service.Internal.OnAwake(service));
                if (lifetime.IsTerminated) return;
            }

            await Task.WhenAll(tasks);
            if (lifetime.IsTerminated) return;
        }

        internal async Task Initialize()
        {
            var tasks = new List<Task>(_services.Count);
            foreach (var service in _services)
            {
                if (service.Lifetime.IsTerminated) return;
                tasks.Add(Service.Internal.OnInitialize(service));
                if (service.Lifetime.IsTerminated) return;
            }

            await Task.WhenAll(tasks);
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