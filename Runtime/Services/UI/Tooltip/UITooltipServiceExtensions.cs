using System;
using System.Collections.Generic;
using Framework.Runtime.Core.Widgets;
using Injections;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public static class UITooltipServiceExtensions
    {
        public static void AddToolTipService(this IUIContextServiceSetup setup)
        {
            var provider = new TooltipProvider();
            setup.Injector.ToValue<IUITooltipProvider>(provider);
            setup.Injector.ToValue<IUITooltipRegister>(provider);

            setup.Register(() => new UITooltipService());
        }

        public static void RegisterTooltip<TWidget>(this IUIContextServiceSetup setup, string path,
            IUIComponentProvider provider = null)
            where TWidget : Widget
        {
            setup.Resolve<IUITooltipRegister>().Register(typeof(TWidget), path, provider);
        }

        class TooltipProvider : IUITooltipProvider, IUITooltipRegister
        {
            private readonly List<UITooltipMap> _list = new();

            public IEnumerable<UITooltipMap> Provide() => _list;

            public void Register(Type type, string path, IUIComponentProvider provider) =>
                _list.Add(new UITooltipMap(type, path, provider));
        }
    }
}