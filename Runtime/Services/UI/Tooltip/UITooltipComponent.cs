using System;
using Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Runtime.Services.UI.Tooltip
{
    [RequireComponent(typeof(RectTransform))]
    public class UITooltipComponent : MonoBehaviour, IUITooltip, IPointerEnterHandler, IPointerExitHandler
    {
        private readonly Lifetime.Definition _def = Lifetime.Eternal.DefineNested();
        private Signal _onEnter;
        private Signal _onExit;

        private void OnDestroy() => _def.Terminate();

        public void SubscribeOnEnter(Lifetime lifetime, Action listener)
        {
            if (_onEnter == null)
            {
                _onEnter = new Signal(_def.Lifetime);
            }

            _onEnter.Subscribe(lifetime, listener);
        }

        public void SubscribeOnExit(Lifetime lifetime, Action listener)
        {
            if (_onExit == null)
            {
                _onExit = new Signal(_def.Lifetime);
            }

            _onExit.Subscribe(lifetime, listener);
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => _onEnter?.Fire();

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => _onExit?.Fire();
    }
}