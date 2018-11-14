using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    internal static class BuilderTools
    {
        internal static CommandLineBuilder AddStandardDirectives(this CommandLineBuilder builder)
            => builder
                .UseDebugDirective()
                .UseParseErrorReporting()
                .UseParseDirective()
                .UseHelp()
                .UseSuggestDirective()
                .RegisterWithDotnetSuggest()
                .UseExceptionHandler();

        internal static CommandLineBuilder Create<TCli, THelper>()
        {
            var baseType = typeof(TCli);
            var assem = baseType.Assembly;
            var assemTypes = assem.GetTypes();
            var typesByBase = assemTypes
                         .GroupBy(x => x.BaseType);
            var commandBuilder = new CommandLineBuilder();
            var commands = CreateSubCommands(typesByBase, baseType);
            foreach (var command in commands)
            {
                commandBuilder.AddCommand(command);
            }
            return commandBuilder;
        }

        private static Command Create(IEnumerable<IGrouping<Type, Type>> typesByBase, Type currentType)
        {
            var command = new Command(currentType.Name);
            var properties = currentType.GetProperties();
            // TODO: Figure out how to recognize argument
            foreach (var property in properties)
            {
                command.AddOption(CreateStronlgyTypedOption(property));
            }
            var subCommands = CreateSubCommands(typesByBase, currentType);
            return command;
        }

        private static IEnumerable<Command> CreateSubCommands(IEnumerable<IGrouping<Type, Type>> typesByBase, Type currentType)
        {
            var derivedTypes = typesByBase
                               .Where(x => x.Key == currentType)
                               .SingleOrDefault();
            var list = new List<Command>();
            if (derivedTypes == null)
            {
                // At the end of recursion
            }
            else
            {
                foreach (var derivedType in derivedTypes)
                {
                    list.Add(Create(typesByBase, derivedType));
                }
            }
            return list;
        }

        private static Option CreateStronlgyTypedOption(PropertyInfo property)
        {
            var methodInfo = typeof(BuilderTools).GetMethod(nameof(CreateStronlgyTypedOptionInternal));
            var constructedMethod = methodInfo.MakeGenericMethod(property.PropertyType);
            return (Option)constructedMethod.Invoke(null, new object[] { property });
        }

        private static Option CreateStronlgyTypedOptionInternal<T>(PropertyInfo property)
        {
            return new Option(property.Name, argument: new Argument<T>());
        }
    }
}
