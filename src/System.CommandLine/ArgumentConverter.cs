// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static System.CommandLine.ArgumentParseResult;

namespace System.CommandLine
{
    internal static class ArgumentConverter
    {
        private static readonly Dictionary<Type, ConvertString> stringConverters;

        static ArgumentConverter()
        {
            stringConverters = new Dictionary<Type, ConvertString> {
                [typeof(bool)] = arg => {
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        return Success(true);
                    }

                    if (bool.TryParse(arg, out var value))
                    {
                        return Success(value);
                    }

                    return Failure(typeof(bool), arg);
                },

                [typeof(string)] = Success,

                [typeof(object)] = Success,
            };
        }

        public static ArgumentParseResult Parse(Type type, string value)
        {
            if (stringConverters.TryGetValue(type, out var convert))
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

            var singleStringConstructor = type.GetConstructors()
                                              .Where(c => {
                                                  var parameters = c.GetParameters();
                                                  return c.IsPublic &&
                                                         parameters.Length == 1 &&
                                                         parameters[0].ParameterType == typeof(string);
                                              })
                                              .SingleOrDefault();

            if (singleStringConstructor != null)
            {
                convert = argument => {
                    var instance = singleStringConstructor.Invoke(new object[] { argument });
                    return Success(instance);
                };

                stringConverters.Add(type, convert);

                return convert(value);
            }

            return Failure(type, value);
        }

        public static ArgumentParseResult Parse<T>(string value)
        {
            return Parse(typeof(T), value);
        }

        public static ArgumentParseResult ParseMany<T>(IReadOnlyCollection<string> arguments)
        {
            var itemType = typeof(T)
                           .GetInterfaces()
                           .SingleOrDefault(i =>
                                                i.IsGenericType &&
                                                i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                           .GenericTypeArguments
                           .Single();

            var allParseResults = arguments
                                  .Select(arg => Parse(itemType, arg))
                                  .ToArray();

            var successfulParseResults = allParseResults
                                         .Where(parseResult => parseResult.IsSuccessful)
                                         .ToArray();

            if (successfulParseResults.Length == arguments.Count)
            {
                dynamic list = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

                foreach (var parseResult in successfulParseResults)
                {
                    if (parseResult.IsSuccessful)
                    {
                        list.Add(((dynamic)parseResult).Value);
                    }
                }

                T value;

                if (typeof(T).IsArray)
                {
                    value = Enumerable.ToArray(list);
                }
                else
                {
                    value = list;
                }

                return Success(value);
            }
            else
            {
                return allParseResults.OfType<FailedArgumentParseResult>().First();
            }
        }

        private static FailedArgumentParseResult Failure(Type type, string value)
        {
            return new FailedArgumentTypeConversionResult(type, value);
        }
    }
}
