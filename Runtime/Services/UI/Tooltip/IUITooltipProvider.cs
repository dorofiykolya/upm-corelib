using System;
using System.Collections.Generic;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public interface IUITooltipProvider
    {
        IEnumerable<UITooltipMap> Provide();
    }

    public interface IUITooltipRegister
    {
        void Register(Type type, string path, IUIComponentProvider provider);
    }
}