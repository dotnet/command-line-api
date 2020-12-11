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

                if (value.EndsWith(Path.DirectorySeparatorChar.ToString()) ||
                    value.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
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

                case IReadOnlyCollection<string> manyValues:
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
            IReadOnlyCollection<string> arguments)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            var itemType = type == typeof(string)
                               ? typeof(string)
                               : GetItemTypeIfEnumerable(type);

            var successfulParseResults = arguments
                                         .Select(arg => ConvertString(argument, itemType, arg))
                                         .OfType<SuccessfulArgumentConversionResult>();

            var list = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

            foreach (var parseResult in successfulParseResults)
            {
                list.Add(parseResult.Value);
            }

            var value = type.IsArray
                            ? (object) Enumerable.ToArray((dynamic) list)
                            : list;

            return Success(argument, value);
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

            return enumerableInterface?.GenericTypeArguments[0];
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
                (type.IsGenericType &&
                 type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private static bool HasStringTypeConverter(this Type type)
        {
            return TypeDescriptor.GetConverter(type) is { } typeConverter
                && typeConverter.CanConvertFrom(typeof(string));
        }

        private static FailedArgumentConversionResult Failure(
            IArgument argument,
            Type type,
            string value)
        {
            return new FailedArgumentTypeConversionResult(argument, type, value);
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
                        ValidationMessages.Instance.RequiredArgumentMissing(symbolResult));

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
            switch (result)
            {
                case SuccessfulArgumentConversionResult successful:
                    return (T)successful.Value!;
                case FailedArgumentConversionResult failed:
                    throw new InvalidOperationException(failed.ErrorMessage);
                case NoArgumentConversionResult _:
                    return default!;
                default:
                    return default!;
            }
        }
    }
}
