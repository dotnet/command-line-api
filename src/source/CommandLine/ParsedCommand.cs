// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public class ParsedCommand : ParsedSymbol
    {
        public ParsedCommand(Command command, ParsedCommand parent = null) : base(command, command?.Name, parent)
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
