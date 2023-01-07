using System;
using Framework.Runtime.Core.Services;

namespace Framework.Runtime.Core.ContextBuilder
{
    public interface IServiceRegister
    {
        void Register(Func<Service> factory);
        void Register<TInterface>(Func<Service> factory);
        void Register<TInterface, TImpl>() where TImpl : Service;
    }
}