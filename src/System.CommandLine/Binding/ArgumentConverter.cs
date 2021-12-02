﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using static System.CommandLine.Binding.ArgumentConversionResult;

namespace System.CommandLine.Binding
{
    internal static class ArgumentConverter
    {
        private static Lazy<MethodInfo> EnumerableEmptyMethod { get; } = new
             (() => typeof(Enumerable).GetMethod(nameof(Array.Empty)), LazyThreadSafetyMode.None);

        private static readonly Dictionary<Type, TryConvertString> _stringConverters = new()
        {
            [typeof(DirectoryInfo)] = (string path, out object? value) =>
            {
                value = new DirectoryInfo(path);
                return true;
            },

            [typeof(int)] = (string token, out object? value) =>
            {
                if (int.TryParse(token, out var intValue))
                {
                    value = intValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(int?)] = (string token, out object? value) =>
            {
                if (int.TryParse(token, out var intValue))
                {
                    value = intValue;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(bool)] = (string token, out object? value) =>
            {
                if (bool.TryParse(token, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },
            
            [typeof(bool?)] = (string token, out object? value) =>
            {
                if (bool.TryParse(token, out var parsed))
                {
                    value = parsed;
                    return true;
                }

                value = default;
                return false;
            },

            [typeof(FileSystemInfo)] = (string path, out object? value) =>
            {
                if (Directory.Exists(path))
                {
                    value = new DirectoryInfo(path);
                }
                else if (path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) ||
                         path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    value = new DirectoryInfo(path);
                }
                else
                {
                    value = new FileInfo(path);
                }

                return true;
            },

            [typeof(FileInfo)] = (string path, out object? value) =>
            {
                value = new FileInfo(path);
                return true;
            },

            [typeof(string)] = (string input, out object? value) =>
            {
                value = input;
                return true;
            },
        };

        private delegate bool TryConvertString(string token, out object? value);

        internal static ArgumentConversionResult ConvertObject(
            IArgument argument,
            Type type,
            object? value,
            LocalizationResources localizationResources)
        {
            switch (value)
            {
                case string singleValue:
                    if (type.IsEnumerable() && !type.HasStringTypeConverter())
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
            IArgument argument,
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
                        return Failure(argument, type, value, localizationResources);
                    }
                }
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

            return Failure(argument, type, value, localizationResources);
        }

        public static ArgumentConversionResult ConvertStrings(
            IArgument argument,
            Type type,
            IReadOnlyList<string> tokens,
            LocalizationResources localizationResources,
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
                itemType = type.GetElementTypeIfEnumerable() ?? typeof(string);
            }

            var (values, isArray) = type.IsArray
                                        ? (CreateArray(itemType, tokens.Count), true)
                                        : (CreateList(itemType, tokens.Count), false);

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
                    return (IList)Activator.CreateInstance(
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

        private static bool TryFindConstructorWithSingleParameterOfType(
            this Type type,
            Type parameterType,
            [NotNullWhen(true)] out ConstructorInfo? ctor)
        {
            var (x, _) = type.GetConstructors()
                             .Select(c => (ctor: c, parameters: c.GetParameters()))
                             .SingleOrDefault(tuple => tuple.ctor.IsPublic &&
                                                       tuple.parameters.Length == 1 &&
                                                       tuple.parameters[0].ParameterType == parameterType);

            if (x is not null)
            {
                ctor = x;
                return true;
            }
            else
            {
                ctor = null;
                return false;
            }
        }

        private static bool HasStringTypeConverter(this Type type) =>
            TypeDescriptor.GetConverter(type) is { } typeConverter &&
            typeConverter.CanConvertFrom(typeof(string));

        private static FailedArgumentConversionResult Failure(
            IArgument argument,
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
                NoArgumentConversionResult _ when toType == typeof(bool) =>
                    Success(conversionResult.Argument,
                            true),
                NoArgumentConversionResult _ when conversionResult.Argument.Arity.MinimumNumberOfValues > 0 =>
                    new MissingArgumentConversionResult(conversionResult.Argument,
                                                        symbolResult.LocalizationResources.RequiredArgumentMissing(symbolResult)),
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
            if (type.GetElementTypeIfEnumerable() is { } itemType)
            {
                if (type.IsArray)
                {
                    return CreateEmptyArray(itemType);
                }

                if (type.IsGenericType)
                {
                    return type.GetGenericTypeDefinition() switch
                    {
                        { } enumerable when enumerable == typeof(IEnumerable<>) => CreateEmptyEnumerable(itemType),
                        { } list when list == typeof(List<>) => CreateEmptyList(itemType),
                        { } array when array == typeof(IList<>) || 
                                       array == typeof(ICollection<>) => CreateEmptyArray(itemType),
                        _ => null
                    };
                }
            }

            return type switch
            {
                { } nonGeneric 
                    when nonGeneric == typeof(IList) ||
                         nonGeneric == typeof(ICollection) ||
                         nonGeneric == typeof(IEnumerable)
                    => CreateEmptyArray(typeof(object)),
                _ when type.IsValueType => Activator.CreateInstance(type),
                _ => null
            };
            
            static object CreateEmptyList(Type itemType)
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            }

            static IEnumerable CreateEmptyEnumerable(Type itemType)
            {
                var genericMethod = EnumerableEmptyMethod.Value.MakeGenericMethod(itemType);
                return (IEnumerable)genericMethod.Invoke(null, new object[0]);
            }

            static Array CreateEmptyArray(Type itemType)
                => Array.CreateInstance(itemType, 0);
        }
    }
}