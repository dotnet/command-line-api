using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine
{
    public static class BuilderExtensions
    {
        public static TBuilder AddCommand<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Action<CommandDefinitionBuilder> configure = null)
            where TBuilder : CommandDefinitionBuilder
        {
            CommandDefinitionBuilder commandDefinitionBuilder = null;

            if (configure != null)
            {
                commandDefinitionBuilder = new CommandDefinitionBuilder();
                configure(commandDefinitionBuilder);
            }

            builder.SymbolDefinitions.Add(
                new CommandDefinition(
                    name,
                    description,
                    commandDefinitionBuilder?.BuildSymbolDefinitions()));

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string[] aliases,
            string description = null,
            Action<ArgumentDefinitionBuilder> arguments = null)
            where TBuilder : CommandDefinitionBuilder
        {
            ArgumentDefinitionBuilder argumentDefinitionBuilder = null;

            if (arguments != null)
            {
                argumentDefinitionBuilder = new ArgumentDefinitionBuilder();
                arguments(argumentDefinitionBuilder);
            }

            builder.SymbolDefinitions.Add(
                new OptionDefinition(
                    aliases,
                    description,
                    argumentDefinitionBuilder?.Build()));

            return builder;
        }

        public static TBuilder AddOption<TBuilder>(
            this TBuilder builder,
            string name,
            string description = null,
            Action<ArgumentDefinitionBuilder> arguments = null)
            where TBuilder : CommandDefinitionBuilder
        {
            return builder.AddOption(new[] { name }, description, arguments);
        }
    }

    public class CommandParserBuilder : CommandDefinitionBuilder
    {
        public CommandParser Build()
        {
            return new CommandParser(
                new ParserConfiguration(
                    SymbolDefinitions.ToArray()));
        }
    }

    public class OptionParserBuilder : CommandDefinitionBuilder
    {
        public OptionParser Build()
        {
            return new OptionParser(
                SymbolDefinitions.Cast<OptionDefinition>().ToArray());
        }
    }

    public class CommandDefinitionBuilder
    {
        protected internal List<SymbolDefinition> SymbolDefinitions { get; } = new List<SymbolDefinition>();

        public IReadOnlyCollection<SymbolDefinition> BuildSymbolDefinitions()
        {
            return SymbolDefinitions;
        }
    }
}
