// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.CommandLine.Binding;

namespace System.CommandLine
{
    public class CommandResult : SymbolResult
    {
        public CommandResult(ICommand command, CommandResult parent = null) : base(command, command?.Name, parent)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public ICommand Command { get; }

        public OptionResult this[string alias] => OptionResult(alias);

        public OptionResult OptionResult(string alias)
        {
            return Children[alias] as OptionResult;
        }

        internal void AddImplicitOption(IOption option)
        {
            Children.Add(CommandLine.OptionResult.CreateImplicit(option, this));
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

        public bool TryGetValueForArgument(string alias, out object value)
        {
            if (alias.IsMatch(Command.Argument.Name))
            {
                value = this.GetValueOrDefault();
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public bool TryGetValueForOption(string alias, out object value)
        {
            var children = Children
                           .Where(o => alias.IsMatch(o.Symbol))
                           .ToArray();

            SymbolResult symbolResult = null;

            if (children.Length > 1)
            {
                throw new ArgumentException($"Ambiguous match while trying to bind parameter {alias} among: {string.Join(",", children.Select(o => o.Name))}");
            }

            if (children.Length == 1)
            {
                symbolResult = children[0];
            }

            if (symbolResult is OptionResult optionResult)
            {
                value = optionResult.GetValueOrDefault();
                return true;
            }
            else
            {
                value = null;
                return false;
            }
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
