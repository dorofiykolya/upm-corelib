using System;
using System.Collections.Generic;
using Framework.Runtime.Core.ContextBuilder;
using Framework.Runtime.Core.Widgets;
using Injections;

namespace Framework.Runtime.Services.UI.Hud
{
    public static class UIHudServiceExtension
    {
        public static void AddHudService(this IUIContextServiceSetup setup)
        {
            var provider = new HudProvider();
            setup.Injector.ToValue<IUIHudProvider>(provider);
            setup.Injector.ToValue<IUIHudRegister>(provider);
            setup.Register(() => new UIHudService());
        }

        public static void RegisterHUD<TWidget>(this IUIContextServiceSetup setup, string path,
            IUIComponentProvider provider = null) where TWidget : Widget
        {
            setup.Resolve<IUIHudRegister>().Register(typeof(TWidget), path, provider);
        }


        class HudProvider : IUIHudProvider, IUIHudRegister
        {
            private readonly List<UIHudMap> _list = new();

            public IEnumerable<UIHudMap> Provide() => _list;

            public void Register(Type type, string path, IUIComponentProvider provider) =>
                _list.Add(new UIHudMap(type, path, provider));
        }
    }
}