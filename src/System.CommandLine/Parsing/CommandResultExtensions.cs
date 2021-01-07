// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;

namespace System.CommandLine.Parsing
{
    public static class CommandResultExtensions
    {
        internal static bool TryGetValueForArgument(
            this CommandResult commandResult,
            IValueDescriptor valueDescriptor,
            out object? value)
        {
            for (var i = 0; i < commandResult.Command.Arguments.Count; i++)
            {
                var argument = commandResult.Command.Arguments[i];

                if (valueDescriptor.ValueName.IsMatch(argument.Name))
                {
                    value = commandResult.FindResultFor(argument)?.GetValueOrDefault();
                    return true;
                }
            }

            value = null;
            return false;
        }

        internal static bool TryGetValueForOption(
            this CommandResult commandResult,
            IValueDescriptor valueDescriptor,
            out object? value)
        {
            for (var i = 0; i < commandResult.Command.Options.Count; i++)
            {
                var option = commandResult.Command.Options[i];

                if (valueDescriptor.ValueName.IsMatch(option))
                {
                    var optionResult = commandResult.FindResultFor(option);

                    if (optionResult?.ConvertIfNeeded(valueDescriptor.ValueType) is SuccessfulArgumentConversionResult successful)
                    {
                        value = successful.Value;
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }
    }
}
