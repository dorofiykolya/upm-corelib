using System;
using System.Collections.Generic;
using Common;
using Framework.Runtime.Core.Services;
using UnityEngine;

namespace Framework.Runtime.Core.ContextBuilder
{
    public class UnityDebugServiceObserver : MonoBehaviour, IServicesObserverRegister
    {
        public static IServicesObserverRegister Create(string context, Lifetime lifetime)
        {
            var go = new GameObject($"{context}-Services");
            DontDestroyOnLoad(go);
            lifetime.AddAction(() => GameObject.Destroy(go));
            return go.AddComponent<UnityDebugServiceObserver>();
        }
        
        [NonSerialized]
        public List<Service> Services = new List<Service>();

        public void Register(Service service)
        {
            Services.Add(service);
        }
    }
}
