// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using static System.CommandLine.Binding.ArgumentConversionResult;

namespace System.CommandLine.Binding
{
    internal static partial class ArgumentConverter
    {
        internal static ArgumentConversionResult ConvertObject(
            ArgumentResult argumentResult,
            Type type,
            object? value)
        {
            switch (value)
            {
                case CliToken singleValue:
                    return ConvertToken(argumentResult, type, singleValue);

                case IReadOnlyList<CliToken> manyValues:
                    return ConvertTokens(argumentResult, type, manyValues);

                default:

                    if (argumentResult.Tokens.Count == 0)
                    {
                        return None(argumentResult);
                    }
                    else
                    {
                        throw new InvalidCastException();
                    }
            }
        }

        private static ArgumentConversionResult ConvertToken(
            ArgumentResult argumentResult,
            Type type,
            CliToken token)
        {
            var value = token.Value;

            if (type.TryGetNullableType(out var nullableType))
            {
                return ConvertToken(argumentResult, nullableType, token);
            }

            if (StringConverters.TryGetValue(type, out var tryConvert))
            {
                if (tryConvert(value, out var converted))
                {
                    return Success(argumentResult, converted);
                }
                else
                {
                    return ArgumentConversionCannotParse(argumentResult, type, value);
                }
            }

            if (type.IsEnum)
            {
#if NET7_0_OR_GREATER
                if (Enum.TryParse(type, value, ignoreCase: true, out var converted))
                {
                    return Success(argumentResult, converted);
                }
#else
                try
                {
                    return Success(argumentResult, Enum.Parse(type, value, true));
                }
                catch (ArgumentException)
                {
                }
#endif
            }

            return ArgumentConversionCannotParse(argumentResult, type, value);
        }

        private static ArgumentConversionResult ConvertTokens(
            ArgumentResult argumentResult,
            Type type,
            IReadOnlyList<CliToken> tokens)
        {
            var itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
            var values = CreateEnumerable(type, itemType, tokens.Count);
            var isArray = values is Array;

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertToken(argumentResult, itemType, token);

                switch (result.Result)
                {
                    case ArgumentConversionResultType.Successful:
                        if (isArray)
                        {
                            values[i] = result.Value;
                        }
                        else
                        {
                            values.Add(result.Value);
                        }

                        break;

                    default: // failures
                        if (argumentResult.Parent is CommandResult)
                        {
                            argumentResult.OnlyTake(i);

                            i = tokens.Count;
                            break;
                        }

                        return result;
                }
            }

            return Success(argumentResult, values);
        }

        internal static TryConvertArgument? GetConverter(CliArgument argument)
        {
            if (argument.Arity is { MaximumNumberOfValues: 1, MinimumNumberOfValues: 1 })
            {
                if (argument.ValueType.TryGetNullableType(out var nullableType) &&
                    StringConverters.TryGetValue(nullableType, out var convertNullable))
                {
                    return (ArgumentResult result, out object? value) => ConvertSingleString(result, convertNullable, out value);
                }

                if (StringConverters.TryGetValue(argument.ValueType, out var convert1))
                {
                    return (ArgumentResult result, out object? value) => ConvertSingleString(result, convert1, out value);
                }

                static bool ConvertSingleString(ArgumentResult result, TryConvertString convert, out object? value) =>
                    convert(result.Tokens[result.Tokens.Count - 1].Value, out value);
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

        internal static ArgumentConversionResult ConvertIfNeeded(
            this ArgumentConversionResult conversionResult,
            Type toType)
        {
            return conversionResult.Result switch
            {
                ArgumentConversionResultType.Successful when !toType.IsInstanceOfType(conversionResult.Value) =>
                    ConvertObject(conversionResult.ArgumentResult,
                                  toType,
                                  conversionResult.Value),

                ArgumentConversionResultType.NoArgument when conversionResult.ArgumentResult.Argument.IsBoolean() =>
                    Success(conversionResult.ArgumentResult, true),
                        
                _ => conversionResult
            };
        }

        internal static T GetValueOrDefault<T>(this ArgumentConversionResult result)
        {
            return result.Result switch
            {
                ArgumentConversionResultType.Successful => (T)result.Value!,
                ArgumentConversionResultType.NoArgument => default!,
                _ => throw new InvalidOperationException(result.ErrorMessage),
            };
        }

        public static bool TryConvertArgument(ArgumentResult argumentResult, out object? value)
        {
            var argument = argumentResult.Argument;

            ArgumentConversionResult result = argument.Arity.MaximumNumberOfValues switch
            {
                // 0 is an implicit bool, i.e. a "flag"
                0 => Success(argumentResult, true),
                1 => ConvertObject(argumentResult,
                                   argument.ValueType,
                                   argumentResult.Tokens.Count > 0
                                       ? argumentResult.Tokens[argumentResult.Tokens.Count - 1]
                                       : null),
                _ => ConvertTokens(argumentResult,
                                    argument.ValueType,
                                    argumentResult.Tokens)
            };

            value = result;
            return result.Result == ArgumentConversionResultType.Successful;
        }
    }
}