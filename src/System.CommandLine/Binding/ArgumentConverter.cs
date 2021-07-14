// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.ComponentModel;    
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using static System.CommandLine.Binding.ArgumentConversionResult;

namespace System.CommandLine.Binding
{
    internal static class ArgumentConverter
    {
        private static readonly Dictionary<Type, Func<string, object>> _converters = new()
        {
            [typeof(FileSystemInfo)] = value =>
            {
                if (Directory.Exists(value))
                {
                    return new DirectoryInfo(value);
                }

                if (value.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                    value.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    return new DirectoryInfo(value);
                }

                return new FileInfo(value);
            }
        };

        internal static ArgumentConversionResult ConvertObject(
            IArgument argument,
            Type type,
            object? value,
            Resources resources)
        {
            if (argument.Arity.MaximumNumberOfValues == 0)
            {
                return Success(argument, true);
            }

            switch (value)
            {
                case string singleValue:
                    if (type.IsEnumerable() && !type.HasStringTypeConverter())
                    {
                        return ConvertStrings(argument, type, new[] { singleValue },resources);
                    }
                    else
                    {
                        return ConvertString(argument, type, singleValue, resources);
                    }

                case IReadOnlyList<string> manyValues:
                    return ConvertStrings(argument, type, manyValues, resources);
            }

            return None(argument);
        }

        private static ArgumentConversionResult ConvertString(
            IArgument argument,
            Type? type,
            string value,
            Resources resources)
        {
            type ??= typeof(string);

            if (TypeDescriptor.GetConverter(type) is { } typeConverter)
            {
                if (typeConverter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        return Success(
                            argument,
                            typeConverter.ConvertFromInvariantString(value));
                    }
                    catch (Exception)
                    {
                        return Failure(argument, type, value, resources);
                    }
                }
            }

            if (_converters.TryGetValue(type, out var convert))
            {
                return Success(
                    argument,
                    convert(value));
            }

            if (type.TryFindConstructorWithSingleParameterOfType(
                typeof(string), out ConstructorInfo? ctor))
            {
                var instance = ctor.Invoke(new object[]
                {
                    value
                });

                return Success(argument, instance);
            }

            return Failure(argument, type, value, resources);
        }

        public static ArgumentConversionResult ConvertStrings(
            IArgument argument,
            Type type,
            IReadOnlyList<string> tokens,
            Resources resources,
            ArgumentResult? argumentResult = null)
        {
            Type itemType;

            if (type == typeof(string))
            {
                itemType = typeof(string);
            }
            else if (type == typeof(bool))
            {
                itemType = typeof(bool);
            }
            else
            {
                itemType = Binder.GetItemTypeIfEnumerable(type) ?? typeof(string);
            }

            var (values, isArray) = type.IsArray
                                        ? (CreateArray(itemType, tokens.Count), true)
                                        : (CreateList(itemType, tokens.Count), false);

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertString(argument, itemType, token, resources);

                switch (result)
                {
                    case FailedArgumentTypeConversionResult _:
                    case FailedArgumentConversionResult _:
                        if (argumentResult is { Parent: CommandResult } )
                        { 
                            argumentResult.OnlyTake(i);

                            // exit the for loop
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

            static IList CreateList(Type itemType, int capacity)
            {
                if (itemType == typeof(string))
                {
                    return new List<string>(capacity);
                }
                else
                {
                    return (IList) Activator.CreateInstance(
                        typeof(List<>).MakeGenericType(itemType),
                        capacity);
                }
            }

            static IList CreateArray(Type itemType, int capacity)
            {
                if (itemType == typeof(string))
                {
                    return new string[capacity];
                }
                else
                {
                    return Array.CreateInstance(itemType, capacity);
                }
            }
        }

        internal static bool HasStringTypeConverter(this Type type) =>
            TypeDescriptor.GetConverter(type) is { } typeConverter && 
            typeConverter.CanConvertFrom(typeof(string));

        private static FailedArgumentConversionResult Failure(
            IArgument argument,
            Type expectedType,
            string value,
            Resources resources)
        {
            return new FailedArgumentTypeConversionResult(argument, expectedType, value, resources);
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
                                  symbolResult.Resources),
                SuccessfulArgumentConversionResult successful when toType == typeof(object) &&
                                                                   conversionResult.Argument.Arity.MaximumNumberOfValues > 1 &&
                                                                   successful.Value is string =>
                    ConvertObject(conversionResult.Argument,
                                  typeof(IEnumerable<string>),
                                  successful.Value,
                                  symbolResult.Resources),
                NoArgumentConversionResult _ when toType == typeof(bool) =>
                    Success(conversionResult.Argument,
                            true),
                NoArgumentConversionResult _ when conversionResult.Argument.Arity.MinimumNumberOfValues > 0 =>
                    new MissingArgumentConversionResult(conversionResult.Argument,
                                                        symbolResult.Resources.RequiredArgumentMissing(symbolResult)),
                NoArgumentConversionResult _ when conversionResult.Argument.Arity.MaximumNumberOfValues > 1 =>
                    Success(conversionResult.Argument,
                            Array.Empty<string>()),
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
                1 => ConvertObject(argument, argument.ValueType, argumentResult.Tokens[argumentResult.Tokens.Count - 1].Value, argumentResult.Resources),
                _ => ConvertStrings(argument, argument.ValueType, argumentResult.Tokens.Select(t => t.Value).ToArray(), argumentResult.Resources, argumentResult)
            };

            return value is SuccessfulArgumentConversionResult;
        }
    }
}
