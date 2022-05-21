using System;
using Injections;

namespace Framework.Runtime.Core.Services
{
    public abstract class Services : IResolve
    {
        private readonly IResolve _resolve;
        

        protected Services(IResolve resolve) => _resolve = resolve;

        public object Resolve(Type type) => _resolve.Resolve(type);
    }
}