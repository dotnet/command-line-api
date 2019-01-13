using System.Collections.Generic;
using System.CommandLine.JackFruit.Reflection;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public static class CommandStrategies2
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

        public static (bool, IEnumerable<Command>) FromDerivedTypes(
                  Command parent, Type baseType)
        {
            var derivedTypes = DerivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(parent, t))
                                    .ToList();
            return (false, derivedTypes);
        }

        public static (bool, IEnumerable<Command>) FromNestedTypes(
                 Command parent, Type baseType)
        {
            var nestedTypes = baseType.GetNestedTypes(Constants.PublicDeclaredInInstance)
                                     ?.Select(t => GetCommand(parent, t))
                                     .ToList();
            return (false, nestedTypes);
        }

        // TODO: Filter this for Ignore methods
        public static (bool, IEnumerable<Command>) FromMethods(Command parent, Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicDeclaredInInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(parent, m))
                            .ToList();
            return (false, commands);

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
                var optionBindingActions = PreBinderContext.Current.OptionBindingActionProvider.Get(command, source);
                reflectionHandler .AddBindings(optionBindingActions );

            }




            var arguments = PreBinderContext.Current.ArgumentProvider.Get(command, source);
            if (arguments.Any())
            {
              // TODO: When multi-arguments merged, update this
                command.Argument = arguments.First();
            }
            var options = PreBinderContext.Current.OptionProvider.Get(command, source);
            var subCommands = PreBinderContext.Current.SubCommandProvider.Get(command, source);
            command.AddOptions(options);
            // Commands add themselves, thus no command.AddCommands(subCommands);
            command.Handler = handler;
            return command;

        }
    }
}
