using System;

namespace Framework.Runtime.Services.UI.Hud
{
    public class UIHudMap
    {
        public Type Type;
        public string Path;
        public IUIComponentProvider Provider;

        public UIHudMap(Type type, string path, IUIComponentProvider provider = null)
        {
            Type = type;
            Path = path;
            Provider = provider ?? new UIHudComponentProvider();
        }
    }
}