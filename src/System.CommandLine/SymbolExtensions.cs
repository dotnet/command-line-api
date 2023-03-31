// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for symbols.
    /// </summary>
    internal static class SymbolExtensions
    {
        internal static IList<CliArgument> Arguments(this CliSymbol symbol)
        {
            switch (symbol)
            {
                case CliOption option:
                    return new[]
                    {
                        option.Argument
                    };
                case CliCommand command:
                    return command.Arguments;
                case CliArgument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }
    }
}