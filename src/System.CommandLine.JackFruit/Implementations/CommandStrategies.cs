﻿using System.Collections.Generic;
using System.CommandLine.JackFruit.Reflection;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class CommandStrategies
    {
        private class DerivedTypeFinder
        {
            private static List<Assembly> assemblies = new List<Assembly>();
            private static List<IGrouping<Type, Type>> typesByBase = new List<IGrouping<Type, Type>>();

            internal static void LoadAssemblyFromType(Type baseType)
            {
                if (!assemblies.Contains(baseType.Assembly))
                {
                    typesByBase.AddRange(baseType.Assembly
                                        .GetTypes()
                                        .GroupBy(x => x.BaseType));
                    assemblies.Add(baseType.Assembly);
                }
            }

            internal static IEnumerable<Type> GetDerivedTypes(Type baseType)
            {
                LoadAssemblyFromType(baseType);
                return typesByBase
                        .Where(x => x.Key == baseType)
                        .SingleOrDefault();
            }
        }

        public static IEnumerable<Command> FromDerivedTypes(
                  Command parent, Type baseType)
            => DerivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(parent, t))
                                    .ToList();

        public static IEnumerable<Command> FromNestedTypes(
                 Command parent, Type baseType) 
            => baseType.GetNestedTypes(Constants.PublicDeclaredInInstance)
                                     ?.Select(t => GetCommand(parent, t))
                                     .ToList();

        // TODO: Filter this for Ignore methods
        public static  IEnumerable<Command> FromMethods(Command parent, Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicDeclaredInInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(parent, m))
                            .ToList();
            return commands;

        }

        public static Command GetCommand<T>(Command parent, T source)
        {
            // There are order dependencies in this method
            var names = PreBinderContext.Current.AliasProvider.Get(parent, source);

            var command = parent == null
                ? new RootCommand(names?.First())
                : new Command(names?.First(), PreBinderContext.Current.DescriptionProvider.Get(parent, source));

            parent?.AddCommand(command);
            var handler = PreBinderContext.Current.HandlerProvider.Get(command, source);

            if (handler is ReflectionCommandHandler reflectionHandler)
            {
                var arguments = PreBinderContext.Current.ArgumentBindingProvider.Get(command, source);
                if (arguments.Any())
                {
                    // TODO: When multi-arguments merged, update this
                    var argumentBinding = arguments.First();
                    reflectionHandler.Binder.AddBinding(argumentBinding);
                    command.Argument = argumentBinding.Symbol as Argument;
                }

                var optionBindingActions = PreBinderContext.Current.OptionBindingProvider.Get(command, source);
                reflectionHandler .AddBindings(optionBindingActions );
                command.AddOptions(optionBindingActions.Select(x => (Option)x.Symbol));
            }
            else
            {
                throw new NotImplementedException("Internal: Currently CommandStrategies only supports ReflectionCommandHandler");
            }
 
            var subCommands = PreBinderContext.Current.SubCommandProvider.Get(command, source);
            // Commands add themselves, thus no command.AddCommands(subCommands);
            command.Handler = handler;
            return command;

        }
    }
}
