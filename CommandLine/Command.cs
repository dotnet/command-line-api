using System;
using System.Collections.Generic;
using System.Linq;
using static CommandLine.Accept;
using static CommandLine.Create;

namespace CommandLine
{
    public class Command : Option
    {
        private readonly List<string> allowedValues = new List<string>();

        public Command(
            string name,
            string help,
            Option[] options = null,
            ArgumentsRule arguments = null,
            Func<AppliedOption, object> materialize = null) :
            base(new[] { name }, help, arguments, options, materialize)
        {
        }

        public Command(
            string name,
            string help,
            params Command[] subcommands) :
            base(new[] { name }, help, options: subcommands)
        {
            var commandNames = subcommands.SelectMany(o => o.Aliases).ToArray();

            var rule =
                ExactlyOneCommandRequired
                    .And(
                        ParseRule(o => !commandNames.Any(
                                           o.AppliedOptions.Single().HasAlias)
                                           ? $"Command '{o.AppliedOptions.Single()}' not recognized. Must be one of:\n\t{string.Join("\n\t", commandNames.Select(v => $"'{v}'"))}"
                                           : "",
                                  commandNames));

            ArgumentsRule = rule;

            allowedValues.AddRange(commandNames);
        }

        protected internal override IReadOnlyCollection<string> AllowedValues =>
            base.AllowedValues.Concat(allowedValues).ToArray();

        internal override bool IsCommand => true;
    }
}