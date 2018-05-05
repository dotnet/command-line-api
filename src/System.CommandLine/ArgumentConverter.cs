using System;
using System.Collections.Generic;
using static System.CommandLine.ArgumentParseResult;

namespace System.CommandLine
{
    internal static class ArgumentConverter
    {
        private static readonly Dictionary<Type, ConvertString> stringConverters;

        static ArgumentConverter()
        {
            stringConverters = new Dictionary<Type, ConvertString>
            {
                [typeof(bool)] = arg =>
                {
                    if (string.IsNullOrWhiteSpace(arg))
                    {
                        return Success(true);
                    }

                    if (bool.TryParse(arg, out var value))
                    {
                        return Success(value);
                    }

                    return Failure<bool>(arg);
                },

                [typeof(string)] = Success,

                [typeof(object)] = Success,

                [typeof(int)] = arg => int.TryParse(arg, out var i)
                                           ? (ArgumentParseResult) Success(i)
                                           : Failure<int>(arg)
            };
        }

        public static ArgumentParseResult Parse<T>(string value)
        {
            if (stringConverters.TryGetValue(typeof(T), out var convert))
            {
                return convert(value);
            }
            else
            {
                return Failure<T>(value);
            }
        }

        private static FailedArgumentParseResult Failure<T>(string value)
        {
            return new FailedArgumentTypeConversionResult<T>(value);
        }
    }
}
