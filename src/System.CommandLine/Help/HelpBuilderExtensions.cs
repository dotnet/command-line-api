// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace System.CommandLine.Help
{
    internal static class HelpBuilderExtensions
    {
        internal static IEnumerable<Symbol> GetParameters(this Symbol symbol)
        {
            switch (symbol)
            {
                case Option option:
                    yield return option;
                    yield break;
                case Command command:
                    foreach (var argument in command.Arguments)
                    {
                        yield return argument;
                    }
                    yield break;
                case Argument argument:
                    yield return argument;
                    yield break;
                default:
                    throw new NotSupportedException();
            }
        }

        internal static (string? Prefix, string Alias) SplitPrefix(this string rawAlias)
        {
            return rawAlias[0] switch
            {
                '/' => ("/", rawAlias[1..]),
                '-' when rawAlias.Length > 1 && rawAlias[1] is '-' => ("--", rawAlias[2..]),
                '-' => ("-", rawAlias[1..]),
                _ => (null, rawAlias)
            };
        }

        internal static IEnumerable<T> RecurseWhileNotNull<T>(this T? source, Func<T, T?> next) where T : class
        {
            while (source is not null)
            {
                yield return source;

                source = next(source);
            }
        }
    }
}