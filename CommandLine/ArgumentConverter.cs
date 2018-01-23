using System;

namespace Microsoft.DotNet.Cli.CommandLine
{
    internal static class ArgumentConverter
    {
        public static bool TryParseAs<T>(string arg, out T o)
        {
            if (typeof(T) == typeof(int) &&
                int.TryParse(arg, out var i))
            {
                o = (dynamic) i;
                return true;
            }

            o = default(T);

            return false;
        }
    }
}
