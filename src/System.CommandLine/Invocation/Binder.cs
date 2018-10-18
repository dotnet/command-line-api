// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Builder;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Invocation
{
    internal static class Binder
    {
        public static object[] BindArguments(InvocationContext context, ParameterInfo[] parameters)
        {
            var arguments = new List<object>();

            for (var index = 0; index < parameters.Length; index++)
            {
                var parameterInfo = parameters[index];

                var parameterName = parameterInfo.Name;

                if (parameterInfo.ParameterType == typeof(ParseResult))
                {
                    arguments.Add(context.ParseResult);
                }
                else if (parameterInfo.ParameterType == typeof(InvocationContext))
                {
                    arguments.Add(context);
                }
                else if (parameterInfo.ParameterType == typeof(IConsole))
                {
                    arguments.Add(context.Console);
                }
                else
                {
                    var argument = context.ParseResult
                                          .CommandResult
                                          .ValueForOption(
                                              FindMatchingOptionName(
                                                  context.ParseResult,
                                                  parameterName));
                    arguments.Add(argument);
                }
            }

            return arguments.ToArray();
        }

        public static void SetProperties(InvocationContext context, object instance)
        {
            PropertyInfo[] properties = instance.GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyType == typeof(ParseResult))
                {
                    propertyInfo.SetValue(instance, context.ParseResult);
                }
                else if (propertyInfo.PropertyType == typeof(InvocationContext))
                {
                    propertyInfo.SetValue(instance, context);
                }
                else if (propertyInfo.PropertyType == typeof(IConsole))
                {
                    propertyInfo.SetValue(instance, context.Console);
                }
                else
                {
                    var argument = context.ParseResult
                                          .CommandResult
                                          .ValueForOption(
                                              FindMatchingOptionName(
                                                  context.ParseResult,
                                                  propertyInfo.Name));
                    propertyInfo.SetValue(instance, argument);
                }
            }
        }

        public static IEnumerable<Option> BuildOptionsFromConstructor<T>()
        {
            var constructor = typeof(T).GetConstructor();

            foreach (var parameter in constructor.GetParameters())
            {
                yield return parameter.BuildOption();
            }
        }

        public static ConstructorInfo GetConstructor(this Type type)
        {
            // TODO: Clean up to consider multiple constructors

            return type.GetConstructors().SingleOrDefault() ??
                   throw new ArgumentException($"No eligible constructor found to bind type {type}");
        }

        public static Option BuildOption(this ParameterInfo parameter)
        {
            return new Option(
                parameter.BuildAlias(),
                parameter.Name,
                new ArgumentBuilder().ParseArgumentsAs(parameter.ParameterType));
        }

        public static Option BuildOption(this PropertyInfo property)
        {
            return new Option(
                property.BuildAlias(),
                property.Name,
                new ArgumentBuilder().ParseArgumentsAs(property.PropertyType));
        }

        public static string FindMatchingOptionName(ParseResult parseResult, string parameterName)
        {
            var candidates = parseResult
                             .CommandResult
                             .Children
                             .Where(s => s.Aliases.Any(Matching))
                             .ToArray();

            if (candidates.Length == 1)
            {
                return candidates[0].Aliases.Single(Matching);
            }

            if (candidates.Length > 1)
            {
                throw new ArgumentException($"Ambiguous match while trying to bind parameter {parameterName} among: {String.Join(",", candidates.ToString())}");
            }

            return parameterName;

            bool Matching(string alias)
            {
                return String.Equals(alias.Replace("-", ""),
                                     parameterName,
                                     StringComparison.OrdinalIgnoreCase);
            }
        }

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
                       : $"-{parameterName}";
        }
    }
}
