// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    public class CommandResult : SymbolResult
    {
        internal CommandResult(
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

        internal virtual RootCommandResult Root => (Parent as CommandResult)?.Root;

        internal bool TryGetValueForArgument(
            IValueDescriptor valueDescriptor,
            out object value)
        {
            foreach (var argument in Command.Arguments)
            {
                if (valueDescriptor.ValueName.IsMatch(argument.Name))
                {
                    value = ArgumentConversionResults[argument.Name].GetValueOrDefault();
                    return true;
                }
            }

            value = null;
            return false;
        }

        public object ValueForOption(string alias)
        {
            if (Children[alias] is OptionResult optionResult)
            {
                if (optionResult.Option.Argument.Arity.MaximumNumberOfValues > 1)
                {
                    return optionResult.GetValueOrDefault<IEnumerable<string>>();
                }
            }

            return ValueForOption<object>(alias);
        }

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
