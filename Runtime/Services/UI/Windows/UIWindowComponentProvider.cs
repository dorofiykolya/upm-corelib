using System;
using Common;
using Framework.Runtime.Utilities;
using Injections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework.Runtime.Services.UI.Windows
{
    public class UIWindowComponentProvider : IUIComponentProvider
    {
        private readonly bool _usePool;
        private readonly Func<ITransformProvider, Transform> _provider;

#pragma warning disable 649
        [Inject] private PrefabResourceManager _prefabResourceManager;
        [Inject] private IInject _injector;
        [Inject] private ITransformProvider _transformProvider;
#pragma warning restore 649

        public UIWindowComponentProvider(bool usePool = true)
        {
            _usePool = usePool;
            _provider = (p) => p.Window;
        }

        public UIWindowComponentProvider(Func<ITransformProvider, Transform> provider, bool usePool = true)
        {
            Assert.IsNotNull(provider);
            _provider = provider;
            _usePool = usePool;
        }

        public void Provide(Lifetime lifetime, string path, Type type, Action<UIComponentProviderContext> onResult)
        {
            var def = lifetime.DefineNested();
            _prefabResourceManager.GetPrefab(path).LoadAsync(def.Lifetime, result =>
            {
                def.Terminate();
                var parent = _provider(_transformProvider);
                MonoBehaviour windowComponent = result.Instantiate<MonoBehaviour>(parent);
                if (parent == null)
                {
                    GameObject.DontDestroyOnLoad(windowComponent.gameObject);
                }

                var context = new UIComponentProviderContext(windowComponent, lifetime);
                context.Lifetime.AddAction(() =>
                {
                    result.Release(windowComponent);
                    if (!_usePool)
                    {
                        result.Collect();
                    }
                    else
                    {
                        windowComponent.transform.SetParent(_transformProvider.Pool, false);
                    }
                });
                onResult(context);
            });
        }
    }
}