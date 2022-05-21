using System;
using Common;

namespace Framework.Runtime.Services.UI
{
  public interface IUIComponentProvider
  {
    void Provide(Lifetime lifetime, string path, Type type, Action<UIComponentProviderContext> onResult);
  }
}
