// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace System.CommandLine.Builder
{
    public class ArgumentBuilder
    {
        private readonly List<Action<Argument>> _configureActions = new List<Action<Argument>>();

        internal List<ValidateSymbol> SymbolValidators { get; set; } = new List<ValidateSymbol>();

        internal void Configure(Action<Argument> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

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
            var argument = new Argument(SymbolValidators);

            foreach (var configure in _configureActions)
            {
                configure(argument);
            }

            return argument;
        }
    }
}
