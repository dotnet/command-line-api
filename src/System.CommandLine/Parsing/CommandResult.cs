// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Binding;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public class CommandResult : SymbolResult
    {
        private ArgumentConversionResultSet _results;

        internal CommandResult(
            ICommand command,
            Token token,
            CommandResult parent = null) :
            base(command ?? throw new ArgumentNullException(nameof(command)),
                 parent)
        {
            Command = command;

            Token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public ICommand Command { get; }

        public OptionResult this[string alias] => OptionResult(alias);

        public OptionResult OptionResult(string alias)
        {
            return Children[alias] as OptionResult;
        }

        public Token Token { get; }


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

        internal ArgumentConversionResultSet ArgumentConversionResults
        {
            get
            {
                if (_results == null)
                {
                    var results = Children
                                  .OfType<ArgumentResult>()
                                  .Select(r => r.Convert(r.Argument));

                    _results = new ArgumentConversionResultSet();

                    foreach (var result in results)
                    {
                        _results.Add(result);
                    }
                }

                return _results;
            }
        }
    }
}
