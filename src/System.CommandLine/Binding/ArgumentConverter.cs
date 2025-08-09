// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Globalization;
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
                case Token singleValue:
                    return ConvertToken(argumentResult, type, singleValue);

                case IReadOnlyList<Token> manyValues:
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
            Token token)
        {
            var value = token.Value;

            if (type.TryGetNullableType(out var nullableType))
            {
                return ConvertToken(argumentResult, nullableType, token);
            }

#if NET7_0_OR_GREATER
            // Prefer span-based parsing when available (ISpanParsable<TSelf>), then IParsable<TSelf>
            if (TryParseWithISpanParsableGeneric(type, value, CultureInfo.CurrentCulture, out var parsedViaISpanParsable))
            {
                return Success(argumentResult, parsedViaISpanParsable);
            }

            if (TryParseWithIParsableGeneric(type, value, CultureInfo.CurrentCulture, out var parsedViaIParsable))
            {
                return Success(argumentResult, parsedViaIParsable);
            }
#endif

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
            IReadOnlyList<Token> tokens)
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

        internal static TryConvertArgument? GetConverter(Argument argument)
        {
            if (argument.Arity is { MaximumNumberOfValues: 1, MinimumNumberOfValues: 1 })
            {
#if NET7_0_OR_GREATER
                // Single-value optimized converter that prefers IParsable<TSelf>, with fallback to existing StringConverters
                return (ArgumentResult result, out object? value) =>
                {
                    var text = result.Tokens[result.Tokens.Count - 1].Value;

                    // Try nullable underlying via IParsable
                    if (argument.ValueType.TryGetNullableType(out var underlying))
                    {
                        if (TryParseWithISpanParsableGeneric(underlying, text, CultureInfo.CurrentCulture, out value))
                        {
                            return true;
                        }

                        if (TryParseWithIParsableGeneric(underlying, text, CultureInfo.CurrentCulture, out value))
                        {
                            return true;
                        }

                        // Try enum names/numeric values for nullable enum
                        if (underlying.IsEnum)
                        {
                            if (Enum.TryParse(underlying, text, ignoreCase: true, out value))
                            {
                                return true;
                            }
                        }

                        if (StringConverters.TryGetValue(underlying, out var convU) && convU(text, out value))
                        {
                            return true;
                        }
                    }

                    // Try declared type via ISpanParsable, then IParsable
                    if (TryParseWithISpanParsableGeneric(argument.ValueType, text, CultureInfo.CurrentCulture, out value))
                    {
                        return true;
                    }

                    if (TryParseWithIParsableGeneric(argument.ValueType, text, CultureInfo.CurrentCulture, out value))
                    {
                        return true;
                    }

                    // Try enum names/numeric values for declared enum type
                    if (argument.ValueType.IsEnum)
                    {
                        if (Enum.TryParse(argument.ValueType, text, ignoreCase: true, out value))
                        {
                            return true;
                        }
                    }

                    // Fallback to existing converters
                    if (StringConverters.TryGetValue(argument.ValueType, out var conv) && conv(text, out value))
                    {
                        return true;
                    }

                    value = null;
                    return false;
                };
#else
                return (ArgumentResult result, out object? value) =>
                {
                    var text = result.Tokens[result.Tokens.Count - 1].Value;

                    // Nullable underlying enum or converter
                    if (argument.ValueType.TryGetNullableType(out var underlying))
                    {
                        if (underlying.IsEnum)
                        {
                            try
                            {
                                value = Enum.Parse(underlying, text, ignoreCase: true);
                                return true;
                            }
                            catch (ArgumentException)
                            {
                            }
                        }

                        if (StringConverters.TryGetValue(underlying, out var convU) && convU(text, out value))
                        {
                            return true;
                        }
                    }

                    // Declared enum type
                    if (argument.ValueType.IsEnum)
                    {
                        try
                        {
                            value = Enum.Parse(argument.ValueType, text, ignoreCase: true);
                            return true;
                        }
                        catch (ArgumentException)
                        {
                        }
                    }

                    // Fallback converters
                    if (StringConverters.TryGetValue(argument.ValueType, out var conv) && conv(text, out value))
                    {
                        return true;
                    }

                    value = null;
                    return false;
                };
#endif
            }

            if (argument.ValueType.CanBeBoundFromScalarValue())
            {
                return TryConvertArgument;
            }

            return default;
        }

#if NET7_0_OR_GREATER
        // Uses a generic method to call TSelf.TryParse via static abstract interface member
        private static bool TryParseWithISpanParsableGeneric(Type targetType, string text, IFormatProvider? provider, out object? parsed)
        {
            var method = _tryParseWithISpanParsableGenericMethod ??= typeof(ArgumentConverter)
                .GetMethod(nameof(TryParseWithISpanParsable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            try
            {
                var closed = method.MakeGenericMethod(targetType);
                var parameters = new object?[] { text, provider, null };
                var success = (bool)closed.Invoke(obj: null, parameters)!;
                parsed = parameters[2];
                return success;
            }
            catch (System.Reflection.TargetInvocationException tie) when (tie.InnerException is System.TypeLoadException or System.MissingMethodException)
            {
                parsed = null;
                return false;
            }
            catch (System.ArgumentException)
            {
                // Thrown when generic constraints are not satisfied
                parsed = null;
                return false;
            }
        }

        private static System.Reflection.MethodInfo? _tryParseWithISpanParsableGenericMethod;

        private static bool TryParseWithISpanParsable<T>(string text, IFormatProvider? provider, out object? parsed)
            where T : ISpanParsable<T>
        {
            if (T.TryParse(text.AsSpan(), provider, out var value))
            {
                parsed = value;
                return true;
            }

            parsed = default;
            return false;
        }

        // Uses a generic method to call TSelf.TryParse via static abstract interface member
        private static bool TryParseWithIParsableGeneric(Type targetType, string text, IFormatProvider? provider, out object? parsed)
        {
            var method = _tryParseWithIParsableGenericMethod ??= typeof(ArgumentConverter)
                .GetMethod(nameof(TryParseWithIParsable), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

            try
            {
                var closed = method.MakeGenericMethod(targetType);
                var parameters = new object?[] { text, provider, null };
                var success = (bool)closed.Invoke(obj: null, parameters)!;
                parsed = parameters[2];
                return success;
            }
            catch (System.Reflection.TargetInvocationException tie) when (tie.InnerException is System.TypeLoadException or System.MissingMethodException)
            {
                parsed = null;
                return false;
            }
            catch (System.ArgumentException)
            {
                // Thrown when generic constraints are not satisfied
                parsed = null;
                return false;
            }
        }

        private static System.Reflection.MethodInfo? _tryParseWithIParsableGenericMethod;

        private static bool TryParseWithIParsable<T>(string text, IFormatProvider? provider, out object? parsed)
            where T : IParsable<T>
        {
            if (T.TryParse(text, provider, out var value))
            {
                parsed = value;
                return true;
            }

            parsed = default;
            return false;
        }
#endif

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

                if (type.TryGetNullableType(out Type? nullableType))
                {
                    type = nullableType;
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