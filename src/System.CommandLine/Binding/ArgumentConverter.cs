// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.ComponentModel;
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
            object value)
        {
            if (value == null &&
                type == typeof(bool))
            {
                // the presence of the parsed symbol is treated as true
                return new SuccessfulArgumentConversionResult(argument, true);
            }

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

                case null:
                    break;
            }

            return None(argument);
        }

        private static ArgumentConversionResult ConvertString(
            IArgument argument,
            Type type,
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
                try
                {
                    return Success(
                        argument,
                        convert(value));
                }
                catch (Exception)
                {
                    return Failure(argument, type, value);
                }
            }

            if (type.TryFindConstructorWithSingleParameterOfType(
                typeof(string), out (ConstructorInfo ctor, ParameterDescriptor parameterDescriptor) tuple))
            {
                var instance = tuple.ctor.Invoke(new object[]
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
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            Type itemType;

            if (type == typeof(string))
            {
                // don't treat items as char
                itemType = typeof(string);
            }
            else
            {
                itemType = GetItemTypeIfEnumerable(type);
            }

            var allParseResults = arguments
                                  .Select(arg => ConvertString(argument, itemType, arg))
                                  .ToArray();

            var successfulParseResults = allParseResults
                                         .OfType<SuccessfulArgumentConversionResult>()
                                         .ToArray();

            if (successfulParseResults.Length == arguments.Count)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

                foreach (var parseResult in successfulParseResults)
                {
                    list.Add(parseResult.Value);
                }

                var value = type.IsArray
                                ? (object)Enumerable.ToArray((dynamic)list)
                                : list;

                return Success(argument, value);
            }
            else
            {
                return allParseResults.OfType<FailedArgumentConversionResult>().First();
            }
        }

        private static Type GetItemTypeIfEnumerable(Type type)
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

            if (enumerableInterface == null)
            {
                return null;
            }

            return enumerableInterface.GenericTypeArguments[0];
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
            return TypeDescriptor.GetConverter(type) is TypeConverter typeConverter
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

            if (TypeDescriptor.GetConverter(type) is TypeConverter typeConverter &&
                typeConverter.CanConvertFrom(typeof(string)))
            {
                return true;
            }

            if (TryFindConstructorWithSingleParameterOfType(type, typeof(string), out _) )
            {
                return true;
            }

            if (GetItemTypeIfEnumerable(type) is Type itemType)
            {
                return itemType.CanBeBoundFromScalarValue();
            }

            return false;
        }

        private static bool TryFindConstructorWithSingleParameterOfType(
            this Type type,
            Type parameterType,
            out (ConstructorInfo ctor, ParameterDescriptor parameterDescriptor) info)
        {
            var (x, y) = type.GetConstructors()
                             .Select(c => (ctor: c, parameters: c.GetParameters()))
                             .SingleOrDefault(tuple => tuple.ctor.IsPublic &&
                                                       tuple.parameters.Length == 1 &&
                                                       tuple.parameters[0].ParameterType == parameterType);

            if (x != null)
            {
                info = (x, new ParameterDescriptor(y[0], new ConstructorDescriptor(x, ModelDescriptor.FromType(type))));
                return true;
            }
            else
            {
                info = (null, null);
                return false;
            }
        }

        internal static ArgumentConversionResult ConvertIfNeeded(
            this ArgumentConversionResult conversionResult,
            SymbolResult symbolResult,
            Type type)
        {
            if (conversionResult == null)
            {
                throw new ArgumentNullException(nameof(conversionResult));
            }

            switch (conversionResult)
            {
                case SuccessfulArgumentConversionResult successful when !type.IsInstanceOfType(successful.Value):
                    return ConvertObject(
                        conversionResult.Argument,
                        type,
                        successful.Value);

                case NoArgumentConversionResult _ when type == typeof(bool):
                    return Success(conversionResult.Argument, true);

                case NoArgumentConversionResult _ when conversionResult.Argument.Arity.MinimumNumberOfValues > 0:
                    return new MissingArgumentConversionResult(
                        conversionResult.Argument,
                        ValidationMessages.Instance.RequiredArgumentMissing(symbolResult));

                case NoArgumentConversionResult _ when type.IsEnumerable():
                    return ConvertObject(
                        conversionResult.Argument,
                        type,
                        Array.Empty<string>());

                case TooManyArgumentsConversionResult _:
                    return conversionResult;

                case MissingArgumentConversionResult _:
                    return conversionResult;

                default:
                    return conversionResult;
            }
        }

        internal static object GetValueOrDefault(this ArgumentConversionResult result) =>
            result.GetValueOrDefault<object>();

        internal static T GetValueOrDefault<T>(this ArgumentConversionResult result)
        {
            switch (result)
            {
                case SuccessfulArgumentConversionResult successful:
                    return (T)successful.Value;
                case FailedArgumentConversionResult failed:
                    throw new InvalidOperationException(failed.ErrorMessage);
                case NoArgumentConversionResult _:
                    return default;
                default:
                    return default;
            }
        }
    }
}
