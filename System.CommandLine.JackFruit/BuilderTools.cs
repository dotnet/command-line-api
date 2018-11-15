using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.JackFruit
{
    public class BuilderTools
    {

        public static CommandLineBuilder CreateBuilder<TResult>(
              IHelpProvider helpProvider = null,
              IInvocationProvider invocationProvider = null,
              IRuleProvider ruleProvider = null,
              AliasStyle aliasStyle = AliasStyle.Attribute,
              ArgumentStyle argumentStyle = ArgumentStyle.Attribute,
              PathStyle pathStyle = PathStyle.Path)
        {
            var builderTools = new BuilderTools<TResult>(helpProvider, invocationProvider,
                ruleProvider, aliasStyle, argumentStyle, pathStyle);
            return builderTools.CreateBuilder();
        }
    }

    internal class BuilderTools<TResult>
    {
        private readonly IHelpProvider helpProvider;
        private readonly IInvocationProvider invocationProvider;
        private readonly IRuleProvider ruleProvider;
        private readonly AliasStyle aliasStyle;
        private readonly ArgumentStyle argumentStyle;
        private readonly PathStyle pathStyle;
        private readonly IEnumerable<IGrouping<Type, Type>> typesByBase;

        internal BuilderTools(
            IHelpProvider helpProvider = null,
            IInvocationProvider invocationProvider = null,
            IRuleProvider ruleProvider = null,
            AliasStyle aliasStyle = AliasStyle.Attribute,
            ArgumentStyle argumentStyle = ArgumentStyle.Attribute,
            PathStyle pathStyle = PathStyle.Path)
        {
            this.helpProvider = helpProvider;
            this.invocationProvider = invocationProvider;
            this.ruleProvider = ruleProvider;
            this.aliasStyle = aliasStyle;
            this.argumentStyle = argumentStyle;
            this.pathStyle = pathStyle;
            this. typesByBase = typeof(TResult).Assembly
                      .GetTypes()
                      .GroupBy(x => x.BaseType);
        }


        internal CommandLineBuilder CreateBuilder() => new CommandLineBuilder()
                    .AddCommands(CreateSubCommands(typesByBase, typeof(TResult)))
                    .AddStandardDirectives();


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
                    list.Add(CreateCommand(typesByBase, derivedType));
                }
            }
            return list;
        }

        private static Command CreateCommand(IEnumerable<IGrouping<Type, Type>> typesByBase, Type currentType)
        {
            var command = new Command(currentType.Name);
            var properties = currentType.GetProperties();
            // TODO: Figure out how to recognize argument
            foreach (var property in properties)
            {
                command.AddOption(TypeBinder.BuildOption(property));
            }
            var subCommands = CreateSubCommands(typesByBase, currentType);
            return command;
        }

    }
}
