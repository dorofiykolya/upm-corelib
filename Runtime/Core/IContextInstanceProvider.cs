using System;
using Common;
using UnityEngine.EventSystems;

namespace Framework.Runtime.Core
{
    public interface IContextInstanceProvider
    {
        EventSystem EventSystem { get; }
        int TargetDisplay { get; }
        bool IsSelected { get; }
        void Subscribe(Lifetime lifetime, Action listener);
    }
}