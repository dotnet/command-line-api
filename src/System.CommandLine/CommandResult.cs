// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.CommandLine.Binding;

namespace System.CommandLine
{
    public class CommandResult : SymbolResult
    {
        public CommandResult(
            ICommand command,
            Token token,
            CommandResult parent = null) :
            base(command ?? throw new ArgumentNullException(nameof(command)),
                 token ?? throw new ArgumentNullException(nameof(token)),
                 parent)
        {
            Command = command;
        }

        public ICommand Command { get; }

        public OptionResult this[string alias] => OptionResult(alias);

        public OptionResult OptionResult(string alias)
        {
            return Children[alias] as OptionResult;
        }

        internal void AddImplicitOption(IOption option)
        {
            Children.Add(option.CreateImplicitResult(this));
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
                       .Select(o => Create(o, token, this, ValidationMessages))
                       .SingleOrDefault();

            if (symbol != null)
            {
                Children.Add(symbol);
            }

            return symbol;
        }

        public bool TryGetValueForArgument(
            IValueDescriptor valueDescriptor,
            out object value)
        {
            foreach (var argument in Command.Arguments)
            {
                if (valueDescriptor.ValueName.IsMatch(argument.Name))
                {
                    value = ArgumentResults[argument.Name].GetValueOrDefault();
                    return true;
                }
            }

            value = null;
            return false;
        }

        public object ValueForOption(
            string alias) =>
            ValueForOption<object>(alias);

        public T ValueForOption<T>(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(alias));
            }

            if (Children[alias] is OptionResult optionResult)
            {
                return optionResult.GetValueOrDefault<T>();
            }
            else
            {
                return default;
            }
        }
    }
}
