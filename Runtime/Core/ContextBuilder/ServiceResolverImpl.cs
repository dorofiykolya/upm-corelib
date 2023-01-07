using System;
using Framework.Runtime.Core.Services;
using Injections;

namespace Framework.Runtime.Core.ContextBuilder
{
    public class ServiceResolverImpl : IServiceResolver
    {
        public ServiceResolverImpl(Type[] interfaces, Func<IInjector, Service> resolver)
        {
            Interfaces = interfaces;
            Resolver = resolver;
        }
        
        public ServiceResolverImpl(Func<IInjector, Service> resolver)
        {
            Resolver = resolver;
        }
        
        public Type[] Interfaces { get; }
        public Func<IInjector, Service> Resolver { get; }
    }
}
