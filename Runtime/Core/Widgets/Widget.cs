using System;
using System.Collections.Generic;
using Common;
using Injections;
using UnityEngine.Pool;

namespace Framework.Runtime.Core.Widgets
{
    public interface IWidgetWithModel : IWidgetWithView
    {
        bool ModelChanged { get; }
        object Model { get; }
        void SetModel(object model);
        void OnBeforeModelChange();
        void OnAfterModelChanged();
    }

    public interface IWidgetWithModel<TModel> : IWidgetWithModel
    {
        new TModel Model { get; }
    }

    public abstract class Widget<TView, TModel> : Widget<TView>, IWidgetWithModel<TModel>
        where TView : class
    {
        private bool _modelChanged;
        
        public TModel Model { get; private set; }

        protected virtual void OnBeforeModelChange()
        {
        }

        protected virtual void OnAfterModelChanged()
        {
        }

        public void SetModel(TModel model)
        {
            OnBeforeModelChange();
            Model = model;
            _modelChanged = true;
            OnAfterModelChanged();
            ((ISubscribeNotify)this).Notify();
        }

        bool IWidgetWithModel.ModelChanged => this._modelChanged;
        object IWidgetWithModel.Model => this.Model;
        void IWidgetWithModel.SetModel(object model) => this.SetModel((TModel)model);
        void IWidgetWithModel.OnBeforeModelChange() => this.OnBeforeModelChange();
        void IWidgetWithModel.OnAfterModelChanged() => this.OnAfterModelChanged();
    }

    public interface IWidgetWithView
    {
        object View { get; }
        void SetView(object view);
        void OnViewAdded();
        void OnViewBeforeRemove();
        void OnViewAfterRemoved();
    }

    public interface IWidgetWithView<TView> : IWidgetWithView where TView : class
    {
        new TView View { get; }
        void SetView(TView view);
    }

    public abstract class Widget<TView> : Widget, IWidgetWithView<TView>, IResolve
        where TView : class
    {
        [Inject] private IResolve _resolve;

        object IResolve.Resolve(Type type) => _resolve.Resolve(type);

        public TView View { get; private set; }

        protected virtual void OnViewAdded()
        {
        }

        protected virtual void OnViewBeforeRemove()
        {
        }

        protected virtual void OnViewAfterRemoved()
        {
        }

        public void SetView(TView view)
        {
            if (view != View)
            {
                OnViewBeforeRemove();
                View = null;
                OnViewAfterRemoved();
                View = view;
                if (view != null)
                {
                    OnViewAdded();
                }

                ((ISubscribeNotify)this).Notify();
            }
        }

        void IWidgetWithView<TView>.SetView(TView view) => this.SetView(view);
        object IWidgetWithView.View => this.View;
        void IWidgetWithView.SetView(object view) => this.SetView((TView)view);
        void IWidgetWithView.OnViewAdded() => this.OnViewAdded();
        void IWidgetWithView.OnViewBeforeRemove() => this.OnViewBeforeRemove();
        void IWidgetWithView.OnViewAfterRemoved() => this.OnViewAfterRemoved();
    }

    public interface ISubscribeNotify
    {
        void Subscribe(Lifetime lifetime, Action listener);

        void Notify();
    }

    public abstract class Widget : IDisposable, ISubscribeNotify
    {
        private readonly List<Widget> _children = new();
        private Lifetime.Definition _definition;
        private Signal _onNotify;
        private bool _initialized;
        private Widget _parent;

        public Lifetime Lifetime => _definition.Lifetime;
        protected Widget Parent => _parent;

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnReady()
        {
        }

        protected virtual void OnClose()
        {
        }

        public void Close() => _definition.Terminate();

        public void Dispose() => _definition.Terminate();

        public void AddWidget(Widget widget)
        {
            if (widget == null)
            {
                throw new ArgumentNullException($"{nameof(widget)} cant be null");
            }

            if (widget._initialized)
            {
                throw new InvalidOperationException($"the widget should not be initialized: {widget}");
            }

            if (_children.Contains(widget))
            {
                throw new ArgumentException($"widget already exist: {widget}");
            }

            var modelWidget = widget as IWidgetWithModel;
            var viewWidget = widget as IWidgetWithView;

            _children.Add(widget);
            var def = _definition.Lifetime.DefineNested(widget.GetType().Name);
            Internal.Initialize(
                widget: widget,
                definition: def,
                beforeInitialization: () =>
                {
                    var nDef = def.Lifetime.DefineNested();
                    ((ISubscribeNotify)widget).Subscribe(nDef.Lifetime, () =>
                    {
                        var modelIsOk = modelWidget == null || modelWidget.ModelChanged;
                        var viewIsOk = viewWidget == null || viewWidget.View != null;
                        if (modelIsOk && viewIsOk)
                        {
                            nDef.Terminate();
                            widget.OnReady();
                        }
                    });
                });
        }

        void ISubscribeNotify.Subscribe(Lifetime lifetime, Action listener) => _onNotify?.Subscribe(lifetime, listener);
        void ISubscribeNotify.Notify() => _onNotify?.Fire();

        internal static class Internal
        {
            internal static void Initialize(Widget widget, Lifetime.Definition definition,
                Action beforeInitialization = null)
            {
                widget._definition = definition;
                widget._onNotify = new Signal(definition.Lifetime);
                widget._initialized = true;
                widget._definition.Lifetime.AddAction(() =>
                {
                    widget.OnClose();

                    if (widget._children.Count != 0)
                    {
                        var pool = ListPool<Widget>.Get();
                        pool.Clear();
                        pool.AddRange(widget._children);
                        foreach (var child in pool)
                        {
                            child._definition.Terminate();
                        }

                        pool.Clear();
                        ListPool<Widget>.Release(pool);
                    }

                    if (widget._parent != null)
                    {
                        widget._parent._children.Remove(widget);
                    }

                    widget._parent = null;
                });

                if (beforeInitialization != null)
                {
                    beforeInitialization();
                }

                widget.OnInitialize();

                ((ISubscribeNotify)widget).Notify();
            }

            internal static void Ready(Widget widget) => widget.OnReady();
        }
    }
}