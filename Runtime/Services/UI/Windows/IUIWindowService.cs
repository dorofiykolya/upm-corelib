using System;
using Common;
using Framework.Runtime.Core.Widgets;

namespace Framework.Runtime.Services.UI.Windows
{
    public interface IUIWindowService
    {
        UIWindowReference[] Opened { get; }
        UIWindowReference Open(Type type, Action<Widget> onOpen, object model);
        void SubscribeOnChanged(Lifetime lifetime, Action listener);
        void SubscribeOnChanged(Lifetime lifetime, Action<Type, UIWindowActionKind> listener);
    }
}