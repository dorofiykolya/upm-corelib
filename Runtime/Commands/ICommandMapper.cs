using System;
using Common;

namespace Framework.Runtime.Commands
{
  public interface ICommandMapper
  {
    Lifetime RegisterCommand(Func<Lifetime, ICommand> factory, bool oneTime = false);
  }
}
