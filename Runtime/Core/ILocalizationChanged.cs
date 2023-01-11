using System;
using Common;

namespace Framework.Runtime.Core
{
    public interface ILocalizationChanged
    {
        void Subscribe(Lifetime lifetime, Action listener);
    }
}
