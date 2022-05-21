using System.Threading;

namespace Framework.Runtime.Utilities
{
  public interface ISynchronizationContext
  {
    void Send(SendOrPostCallback action, object state);
    void Post(SendOrPostCallback action, object state);
  }

  public class SynchronizationContextWrapper : ISynchronizationContext
  {
    public System.Threading.SynchronizationContext Context { get; }

    public SynchronizationContextWrapper(System.Threading.SynchronizationContext context)
    {
      Context = context;
    }

    public void Send(SendOrPostCallback action, object state)
    {
      Context.Send(action, state);
    }

    public void Post(SendOrPostCallback action, object state)
    {
      Context.Post(action, state);
    }
  }

}
