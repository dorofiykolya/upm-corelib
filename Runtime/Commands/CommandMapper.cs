﻿using System;
using System.Collections.Generic;
using Common;
using Injections;

namespace Framework.Runtime.Commands
{
    public class CommandMapper : ICommandMapper
    {
        private Lifetime _lifetime;
        private Type _messageType;
        private readonly IInjector _injector;
        private List<Type> _map;
        private List<CommandFactory> _commands;

        public CommandMapper(Lifetime lifetime, Type messageType, IInjector injector)
        {
            _lifetime = lifetime;
            _messageType = messageType;
            _injector = new Injector(injector);
            _commands = new List<CommandFactory>();
        }

        public Lifetime RegisterCommand(Func<Lifetime, ICommand> factory, bool oneTime = false)
        {
            var lifetime = Lifetime.Define(_lifetime);
            var commandFactory = new CommandFactory(factory, oneTime, lifetime);
            _commands.Add(commandFactory);
            lifetime.Lifetime.AddAction(() =>
            {
                _commands.Remove(commandFactory);
            });
            return lifetime.Lifetime;
        }

        public void Tell(object message)
        {
            foreach (var factory in _commands.ToArray())
            {
                var command = factory.Factory(factory.Lifetime.Lifetime);

                _injector.ToValue<Lifetime.Definition>(factory.Lifetime);
                _injector.ToValue(_messageType, message);
                _injector.Inject(command);
                command.Execute();
                _injector.UnRegister(_messageType);
                _injector.UnRegister(typeof(Lifetime.Definition));
                if (factory.OneTime)
                {
                    factory.Lifetime.Terminate();
                }
            }
        }

        private class CommandFactory
        {
            public Func<Lifetime, ICommand> Factory;
            public bool OneTime;
            public Lifetime.Definition Lifetime;

            public CommandFactory(Func<Lifetime, ICommand> factory, bool oneTime, Lifetime.Definition lifetime)
            {
                Factory = factory;
                OneTime = oneTime;
                Lifetime = lifetime;
            }
        }
    }
}