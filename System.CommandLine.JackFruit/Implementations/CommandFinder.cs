﻿using System;
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
                  Command parent, Type baseType)
        {
            var derivedTypes = DerivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(parent, t))
                                    .ToList();
            return (false, derivedTypes);
        }

        private static (bool, IEnumerable<Command>) FromNestedTypes(
                 Command parent, Type baseType)
        {
            var nestedTypes = baseType.GetNestedTypes(Constants.PublicDeclaredInInstance)
                                     ?.Select(t => GetCommand(parent, t))
                                     .ToList();
            return (false, nestedTypes);
        }

        // TODO: Filter this for Ignore methods
        private static (bool, IEnumerable<Command>) FromMethods(Command parent, Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicDeclaredInInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(parent, m))
                            .ToList();
            return (false, commands);

            //var method = baseType.GetMethod("InvokeAsync", Reflection.Constants.PublicAndInstance);
            //return (method != null, PreBinderContext.Current.SubCommandFinder.Get(method)) ;
        }

        // Command is passed in for Root command
        internal static Command GetCommand<T>(Command parent, T source, Command command = null)
        {
            // There are order depednecies in this method
            // Arguments vs. Options - Fix has to handle args defined in parent type for hybrid: 
            // Approach - create both and remove the option after creation - extra work, but no order dependency
            // Alternate - add the options to the command earlier and pass to OptionFinder
            var names = PreBinderContext.Current.AliasFinder.Get(parent, source);
            var help = PreBinderContext.Current.HelpFinder.Get(parent, source);
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(parent, source);
            command = command ?? new Command(names?.First(), help);
            if (arguments.Any())
            {
                // TODO: When multi-arguments merged, update this
                command.Argument = arguments.First();
            }
            var options = PreBinderContext.Current.OptionFinder.Get(parent, source);
            var handler = PreBinderContext.Current.HandlerFinder.Get(parent, source);
            var subCommands = PreBinderContext.Current.SubCommandFinder.Get(command, source);
            command.AddOptions(options);
            command.AddCommands(subCommands);
            command.Handler = handler;
            return command;
        }

        public static CommandFinder Default()
            => new CommandFinder()
                   .AddApproachFromFunc<Type>(FromDerivedTypes)
                   .AddApproachFromFunc<Type>(FromNestedTypes)
                   .AddApproachFromFunc<Type>(FromMethods);
    }
}
