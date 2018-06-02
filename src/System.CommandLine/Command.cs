// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    public class Command : Symbol
    {
        public Command(CommandDefinition commandDefinition, Command parent = null) : base(commandDefinition, commandDefinition?.Name, parent)
        {
            Definition = commandDefinition ?? throw new ArgumentNullException(nameof(commandDefinition));
        }

        public CommandDefinition Definition { get; }

        public Option this[string alias] => (Option) Children[alias];

        internal void AddImplicitOption(OptionDefinition optionDefinition)
        {
            Children.Add(Option.CreateImplicit(optionDefinition, this));
        }

        internal override Symbol TryTakeToken(Token token) =>
            TryTakeArgument(token) ??
            TryTakeOptionOrCommand(token);

        private Symbol TryTakeOptionOrCommand(Token token)
        {
            var symbol =
                Children.SingleOrDefault(o => o.SymbolDefinition.HasRawAlias(token.Value));

            if (symbol != null)
            {
                symbol.OptionWasRespecified = true;
                return symbol;
            }

            symbol =
                Definition.SymbolDefinitions
                          .Where(o => o.RawAliases.Contains(token.Value))
                          .Select(o => Create(o, token.Value, this, ValidationMessages))
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
