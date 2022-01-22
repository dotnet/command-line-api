// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using static System.CommandLine.Binding.ArgumentConversionResult;

namespace System.CommandLine.Binding
{
    internal static partial class ArgumentConverter
    {
        private delegate bool TryConvertString(string token, out object? value);

        internal static ArgumentConversionResult ConvertObject(
            Argument argument,
            Type type,
            object? value,
            LocalizationResources localizationResources)
        {
            switch (value)
            {
                case string singleValue:
                    if (type.IsEnumerable())
                    {
                        return ConvertStrings(argument, type, new[] { singleValue }, localizationResources);
                    }
                    else
                    {
                        return ConvertString(argument, type, singleValue, localizationResources);
                    }

                case IReadOnlyList<string> manyValues:
                    return ConvertStrings(argument, type, manyValues, localizationResources);
            }

            return None(argument);
        }

        private static ArgumentConversionResult ConvertString(
            Argument argument,
            Type type,
            string value,
            LocalizationResources localizationResources)
        {
            if (_stringConverters.TryGetValue(type, out var tryConvert))
            {
                if (tryConvert(value, out var converted))
                {
                    return Success(argument, converted);
                }
                else
                {
                    return Failure(argument, type, value, localizationResources);
                }
            }

            if (type.IsEnum)
            {
                try
                {
                    return Success(argument, Enum.Parse(type, value, true));
                }
                catch (ArgumentException)
                {
                    // TODO: (ConvertString) find a way to do this without the try..catch
                }
            }

            return Failure(argument, type, value, localizationResources);
        }

        public static ArgumentConversionResult ConvertStrings(
            Argument argument,
            Type type,
            IReadOnlyList<string> tokens,
            LocalizationResources localizationResources,
            ArgumentResult? argumentResult = null)
        {
            Type itemType;

            if (type == typeof(string))
            {
                type = typeof(string[]);
                itemType = typeof(string);
            }
            else
            {
                itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
            }

            var values = CreateEnumerable(type, itemType, tokens.Count);
            var isArray = values is Array;

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertString(argument, itemType, token, localizationResources);

                switch (result)
                {
                    case FailedArgumentTypeConversionResult _:
                    case FailedArgumentConversionResult _:
                        if (argumentResult is { Parent: CommandResult })
                        {
                            argumentResult.OnlyTake(i);

                            i = tokens.Count;
                            break;
                        }

                        return result;

                    case SuccessfulArgumentConversionResult success:
                        if (isArray)
                        {
                            values[i] = success.Value;
                        }
                        else
                        {
                            values.Add(success.Value);
                        }

                        break;
                }
            }

            return Success(argument, values);
        }

        internal static TryConvertArgument? GetConverter(Argument argument)
        {
            if (argument.ValueType == typeof(bool))
            {
                return TryConvertBoolArgument;
            }

            if (_stringConverters.TryGetValue(argument.ValueType, out var converter))
            {
                switch (argument.Arity.MaximumNumberOfValues)
                {
                    case 1:
                        return ConvertSingleString;

                        bool ConvertSingleString(ArgumentResult result, out object? value)
                        {
                            return converter(result.Tokens[result.Tokens.Count - 1].Value, out value);
                        }
                }
            }

            if (argument.ValueType.CanBeBoundFromScalarValue())
            {
                return TryConvertArgument;
            }

            return default;
        }

        private static bool CanBeBoundFromScalarValue(this Type type)
        {
            while (true)
            {
                if (type.IsPrimitive || type.IsEnum)
                {
                    return true;
                }

                if (type == typeof(string))
                {
                    return true;
                }

                if (type.GetElementTypeIfEnumerable() is { } itemType)
                {
                    type = itemType;
                    continue;
                }

                return false;
            }
        }

        private static FailedArgumentConversionResult Failure(
            Argument argument,
            Type expectedType,
            string value,
            LocalizationResources localizationResources)
        {
            return new FailedArgumentTypeConversionResult(argument, expectedType, value, localizationResources);
        }

        internal static ArgumentConversionResult ConvertIfNeeded(
            this ArgumentConversionResult conversionResult,
            SymbolResult symbolResult,
            Type toType)
        {
            return conversionResult switch
            {
                SuccessfulArgumentConversionResult successful when !toType.IsInstanceOfType(successful.Value) =>
                    ConvertObject(conversionResult.Argument,
                                  toType,
                                  successful.Value,
                                  symbolResult.LocalizationResources),
                SuccessfulArgumentConversionResult successful when toType == typeof(object) &&
                                                                   conversionResult.Argument.Arity.MaximumNumberOfValues > 1 &&
                                                                   successful.Value is string =>
                    ConvertObject(conversionResult.Argument,
                                  typeof(IEnumerable<string>),
                                  successful.Value,
                                  symbolResult.LocalizationResources),
                NoArgumentConversionResult _ when toType == typeof(bool) || toType == typeof(bool?) =>
                    Success(conversionResult.Argument, true),
                NoArgumentConversionResult _ when conversionResult.Argument.Arity.MinimumNumberOfValues > 0 =>
                    new MissingArgumentConversionResult(conversionResult.Argument,
                                                        symbolResult.LocalizationResources.RequiredArgumentMissing(symbolResult)),
                NoArgumentConversionResult _ when conversionResult.Argument.Arity.MaximumNumberOfValues > 1 =>
                    Success(conversionResult.Argument, Array.Empty<string>()),
                _ => conversionResult
            };
        }

        [return: MaybeNull]
        internal static T GetValueOrDefault<T>(this ArgumentConversionResult result)
        {
            return result switch
            {
                SuccessfulArgumentConversionResult successful => (T)successful.Value!,
                FailedArgumentConversionResult failed => throw new InvalidOperationException(failed.ErrorMessage),
                NoArgumentConversionResult _ => default!,
                _ => default!,
            };
        }

        public static bool TryConvertBoolArgument(ArgumentResult argumentResult, out object? value)
        {
            if (argumentResult.Tokens.Count == 0)
            {
                value = true;
                return true;
            }
            else
            {
                var success = bool.TryParse(argumentResult.Tokens[0].Value, out var parsed);
                value = parsed;
                return success;
            }
        }

        public static bool TryConvertArgument(ArgumentResult argumentResult, out object? value)
        {
            var argument = argumentResult.Argument;

            value = argument.Arity.MaximumNumberOfValues switch
            {
                // 0 is an implicit bool, i.e. a "flag"
                0 => Success(argumentResult.Argument, true),
                1 => ConvertObject(argument,
                                   argument.ValueType,
                                   argumentResult.Tokens.Count > 0
                                       ? argumentResult.Tokens[argumentResult.Tokens.Count - 1].Value
                                       : null, argumentResult.LocalizationResources),
                _ => ConvertStrings(argument,
                                    argument.ValueType,
                                    argumentResult.Tokens.Select(t => t.Value).ToArray(),
                                    argumentResult.LocalizationResources,
                                    argumentResult)
            };

            return value is SuccessfulArgumentConversionResult;
        }

        internal static object? GetDefaultValue(Type type)
        {
            if (type.IsNullable())
            {
                return null;
            }

            if (type.GetElementTypeIfEnumerable() is { } itemType)
            {
                return CreateEnumerable(type, itemType);
            }

            return type switch
            {
                { } nonGeneric
                    when nonGeneric == typeof(IList) ||
                         nonGeneric == typeof(ICollection) ||
                         nonGeneric == typeof(IEnumerable)
                    => CreateEmptyArray(typeof(object)),
                _ when type.IsValueType => CreateDefaultValueType(type),
                _ => null
            };
        }
    }
}