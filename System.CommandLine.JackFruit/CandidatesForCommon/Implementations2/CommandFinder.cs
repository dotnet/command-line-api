using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.JackFruit
{
    public class CommandFinder : FinderForListBase<Command>
    {
        public CommandFinder(params Approach<IEnumerable<Command>>[] approaches)
            : base(approaches: approaches)
        { }

        private class DerivedTypeFinder
        {
            private IEnumerable<IGrouping<Type, Type>> typesByBase;

            internal DerivedTypeFinder(Type rootType)
            {
                typesByBase = rootType.Assembly
                                  .GetTypes()
                                  .GroupBy(x => x.BaseType);
            }

            internal IEnumerable<Type> GetDerivedTypes(Type baseType)
            => typesByBase
                        .Where(x => x.Key == baseType)
                        .SingleOrDefault();
        }

        private static (bool, IEnumerable<Command>) FromDerivedTypes(
                DerivedTypeFinder derivedTypeFinder, Type baseType)
        {
            var derivedTypes = derivedTypeFinder.GetDerivedTypes(baseType)
                                    ?.Select(t => GetCommand(t))
                                    .ToList();
            return (derivedTypes == null || derivedTypes.Any(), derivedTypes);
        }

        // TODO: Filter this for Ignore methods
        private static (bool, IEnumerable<Command>) FromMethod(Type baseType)
        {
            var methods = baseType.GetMethods(Reflection.Constants.PublicThisInstance)
                            .Where(m => !m.IsSpecialName);
            var commands = methods
                            .Select(m => GetCommand(m))
                            .ToList();
            return ((commands != null && commands.Any(), commands));

            //var method = baseType.GetMethod("InvokeAsync", Reflection.Constants.PublicAndInstance);
            //return (method != null, PreBinderContext.Current.SubCommandFinder.Get(method)) ;
        }

        // Command is passed in for Root command
        internal static Command GetCommand<T>(T source, Command command = null)
        {
            var names = PreBinderContext.Current.AliasFinder.Get(source);
            var help = PreBinderContext.Current.HelpFinder.Get(source);
            var arguments = PreBinderContext.Current.ArgumentFinder.Get(source);
            var options = PreBinderContext.Current.OptionFinder.Get(source);
            var handler = PreBinderContext.Current.HandlerFinder.Get(source);
            command = command ?? new Command(names?.First(), help);
            // TODO: When multi-arguments merged, update this
            if (arguments.Any())
            {
                command.Argument = arguments.First();
            }
            var subCommands = PreBinderContext.Current.SubCommandFinder.Get(source);
            command.Handler = handler;
            command.AddOptions(options);
            return command;
        }

        public static Approach<IEnumerable<Command>> DerivedTypeApproach(Type rootType)
            => Approach<IEnumerable<Command>>.CreateApproach<Type>(
                           t => FromDerivedTypes(new DerivedTypeFinder(rootType), t));

        public static Approach<IEnumerable<Command>> MethodApproach()
            => Approach<IEnumerable<Command>>.CreateApproach<Type>(FromMethod);

        public static CommandFinder Default()
            => new CommandFinder(MethodApproach());
    }
}
