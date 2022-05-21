using Common;
using Framework.Runtime.Core.ContextBuilder;
using Framework.Runtime.Core.Loggers;
using Injections;

namespace Framework.Runtime.Services.UI
{
    public interface IUIContextServiceSetup : IContextServiceSetup
    {
        public static IUIContextServiceBuilder Default(Logger logger, Lifetime lifetime, IInjector injector) =>
            new UIContextServiceBuilder(logger, lifetime, injector);

        private class UIContextServiceBuilder : ContextServiceBuilder, IUIContextServiceBuilder
        {
            public UIContextServiceBuilder(Logger logger, Lifetime lifetime, IInjector injector) : base(logger,
                lifetime,
                injector)
            {
            }
        }
    }

    public interface IUIContextServiceBuilder : IUIContextServiceSetup, IContextServiceBuilder
    {
    }
}