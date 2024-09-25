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
        private readonly IInjector _injector;
        private readonly IServicesObserverRegister _observer;
        private readonly List<IServiceResolver> _serviceFactories = new List<IServiceResolver>();
        private readonly ContextServiceBuilderOptions _options;
        private List<Service> _services;

        public IInjector Injector => _injector;

        public ContextServiceRegisterImpl(
            IInjector injector,
            IServicesObserverRegister observer,
            Action<ContextServiceBuilderOptions> options
        )
        {
            _injector = injector;
            _observer = observer;
            _options = new ContextServiceBuilderOptions();
            if (options != null) 
            {
                options(_options);
            }
        }

        public ContextServiceRegisterImpl(IInjector injector)
        {
            _injector = injector;
        }

        public void Register(IServiceResolver resolver)
        {
            _serviceFactories.Add(resolver);
        }

        internal async Task Awake(Lifetime lifetime, Logger logger)
        {
            if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
            {
                logger.V($"Begin to prepare services");
            }

            _services = new List<Service>(_serviceFactories.Count);
            foreach (var binder in _serviceFactories)
            {
                var service = binder.Resolver(_injector);
                if (_observer != null)
                {
                    _observer.Register(service);
                }

                if (service == null)
                {
                    var interfaces = "";
                    if (binder.Interfaces != null && binder.Interfaces.Length != 0)
                    {
                        interfaces = string.Join(",", binder.Interfaces.Select(t => t.Name));
                    }

                    throw new NullReferenceException($"Service {interfaces} cannot be null");
                }

                if (binder.Interfaces != null && binder.Interfaces.Length != 0)
                {
                    foreach (var binderInterface in binder.Interfaces)
                    {
                        _injector.ToValue(binderInterface, service);
                    }
                }
                else
                {
                    _injector.ToValue(service);
                }

                Service.Internal.Inject(service, lifetime, logger.WithTag(service.GetType()), _injector);
                _services.Add(service);
            }

            if ((logger.LogFlag & LoggerFlag.Verbose) != 0)
            {
                logger.V($"Services [\n{string.Join(",\n", _services.Select(x => x.GetType().Name))}\n]");
            }

            foreach (var service in _services)
            {
                _injector.Inject(service);
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

            if (_options.InitializationStrategy == ContextServiceInitializationStrategy.Sequential)
            {
                foreach (var task in tasks)
                {
                    await task;
                }
            }
            else
            {
                await Task.WhenAll(tasks);
            }

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

            if (_options.InitializationStrategy == ContextServiceInitializationStrategy.Sequential)
            {
                foreach (var task in tasks)
                {
                    await task;
                }
            }
            else
            {
                await Task.WhenAll(tasks);
            }
        }

        private async void OnComplete(Logger logger, Task task, string message)
        {
            await task;
            logger.V(message);
        }
    }
}