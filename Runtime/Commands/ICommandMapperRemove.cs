using System;

namespace Framework.Runtime.Commands
{
  public interface ICommandMapperRemove
  {
    void Remove<T>() where T : ICommand;
    void Remove(Type type);
  }
}
