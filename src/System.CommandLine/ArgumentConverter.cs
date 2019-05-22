// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using static System.CommandLine.ArgumentResult;

namespace System.CommandLine
{
    internal static class ArgumentConverter
    {
        internal static ArgumentResult Parse(
            IArgument argument,
            Type type, 
            object value)
        {
            switch (value)
            {
                // try to parse the single string argument to the requested type
                case string stringArg:
                    return Parse(argument, type, stringArg);

                // try to parse the multiple string arguments to the request type
                case IReadOnlyCollection<string> arguments:
                    return ParseMany(argument, type, arguments);

                case null:
                    if (type == typeof(bool))
                    {
                        // the presence of the parsed symbol is treated as true
                        return new SuccessfulArgumentResult(argument, true);
                    }

                    break;
            }

            return None(argument);
        }



        public static ArgumentResult Parse(
            IArgument argument,
            Type type,
            string value)
        {
            if (TypeDescriptor.GetConverter(type) is TypeConverter typeConverter)
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

            if (type.TryFindConstructorWithSingleParameterOfType(
                typeof(string), out (ConstructorInfo ctor, ParameterDescriptor parameterDescriptor) tuple))
            {
                if (value == null &&
                    !tuple.parameterDescriptor.AllowsNull)
                {
                    return Success(argument, type.GetDefaultValueForType());
                }

                var instance = tuple.ctor.Invoke(new object[]
                {
                    value
                });

                return Success(argument, instance);
            }

            return Failure(argument, type, value);
        }

        public static ArgumentResult ParseMany(
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
                                  .Select(arg => Parse(argument, itemType, arg))
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

                return Success(argument, value);
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

        private static FailedArgumentResult Failure(
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
    }
}
