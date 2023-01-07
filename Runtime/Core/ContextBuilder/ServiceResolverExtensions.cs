using System;
using Framework.Runtime.Core.Services;
using Injections.Resolvers;

namespace Framework.Runtime.Core.ContextBuilder
{
    public static class ServiceResolverExtensions
    {
        public static void Register(
                this IServiceRegister register,
                Func<Service> factory
        )
        {
            register.Register(new ServiceResolverImpl(_ => factory()));
        }

        public static void Register<TInterface>(
                this IServiceRegister register,
                Func<Service> factory
        )
        {
            register.Register(new ServiceResolverImpl(new[]
            {
                typeof(TInterface),
            }, _ => factory()));
        }

        public static void Register<TInterface1, TInterface2>(
                this IServiceRegister register,
                Func<Service> factory
        )
        {
            register.Register(new ServiceResolverImpl(new[]
            {
                typeof(TInterface1),
                typeof(TInterface2),
            }, _ => factory()));
        }

        public static void Register<TInterface1, TInterface2, TInterface3>(
                this IServiceRegister register,
                Func<Service> factory
        )
        {
            register.Register(new ServiceResolverImpl(new[]
            {
                typeof(TInterface1),
                typeof(TInterface2),
                typeof(TInterface3),
            }, _ => factory()));
        }

        public static void Register<TInterface, TImpl>(this IServiceRegister register) where TImpl : Service
        {
            register.Register(new ServiceResolverImpl(new[]
            {
                typeof(TInterface)
            }, (injector) =>
            {
                var resolver = new SingletonResolver(typeof(TImpl), false);
                var result = (TImpl)resolver.Resolve(injector, typeof(TImpl));
                resolver.OnUnRegister();
                return result;
            }));
        }

        public static void Register<TInterface1, TInterface2, TImpl>(this IServiceRegister register) where TImpl : Service
        {
            register.Register(new ServiceResolverImpl(new[]
            {
                typeof(TInterface1),
                typeof(TInterface2)
            }, (injector) =>
            {
                var resolver = new SingletonResolver(typeof(TImpl), false);
                var result = (TImpl)resolver.Resolve(injector, typeof(TImpl));
                resolver.OnUnRegister();
                return result;
            }));
        }

        public static void Register<TInterface1, TInterface2, TInterface3, TImpl>(this IServiceRegister register) where TImpl : Service
        {
            register.Register(new ServiceResolverImpl(new[]
            {
                typeof(TInterface1),
                typeof(TInterface2),
                typeof(TInterface3),
            }, (injector) =>
            {
                var resolver = new SingletonResolver(typeof(TImpl), false);
                var result = (TImpl)resolver.Resolve(injector, typeof(TImpl));
                resolver.OnUnRegister();
                return result;
            }));
        }
    }
}
