// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ParsedSymbolExtensions
    {
        public static IEnumerable<ParseError> ValidateAll(
            this ParsedSymbol option) =>
            new[] { option.Validate() }
                .Concat(
                    option.Children
                          .SelectMany(ValidateAll))
                .Where(o => o != null);

        internal static IEnumerable<ParsedSymbol> FlattenBreadthFirst(
            this IEnumerable<ParsedSymbol> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var item in options.FlattenBreadthFirst(o => o.Children))
            {
                yield return item;
            }
        }

        public static T GetValueOrDefault<T>(this ParsedSymbol option)
        {
            if (option?.Result() is SuccessfulArgumentParseResult<T> successfulResult)
            {
                return successfulResult.Value;
            }

            return default(T);
        }
    }
}
