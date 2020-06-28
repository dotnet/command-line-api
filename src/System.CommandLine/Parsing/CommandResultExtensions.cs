// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.CommandLine.Parsing
{
    public static class CommandResultExtensions
    {
        public static object? GetArgumentValueOrDefault(
            this CommandResult commandResult,
            string argumentName)
        {
            return commandResult.GetArgumentValueOrDefault<object?>(argumentName);
        }

        [return: MaybeNull]
        public static T GetArgumentValueOrDefault<T>(
            this CommandResult commandResult,
            string argumentName)
        {
            var conversionResult =
                commandResult.ArgumentConversionResults
                             .SingleOrDefault(r => r.Argument.Name == argumentName);

            return conversionResult.GetValueOrDefault<T>();
        }

        internal static bool TryGetValueForOption(
            this CommandResult commandResult,
            IValueDescriptor valueDescriptor,
            out object? value)
        {
            SymbolResult? symbolResult = SymbolFromExplicitSymbol(commandResult, valueDescriptor);
            if (symbolResult is null)
            {
                symbolResult = SymbolFromName(commandResult, valueDescriptor);
            }

            if (symbolResult is OptionResult optionResult)
            {
                if (optionResult.ConvertIfNeeded(valueDescriptor.ValueType) is SuccessfulArgumentConversionResult successful)
                {
                    value = successful.Value;
                    return true;
                }
            }

            value = null;
            return false;

            static SymbolResult? SymbolFromName(CommandResult commandResult, IValueDescriptor valueDescriptor)
            {
                var children = commandResult
                                           .Children
                                           .Where(o => valueDescriptor.ValueName?.IsMatch(o.Symbol) == true)
                                           .ToArray();

                SymbolResult? symbolResult = null;

                if (children.Length > 1)
                {
                    throw new ArgumentException(
                        $"Ambiguous match while trying to bind parameter {valueDescriptor.ValueName} among: {string.Join(",", children.Select(o => o.Symbol.Name))}");
                }

                if (children.Length == 1)
                {
                    symbolResult = children[0];
                }

                return symbolResult;
            }

            static SymbolResult? SymbolFromExplicitSymbol(CommandResult commandResult, IValueDescriptor valueDescriptor)
            {
                var children = commandResult
                                           .Children
                                           .Where(o => valueDescriptor == o.Symbol)
                                           .ToArray();

                SymbolResult? symbolResult = null;

                if (children.Length > 1)
                {
                    throw new ArgumentException(
                        $"Ambiguous match while trying to bind parameter {valueDescriptor.ValueName} among: {string.Join(",", children.Select(o => o.Symbol.Name))}");
                }

                if (children.Length == 1)
                {
                    symbolResult = children[0];
                }

                return symbolResult;
            }
        }
    }
}
