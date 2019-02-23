// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine.Binding
{
    internal class CurrentSymbolResultValueSource : IValueSource
    {
        public bool TryGetValue(
            IValueDescriptor valueDescriptor,
            BindingContext bindingContext,
            out object value)
        {
            var commandResult = bindingContext.ParseResult.CommandResult;

            var optionResult = FindMatchingSymbol(commandResult, valueDescriptor);

            if (optionResult != null)
            {
                value = optionResult.GetValueOrDefault();
                return true;
            }

            if (valueDescriptor.Name.IsMatch(
                commandResult.Command.Argument.Name))
            {
                value = commandResult.GetValueOrDefault();
                return true;
            }

            value = null;
            return false;
        }

        private SymbolResult FindMatchingSymbol(
            CommandResult result,
            IValueDescriptor valueDescriptor)
        {
            var options = result
                          .Children
                          .Where(o => valueDescriptor.Name.IsMatch(o.Symbol))
                          .ToArray();

            if (options.Length == 1)
            {
                return options[0];
            }

            if (options.Length > 1)
            {
                throw new ArgumentException($"Ambiguous match while trying to bind parameter {valueDescriptor} among: {string.Join(",", options.Select(o => o.Name))}");
            }

            return null;
        }
    }
}
