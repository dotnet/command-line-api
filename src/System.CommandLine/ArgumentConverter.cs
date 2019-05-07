﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using static System.CommandLine.ArgumentResult;

namespace System.CommandLine
{
    internal static class ArgumentConverter
    {
        private static readonly ConcurrentDictionary<Type, ConvertString> _stringConverters = new ConcurrentDictionary<Type, ConvertString>();

        internal static ArgumentResult Parse(Type type, object value)
        {
            switch (value)
            {
                // try to parse the single string argument to the requested type
                case string argument:
                    return Parse(type, argument);

                // try to parse the multiple string arguments to the request type
                case IReadOnlyCollection<string> arguments:
                    return ParseMany(type, arguments);

                case null:
                    if (type == typeof(bool))
                    {
                        // the presence of the parsed symbol is treated as true
                        return new SuccessfulArgumentResult(true);
                    }

                    break;
            }

            return null;
        }

        public static ArgumentResult Parse(Type type, string value)
        {
            if (_stringConverters.TryGetValue(type, out var convert))
            {
                return convert(value);
            }

            if (TypeDescriptor.GetConverter(type) is TypeConverter typeConverter)
            {
                if (typeConverter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        return Success(typeConverter.ConvertFromInvariantString(value));
                    }
                    catch (Exception)
                    {
                        return Failure(type, value);
                    }
                }
            }

            if (type.ConstructorWithSingleParameterOfType(typeof(string)) is ConstructorInfo ctor)
            {
                convert = _stringConverters.GetOrAdd(
                    type,
                    _ => arg =>
                    {
                        var instance = ctor.Invoke(new object[] { arg });
                        return Success(instance);
                    });

                return convert(value);
            }

            return Failure(type, value);
        }

        public static ArgumentResult ParseMany(
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
                                  .Select(arg => Parse(itemType, arg))
                                  .ToArray();

            var successfulParseResults = allParseResults
                                         .OfType<SuccessfulArgumentResult>()
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

                return Success(value);
            }
            else
            {
                return allParseResults.OfType<FailedArgumentResult>().First();
            }
        }

        private static Type GetItemTypeIfEnumerable(Type type)
        {
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

            bool IsEnumerable(Type i)
            {
                return i.IsGenericType &&
                       i.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            }
        }

        private static FailedArgumentResult Failure(Type type, string value)
        {
            return new FailedArgumentTypeConversionResult(type, value);
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

            if (ConstructorWithSingleParameterOfType(type, typeof(string)) != null)
            {
                return true;
            }

            if (GetItemTypeIfEnumerable(type) is Type itemType)
            {
                return itemType.CanBeBoundFromScalarValue();
            }

            return false;
        }

        private static ConstructorInfo ConstructorWithSingleParameterOfType(
            this Type type,
            Type parameterType) =>
            type.GetConstructors()
                .Where(c =>
                {
                    var parameters = c.GetParameters();
                    return c.IsPublic &&
                           parameters.Length == 1 &&
                           parameters[0].ParameterType == parameterType;
                })
                .SingleOrDefault();
    }
}
