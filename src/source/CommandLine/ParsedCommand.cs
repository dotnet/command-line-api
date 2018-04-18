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

        public override ParsedSymbol TryTakeToken(Token token) =>
            TryTakeArgument(token) ??
            TryTakeOptionOrCommand(token);

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

        private ParsedSymbol TryTakeOptionOrCommand(Token token)
        {
            var child = Children
                .SingleOrDefault(o =>
                                     o.Symbol.DefinedSymbols
                                      .Any(oo => oo.RawAliases.Contains(token.Value)));

            if (child != null)
            {
                return child.TryTakeToken(token);
            }

            if (token.Type == TokenType.Command &&
                Children.Any(o => o.Symbol is Command && !o.HasAlias(token.Value)))
            {
                // if a subcommand has already been applied, don't accept this one
                return null;
            }

            var parsedSymbol =
                Children.SingleOrDefault(o => o.Symbol.HasRawAlias(token.Value));

            if (parsedSymbol != null)
            {
                parsedSymbol.OptionWasRespecified();
                return parsedSymbol;
            }

            parsedSymbol =
                Symbol.DefinedSymbols
                      .Where(o => o.RawAliases.Contains(token.Value))
                      .Select(o => Create(o, token.Value, this))
                      .SingleOrDefault();

            if (parsedSymbol != null)
            {
                Children.Add(parsedSymbol);
            }

            return parsedSymbol;
        }
    }
}
