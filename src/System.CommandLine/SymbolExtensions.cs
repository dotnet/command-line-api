// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.CommandLine.Parsing;

namespace System.CommandLine
{
    /// <summary>
    /// Provides extension methods for symbols.
    /// </summary>
    internal static class SymbolExtensions
    {
        internal static IList<Argument> Arguments(this Symbol symbol)
        {
            switch (symbol)
            {
                case Option option:
                    return new[]
                    {
                        option.Argument
                    };
                case Command command:
                    return command.Arguments;
                case Argument argument:
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