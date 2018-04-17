using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedCommand : ParsedSymbol
    {
        public ParsedCommand(Command command) : base(command, command?.Name)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));

            AddImplicitOptions(command);
        }

        public Command Command { get; }

        public ParsedOption this[string alias] => (ParsedOption) Children[alias];

        private void AddImplicitOptions(Command option)
        {
            foreach (var childOption in option.DefinedSymbols.OfType<Option>())
            {
                if (!Children.Contains(childOption.Name) &&
                    childOption.ArgumentsRule.HasDefaultValue)
                {
                    Children.Add(
                        new ParsedOption(childOption, childOption.Name));
                }
            }
        }
    }
}
