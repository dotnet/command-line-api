// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    public class CommandResult : SymbolResult
    {
        public CommandResult(ICommand command, CommandResult parent = null) : base(command, command?.Name, parent)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public ICommand Command { get; }

        public OptionResult this[string alias] => (OptionResult) Children[alias];

        internal void AddImplicitOption(IOption option)
        {
            Children.Add(OptionResult.CreateImplicit(option, this));
        }

        internal override SymbolResult TryTakeToken(Token token) =>
            TryTakeArgument(token) ??
            TryTakeOptionOrCommand(token);

        private SymbolResult TryTakeOptionOrCommand(Token token)
        {
            var symbol =
                Children.SingleOrDefault(o => o.Symbol.HasRawAlias(token.Value));

            if (symbol != null)
            {
                symbol.OptionWasRespecified = true;
                return symbol;
            }

            symbol =
                Command.Children
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
