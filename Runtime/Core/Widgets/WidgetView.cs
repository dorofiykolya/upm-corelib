using Common;
using UnityEngine;

namespace Framework.Runtime.Core.Widgets
{
    public class WidgetView : MonoBehaviour
    {
        private Lifetime.Definition _definition;

        protected Lifetime Lifetime => _definition.Lifetime;

        protected virtual void OnAwake()
        {
        }

        private void Awake()
        {
            _definition = Lifetime.Define(Lifetime.Eternal);
            OnAwake();
        }

        private void OnDestroy() => _definition.Terminate();
    }
}