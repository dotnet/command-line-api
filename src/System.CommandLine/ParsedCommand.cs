// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace System.CommandLine
{
    public class ParsedCommand : ParsedSymbol
    {
        public ParsedCommand(CommandDefinition commandDefinition, ParsedCommand parent = null) : base(commandDefinition, commandDefinition?.Name, parent)
        {
            CommandDefinition = commandDefinition ?? throw new ArgumentNullException(nameof(commandDefinition));

            AddImplicitOptions(commandDefinition);
        }

        public CommandDefinition CommandDefinition { get; }

        public ParsedOption this[string alias] => (ParsedOption) Children[alias];

        public override ParsedSymbol TryTakeToken(Token token) =>
            TryTakeArgument(token) ??
            TryTakeOptionOrCommand(token);

        private void AddImplicitOptions(CommandDefinition commandDefinition)
        {
            foreach (var childOption in commandDefinition.SymbolDefinitions.OfType<OptionDefinition>())
            {
                if (!Children.Contains(childOption.Name) &&
                    childOption.ArgumentDefinition.HasDefaultValue)
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
                                     o.SymbolDefinition.SymbolDefinitions
                                      .Any(oo => oo.RawAliases.Contains(token.Value)));

            if (child != null)
            {
                return child.TryTakeToken(token);
            }

            if (token.Type == TokenType.Command &&
                Children.Any(o => o.SymbolDefinition is CommandDefinition &&
                                  !o.HasAlias(token.Value)))
            {
                // if a subcommand has already been applied, don't accept this one
                return null;
            }

            var parsedSymbol =
                Children.SingleOrDefault(o => o.SymbolDefinition.HasRawAlias(token.Value));

            if (parsedSymbol != null)
            {
                parsedSymbol.OptionWasRespecified();
                return parsedSymbol;
            }

            parsedSymbol =
                SymbolDefinition.SymbolDefinitions
                      .Where(o => o.RawAliases.Contains(token.Value))
                      .Select(o => Create(o, token.Value, this))
                      .SingleOrDefault();

            if (parsedSymbol != null)
            {
                Children.Add(parsedSymbol);
            }

            return parsedSymbol;
        }

        public object ValueForOption(
            string alias) =>
            ValueForOption<object>(alias);

        public T ValueForOption<T>(
            string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            return Children[alias].GetValueOrDefault<T>();
        }
    }
}
