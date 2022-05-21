using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Loggers;
using Injections;

namespace Framework.Runtime.Core.Services
{
    public abstract class Service
    {
        private IResolve _resolve;

        public Lifetime Lifetime { get; private set; }
        public Logger Logger { get; private set; }

        protected virtual Task OnAwake() => Task.CompletedTask;

        protected virtual Task OnInitialize() => Task.CompletedTask;

        protected T Resolve<T>() => (T)_resolve.Resolve(typeof(T));

        internal static class Internal
        {
            public static void Inject(Service service, Lifetime lifetime, Logger logger, IResolve resolve)
            {
                service._resolve = resolve;
                service.Logger = logger;
                service.Lifetime = lifetime;
            }

            public static Task OnAwake(Service service) => service.OnAwake();
            public static Task OnInitialize(Service service) => service.OnInitialize();
        }
    }
}