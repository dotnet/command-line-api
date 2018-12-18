// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace System.CommandLine.Invocation
{
    internal static class Binder
    {
        public static Option BuildOption(this ParameterInfo parameter)
        {
            var argument = new Argument
                           {
                               ArgumentType = parameter.ParameterType
                           };

            if (parameter.HasDefaultValue)
            {
                argument.SetDefaultValue(() => parameter.DefaultValue);
            }

            var option = new Option(
                parameter.BuildAlias(),
                parameter.Name,
                argument);

            return option;
        }

        public static Option BuildOption(this PropertyInfo property)
        {
            return new Option(
                property.BuildAlias(),
                property.Name,
                new Argument
                {
                    ArgumentType = property.PropertyType
                });
        }

        public static string FindMatchingOptionName(
            ParseResult parseResult,
            string parameterName)
        {
            var aliases = parseResult
                          .CommandResult
                          .Children
                          .SelectMany(c => c.Aliases)
                          .Where(parameterName.IsMatch)
                          .ToArray();

            if (aliases.Length == 1)
            {
                return aliases[0];
            }

            if (aliases.Length > 1)
            {
                throw new ArgumentException($"Ambiguous match while trying to bind parameter {parameterName} among: {string.Join(",", aliases)}");
            }

            return parameterName;
        }

        internal static bool IsMatch(this string parameterName, string alias) =>
            string.Equals(alias.RemovePrefix()
                               .FromKebabCase(),
                          parameterName,
                          StringComparison.OrdinalIgnoreCase);

        private static readonly HashSet<Type> _infrastructureTypes = new HashSet<Type>(
            new[]
            {
                typeof(IConsole),
                typeof(ITerminal),
                typeof(InvocationContext),
                typeof(ParseResult),
                typeof(CancellationToken)
            }
        );

        internal static IEnumerable<PropertyInfo> OmitInfrastructureTypes(
            this IEnumerable<PropertyInfo> source) =>
            source.Where(i => !_infrastructureTypes.Contains(i.PropertyType));

        internal static IEnumerable<ParameterInfo> OmitInfrastructureTypes(
            this IEnumerable<ParameterInfo> source) =>
            source.Where(i => !_infrastructureTypes.Contains(i.ParameterType));

        public static string BuildAlias(this ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return BuildAlias(parameter.Name);
        }

        public static string BuildAlias(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return BuildAlias(property.Name);
        }

        private static string BuildAlias(string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(parameterName));
            }

            return parameterName.Length > 1
                       ? $"--{parameterName.ToKebabCase()}"
                       : $"-{parameterName.ToLowerInvariant()}";
        }
    }
}
