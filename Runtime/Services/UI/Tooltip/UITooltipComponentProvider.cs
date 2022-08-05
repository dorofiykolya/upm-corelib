using System;
using Common;
using Framework.Runtime.Utilities;
using Injections;
using UnityEngine;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public class UITooltipComponentProvider : IUIComponentProvider
    {
#pragma warning disable 649
        [Inject] private PrefabResourceManager _prefabResourceManager;
        [Inject] private IInject _injector;
        [Inject] private ITransformProvider _transformProvider;
#pragma warning restore 649

        public void Provide(Lifetime lifetime, string path, Type type, Action<UIComponentProviderContext> onResult)
        {
            var def = lifetime.DefineNested();
            _prefabResourceManager.GetPrefab(path).LoadAsync(def.Lifetime, result =>
            {
                def.Terminate();

                var parent = _transformProvider.Tooltip;
                var view = result.Instantiate(type, parent);
                
                if (parent == null)
                {
                    GameObject.DontDestroyOnLoad(view.gameObject);
                }

                var context = new UIComponentProviderContext(view, lifetime);
                context.Lifetime.AddAction(() =>
                {
                    result.Release(view);
                    result.Collect();
                });
                onResult(context);
            });
        }
    }
}