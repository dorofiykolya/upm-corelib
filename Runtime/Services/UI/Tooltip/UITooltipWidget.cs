using Framework.Runtime.Core.Widgets;

namespace Framework.Runtime.Services.UI.Tooltip
{
    public class UITooltipWidget<TTooltip, TTooltipModel> : Widget<IUITooltip, TTooltipModel>
        where TTooltip : Widget, IWidgetWithModel<TTooltipModel>
    {
        private readonly UITooltipService _service;

        public UITooltipWidget(UITooltipService service) => _service = service;

        protected override void OnReady()
        {
            View.SubscribeOnEnter(Lifetime, () =>
            {
                var reference = _service.Open<TTooltip, TTooltipModel>(Model);
                Lifetime.AddAction(reference.Close);
                View.SubscribeOnExit(Lifetime, () => { reference.Close(); });
            });
        }
    }
}