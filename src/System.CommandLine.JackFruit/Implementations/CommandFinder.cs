using System;
using System.Collections.Generic;
using System.CommandLine.JackFruit.Reflection;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class CommandFinder : FinderBaseForList<CommandFinder, Command>
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

        private static (bool, IEnumerable<Command>) FromDerivedTypes(
                  Command[] parents, Type baseType)
        {
            var derivedTypes = DerivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(parents, t))
                                    .ToList();
            return (false, derivedTypes);
        }

        private static (bool, IEnumerable<Command>) FromNestedTypes(
                 Command[] parents, Type baseType)
        {
            var nestedTypes = baseType.GetNestedTypes(Constants.PublicDeclaredInInstance)
                                     ?.Select(t => GetCommand(parents, t))
                                     .ToList();
            return (false, nestedTypes);
        }

        // TODO: Filter this for Ignore methods
        private static (bool, IEnumerable<Command>) FromMethods(Command[] parents, Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicDeclaredInInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(parents, m))
                            .ToList();
            return (false, commands);

            //var method = baseType.GetMethod("InvokeAsync", Reflection.Constants.PublicAndInstance);
            //return (method != null, PreBinderContext.Current.SubCommandFinder.Get(method)) ;
        }

        internal static Command GetCommand<T>(Command[] parents, T source)
        {
            // There are order dependencies in this method
            var names = PreBinderContext.Current.AliasFinder.Get(parents, source);

            var command = parents == null
                ? new RootCommand(names?.First())
                : new Command(names?.First(), PreBinderContext.Current.HelpFinder.Get(parents, source));

            parents = parents == null
                ? new Command[] { command }
                : PrependParentsWithCommand();

            var arguments = PreBinderContext.Current.ArgumentFinder.Get(parents, source);
            if (arguments.Any())
            {
                // TODO: When multi-arguments merged, update this
                command.Argument = arguments.First();
            }
            var options = PreBinderContext.Current.OptionFinder.Get(parents, source);
            var handler = PreBinderContext.Current.HandlerFinder.Get(parents, source);
            var subCommands = PreBinderContext.Current.SubCommandFinder.Get(parents, source);
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



        public static CommandFinder Default()
            => new CommandFinder()
                   .AddApproachFromFunc<Type>(FromDerivedTypes)
                   .AddApproachFromFunc<Type>(FromNestedTypes)
                   .AddApproachFromFunc<Type>(FromMethods);
    }
}
