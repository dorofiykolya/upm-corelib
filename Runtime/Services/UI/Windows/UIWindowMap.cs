using System;
using Framework.Runtime.Core.Widgets;

namespace Framework.Runtime.Services.UI.Windows
{
    public class UIWindowMap<T> : UIWindowMap where T : Widget
    {
        public UIWindowMap(string path, bool isFullscreen, IUIComponentProvider provider = null) : base(typeof(T), path,
            isFullscreen, provider)
        {
        }
    }

    public class UIWindowMap
    {
        public Type Type;
        public string Path;
        public bool IsFullscreen;
        public IUIComponentProvider Provider;

        public UIWindowMap(Type type, string path, bool isFullscreen, IUIComponentProvider provider = null)
        {
            Type = type;
            Path = path;
            IsFullscreen = isFullscreen;
            Provider = provider ?? new UIWindowComponentProvider();
        }
    }
}