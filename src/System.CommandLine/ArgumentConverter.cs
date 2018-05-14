using System;
using System.Collections.Generic;
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

                [typeof(int)] = arg => int.TryParse(arg, out var i)
                                           ? (ArgumentParseResult)Success(i)
                                           : Failure(typeof(int), arg)
            };
        }

        public static ArgumentParseResult Parse(Type type, string value)
        {
            if (stringConverters.TryGetValue(type, out var convert))
            {
                return convert(value);
            }
            else
            {
                return Failure(type, value);
            }
        }

        public static ArgumentParseResult Parse<T>(string value)
        {
            if (stringConverters.TryGetValue(typeof(T), out var convert))
            {
                return convert(value);
            }
            else
            {
                return Failure(typeof(T), value);
            }
        }

        public static ArgumentParseResult ParseMany<T>(IReadOnlyCollection<string> arguments)
        {
            var itemType = typeof(T)
                           .GetInterfaces()
                           .SingleOrDefault(i =>
                                                i.IsGenericType &&
                                                i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                           ?.GenericTypeArguments
                           ?.Single();

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
                    value = Enumerable.ToList(list);
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
