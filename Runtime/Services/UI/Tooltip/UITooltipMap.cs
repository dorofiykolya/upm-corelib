using System;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public class UITooltipMap
    {
        public Type Type;
        public string Path;
        public IUIComponentProvider Provider;

        public UITooltipMap(Type type, string path, IUIComponentProvider provider = null)
        {
            Type = type;
            Path = path;
            Provider = provider ?? new UITooltipComponentProvider();
        }
    }
}