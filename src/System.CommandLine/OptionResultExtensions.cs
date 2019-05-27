// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace System.CommandLine
{
    public static class OptionResultExtensions
    {
        internal static ArgumentResult GetValueAs(
            this OptionResult optionResult,
            Type type)
        {
            if (optionResult == null)
            {
                throw new ArgumentNullException(nameof(optionResult));
            }

            if (type == null)
            {
                type = typeof(object);
            }

            if (CommandLineConfiguration.UseNewParser)
            {
                return SymbolResult.Parse(
                    optionResult,
                    optionResult.Option.Argument);
            }
            else
            {
                return optionResult.ArgumentResult
                                   .GetValueAs(type);
            }
        }

        public static object GetValueOrDefault(this OptionResult optionResult)
        {
            return optionResult.GetValueOrDefault<object>();
        }

        public static T GetValueOrDefault<T>(this OptionResult optionResult)
        {
            return optionResult.GetValueAs(typeof(T)).GetValueOrDefault<T>();
        }
    }
}
