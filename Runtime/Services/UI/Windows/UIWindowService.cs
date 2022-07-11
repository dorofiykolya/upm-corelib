#pragma warning disable CS0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Framework.Runtime.Core.Services;
using Framework.Runtime.Core.Widgets;
using Framework.Runtime.Utilities.Components;
using Injections;
using UnityEngine.Assertions;

namespace Framework.Runtime.Services.UI.Windows
{
    public class UIWindowService : Service, IUIWindowService
    {
        private class UIWindowContext
        {
            private readonly UIWindowReference _reference;
            private readonly Lifetime.Definition _definition;

            public UIWindowContext(UIWindowReference reference, Lifetime.Definition definition)
            {
                _reference = reference;
                _definition = definition;
            }

            public Action<Action> Factory;

            public Lifetime.Definition Definition => _definition;
            public UIWindowReference Reference => _reference;
        }

        [Inject] private IInjector _injector;
        [Inject] private IUIWindowsProvider _windowsProvider;

        private readonly Dictionary<Type, UIWindowMap> _map = new();
        private readonly List<UIWindowContext> _opened = new();
        private readonly LinkedList<UIWindowContext> _queue = new();
        private Signal _onChanged;
        private Signal<Type, UIWindowActionKind> _onChangedEx;

        public UIWindowReference[] Opened => _opened.Select(w => w.Reference).ToArray();

        public UIWindowReference[] Queue => _queue.Select(w => w.Reference).ToArray();

        protected override Task OnAwake()
        {
            _onChanged = new Signal(Lifetime);
            _onChangedEx = new Signal<Type, UIWindowActionKind>(Lifetime);
            return base.OnAwake();
        }

        protected override Task OnInitialize()
        {
            foreach (var windowMap in _windowsProvider.Provide())
            {
                _map[windowMap.Type] = windowMap;
                _injector.ToFactory(windowMap.Type);
            }

            return base.OnInitialize();
        }

        public UIWindowReference Open(Type type, Action<Widget> onOpen, object model)
        {
            if (!type.IsSubclassOf(typeof(Widget)))
            {
                throw new ArgumentException($"{nameof(type)} is not subclass of {typeof(Widget)}");
            }

            var definition = Lifetime.Define(Lifetime);
            var reference = new UIWindowReference(definition, _map[type].IsFullscreen, type, model);
            var context = new UIWindowContext(reference, definition);
            Enqueue(type, onOpen, context, model);
            return reference;
        }

        private void Enqueue(Type type, Action<Widget> onOpen, UIWindowContext context, object model)
        {
            var definition = context.Definition;
            context.Factory = callback =>
            {
                var map = _map[type];
                _injector.Inject(map.Provider);
                map.Provider.Provide(definition.Lifetime, map.Path, map.Type, providerContext =>
                {
                    var mediator = (Widget)_injector.Resolve(type);
                    var view = providerContext.Component;

                    view.gameObject.AddComponent<SignalMonoBehaviour>().DestroySignal
                        .Subscribe(definition.Lifetime, definition.Terminate);

                    definition.Lifetime.AddAction(() =>
                    {
                        //Widget.Internal.Close(mediator);
                        _opened.Remove(context);
                        providerContext.Terminate();
                        _onChangedEx.Fire(type, UIWindowActionKind.WindowClosed);
                        _onChanged.Fire();
                    });

                    _opened.Add(context);

                    Widget.Internal.Initialize(_injector, mediator, definition);

                    if (!definition.IsTerminated)
                    {
                        var mediatorModel = mediator as IWidgetWithModel;
                        if (model != null)
                        {
                            Assert.IsNotNull(mediatorModel);
                            mediatorModel.SetModel(model);
                        }

                        if (!definition.IsTerminated)
                        {
                            var mediatorView = (IWidgetWithView)mediator;
                            mediatorView.SetView(view);

                            Widget.Internal.Ready(mediator);
                            if (!definition.IsTerminated)
                            {
                                if (onOpen != null)
                                {
                                    onOpen(mediator);
                                }
                            }
                        }
                    }

                    callback();

                    _onChangedEx.Fire(type, UIWindowActionKind.WindowOpened);
                    _onChanged.Fire();
                });
            };

            _queue.AddLast(context);
            definition.Lifetime.AddAction(() => { _queue.Remove(context); });
            context.Factory(() => { });
        }

        public void SubscribeOnChanged(Lifetime lifetime, Action listener)
        {
            _onChanged.Subscribe(lifetime, listener);
        }

        public void SubscribeOnChanged(Lifetime lifetime, Action<Type, UIWindowActionKind> listener)
        {
            _onChangedEx.Subscribe(lifetime, listener);
        }
    }
}