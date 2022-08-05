using Common;
using UnityEngine;

namespace Framework.Runtime.Services.UI
{
    public class UIComponentProviderContext
    {
        private readonly Lifetime.Definition _definition;

        public Component Component { get; private set; }

        public UIComponentProviderContext(Component component, Lifetime lifetime)
        {
            Component = component;
            _definition = Lifetime.Define(lifetime);
        }

        public Lifetime Lifetime => _definition.Lifetime;

        public void Terminate() => _definition.Terminate();
    }
}