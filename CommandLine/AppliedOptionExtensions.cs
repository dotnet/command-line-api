using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.DotNet.Cli.CommandLine
{
    public static class AppliedOptionExtensions
    {
        public static IEnumerable<OptionError> ValidateAll(
            this AppliedOption option) =>
            new[] { option.Validate() }
                .Concat(
                    option.AppliedOptions
                          .SelectMany(ValidateAll))
                .Where(o => o != null);

        internal static IEnumerable<AppliedOption> FlattenBreadthFirst(
            this IEnumerable<AppliedOption> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (var item in options.FlattenBreadthFirst(o => o.AppliedOptions))
            {
                yield return item;
            }
        }
    }
}