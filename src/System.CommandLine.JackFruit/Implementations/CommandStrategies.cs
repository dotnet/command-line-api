using System.Collections.Generic;
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

        public static (bool, IEnumerable<Command>) FromDerivedTypes(
                  Command[] parents, Type baseType)
        {
            var derivedTypes = DerivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(parents, t))
                                    .ToList();
            return (false, derivedTypes);
        }

        public static (bool, IEnumerable<Command>) FromNestedTypes(
                 Command[] parents, Type baseType)
        {
            var nestedTypes = baseType.GetNestedTypes(Constants.PublicDeclaredInInstance)
                                     ?.Select(t => GetCommand(parents, t))
                                     .ToList();
            return (false, nestedTypes);
        }

        // TODO: Filter this for Ignore methods
        public static (bool, IEnumerable<Command>) FromMethods(Command[] parents, Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicDeclaredInInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(parents, m))
                            .ToList();
            return (false, commands);

        }

        public static Command GetCommand<T>(Command[] parents, T source)
        {
            // There are order dependencies in this method
            var names = PreBinderContext.Current.AliasProvider.Get(parents, source);

            var command = parents == null
                ? new RootCommand(names?.First())
                : new Command(names?.First(), PreBinderContext.Current.DescriptionProvider.Get(parents, source));

            parents = parents == null
                ? new Command[] { command }
                : PrependParentsWithCommand();

            var arguments = PreBinderContext.Current.ArgumentProvider.Get(parents, source);
            if (arguments.Any())
            {
                // TODO: When multi-arguments merged, update this
                command.Argument = arguments.First();
            }
            var options = PreBinderContext.Current.OptionProvider.Get(parents, source);
            var handler = PreBinderContext.Current.HandlerProvider.Get(parents, source);
            var subCommands = PreBinderContext.Current.SubCommandProvider.Get(parents, source);
            command.AddOptions(options);
            command.AddCommands(subCommands);
            command.Handler = handler;
            return command;

            Command[] PrependParentsWithCommand()
            {
                var parentList = parents.ToList();
                parentList.Insert(0, command);
                return parentList.ToArray();
            }
        }
    }
}
