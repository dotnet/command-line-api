using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedCommand : Parsed
    {
        public ParsedCommand(Command command) : base(command?.Name)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));

            AddImplicitOptions(command);
        }

        public Command Command { get; }

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
