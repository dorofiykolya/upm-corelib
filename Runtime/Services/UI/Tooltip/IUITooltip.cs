using System;
using Common;
using UnityEngine.EventSystems;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public interface IUITooltip
    {
        void SubscribeOnEnter(Lifetime lifetime, Action<PointerEventData> listener);
        void SubscribeOnExit(Lifetime lifetime, Action<PointerEventData> listener);
        void SubscribeOnMove(Lifetime lifetime, Action<PointerEventData> listener);
    }
}