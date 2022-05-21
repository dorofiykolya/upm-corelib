namespace Framework.Runtime.Commands
{
    public interface IMapCommand
    {
        ICommandMapper Map<TMessage>() where TMessage : IMessage;
    }
}