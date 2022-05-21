using Common;
using Injections;

namespace Framework.Runtime.Core
{
    public interface IContext : IResolve
    {
        Lifetime Lifetime { get; }
    }
}