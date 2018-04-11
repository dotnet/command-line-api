using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedCommand : Parsed
    {
        public ParsedCommand(Command command) : base(command, command?.Name)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));

            AddImplicitOptions(command);
        }

        public Command Command { get; }

        public ParsedOption this[string alias] => (ParsedOption) ParsedOptions[alias];

        private void AddImplicitOptions(Command option)
        {
            foreach (var childOption in option.DefinedOptions)
            {
                if (!childOption.IsCommand &&
                    !ParsedOptions.Contains(childOption.Name) &&
                    childOption.ArgumentsRule.HasDefaultValue)
                {
                    ParsedOptions.Add(
                        new ParsedOption(childOption, childOption.Name));
                }
            }
        }
    }
}
