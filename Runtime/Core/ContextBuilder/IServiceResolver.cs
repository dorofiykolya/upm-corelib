using System;
using Framework.Runtime.Core.Services;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public interface IServiceResolver
    {
        Type[] Interfaces { get; }
        Func<IInjector, Service> Resolver { get; }
    }
}
