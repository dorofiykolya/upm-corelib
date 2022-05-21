using Framework.Runtime.Commands;
using Framework.Runtime.Core.ContextBuilder;
using Injections;

namespace Framework.Runtime.Services.Commands
{
    public static class CommandMapExtensions
    {
        public static void AddCommandMap(this IContextSetup setup)
        {
            var commandMap = new CommandMap(setup.Lifetime, setup.Injector);

            setup.Injector.ToValue(commandMap);
            setup.Injector.ToValue<ITellMessage>(commandMap);
            setup.Injector.ToValue<IMapCommand>(commandMap);
        }

        public static IMapCommand MapCommand(this IContextSetup setup)
        {
            return setup.Resolve<IMapCommand>();
        }

        public static void Tell(this IContextSetup setup, IMessage message)
        {
            setup.Resolve<ITellMessage>().Tell(message);
        }
    }
}