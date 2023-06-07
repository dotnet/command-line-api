// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace System.CommandLine.NamingConventionBinder;

internal static class CommandResultExtensions
{
    internal static bool TryGetValueForArgument(
        this CommandResult commandResult,
        IValueDescriptor valueDescriptor,
        out object? value)
    {
        var arguments = commandResult.Command.Arguments;

        for (var i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];

            if (valueDescriptor.ValueName.IsMatch(RemovePrefix(argument.Name)))
            {
                if (commandResult.GetResult(argument) is { } argumentResult)
                {
                    value = argumentResult.GetValueOrDefault<object>();
                }
                else
                {
                    value = valueDescriptor.GetDefaultValue();
                }

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
        var options = commandResult.Command.Options;

        for (var i = 0; i < options.Count; i++)
        {
            if (options[i] is CliOption option)
            {
                var hasMatchingAlias =
                    HasMatchingAlias(valueDescriptor, option);

                if (hasMatchingAlias)
                {
                    var optionResult = commandResult.GetResult(option);

                    if (optionResult is not null)
                    {
                        value = optionResult.GetValueOrDefault<object>();

                        return true;
                    }
                }
            }
        }

        value = null;
        return false;

        static bool HasMatchingAlias(
            IValueDescriptor valueDescriptor,
            CliOption option)
        {
            string nameWithoutPrefix = RemovePrefix(option.Name);
            if (valueDescriptor.ValueName.Equals(nameWithoutPrefix, StringComparison.OrdinalIgnoreCase) || valueDescriptor.ValueName.IsMatch(nameWithoutPrefix))
            {
                return true;
            }

            foreach (var alias in option.Aliases)
            {
                if (valueDescriptor.ValueName.IsMatch(alias))
                {
                    return true;
                }
            }

            return false;
        }
    }

    private static bool IsMatch(this string parameterName, string alias)
    {
        var parameterNameIndex = 0;

        var indexAfterPrefix = IndexAfterPrefix(alias);
        var parameterCandidateLength = alias.Length - indexAfterPrefix;

        for (var aliasIndex = indexAfterPrefix;
             aliasIndex < alias.Length && parameterNameIndex < parameterName.Length;
             aliasIndex++)
        {
            var aliasChar = alias[aliasIndex];

            if (aliasChar == '-')
            {
                parameterCandidateLength--;
                continue;
            }

            var parameterNameChar = parameterName[parameterNameIndex];

            if (aliasChar == '|')
            {
                // replacing "|" with "or"
                parameterNameIndex += 2;
                parameterCandidateLength++;
                continue;
            }

            if (char.ToUpperInvariant(parameterNameChar) != char.ToUpperInvariant(aliasChar))
            {
                return false;
            }

            parameterNameIndex++;
        }

        if (parameterCandidateLength == parameterName.Length)
        {
            return true;
        }

        return false;

        static int IndexAfterPrefix(string alias)
        {
            if (alias.Length > 0)
            {
                switch (alias[0])
                {
                    case '-' when alias.Length > 1 && alias[1] == '-':
                        return 2;
                    case '-':
                    case '/':
                        return 1;
                }
            }

            return 0;
        }
    }

    private static string RemovePrefix(string name)
    {
        int prefixLength = GetPrefixLength(name);
        return prefixLength > 0
                   ? name.Substring(prefixLength)
                   : name;

        static int GetPrefixLength(string name)
        {
            if (name[0] == '-')
            {
                return name.Length > 1 && name[1] == '-'
                           ? 2
                           : 1;
            }

            if (name[0] == '/')
            {
                return 1;
            }

            return 0;
        }
    }
}