using System;
using Common;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public interface IUITooltip
    {
        void SubscribeOnEnter(Lifetime lifetime, Action listener);
        void SubscribeOnExit(Lifetime lifetime, Action listener);
    }
}