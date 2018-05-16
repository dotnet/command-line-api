// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace System.CommandLine
{
    public class Command : Symbol
    {
        public Command(CommandDefinition commandDefinition, Command parent = null) : base(commandDefinition, commandDefinition?.Name, parent)
        {
            Definition = commandDefinition ?? throw new ArgumentNullException(nameof(commandDefinition));

            AddImplicitOptions(commandDefinition);
        }

        public CommandDefinition Definition { get; }

        public Option this[string alias] => (Option) Children[alias];

        public override Symbol TryTakeToken(Token token) =>
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
                        new Option(childOption, childOption.Name));
                }
            }
        }

        private Symbol TryTakeOptionOrCommand(Token token)
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

            var symbol =
                Children.SingleOrDefault(o => o.SymbolDefinition.HasRawAlias(token.Value));

            if (symbol != null)
            {
                symbol.OptionWasRespecified();
                return symbol;
            }

            symbol =
                Definition.SymbolDefinitions
                      .Where(o => o.RawAliases.Contains(token.Value))
                      .Select(o => Create(o, token.Value, this))
                      .SingleOrDefault();

            if (symbol != null)
            {
                Children.Add(symbol);
            }

            return symbol;
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
