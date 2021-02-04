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
        private static readonly Dictionary<Type, Func<string, object>> _converters = new Dictionary<Type, Func<string, object>>
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
            object? value)
        {
            switch (value)
            {
                case string singleValue:
                    if (type.IsEnumerable() && !type.HasStringTypeConverter())
                    {
                        return ConvertStrings(argument, type, new[] { singleValue });
                    }
                    else
                    {
                        return ConvertString(argument, type, singleValue);
                    }

                case IReadOnlyList<string> manyValues:
                    return ConvertStrings(argument, type, manyValues);
            }

            return None(argument);
        }

        private static ArgumentConversionResult ConvertString(
            IArgument argument,
            Type? type,
            string value)
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
                        return Failure(argument, type, value);
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

            return Failure(argument, type, value);
        }

        public static ArgumentConversionResult ConvertStrings(
            IArgument argument,
            Type type,
            IReadOnlyList<string> tokens,
            ArgumentResult? argumentResult = null)
        {
            var itemType = type == typeof(string)
                               ? typeof(string)
                               : GetItemTypeIfEnumerable(type);

            var (values, isArray) = type.IsArray
                             ? (CreateArray(itemType!, tokens.Count), true)
                             : (CreateList(itemType!, tokens.Count), false);

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];

                var result = ConvertString(argument, itemType, token);

                switch (result)
                {
                    case FailedArgumentTypeConversionResult _:
                    case FailedArgumentConversionResult _:
                        if (argumentResult is { })
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

        private static Type? GetItemTypeIfEnumerable(Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            var enumerableInterface =
                IsEnumerable(type)
                    ? type
                    : type
                      .GetInterfaces()
                      .FirstOrDefault(IsEnumerable);

            return enumerableInterface?.GenericTypeArguments switch
            {
                { Length: 1 } genericTypeArguments => genericTypeArguments[0],
                _ => null
            };
        }

        internal static bool IsEnumerable(this Type type)
        {
            if (type == typeof(string))
            {
                return false;
            }

            return 
                type.IsArray 
                ||
                typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool HasStringTypeConverter(this Type type)
        {
            return TypeDescriptor.GetConverter(type) is { } typeConverter
                && typeConverter.CanConvertFrom(typeof(string));
        }

        private static FailedArgumentConversionResult Failure(
            IArgument argument,
            Type expectedType,
            string value)
        {
            return new FailedArgumentTypeConversionResult(argument, expectedType, value);
        }

        public static bool CanBeBoundFromScalarValue(this Type type)
        {
            if (type.IsPrimitive ||
                type.IsEnum)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            if (TypeDescriptor.GetConverter(type) is { } typeConverter &&
                typeConverter.CanConvertFrom(typeof(string)))
            {
                return true;
            }

            if (TryFindConstructorWithSingleParameterOfType(type, typeof(string), out _))
            {
                return true;
            }

            if (GetItemTypeIfEnumerable(type) is { } itemType)
            {
                return itemType.CanBeBoundFromScalarValue();
            }

            return false;
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

            if (x != null)
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

        internal static ArgumentConversionResult ConvertIfNeeded(
            this ArgumentConversionResult conversionResult,
            SymbolResult symbolResult,
            Type toType)
        {
            if (conversionResult is null)
            {
                throw new ArgumentNullException(nameof(conversionResult));
            }

            switch (conversionResult)
            {
                case SuccessfulArgumentConversionResult successful when !toType.IsInstanceOfType(successful.Value):
                    return ConvertObject(
                        conversionResult.Argument,
                        toType,
                        successful.Value);

                case SuccessfulArgumentConversionResult successful
                    when toType == typeof(object) && conversionResult.Argument.Arity.MaximumNumberOfValues > 1 &&
                         successful.Value is string:
                    return ConvertObject(
                        conversionResult.Argument,
                        typeof(IEnumerable<string>),
                        successful.Value);

                case NoArgumentConversionResult _ when toType == typeof(bool):
                    return Success(conversionResult.Argument, true);

                case NoArgumentConversionResult _ when conversionResult.Argument.Arity.MinimumNumberOfValues > 0:
                    return new MissingArgumentConversionResult(
                        conversionResult.Argument,
                        Resources.Instance.RequiredArgumentMissing(symbolResult));

                case NoArgumentConversionResult _ when conversionResult.Argument.Arity.MaximumNumberOfValues > 1:
                    return Success(
                        conversionResult.Argument,
                        Array.Empty<string>());

                default:
                    return conversionResult;
            }
        }

        internal static object? GetValueOrDefault(this ArgumentConversionResult result) =>
            result.GetValueOrDefault<object?>();

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

        [return: MaybeNull]
        internal static T GetDefaultValue<T>()
        {
            return (T)GetDefaultValue(typeof(T));
        }

        private static MethodInfo EnumerableEmptyMethod { get; }
            = typeof(Enumerable).GetMethod(nameof(Enumerable.Empty));

        internal static object? GetDefaultValue(Type type)
        {
            if (type == typeof(string)) return "";
            if (type == typeof(bool)) return false;
            if (GetItemTypeIfEnumerable(type) is Type itemType)
            {
                if (type.IsArray)
                {
                    return CreateEmptyArray(itemType);
                }
                if (type.IsGenericType)
                {
                    return type.GetGenericTypeDefinition() switch
                    {
                        Type enumerable when enumerable == typeof(IEnumerable<>) => GetEmptyEnumerable(itemType),
                        Type list when list == typeof(List<>) => GetEmptyList(itemType),
                        Type array when array == typeof(IList<>) || 
                                        array == typeof(ICollection<>) => CreateEmptyArray(itemType),
                        _ => null
                    };
                }
            }
            return type switch
            {
                Type nonGeneric 
                    when nonGeneric == typeof(IList) ||
                         nonGeneric == typeof(ICollection) ||
                         nonGeneric == typeof(IEnumerable)
                    => CreateEmptyArray(typeof(object)),
                _ => null
            };
            
            static object GetEmptyList(Type itemType)
            {
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            }

            static IEnumerable GetEmptyEnumerable(Type itemType)
            {
                var genericMethod = EnumerableEmptyMethod.MakeGenericMethod(itemType);
                return (IEnumerable)genericMethod.Invoke(null, new object[0]);
            }

            static Array CreateEmptyArray(Type itemType)
                => Array.CreateInstance(itemType, 0);
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

            switch (argument.Arity.MaximumNumberOfValues)
            {
                case 1:
                    value = ConvertObject(
                        argument,
                        argument.ValueType,
                        argumentResult.Tokens[0].Value);
                    break;

                default:
                    value = ConvertStrings(
                        argument,
                        argument.ValueType,
                        argumentResult.Tokens.Select(t => t.Value).ToArray(),
                        argumentResult);
                    break;
            }

            return value is SuccessfulArgumentConversionResult;
        }
    }
}
