using System;
using System.Collections.Generic;
using static Microsoft.DotNet.Cli.CommandLine.ArgumentParseResult;

namespace Microsoft.DotNet.Cli.CommandLine
{
    internal static class ArgumentConverter
    {
        private static readonly Dictionary<Type, ConvertString> converters;

        static ArgumentConverter()
        {
            converters = new Dictionary<Type, ConvertString>
            {
                [typeof(string)] = Success,

                [typeof(int)] = s => int.TryParse(s, out var i)
                                         ? (ArgumentParseResult) Success(i)
                                         : Failure(s)
            };
        }

        public static ArgumentParseResult Parse<T>(string arg)
        {
            if (converters.TryGetValue(typeof(T), out var convert))
            {
                return convert(arg);
            }
            else
            {
                return Failure($"Cannot parse argument '{arg}' as {typeof(T)}");
            }
        }
    }

    internal delegate ArgumentParseResult ConvertString(string argument);
}
