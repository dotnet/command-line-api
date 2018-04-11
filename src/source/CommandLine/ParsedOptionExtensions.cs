// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class ParsedOptionExtensions
    {
        public static IEnumerable<OptionError> ValidateAll(
            this ParsedOption option) =>
            new[] { option.Validate() }
                .Concat(
                    option.ParsedOptions
                          .SelectMany(ValidateAll))
                .Where(o => o != null);

        internal static IEnumerable<ParsedOption> FlattenBreadthFirst(
            this IEnumerable<ParsedOption> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var item in options.FlattenBreadthFirst(o => o.ParsedOptions))
            {
                yield return item;
            }
        }

        public static T Value<T>(this ParsedOption option)
        {
            if (option != null)
            {
                return (T) option.Value();
            }
            else
            {
                return default(T);
            }
        }
    }
}
