using System.Collections.Generic;
using System.CommandLine.Invocation;
using System.CommandLine.JackFruit.Reflection;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class CommandProvider
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

        public static IEnumerable<Command> FromDerivedTypes(Command parent, Type baseType)
            => DerivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(parent, t))
                                    .ToList();

        public static IEnumerable<Command> FromNestedTypes(Command parent, Type baseType)
            => baseType.GetNestedTypes(Constants.PublicDeclaredInInstance)
                                     ?.Select(t => GetCommand(parent, t))
                                     .ToList();

        // TODO: Filter this for Ignore methods
        public static IEnumerable<Command> FromMethods(Command parent, Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicDeclaredInInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(parent, ReflectionCommandHandler.Create(m), m))
                            .ToList();
            return commands;

        }

        public static Command GetCommand(Command parent, Type type)
            => GetCommand(parent, ReflectionCommandHandler.Create(type), type);

        public static Command GetCommand<T>(Command parent, ReflectionCommandHandler handler, T source)
        {
            // There are order dependencies in this method
            var names = PreBinderContext.Current.AliasStrategies.Get(parent, source);

            var command = parent == null
                ? new RootCommand(names?.First())
                : new Command(names?.First(), PreBinderContext.Current.DescriptionStrategies.Get(parent, source));

            parent?.AddCommand(command);

            var sourceAndArguments = PreBinderContext.Current.ArgumentBindingStrategies.Get(command, source);

            if (sourceAndArguments.Any())
            {
                // TODO: When multi-arguments merged, update this
                var (argSource, argument) = sourceAndArguments.First();
                handler.Binder.AddBinding(argSource, argument);
                command.Argument = argument;
            }

            var sourceAndOptions = PreBinderContext.Current.OptionBindingStrategies.Get(command, source);
            foreach ((object optionSource, Option option) in sourceAndOptions)
            {
                handler.Binder.AddBinding(optionSource, option);
                command.AddOption(option);
            }

            var subCommands = PreBinderContext.Current.SubCommandStrategies.Get(command, source);
            // Commands add themselves, thus no command.AddCommands(subCommands);
            command.Handler = handler;
            return command;

        }
    }
}
