using System;
using Common;

namespace Framework.Runtime.Core
{
    public abstract class Context : IContext
    {
        public Lifetime Lifetime { get; }

        protected Context(Lifetime lifetime)
        {
            Lifetime = lifetime;
        }

        public abstract object Resolve(Type type);
    }
}