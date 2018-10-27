// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class ArgumentBuilder
    {
        private readonly List<Action<Argument>> _configureActions = new List<Action<Argument>>();

        internal HelpDetail Help { get; set; }

        internal List<ValidateSymbol> SymbolValidators { get; set; } = new List<ValidateSymbol>();

        internal HashSet<string> ValidTokens { get; } = new HashSet<string>();

        internal void Configure(Action<Argument> action)
        {
            _configureActions.Add(action);
        }

        public void AddValidator(ValidateSymbol validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            SymbolValidators.Add(validator);
        }

        public Argument Build()
        {
            AddTokenValidator();

            var argument = new Argument(SymbolValidators);

            if (Help != null)
            {
                argument.Help.Description = Help.Description;
                argument.Help.Name = Help.Name;
                argument.Help.IsHidden = Help.IsHidden;
            }

            foreach (var configure in _configureActions)
            {
                configure(argument);
            }

            return argument;
        }

        private void AddTokenValidator()
        {
            if (ValidTokens.Count == 0)
            {
                return;
            }

            AddValidator(symbol =>
            {
                if (symbol.Arguments.Count == 0)
                {
                    return null;
                }

                foreach (var arg in symbol.Arguments)
                {
                    if (!ValidTokens.Any(value => string.Equals(arg, value, StringComparison.OrdinalIgnoreCase)))
                    {
                        return symbol.ValidationMessages.UnrecognizedArgument(arg, ValidTokens);
                    }
                }

                return null;
            });
        }
    }
}
